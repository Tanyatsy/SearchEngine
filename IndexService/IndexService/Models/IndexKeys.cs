using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace IndexService.Models
{
    public class IndexKeys
    {
        public Guid Id { get; set; }
        public List<string> Keywords { get; set; }
    }
}
