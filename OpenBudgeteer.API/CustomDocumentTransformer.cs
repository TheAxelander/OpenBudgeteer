using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OpenBudgeteer.API;

public class CustomDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Version = "1.1.0";
        document.Info.Title = "OpenBudgeteer API";
        return Task.CompletedTask;
    }
}