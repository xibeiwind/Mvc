using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc
{
    public class ProducesDefaultResponseAttribute : Attribute, IApiResponseMetadataProvider
    {
        public ProducesDefaultResponseAttribute(int statusCode)
        {

        }

        public ProducesDefaultResponseAttribute() : this(StatusCodes.Status200OK)
        {
            
        }

        Type IApiResponseMetadataProvider.Type => null;

        int IApiResponseMetadataProvider.StatusCode { get; }

        void IApiResponseMetadataProvider.SetContentTypes(MediaTypeCollection contentTypes)
        {
        }
    }
}
