using CQRSlite.Events;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.ReadModels
{
    public class CustomerEventEntity
    {
        [BsonElement("Id")]
        public Guid Id { get; set; }

        [BsonElement("EventId")]
        public Guid EventId { get; set; }

        [BsonElement("Version")]
        public int Version { get; set; }

        [BsonElement("TimeStamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [BsonElement("EventType")]
        public string EventType { get; set; }

        [BsonElement("EventData")]
        public IEvent EventData { get; set; }
    }
}
