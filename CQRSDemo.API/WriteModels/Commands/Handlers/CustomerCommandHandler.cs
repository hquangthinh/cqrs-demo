using CQRSDemo.API.WriteModels.Domain.Aggregates;
using CQRSDemo.API.WriteModels.VOs;
using CQRSlite.Commands;
using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.Commands.Handlers
{
    public class CustomerCommandHandler : ICommandHandler<CreateCustomerCommand>,
                                        ICommandHandler<UpdateCustomerCommand>,
                                        ICommandHandler<DeleteCustomerCommand>
    {
        private readonly ISession _session;
        private NLog.Logger logger = NLog.LogManager.GetLogger("CustomerCommandHandlers");
        public CustomerCommandHandler(ISession session)
        {
            _session = session;
        }

        public Task Handle(CreateCustomerCommand command)
        {
            var item = new CustomerAggregate(
                command.Id,
                command.Email,
                command.Name,
                command.Age,
                command.Phones.Select(x => new Phone()
                {
                    Type = x.Type,
                    AreaCode = x.AreaCode,
                    Number = x.Number
                }).ToList(),
                command.ExpectedVersion);
            _session.Add(item);
            return _session.Commit();
        }

        private Task<T> Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot
        {
            try
            {
                return _session.Get<T>(id, expectedVersion);
            }
            catch (Exception e)
            {
                logger.Error("Cannot get object of type {0} with id={1} ({2}) from session", typeof(T), id, expectedVersion);
                throw e;
            }
        }

        public async Task Handle(UpdateCustomerCommand command)
        {
            logger.Info("Handling UpdateCustomerCommand {0} ({1})", command.Id, command.ExpectedVersion);
            CustomerAggregate item = await Get<CustomerAggregate>(command.Id);
            item.Update(
                command.Id,
                command.Name,
                command.Age,
                command.Phones.Select(x => new Phone()
                {
                    Type = x.Type,
                    AreaCode = x.AreaCode,
                    Number = x.Number
                }).ToList(),
                command.ExpectedVersion);
            await _session.Commit();
        }

        public async Task Handle(DeleteCustomerCommand command)
        {
            CustomerAggregate item = await Get<CustomerAggregate>(command.Id);
            item.Delete();
            await _session.Commit();
        }
    }
}
