// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc
{
    [StatusCode(StatusCodes.Status204NoContent)]
    public class NoContentResult : StatusCodeResult
    {
        public NoContentResult()
            : base(StatusCodes.Status204NoContent)
        {
        }
    }
}
