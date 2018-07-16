using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc.Analyzers.ExtractToConvention_AddsAttributesToExistingConventionMethod._INPUT_
{
    public static class Convention
    {
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        [ProducesResponseType(401)]
        public static void Get(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Suffix)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
            object id)
        { }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Post(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
            object model)
        { }
    }
}
