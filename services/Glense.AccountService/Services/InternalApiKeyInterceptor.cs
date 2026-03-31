using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Glense.AccountService.Services;

/// <summary>
/// gRPC server interceptor that validates the X-Internal-Api-Key header
/// on incoming gRPC calls from other services.
/// </summary>
public class InternalApiKeyInterceptor : Interceptor
{
    private readonly string _expectedApiKey;
    private readonly ILogger<InternalApiKeyInterceptor> _logger;
    private const string ApiKeyHeader = "x-internal-api-key";

    public InternalApiKeyInterceptor(IConfiguration configuration, ILogger<InternalApiKeyInterceptor> logger)
    {
        _expectedApiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
            ?? configuration["InternalApiKey"]
            ?? throw new InvalidOperationException("INTERNAL_API_KEY is not configured");
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var apiKey = context.RequestHeaders.GetValue(ApiKeyHeader);

        if (string.IsNullOrEmpty(apiKey) || apiKey != _expectedApiKey)
        {
            _logger.LogWarning("Rejected gRPC call to {Method} — invalid or missing API key from {Peer}",
                context.Method, context.Peer);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or missing internal API key"));
        }

        return await continuation(request, context);
    }
}
