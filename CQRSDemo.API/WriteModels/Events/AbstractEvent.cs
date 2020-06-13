using CQRSlite.Events;
using System;

namespace CQRSDemo.API.WriteModels.Events
{
    public class AbstractEvent : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
