namespace proyect.application.Exceptions;

public enum BusinessError { Validation, NotFound, Conflict }

public sealed class BusinessRuleException(string message, BusinessError error = BusinessError.Validation) : Exception(message)
{
    public BusinessError Error { get; } = error;
}
