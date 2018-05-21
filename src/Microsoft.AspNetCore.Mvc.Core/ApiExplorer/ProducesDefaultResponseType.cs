using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc
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
