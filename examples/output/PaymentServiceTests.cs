// ==========================================
// Tests — xUnit + Moq (auto-generated for [Test])
// ==========================================
using Xunit;
using Moq;
using Polly;
using Tommy.Payments.Domain;
using Tommy.Payments.Gateways;
using Tommy.Payments.Infrastructure;
using Tommy.Payments.Services;
using Microsoft.Extensions.Logging.Abstractions;
using MassTransit;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentGateway> _gw = new();
    private readonly Mock<IPaymentRepository> _repo = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IPublishEndpoint> _bus = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        var provider = new ResiliencePipelineProviderStub();
        _sut = new PaymentService(_gw.Object, _repo.Object, _cache.Object, _bus.Object,
            NullLogger<PaymentService>.Instance, provider);
        _cache.Setup(c => c.SetNxAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);
    }

    [Fact]
    public async Task CreatePayment_ZeroAmount_Throws()
    {
        var req = new PaymentRequest { OrderId = 1, Amount = 0, Email = "a@b.c" };
        await Assert.ThrowsAsync<PaymentFailedException>(() => _sut.CreatePaymentAsync(req));
    }

    [Fact]
    public async Task CreatePayment_Duplicate_ReturnsConflict()
    {
        _cache.Setup(c => c.SetNxAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);
        var result = await _sut.CreatePaymentAsync(new PaymentRequest { OrderId = 1, Amount = 10 });
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Conflict>(result);
    }

    [Fact]
    public async Task CreatePayment_CardDeclined_Throws()
    {
        _gw.Setup(g => g.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await Assert.ThrowsAsync<PaymentFailedException>(() =>
            _sut.CreatePaymentAsync(new PaymentRequest { OrderId = 2, Amount = 10 }));
    }

    [Theory]
    [InlineData(10.0)]
    [InlineData(99.99)]
    public async Task CreatePayment_Success_PersistsAndPublishes(double amount)
    {
        _gw.Setup(g => g.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _sut.CreatePaymentAsync(new PaymentRequest { OrderId = 3, Amount = amount, Email = "x@y.z" });

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<object>>(result);
        _repo.Verify(r => r.InsertAsync(3, amount, "x@y.z", It.IsAny<CancellationToken>()), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<PaymentCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync("payment_3", It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ResiliencePipelineProviderStub : ResiliencePipelineProvider<string>
{
    private readonly ResiliencePipeline _p = new ResiliencePipelineBuilder().Build();
    public override ResiliencePipeline GetPipeline(string key) => _p;
    public override bool TryGetPipeline(string key, out ResiliencePipeline pipeline)
    { pipeline = _p; return true; }
}