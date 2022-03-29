using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankService.Context;
using RankService.MessageBus;
using RankService.Models;
using System.Collections.Generic;
using System.Linq;

namespace RankService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RankController : ControllerBase
    {
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<RankController> _logger;
        private readonly SearchDataContext _context;


        public RankController(
            ILogger<RankController> logger,
            IMessageBusClient messageBus,
            SearchDataContext context)
        {
            _messageBus = messageBus;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IEnumerable<SearchData> GetSearches()
        {
            return _context.SearchData;
        }

        [HttpGet("searchdata/{word}")]
        public IEnumerable<SearchData> GetSearchDataByWord([FromRoute] string word)
        {
            return _context.SearchData
                .Where(data => data.Text.Contains(word))
                .ToList();
        }

        [HttpGet("autocomplete/{word}")]
        public List<string> GetAutocompleteDataByWord([FromRoute] string word)
        {
            return _context.SearchData
                .Where(data => data.Text.StartsWith(word))
                .Select(data => data.Text)
                .ToList();
        }
    }
}
