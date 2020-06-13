using CQRSDemo.API.ReadModels;
using CQRSDemo.API.ReadModels.Repositories;
using CQRSlite.Events;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.EventStore
{
    public class CustomerEventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;
        private readonly CustomerEventModelRepository _eventStoreRepository;
        private readonly Dictionary<Guid, List<IEvent>> customerInMemDictionary = new Dictionary<Guid, List<IEvent>>();
        private Logger logger = LogManager.GetLogger("CustomerEventStore");

        public CustomerEventStore(IEventPublisher eventPublisher, CustomerEventModelRepository eventStoreRepository)
        {
            _publisher = eventPublisher;
            _eventStoreRepository = eventStoreRepository;
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            List<IEvent> customerEvents;
            customerInMemDictionary.TryGetValue(aggregateId, out customerEvents);
            if (customerEvents != null)
            {
                return customerEvents.Where(x => x.Version > fromVersion);
            }
            return new List<IEvent>();
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Get(aggregateId, fromVersion));
        }

        public void Save(IEvent @event)
        {
            List<IEvent> customerEvents;

            customerInMemDictionary.TryGetValue(@event.Id, out customerEvents);

            if (customerEvents == null)
            {
                customerEvents = new List<IEvent>();
                customerInMemDictionary.Add(@event.Id, customerEvents);
            }

            customerEvents.Add(@event);

            _eventStoreRepository.AddEvent(new CustomerEventEntity 
            {
                Id = Guid.NewGuid(),
                EventId = @event.Id,
                Version = @event.Version,
                TimeStamp = @event.TimeStamp,
                EventType = @event.GetType().Name,
                EventData = @event
            });
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            foreach(var evt in events)
            {
                Save(evt);
                await _publisher.Publish(evt, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
