using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Glense.VideoCatalogue.GrpcClients;

/// <summary>
/// gRPC client interceptor that attaches the X-Internal-Api-Key header
/// to outgoing gRPC calls to other services.
/// </summary>
public class InternalApiKeyClientInterceptor : Interceptor
{
    private readonly string _apiKey;

    public InternalApiKeyClientInterceptor(IConfiguration configuration)
    {
        _apiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
            ?? configuration["InternalApiKey"]
            ?? throw new InvalidOperationException("INTERNAL_API_KEY is not configured");
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        headers.Add("x-internal-api-key", _apiKey);

        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }
}
