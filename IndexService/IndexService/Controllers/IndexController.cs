using IndexService.Context;
using IndexService.MessageBus;
using IndexService.Models;
using IndexService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndexService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<IndexController> _logger;
        private readonly PageDataContext _context;
        private readonly IndexRepository _indexRepository;

        public IndexController(
            ILogger<IndexController> logger,
            IMessageBusClient messageBus,
            IndexRepository indexRepository,
            PageDataContext context)
        {
            _indexRepository = indexRepository;
            _messageBus = messageBus;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IEnumerable<PageData> GetIndexies()
        {
            return _context.PageData;
        }

        [PortActionConstraint(5001)]
        [HttpGet("pagedata/{word}")]
        public async Task<IEnumerable<PageData>> GetPageDataByWordAsync([FromRoute] string word)
        {
            await Task.Delay(2000);
            return _context.PageData.Where(data => data.Text.ToLower().Contains(word.ToLower())).ToList();
        }

        [PortActionConstraint(5011)]
        [HttpGet("pagedata/{word}")]
        public IEnumerable<PageData> GetPageDataByWordPriority([FromRoute] string word)
        {
            return _context.PageData.Where(data => data.Text.ToLower().Contains(word.ToLower())).ToList();
        }

        [HttpGet("autocomplete/{word}")]
        public List<string> GetAutocompleteDataByWord([FromRoute] string word)
        {
            return _indexRepository.GetByText(word);
        }

        [HttpPost]
        public void PostPageData([FromBody] PageData data)
        {
            _context.PageData.Add(data);
            _context.SaveChanges();

           
            var words = data.Text.Split(new Char[]{ ' ', ','}).GroupBy(x => x)
                        .Where(group => group.Count() > 1 && group.Key.Length > 3)
                        .Select(group => group.Key.Trim().ToLower())
                        .ToList();

            var index = new IndexKeys()
            {
                Id = data.Id,
                Keywords = words
            };

            _indexRepository.Create(index);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PortActionConstraint : ActionMethodSelectorAttribute
    {
        public PortActionConstraint(int port)
        {
            Port = port;
        }

        public int Port { get; }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            //external port
            var externalPort = routeContext.HttpContext.Request.Host.Port;
            //local port 
            var localPort = routeContext.HttpContext.Connection.LocalPort;
            //write here your custom logic. for example  
            return Port == localPort;
        }
    }
}
