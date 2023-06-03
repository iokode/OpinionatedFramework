using System;

namespace IOKode.OpinionatedFramework.Ensuring;

/// <summary>
/// The Thrower class is used to handle the validation result within the Ensure API. 
/// It receives a boolean flag indicating the validation result and provides a method 
/// to throw an exception when the validation does not pass.
/// </summary>
public class Thrower
{
    private readonly bool _validationWasPassed;

    /// <summary>
    /// Initializes a new instance of the Thrower class.
    /// </summary>
    /// <param name="validationWasPassed">Boolean value representing the result of the validation check.</param>
    public Thrower(bool validationWasPassed)
    {
        _validationWasPassed = validationWasPassed;
    }

    /// <summary>
    /// Throws the provided exception if the validation did not pass.
    /// </summary>
    /// <param name="ex">The exception to be thrown if the validation did not pass.</param>
    /// <exception cref="Exception">Thrown if the validation did not pass.</exception>
    public void ElseThrows(Exception ex)
    {
        if (!_validationWasPassed)
        {
            throw ex;
        }
    }

}