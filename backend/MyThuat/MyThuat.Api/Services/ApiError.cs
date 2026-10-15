namespace MyThuat.Api.Services;
public sealed class ApiError(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
    public static void Require(bool condition, string message, int status = 400)
    {
        if (!condition) throw new ApiError(status, message);
    }
}
