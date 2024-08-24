using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

namespace ACA_CDNProtoType.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CDNController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        public CDNController(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                // Return the cached value
                return Ok(cachedValue);
            }
            else
            {
                // Forward the request to Google if not found in cache
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://www.google.com/search?q={key}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Cache the response content
                    _cache.Set(key, content, TimeSpan.FromMinutes(10));

                    // Return the response content
                    return Ok(content);
                }

                return StatusCode((int)response.StatusCode);
            }
        }

    }
}
