//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using Microsoft.AspNetCore.Mvc.Controllers;
//using Microsoft.AspNetCore.Mvc.Infrastructure;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using Microsoft.Extensions.Options;

//namespace Microsoft.AspNetCore.Mvc.ApiExplorer
//{
//    public class ApiBehaviorApiDescriptionProvider : IApiDescriptionProvider
//    {
//        private readonly ApiResponseTypeCollator _collator;

//        public ApiBehaviorApiDescriptionProvider(
//            IModelMetadataProvider modelMetadataProvider, 
//            IOptions<MvcOptions> options,
//            IActionResultTypeMapper mapper)
//        {
//            _collator = new ApiResponseTypeCollator(modelMetadataProvider, mapper, options.Value);
//        }

//        public int Order => -1000 + 10;

//        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
//        {
            
//        }

//        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
//        {
//            foreach (var apiDescription in context.Results)
//            {
//                if (!(apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
//                {
//                    continue;
//                }

//                if (apiDescription.SupportedResponseTypes.Count > 0)
//                {
//                    // Don't read values from conventions if the action has values for it.
//                    continue;
//                }

//                var apiControllerFilter = apiDescription.ActionDescriptor.FilterDescriptors.FirstOrDefault(f => f.Filter is ApiControllerAttribute);
//                var apiControllerAttribute = (ApiControllerAttribute)apiControllerFilter?.Filter;

//                if (apiControllerAttribute?.ConventionType == null)
//                {
//                    continue;
//                }

//                var conventions = GetMethodConvention(apiDescription, controllerActionDescriptor, apiControllerAttribute.ConventionType);
//                foreach (var apiResponseType in conventions.SupportedResponseTypes)
//                {
//                    apiDescription.SupportedResponseTypes.Add(apiResponseType);
//                }
//            }
//        }

       

//        public class Convention
//        {
//            /// <summary>
//            /// Gets the list of possible formats for a response.
//            /// </summary>
//            public IList<ApiResponseType> SupportedResponseTypes { get; set; }
//        }
//    }
//}
