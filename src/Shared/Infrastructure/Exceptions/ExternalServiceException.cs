namespace Infrastructure;

public class ExternalServiceException(string message) : InfastructureException(message, 503)
{
}
