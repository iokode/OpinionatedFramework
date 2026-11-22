using System;
using System.Collections;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The errors found while validating the data of a command or an input. Empty when the data is valid.
/// </summary>
public sealed class ValidationErrors : IReadOnlyCollection<ValidationError>
{
    private readonly List<ValidationError> errors = [];

    public int Count => this.errors.Count;

    /// <summary>
    /// Adds an error about a property of the object that is validated.
    /// </summary>
    /// <param name="propertyName">The name of the property, such as <c>nameof(Name)</c>.</param>
    /// <param name="message">What is wrong with the property.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyName"/> or <paramref name="message"/> is <see langword="null"/>, empty or white space.
    /// </exception>
    public void Add(string propertyName, string message)
    {
        Add(new ValidationPath(new PropertyPathSegment(propertyName)), message);
    }

    /// <summary>
    /// Adds an error about the data at <paramref name="path"/> of the object that is validated.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty or white space.</exception>
    public void Add(ValidationPath path, string message)
    {
        this.errors.Add(new PathValidationError(path, message));
    }

    /// <summary>
    /// Adds an error about the object that is validated as a whole, such as a rule that involves several of its fields.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty or white space.</exception>
    public void AddForObject(string message)
    {
        this.errors.Add(new ObjectValidationError(message));
    }

    /// <summary>
    /// Adds the errors of data inside the object that is validated, prefixing <paramref name="segment"/> to their path.
    /// An error about the whole inner object becomes an error with <paramref name="segment"/> as its path.
    /// </summary>
    internal void AddNested(ValidationPathSegment segment, ValidationErrors nested)
    {
        foreach (var error in nested.errors)
        {
            this.errors.Add(error switch
            {
                PathValidationError pathError => new PathValidationError(pathError.Path.Prepend(segment), pathError.Message),
                ObjectValidationError objectError => new PathValidationError(new ValidationPath(segment), objectError.Message),
                _ => throw new InvalidOperationException($"Unknown validation error '{error.GetType()}'.")
            });
        }
    }

    public IEnumerator<ValidationError> GetEnumerator()
    {
        return this.errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
