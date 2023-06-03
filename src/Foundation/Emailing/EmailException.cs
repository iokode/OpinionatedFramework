using System;

namespace IOKode.OpinionatedFramework.Emailing;

public class EmailException : Exception
{
    public EmailException(string message) : base(message)
    {
    }

    public EmailException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}