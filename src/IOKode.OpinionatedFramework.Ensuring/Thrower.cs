using System;

namespace IOKode.OpinionatedFramework.Ensuring;

public class Thrower
{
    private readonly bool _isValid;

    public Thrower(bool isValid)
    {
        _isValid = isValid;
    }

    /// <exception cref="Exception">Thrown an exception when the validation no passes.</exception>
    public void ElseThrows(Exception ex)
    {
        if (!_isValid)
        {
            throw ex;
        }
    }
}