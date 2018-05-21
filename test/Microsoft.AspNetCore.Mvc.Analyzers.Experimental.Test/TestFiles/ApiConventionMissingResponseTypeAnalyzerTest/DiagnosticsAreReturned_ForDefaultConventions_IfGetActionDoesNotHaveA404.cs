using Microsoft.AspNetCore.Mvc;

namespace MyApp.ApiConventionMissingResponseTypeAnalyzerTest
{
    [ApiController]
    public class DiagnosticsAreReturned_ForDefaultConventions_IfGetActionDoesNotHaveA404Controller : ControllerBase
    {
        /*MM*/public ActionResult<object> Get(int id)
        {
            return new object();
        }
    }
}
