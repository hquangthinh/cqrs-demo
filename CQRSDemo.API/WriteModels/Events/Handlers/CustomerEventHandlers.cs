using CQRSDemo.API.Models.Mongo;
using CQRSDemo.API.ReadModels.Repositories;
using CQRSDemo.API.WriteModels.VOs;
using CQRSlite.Events;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.Events.Handlers
{
    public class CustomerCreatedEventHandler : IBusEventHandler
    {
        private readonly CustomerReadModelRepository readModelRepository;
        private Logger logger = LogManager.GetLogger("CustomerCreatedEventHandler");

        public CustomerCreatedEventHandler(CustomerReadModelRepository readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        public Type HandlerType
        {
            get { return typeof(CustomerCreatedEvent); }
        }

        public async Task Handle(IEvent @event)
        {
            CustomerCreatedEvent customerCreatedEvent = (CustomerCreatedEvent)@event;

            await readModelRepository.Create(new CustomerViewEntity()
            {
                Id = customerCreatedEvent.Id.ToString(),
                Email = customerCreatedEvent.Email,
                Name = customerCreatedEvent.Name,
                Age = customerCreatedEvent.Age,
                Phones = customerCreatedEvent.Phones.Select(x =>
                    new PhoneViewEntity()
                    {
                        Type = x.Type,
                        AreaCode = x.AreaCode,
                        Number = x.Number
                    }).ToList()
            });
            logger.Info("A new CustomerCreatedEvent has been processed: {0} ({1})", customerCreatedEvent.Id, customerCreatedEvent.Version);
        }
    }

    public class CustomerUpdatedEventHandler : IBusEventHandler
    {
        private readonly CustomerReadModelRepository readModelRepository;

        private Logger logger = LogManager.GetLogger("CustomerUpdatedEventHandler");
        public CustomerUpdatedEventHandler(CustomerReadModelRepository readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        public Type HandlerType
        {
            get { return typeof(CustomerUpdatedEvent); }
        }

        public async Task Handle(IEvent @event)
        {
            CustomerUpdatedEvent customerUpdatedEvent = (CustomerUpdatedEvent)@event;

            CustomerViewEntity customer = await readModelRepository.GetCustomer(@event.Id);
            await readModelRepository.Update(new CustomerViewEntity()
            {
                Id = customerUpdatedEvent.Id.ToString(),
                Email = customer.Email,
                Name = customerUpdatedEvent.Name != null ? customerUpdatedEvent.Name : customer.Name,
                Age = customerUpdatedEvent.Age != 0 ? customerUpdatedEvent.Age : customer.Age,
                Phones = customerUpdatedEvent.Phones != null ? customerUpdatedEvent.Phones.Select(x =>
                    new PhoneViewEntity()
                    {
                        Type = x.Type,
                        AreaCode = x.AreaCode,
                        Number = x.Number
                    }).ToList() : customer.Phones
            });
            logger.Info("A new CustomerUpdatedEvent has been processed: {0} ({1})", customerUpdatedEvent.Id, customerUpdatedEvent.Version);
        }
    }

    public class CustomerDeletedEventHandler : IBusEventHandler
    {
        private readonly CustomerReadModelRepository readModelRepository;
        private Logger logger = LogManager.GetLogger("CustomerDeletedEventHandler");

        public CustomerDeletedEventHandler(CustomerReadModelRepository readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        public Type HandlerType
        {
            get { return typeof(CustomerDeletedEvent); }
        }

        public async Task Handle(IEvent @event)
        {
            CustomerDeletedEvent customerDeletedEvent = (CustomerDeletedEvent)@event;

            await readModelRepository.Remove(customerDeletedEvent.Id);
            logger.Info("A new CustomerDeletedEvent has been processed: {0} ({1})", customerDeletedEvent.Id, customerDeletedEvent.Version);
        }
    }
}
