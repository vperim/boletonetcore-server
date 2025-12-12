using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoletoNetCore.Server.Versioning;

public class SwaggerVersioningOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var version = context.ApiDescription.GroupName;
        if (string.IsNullOrEmpty(version))
            return;

        operation.OperationId = GenerateOperationId(context);
    }

    private static string GenerateOperationId(OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var serviceName = method.DeclaringType?.Name.Replace("Service", "") ?? "Unknown";
        var methodName = method.Name;

        return $"{serviceName}_{methodName}";
    }
}
