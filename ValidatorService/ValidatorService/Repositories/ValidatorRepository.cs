using ValidatorService.Context;
using ValidatorService.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValidatorService.Elasticsearch;

namespace ValidatorService.Repositories
{
    public class ValidatorRepository
    {
        private readonly IMongoCollection<ValidatorKeys> _validSearches;
        static MongoClient client = new MongoClient("mongodb://mongo:27017");
        public ValidatorRepository(IValidatorDatabaseSettings settings)
        {
            var database = client.GetDatabase("test");
            _validSearches = database.GetCollection<ValidatorKeys>("Validator");
        }

        public List<ValidatorKeys> Get()
        {
            List<ValidatorKeys> validSearches;
            validSearches = _validSearches.Find(Validator => true).ToList();
            return validSearches;
        }

        public ValidatorKeys Get(Guid id) =>
            _validSearches.Find<ValidatorKeys>(Validator => Validator.Id == id).FirstOrDefault();

        public async Task SaveWordAsync(ValidatorKeys data)
        {
            try
            {
                await _validSearches.InsertOneAsync(data);
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e.Message, "Error writing to Mongo1");

                var client2 = new MongoClient("mongodb://mongo2:27017");
                var database2 = client2.GetDatabase("test");
                var _validSearches2 = database2.GetCollection<ValidatorKeys>("Validator");
                try
                {
                    await _validSearches2.InsertOneAsync(data);
                }
                catch (Exception e2)
                {
                    ElkSearching.logger.Error(e2.Message, "Error writing to Mongo2");

                    var client3 = new MongoClient("mongodb://mongo3:27017");
                    var database3 = client3.GetDatabase("test");
                    var _validSearches3 = database3.GetCollection<ValidatorKeys>("Validator");
                    try
                    {
                        await _validSearches3.InsertOneAsync(data);
                    }
                    catch (Exception e3)
                    {
                        ElkSearching.logger.Error(e3.Message, "Error writing to Mongo3");
                        await SaveWordAsync(data);
                    }
                }
            }
        }

        public async Task AbortTransactionAsync(Guid transactionId)
        {
            try
            {
                await _validSearches.DeleteOneAsync(x => x.Id == transactionId);
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e.Message, "Error abort transaction to Mongo1");

                var client2 = new MongoClient("mongodb://mongo2:27017");
                var database2 = client2.GetDatabase("test");
                var _validSearches2 = database2.GetCollection<ValidatorKeys>("Validator");
                try
                {
                    await _validSearches2.DeleteOneAsync(x => x.Id == transactionId);
                }
                catch (Exception e2)
                {
                    ElkSearching.logger.Error(e2.Message, "Error abort transaction to Mongo2");

                    var client3 = new MongoClient("mongodb://mongo3:27017");
                    var database3 = client3.GetDatabase("test");
                    var _validSearches3 = database3.GetCollection<ValidatorKeys>("Validator");
                    try
                    {
                        await _validSearches3.DeleteOneAsync(x => x.Id == transactionId);
                    }
                    catch (Exception e3)
                    {
                        ElkSearching.logger.Error(e3.Message, "Error abort transaction to Mongo3");

                        await AbortTransactionAsync(transactionId);
                    }
                }
            }
        }
    }
}
