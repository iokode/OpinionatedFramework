using System;

namespace IOKode.OpinionatedFramework.Events.Extensions;

/// <summary>
/// Declares the events the framework raises on behalf of the application.
/// </summary>
public static class BuiltInEventDeclarations
{
    /// <summary>
    /// Declares that this application raises every built-in event.
    /// </summary>
    /// <remarks>
    /// Built-in events are opt-in: a driver raises one only for an application that declared it, exactly as it
    /// dispatches nothing else the application did not declare it raises. Reacting to one is declared apart,
    /// with <see cref="EventDeclarations{TPolicy}.Handles{TEvent,THandler}"/>, like any other event.
    /// </remarks>
    /// <param name="declarations">The event declarations of the selected driver.</param>
    /// <typeparam name="TPolicy">The execution policy owned by the driver.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="declarations"/> is <see langword="null"/>.</exception>
    public static void PublishesBuiltInEvents<TPolicy>(this EventDeclarations<TPolicy> declarations)
        where TPolicy : new()
    {
        ArgumentNullException.ThrowIfNull(declarations);

        declarations.Publishes<EventHandlerExhausted>();
    }
}
