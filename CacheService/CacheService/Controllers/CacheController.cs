using CacheService.Extensions;
using CacheService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheController> _logger;
        
        public CacheController(IDistributedCache distributedCache,
             ILogger<CacheController> logger)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet("/empty")]
        public List<PageData> Get()
        {
            return new List<PageData>();
        }

        [HttpGet]
        public async Task<List<PageData>> GetAsync([FromQuery] string text)
        {
            var data = await Extensions.DistributedCacheExtensions.GetRecordAsync<List<PageData>>(_distributedCache, text);
           /* throw new Exception("Server is down");*/
            return data ?? new List<PageData>();
        }

        [HttpGet("/Cache/complete")]
        public async Task<List<string>> GetCompleteAsync([FromQuery] string text)
        {
            var data = await Extensions.DistributedCacheExtensions.GetRecordAsync<List<string>>(_distributedCache, text);
            /* throw new Exception("Server is down");*/
            return data ?? new List<string>();
        }
    }
}
