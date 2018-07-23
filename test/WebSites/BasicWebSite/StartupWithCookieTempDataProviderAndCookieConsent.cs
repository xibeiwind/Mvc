// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BasicWebSite
{
    public class StartupWithCookieTempDataProviderAndCookieConsent
    {
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddMvc();

            services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = httpContext => true;
            });

            services.ConfigureBaseWebSiteAuthPolicies();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseGlobalRouting();

            app.UseDeveloperExceptionPage();

            app.UseCookiePolicy();

            app.UseMvcWithEndpoint(routes =>
            {
                routes.MapEndpoint(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

