using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class StatusCodeAttribute : Attribute
    {
        public StatusCodeAttribute(int statusCode)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }

}
