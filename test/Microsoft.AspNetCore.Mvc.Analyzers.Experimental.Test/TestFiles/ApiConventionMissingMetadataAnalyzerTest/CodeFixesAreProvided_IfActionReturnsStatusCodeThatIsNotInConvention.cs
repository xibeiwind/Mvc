using Microsoft.AspNetCore.Mvc;

namespace MyApp.ApiConventionMissingMetadataAnalyzerTest
{
    [ApiController]
    public class CodeFixesAreProvided_IfActionReturnsStatusCodeThatIsNotInConventionController : ControllerBase
    {
        [ProducesResponseType(204)]
        public ActionResult<object> Get(int id)
        {
            if (id <= -1)
            {
                return NotFound();
            }

            if (id == 0)
            {
                return NoContent();
            }

            return new object();
        }
    }
}
