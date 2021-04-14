using System;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers
{
    public class LinkController : ControllerBase
    {
        private readonly IStorage _storage;

        public LinkController(IStorage storage)
        {
            _storage = storage;
        }

        [HttpPost("link")]
        public IActionResult Post([FromBody] string url)
        {
            if (NotValidUrl(url))
            {
                return BadRequest();
            }

            var shortName = _storage.Add(url);
            return CreatedAtRoute("Follow", new { id = shortName }, url);
        }

        [HttpGet("~/{id}", Name = "Follow")]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var link = _storage.Get(id);

            if (!string.IsNullOrEmpty(link))
            {
                return Redirect(link);
            }

            return NotFound();
        }

        private static bool NotValidUrl(string source) => !Uri.TryCreate(source, UriKind.Absolute, out Uri _);
    }
}
