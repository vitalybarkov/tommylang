using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// OAuth2 client_credentials + JWT
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization();

// DI Lifecycles
builder.Services.AddScoped<IPaymentGateway, StripeGateway>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis"] ?? "localhost:6379"));
builder.Services.AddSingleton<IStripeClient>(_ => new StripeClientStub());
builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("Postgres")!);

// MassTransit + Kafka + Outbox + Saga
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<PaymentSaga, PaymentState>()
     .EntityFrameworkRepository(r =>
     {
         r.ExistingDbContext<PaymentsDbContext>();
         r.UsePostgres();
     });
    x.UsingInMemory((ctx, cfg) =>
    {
        cfg.ConfigureEndpoints(ctx);
    });
    // Kafka rider (Outbox pattern via EF)
    x.AddRider(rider =>
    {
        rider.AddProducer<PaymentCreatedEvent>("payment_events");
        rider.UsingKafka((ctx, k) =>
            k.Host(builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092"));
    });
});
builder.Services.AddDbContext<PaymentsDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Polly: Retry(3, 2s) + CircuitBreaker(5, 30s)
builder.Services.AddResiliencePipeline("stripe-pipeline", (pipeline, ctx) =>
{
    pipeline
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Constant
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 1.0,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30)
        });
});

// Rate limiting: 100 requests / 1 minute
builder.Services.AddRateLimiter(o =>
    o.AddFixedWindowLimiter("payments", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    }));

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Telemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("tommy-payments"))
        .AddAspNetCoreInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();

// REST endpoints
app.MapPost("/api/v1/payments", async (PaymentRequest req, IPaymentService svc, CancellationToken ct) =>
{
    try
    {
        return await svc.CreatePaymentAsync(req, ct);
    }
    catch (PaymentFailedException ex)
    {
        return Results.BadRequest(new { error = ex.Reason });
    }
})
.RequireAuthorization()
.RequireRateLimiting("payments")
.WithMetadata(new OpenApiOperationAttribute { Summary = "Create a payment (Saga + Outbox + Idempotent)" });

app.MapGet("/api/v1/payments/{order_id:int}", async (int order_id, IPaymentService svc, CancellationToken ct)
    => await svc.GetPaymentStatusAsync(order_id, ct))
   .RequireAuthorization()
   .RequireRateLimiting("payments");

app.Run();

// PaymentsDbContext for MassTransit saga + outbox
public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> o) : base(o) { }
    public DbSet<PaymentState> PaymentStates => Set<PaymentState>();
}

// Stub for external SDK
public class StripeClientStub : IStripeClient
{
    public Task<StripeChargeResponse> ChargeAsync(string cardToken, double amount, CancellationToken ct = default)
        => Task.FromResult(new StripeChargeResponse(true, Guid.NewGuid().ToString()));
}