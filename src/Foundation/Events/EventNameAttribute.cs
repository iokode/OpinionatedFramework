using System;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Declares the stable name that identifies an event type beyond the running process.
/// </summary>
/// <remarks>
/// The CLR type is not a durable identifier: it is bound to an assembly and a namespace, so renaming either one
/// would make a stored or in-flight event unreadable. The declared name is what a dispatcher writes into a
/// payload and what a store keeps, so it must outlive refactors and is expected never to change once released.
/// </remarks>
/// <example>
/// <code>
/// [EventName("order.submitted")]
/// public class OrderSubmitted : IPublishableEvent, ISubscribableEvent
/// {
///     public required Guid OrderId { get; init; }
/// }
/// </code>
/// </example>
/// <param name="name">The stable name identifying the event type.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EventNameAttribute(string name) : Attribute
{
    /// <summary>Gets the stable name identifying the event type.</summary>
    public string Name { get; } = name;
}
