using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ProducesDefaultResponseAttribute : Attribute, IApiResponseMetadataProvider
    {
        public Type Type => null;

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        void IApiResponseMetadataProvider.SetContentTypes(MediaTypeCollection contentTypes)
        {
        }
    }
}
