using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.AspNetCore.Mvc.Analyzers.ApiResponseMetadata
{
    internal sealed class AddProducesResponseTypeAttributesCodeFixStrategy : ApiResponseMetadataCodeFixStrategy
    {
        public override async Task ExecuteAsync(ApiResponseMetadataCodeFixStrategyContext context)
        {
            var documentEditor = await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false);

            foreach (var metadata in context.UndocumentedMetadata)
            {
                var producesResponseTypeAttribute = CreateProducesResponseTypeAttribute(metadata);
                documentEditor.AddAttribute(context.MethodSyntax, producesResponseTypeAttribute);
            }

            context.ChangedSolution = documentEditor.GetChangedDocument().Project.Solution;
        }
    }
}
