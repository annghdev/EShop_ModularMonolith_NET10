namespace Kernel.Domain;

public class ConflictException(string operation)
    : DomainException($"{operation} is conflict", 409);
