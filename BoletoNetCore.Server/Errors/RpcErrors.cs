using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;

namespace BoletoNetCore.Server.Errors;

public static class RpcErrors
{
    public static RpcException InvalidArgument(string message) =>
        CreateException(Code.InvalidArgument, message);

    public static RpcException InvalidArgument(params (string Field, string Description)[] violations)
    {
        var badRequest = new BadRequest();
        foreach (var (field, description) in violations)
        {
            badRequest.FieldViolations.Add(new BadRequest.Types.FieldViolation
            {
                Field = field,
                Description = description
            });
        }

        return CreateException(Code.InvalidArgument, "Validation failed", Any.Pack(badRequest));
    }

    public static RpcException NotFound(string message) =>
        CreateException(Code.NotFound, message);

    public static RpcException FailedPrecondition(string message) =>
        CreateException(Code.FailedPrecondition, message);

    public static RpcException Internal(string message) =>
        CreateException(Code.Internal, message);

    private static RpcException CreateException(Code code, string message, params Any[] details)
    {
        var status = new Google.Rpc.Status
        {
            Code = (int)code,
            Message = message
        };

        foreach (var detail in details)
            status.Details.Add(detail);

        var metadata = new Metadata
        {
            { "grpc-status-details-bin", status.ToByteArray() }
        };

        var grpcCode = code switch
        {
            Code.InvalidArgument => StatusCode.InvalidArgument,
            Code.NotFound => StatusCode.NotFound,
            Code.FailedPrecondition => StatusCode.FailedPrecondition,
            Code.Internal => StatusCode.Internal,
            _ => StatusCode.Unknown
        };

        return new RpcException(new Grpc.Core.Status(grpcCode, message), metadata);
    }
}
