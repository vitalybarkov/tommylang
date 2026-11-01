using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using OpenTelemetry.Trace;

namespace Tommy.Payments.Services;

public interface IPaymentService
{
    Task<IResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default);
    Task<IResult> GetPaymentStatusAsync(int orderId, CancellationToken ct = default);
}

[Scoped]
public class PaymentService : IPaymentService
{
    private readonly IPaymentGateway _gateway;
    private readonly IPaymentRepository _repo;
    private readonly ICacheService _cache;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<PaymentService> _logger;
    private readonly ResiliencePipeline _pipeline;

    public PaymentService(
        IPaymentGateway gateway,
        IPaymentRepository repo,
        ICacheService cache,
        IPublishEndpoint bus,
        ILogger<PaymentService> logger,
        ResiliencePipelineProvider<string> pipelines)
    {
        _gateway = gateway;
        _repo = repo;
        _cache = cache;
        _bus = bus;
        _logger = logger;
        _pipeline = pipelines.GetPipeline("stripe-pipeline");
    }

    public async Task<IResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        // Validation
        if (request.Amount <= 0.0)
            throw new PaymentFailedException("Amount must be greater than zero");

        // [Idempotent] Redis SETNX on order_id
        var idemKey = $"idem:order:{request.OrderId}";
        var acquired = await _cache.SetNxAsync(idemKey, "1", TimeSpan.FromMinutes(10), ct);
        if (!acquired)
        {
            _logger.LogWarning("Duplicate payment for order {OrderId}", request.OrderId);
            return Results.Conflict("Payment already processed");
        }

        try
        {
            // Charge with Polly: Retry(3, 2s) + CircuitBreaker(5, 30s)
            var success = await _pipeline.ExecuteAsync(
                async token => await _gateway.ChargeAsync(request, token), ct);

            if (!success)
                throw new PaymentFailedException("Card declined");

            // Persist (parameterized)
            await _repo.InsertAsync(request.OrderId, request.Amount, request.Email, ct);

            // Publish (Outbox guarantees at-least-once with DB tx)
            await _bus.Publish(new PaymentCreatedEvent(request.OrderId), ct);

            // [InvalidateCache]
            await _cache.RemoveAsync($"payment_{request.OrderId}", ct);

            return Results.Created($"/api/v1/payments/{request.OrderId}",
                new { message = "Payment processed successfully", order_id = request.OrderId });
        }
        catch
        {
            await _cache.RemoveAsync(idemKey, ct); // release idempotency on failure
            throw;
        }
    }

    public async Task<IResult> GetPaymentStatusAsync(int orderId, CancellationToken ct = default)
    {
        var status = await _cache.GetAsync($"payment_{orderId}", ct) ?? "unknown";
        return Results.Ok(new { status });
    }
}