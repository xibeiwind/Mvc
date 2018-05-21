// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public abstract class DefaultApiConventions
    {
        [ProducesDefaultResponse]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public abstract void Get<TModel>(int id);

        [ProducesDefaultResponse]
        public abstract void Get<TModel>();

        [ProducesDefaultResponse(StatusCode = StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public abstract void Post<TModel>(TModel model);

        [ProducesDefaultResponse]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public abstract void Put<TModel>(int id, TModel model);

        [ProducesDefaultResponse]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public abstract void Delete<TModel>(int id);
    }
}
