using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace MvcSandbox.Controllers
{
    [ApiController(ConventionType = typeof(DefaultConventions))]
    [Route("[controller]/[action]")]
    public class PersonController : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> Get(int id)
        {
            await Task.Delay(0);
            return new Person();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> Get()
        {
            await Task.Delay(0);

            var persons = DbContext.Persons.Where(x => x.IsCool);
            return persons.ToList();
        }

        private class DbContext
        {
            public static ICollection<Person> Persons { get; }
        }
    }

    public abstract class DefaultConventions
    {
        [ProducesDefaultResponse]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public abstract Task<ActionResult<TModel>> Get<TModel>(int id);

        [ProducesDefaultResponse]
        public abstract Task<ActionResult<IEnumerable<TModel>>> Get<TModel>();

        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public abstract Task<ActionResult<TModel>> Post<TModel>(int id, TModel model);
    }

    public class Person
    {
        public bool IsCool { get; set; }
    }

}
