// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RequestServicesTest : RequestServicesTestBase<BasicWebSite.Startup>
    {
        public RequestServicesTest(MvcTestFixture<BasicWebSite.Startup> fixture)
            : base(fixture)
        {
        }
 
        [Fact(Skip = "Working in release/master")]
        public override Task RequestServices_Constraint()
        {
            return Task.CompletedTask;
        }
    }
}