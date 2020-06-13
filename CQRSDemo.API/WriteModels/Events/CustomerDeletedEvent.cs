using System;

namespace CQRSDemo.API.WriteModels.Events
{
    public class CustomerDeletedEvent : AbstractEvent
    {
        public CustomerDeletedEvent(Guid id, int version)
        {
            Id = id;
            Version = version;
        }
    }
}
