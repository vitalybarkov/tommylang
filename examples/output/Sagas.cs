using MassTransit;

namespace Tommy.Payments.Sagas;

public class PaymentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public double Amount { get; set; }
}

public class PaymentSaga : MassTransitStateMachine<PaymentState>
{
    public State Processing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<PaymentCreatedEvent> PaymentCreated { get; private set; } = null!;

    public PaymentSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => PaymentCreated, x => x.CorrelateById(c => c.Message.OrderId));

        Initially(
            When(PaymentCreated)
                .TransitionTo(Processing)
                .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
                .TransitionTo(Completed));

        SetCompletedWhenFinalized();
    }
}