namespace Infrastructure;

public class InfastructureException(string message, int statusCode = 500) : Exception(message)
{
    public int StatusCode => statusCode;
}


