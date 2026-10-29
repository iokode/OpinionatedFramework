using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>
/// A delivery handed to a filter directly, without the broker, carrying a token the test controls.
/// </summary>
/// <remarks>
/// Cancelling a delivery for real means stopping the bus the other tests share. A filter only reads the message
/// and the token, so everything else refuses to be called.
/// </remarks>
public sealed class FakeConsumeContext<TMessage>(TMessage message, CancellationToken cancellationToken)
    : ConsumeContext<TMessage>
    where TMessage : class
{
    public TMessage Message => message;

    public CancellationToken CancellationToken => cancellationToken;

    public Guid? MessageId => throw NotUsed();
    public Guid? RequestId => throw NotUsed();
    public Guid? CorrelationId => throw NotUsed();
    public Guid? ConversationId => throw NotUsed();
    public Guid? InitiatorId => throw NotUsed();
    public IDictionary<string, object> Headers => throw NotUsed();

    public Task Send<T>(Uri address, T message, Action<ISendContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task Send<T>(Uri address, object message, Action<ISendContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task Forward<T>(Uri address, T message, CancellationToken cancellationToken = default)
        where T : class =>
        throw NotUsed();

    public Task Forward<T>(Uri address, object message, CancellationToken cancellationToken = default)
        where T : class =>
        throw NotUsed();

    public Task RespondAsync<T>(T message, Action<ISendContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task RespondAsync<T>(object message, Action<ISendContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task Publish<T>(object message, Action<IPublishContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task Publish<T>(T message, Action<IPublishContext>? contextCallback = null,
        CancellationToken cancellationToken = default)
        where T : class => throw NotUsed();

    public Task<ISendEndpoint> GetSendEndpoint(Uri uri) => throw NotUsed();

    private static NotSupportedException NotUsed() => new("A filter under test does not use this member.");
}

/// <summary>The rest of a handler's pipe, standing in for a handler that fails with the given exception.</summary>
public sealed class FailingPipe<TContext>(Exception exception) : IPipe<TContext>
    where TContext : class, PipeContext
{
    public Task Send(TContext context) => Task.FromException(exception);
}
