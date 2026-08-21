using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Odonto.API;

public static class OpenApiConfiguration
{
    private const string BearerScheme = "Bearer";

    public static void ConfigurarJwt(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??=
                new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerScheme] =
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe somente o token JWT, sem adicionar o prefixo Bearer."
                };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, _) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            var permiteAnonimo = metadata.OfType<IAllowAnonymous>().Any();
            var exigeAutorizacao = metadata.OfType<IAuthorizeData>().Any();

            if (!exigeAutorizacao || permiteAnonimo)
            {
                return Task.CompletedTask;
            }

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = []
            });

            return Task.CompletedTask;
        });
    }
}
