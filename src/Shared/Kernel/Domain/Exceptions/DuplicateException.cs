namespace Kernel.Domain;

public class DuplicateException(string entityType, string name)
      : DomainException($"{entityType} with name {name} is exists", 409);