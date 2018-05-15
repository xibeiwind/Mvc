using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ApiBehaviorApiDescriptionProvider : IApiDescriptionProvider
    {
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                if (!(apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                {
                    continue;
                }

                var apiControllerFilter = apiDescription.ActionDescriptor.FilterDescriptors.FirstOrDefault(f => f.Filter is ApiControllerAttribute);
                var apiControllerAttribute = (ApiControllerAttribute)apiControllerFilter?.Filter;

                if (apiControllerAttribute?.Conventions == null)
                {
                    continue;
                }

                var conventions = GetMethodConvention(controllerActionDescriptor, apiControllerAttribute.Conventions);

            }
        }

        private Convention GetMethodConvention(ControllerActionDescriptor controllerActionDescriptor, Type conventions)
        {
            var methodInfo = controllerActionDescriptor.MethodInfo;
            var method = conventions.GetMethod(
                methodInfo.Name,
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
                modifiers: null);

            if (method == null)
            {
                return null;
            }

            return new Convention
            {

            }
        }

        public class Convention
        {
            /// <summary>
            /// Gets the list of possible formats for a response.
            /// </summary>
            public IList<ApiResponseType> SupportedResponseTypes { get; } = new List<ApiResponseType>();
        }
    }
}
