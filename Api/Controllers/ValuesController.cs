using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;
using System.Collections.Generic;

namespace Api.Controllers
{
    // [Microsoft.AspNetCore.Authorization.Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            // throw new System.Exception("This is insanity!!!");
            // Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter g;
            return BadRequest("this is bad news");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            var apiu = this.ToApiUser();

            var userName = User.Identity.Name;
            return $"Super secret content, I hope you've got clearance for this {userName}...";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
