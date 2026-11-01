namespace Tommy.Payments.Gateways;

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(PaymentRequest request, CancellationToken ct = default);
}

[Scoped]
public class StripeGateway : IPaymentGateway
{
    private readonly IStripeClient _stripe;
    private readonly ILogger<StripeGateway> _logger;

    public string ApiKey { get; set; } = string.Empty;

    public StripeGateway(IStripeClient stripe, ILogger<StripeGateway> logger)
    {
        _stripe = stripe;
        _logger = logger;
    }

    public async Task<bool> ChargeAsync(PaymentRequest request, CancellationToken ct = default)
    {
        // [PII] masked — never log raw card/email
        _logger.LogInformation("Charging order {OrderId} amount {Amount}", request.OrderId, request.Amount);
        var response = await _stripe.ChargeAsync(request.CardToken, request.Amount, ct);
        return response.Success;
    }
}

// External SDK stub (auto-generated per Zero-Hallucination policy)
public interface IStripeClient
{
    Task<StripeChargeResponse> ChargeAsync(string cardToken, double amount, CancellationToken ct = default);
}
public record StripeChargeResponse(bool Success, string? TransactionId);