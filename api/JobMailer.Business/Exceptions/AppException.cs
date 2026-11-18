namespace JobMailer.Business.Exceptions;

public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : AppException(message, 404);

public sealed class UnauthorizedAppException(string message) : AppException(message, 401);
