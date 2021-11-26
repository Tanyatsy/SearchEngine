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
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("IndexDB");

            _indexes = database.GetCollection<IndexKeys>("Indexes");

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
            var indexes = _indexes.Find(index => index.Keywords.Any(key => key.StartsWith(text))).ToList();
            var result = indexes.SelectMany(index => index.Keywords.FindAll(key => key.StartsWith(text))).Distinct().ToList();
            return result;
        }

        public IndexKeys Create(IndexKeys indexKeys)
        {
            _indexes.InsertOne(indexKeys);
            return indexKeys;
        }

    }
}
