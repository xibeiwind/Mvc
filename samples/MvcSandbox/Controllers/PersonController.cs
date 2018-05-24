using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PersonController : ControllerBase
    {
        [HttpGet("{personId}")]
        public async Task<ActionResult<Person>> Get(int personId)
        {
            await Task.Delay(0);
            return new Person();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> Get()
        {
            await Task.Delay(0);

            return Enumerable.Empty<Person>().ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Post(Person person)
        {
            await Task.Delay(0);

            if (person.IsCool)
            {
                return UnprocessableEntity();
            }

            return Ok();
        }
    }

    public class Person
    {
        public bool IsCool { get; set; }
    }

}
