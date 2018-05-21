using Microsoft.AspNetCore.Mvc;

namespace MyApp.ApiConventionMissingResponseTypeAnalyzerTest
{
    [ApiController]
    public class DiagnosticsAreReturned_IfActionMethodHasAttributeController : ControllerBase
    {
        /*MM*/[ProducesDefaultResponse]
        [ProducesResponseType(404)]
        public ActionResult<object> Get(int id)
        {
            return new object();
        }
    }
}
