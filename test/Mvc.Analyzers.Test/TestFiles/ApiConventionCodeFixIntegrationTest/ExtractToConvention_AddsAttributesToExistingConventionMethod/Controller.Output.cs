namespace Microsoft.AspNetCore.Mvc.Analyzers.ExtractToConvention_AddsAttributesToExistingConventionMethod._OUTPUT_
{
    [ApiController]
    [ApiConventionType(typeof(Convention))]
    public class Controller : ControllerBase
    {
        public ActionResult<string> GetPerson(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            else if (!User.IsInRole("SuperAdmin"))
            {
                return Unauthorized();
            }

            return string.Empty;
        }
    }
}
