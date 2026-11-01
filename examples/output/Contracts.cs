using System;
using System.Collections.Generic;

namespace Tommy.Payments.Domain;

public struct PaymentRequest
{
    public int OrderId { get; set; }
    public double Amount { get; set; }
    public string Email { get; set; }      // [PII] masked in logs
    public string CardToken { get; set; }  // [Encrypt] at rest

    public override string ToString()
        => $"PaymentRequest {{ OrderId = {OrderId}, Amount = {Amount}, Email = ****, CardToken = **** }}";
}

public class PaymentFailedException : Exception
{
    public string Reason { get; }
    public PaymentFailedException(string reason) : base(reason) => Reason = reason;
}