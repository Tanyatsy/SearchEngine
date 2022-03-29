using ValidatorService.Context;
using ValidatorService.MessageBus;
using ValidatorService.Models;
using ValidatorService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System;
using ValidatorService.Elasticsearch;

namespace ValidatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValidatorController : ControllerBase
    {
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<ValidatorController> _logger;
        private readonly ValidatorRepository _ValidatorRepository;

        public ValidatorController(
            ILogger<ValidatorController> logger,
            IMessageBusClient messageBus,
            ValidatorRepository ValidatorRepository)
        {
            _ValidatorRepository = ValidatorRepository;
            _messageBus = messageBus;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<ValidatorKeys> GetValidateData()
        {
            return _ValidatorRepository.Get();
        }


        [HttpPost("validate")]
        public async Task<HttpStatusCode> GetDataByWordAsync([FromBody] ValidatorKeys data)
        {
            if (Regex.IsMatch(data.Keyword, "^[a-zA-Z0-9]*$"))
            {
               await _ValidatorRepository.SaveWordAsync(data);
               return HttpStatusCode.OK;
            }
            else
            {
                ElkSearching.logger.Fatal($"Search contains wrong symbols: {data.Keyword}");
                return HttpStatusCode.BadRequest;
            }
        }

        [HttpDelete("abort/{transactionId}")]
        public async Task AbortDataByTransactionIdAsync([FromRoute] Guid transactionId)
        {
           await _ValidatorRepository.AbortTransactionAsync(transactionId);
        }
    }
}
