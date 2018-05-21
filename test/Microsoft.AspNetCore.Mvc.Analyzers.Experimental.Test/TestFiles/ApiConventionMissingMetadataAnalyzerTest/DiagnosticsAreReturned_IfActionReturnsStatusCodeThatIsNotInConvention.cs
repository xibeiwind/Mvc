using Microsoft.AspNetCore.Mvc;

namespace MyApp.ApiConventionMissingMetadataAnalyzerTest
{
    [ApiController]
    public class DiagnosticsAreReturned_IfActionReturnsStatusCodeThatIsNotInConventionController : ControllerBase
    {
        public ActionResult<object> Get(int id)
        {
            if (id <= -1)
            {
                return NotFound();
            }

            if (id == 0)
            {
                /*MM*/return NoContent();
            }

            return new object();
        }
    }
}
