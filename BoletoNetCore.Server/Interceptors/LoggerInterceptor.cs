using Grpc.Core;
using Grpc.Core.Interceptors;

namespace BoletoNetCore.Server.Interceptors;

public class LoggerInterceptor : Interceptor
{
    private readonly ILogger<LoggerInterceptor> logger;

    public LoggerInterceptor(ILogger<LoggerInterceptor> logger)
    {
        this.logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        LogCall<TRequest>(MethodType.Unary, context);

        try
        {
            return await continuation(request, context);
        }
        catch (RpcException rpcEx)
        {
            LogRpcException(rpcEx, context);
            throw;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        LogCall<TRequest>(MethodType.ClientStreaming, context);

        try
        {
            return await continuation(requestStream, context);
        }
        catch (RpcException rpcEx)
        {
            LogRpcException(rpcEx, context);
            throw;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        LogCall<TRequest>(MethodType.ServerStreaming, context);

        try
        {
            await continuation(request, responseStream, context);
        }
        catch (RpcException rpcEx)
        {
            LogRpcException(rpcEx, context);
            throw;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        LogCall<TRequest>(MethodType.DuplexStreaming, context);

        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (RpcException rpcEx)
        {
            LogRpcException(rpcEx, context);
            throw;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context);
        }
    }

    private void LogCall<TRequest>(MethodType methodType, ServerCallContext context)
        where TRequest : class
    {
        this.logger.LogDebug("gRPC {MethodType} call: {Method}", methodType, context.Method);
    }

    private void LogRpcException(RpcException rpcEx, ServerCallContext context)
    {
        this.logger.LogWarning(
            "gRPC {Status} in {Method}: {Detail}",
            rpcEx.StatusCode,
            context.Method,
            rpcEx.Status.Detail);
    }

    private RpcException HandleException(Exception ex, ServerCallContext context)
    {
        this.logger.LogError(ex, "Unhandled exception in {Method}", context.Method);

        var statusCode = ex switch
        {
            ArgumentException => StatusCode.InvalidArgument,
            InvalidOperationException => StatusCode.FailedPrecondition,
            NotImplementedException => StatusCode.Unimplemented,
            UnauthorizedAccessException => StatusCode.PermissionDenied,
            TimeoutException => StatusCode.DeadlineExceeded,
            OperationCanceledException => StatusCode.Cancelled,
            _ => StatusCode.Internal
        };

        return new RpcException(new Status(statusCode, ex.Message));
    }
}
