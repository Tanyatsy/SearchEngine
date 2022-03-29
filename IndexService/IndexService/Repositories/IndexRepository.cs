using IndexService.Elasticsearch;
using IndexService.Context;
using IndexService.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexService.Repositories
{
    public class IndexRepository
    {
        private readonly IMongoCollection<IndexKeys> _indexes;
        public IndexRepository(IIndexDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://mongo:27017");
            var database = client.GetDatabase("test");

            _indexes = database.GetCollection<IndexKeys>("Index");
        }

        public List<IndexKeys> Get()
        {
            List<IndexKeys> employees;
            employees = _indexes.Find(index => true).ToList();
            return employees;
        }

        public IndexKeys Get(Guid id) =>
            _indexes.Find<IndexKeys>(index => index.Id == id).FirstOrDefault();

        public List<string> GetByText(string text)
        {
            var indexes = TryGetIndexesByText(text);
            var result = indexes.SelectMany(index => index.Keywords.FindAll(key => key.StartsWith(text))).Distinct().ToList();
            return result;
        }

        public IndexKeys Create(IndexKeys indexKeys)
        {
            _indexes.InsertOne(indexKeys);
            return indexKeys;
        }

        private List<IndexKeys> TryGetIndexesByText(string text) 
        {
            try
            {
                _indexes.WithReadPreference(ReadPreference.Nearest);
                return _indexes.Find(index => index.Keywords.Any(key => key.StartsWith(text))).ToList();
            }
            catch (Exception e1)
            {
                ElkSearching.logger.Error(e1, "Get Indexes from mongo1");

                var client2 = new MongoClient("mongodb://mongo2:27017");
                var database2 = client2.GetDatabase("test");
                var _indexes2 = database2.GetCollection<IndexKeys>("Index");
                try
                {
                    return _indexes2.Find(index => index.Keywords.Any(key => key.StartsWith(text))).ToList();
                }
                catch (Exception e2)
                {
                    ElkSearching.logger.Error(e2, "Get Indexes from mongo2");

                    var client3 = new MongoClient("mongodb://mongo3:27017");
                    var database3 = client3.GetDatabase("test");
                    var _indexes3 = database3.GetCollection<IndexKeys>("Index");
                    try
                    {
                        return _indexes3.Find(index => index.Keywords.Any(key => key.StartsWith(text))).ToList();
                    }
                    catch (Exception e3)
                    {
                        ElkSearching.logger.Error(e3, "Get Indexes from mongo3");
                        return TryGetIndexesByText(text);
                    }
                }
            }
        }

    }
}
