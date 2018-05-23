using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    internal class ApiResponseTypeCollator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IActionResultTypeMapper _mapper;
        private readonly MvcOptions _mvcOptions;

        public ApiResponseTypeCollator(
            IModelMetadataProvider modelMetadataProvider,
            IActionResultTypeMapper mapper,
            MvcOptions mvcOptions)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _mapper = mapper;
            _mvcOptions = mvcOptions;
        }

        public IList<ApiResponseType> GetApiResponseTypes(ControllerActionDescriptor action)
        {
            var declaredReturnType = GetDeclaredReturnType(action);
            var runtimeReturnType = GetRuntimeReturnType(declaredReturnType);

            var responseMetadataAttributes = GetResponseMetadataAttributes(action);
            if (responseMetadataAttributes.Length == 0)
            {
                responseMetadataAttributes = GetResponseMetadataAttributesFromConventions(action);
            }

            var apiResponseTypes = GetApiResponseTypes(responseMetadataAttributes, runtimeReturnType);
            return apiResponseTypes;
        }

        private IApiResponseMetadataProvider[] GetResponseMetadataAttributesFromConventions(ControllerActionDescriptor action)
        {
            var filter = (IApiBehaviorConventionProviderType)action.FilterDescriptors.FirstOrDefault(f => f.Filter is IApiBehaviorConventionProviderType)?.Filter;
            if (filter == null)
            {
                return Array.Empty<IApiResponseMetadataProvider>();
            }

            var method = GetConventionMethod(action.MethodInfo, filter.ConventionType);
            if (method == null)
            {
                return Array.Empty<IApiResponseMetadataProvider>();
            }

            return method.GetCustomAttributes(inherit: false).OfType<IApiResponseMetadataProvider>().ToArray();
        }

        private MethodInfo GetConventionMethod(MethodInfo methodInfo, Type conventions)
        {
            var methodParameters = methodInfo.GetParameters();
            var conventionMethods = conventions.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            return conventionMethods.FirstOrDefault(conventionMethod =>
            {
                // methodInfo = PostUser, convention = Post
                if (!conventionMethod.Name.StartsWith(methodInfo.Name))
                {
                    return false;
                }

                var conventionMethodParameters = conventionMethod.GetParameters();
                if (conventionMethodParameters.Length != methodParameters.Length)
                {
                    return false;
                }

                for (var i = 0; i < conventionMethodParameters.Length; i++)
                {
                    if (conventionMethodParameters[i].ParameterType.IsGenericTypeDefinition)
                    {
                        // Use TModel as wildcard
                        continue;
                    }
                    else if (IsNameMatch(methodParameters[i].Name, conventionMethodParameters[i].Name))
                    {
                        return false;
                    }
                }

                return true;
            });

            bool IsNameMatch(string name, string conventionName)
            {
                // name = id, conventionName = id
                if (string.Equals(name, conventionName, StringComparison.Ordinal))
                {
                    return true;
                }

                // name = personId, conventionName = id
                if (name.Length > conventionName.Length && 
                    char.IsLower(name[name.Length - conventionName.Length]) &&
                    )
                {
                    for (var i = 0; i < conventionName.Length; i++)
                    {
                        if (i == 0)
                    }
                }
            }
        }

        protected virtual IApiResponseMetadataProvider[] GetResponseMetadataAttributes(ControllerActionDescriptor action)
        {
            if (action.FilterDescriptors == null)
            {
                return null;
            }

            // This technique for enumerating filters will intentionally ignore any filter that is an IFilterFactory
            // while searching for a filter that implements IApiResponseMetadataProvider.
            //
            // The workaround for that is to implement the metadata interface on the IFilterFactory.
            return action.FilterDescriptors
                .Select(fd => fd.Filter)
                .OfType<IApiResponseMetadataProvider>()
                .ToArray();
        }

        private IList<ApiResponseType> GetApiResponseTypes(
           IApiResponseMetadataProvider[] responseMetadataAttributes,
           Type type)
        {
            var results = new List<ApiResponseType>();

            // Build list of all possible return types (and status codes) for an action.
            var objectTypes = new Dictionary<int, Type>();

            // Get the content type that the action explicitly set to support.
            // Walk through all 'filter' attributes in order, and allow each one to see or override
            // the results of the previous ones. This is similar to the execution path for content-negotiation.
            var contentTypes = new MediaTypeCollection();
            if (responseMetadataAttributes != null)
            {
                foreach (var metadataAttribute in responseMetadataAttributes)
                {
                    metadataAttribute.SetContentTypes(contentTypes);

                    if (metadataAttribute.Type != null)
                    {
                        objectTypes[metadataAttribute.StatusCode] = metadataAttribute.Type;
                    }
                    else if (metadataAttribute is ProducesDefaultResponseAttribute && type != null)
                    {
                        objectTypes[metadataAttribute.StatusCode] = type;
                    }
                }
            }

            // Set the default status only when no status has already been set explicitly
            if (objectTypes.Count == 0 && type != null)
            {
                objectTypes[StatusCodes.Status200OK] = type;
            }

            if (contentTypes.Count == 0)
            {
                contentTypes.Add((string)null);
            }

            var responseTypeMetadataProviders = _mvcOptions.OutputFormatters.OfType<IApiResponseTypeMetadataProvider>();

            foreach (var objectType in objectTypes)
            {
                if (objectType.Value == null || objectType.Value == typeof(void))
                {
                    results.Add(new ApiResponseType()
                    {
                        StatusCode = objectType.Key,
                        Type = objectType.Value
                    });

                    continue;
                }

                var apiResponseType = new ApiResponseType()
                {
                    Type = objectType.Value,
                    StatusCode = objectType.Key,
                    ModelMetadata = _modelMetadataProvider.GetMetadataForType(objectType.Value)
                };

                foreach (var contentType in contentTypes)
                {
                    foreach (var responseTypeMetadataProvider in responseTypeMetadataProviders)
                    {
                        var formatterSupportedContentTypes = responseTypeMetadataProvider.GetSupportedContentTypes(
                            contentType,
                            objectType.Value);

                        if (formatterSupportedContentTypes == null)
                        {
                            continue;
                        }

                        foreach (var formatterSupportedContentType in formatterSupportedContentTypes)
                        {
                            apiResponseType.ApiResponseFormats.Add(new ApiResponseFormat()
                            {
                                Formatter = (IOutputFormatter)responseTypeMetadataProvider,
                                MediaType = formatterSupportedContentType,
                            });
                        }
                    }
                }

                results.Add(apiResponseType);
            }

            return results;
        }

        private Type GetDeclaredReturnType(ControllerActionDescriptor action)
        {
            var declaredReturnType = action.MethodInfo.ReturnType;
            if (declaredReturnType == typeof(void) ||
                declaredReturnType == typeof(Task))
            {
                return typeof(void);
            }

            // Unwrap the type if it's a Task<T>. The Task (non-generic) case was already handled.
            Type unwrappedType = declaredReturnType;
            if (declaredReturnType.IsGenericType &&
                declaredReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                unwrappedType = declaredReturnType.GetGenericArguments()[0];
            }

            // If the method is declared to return IActionResult or a derived class, that information
            // isn't valuable to the formatter.
            if (typeof(IActionResult).IsAssignableFrom(unwrappedType))
            {
                return null;
            }

            // If we get here, the type should be a user-defined data type or an envelope type
            // like ActionResult<T>. The mapper service will unwrap envelopes.
            unwrappedType = _mapper.GetResultDataType(unwrappedType);
            return unwrappedType;
        }

        public Type GetRuntimeReturnType(Type declaredReturnType)
        {
            // If we get here, then a filter didn't give us an answer, so we need to figure out if we
            // want to use the declared return type.
            //
            // We've already excluded Task, void, and IActionResult at this point.
            //
            // If the action might return any object, then assume we don't know anything about it.
            if (declaredReturnType == typeof(object))
            {
                return null;
            }

            return declaredReturnType;
        }
    }
}
