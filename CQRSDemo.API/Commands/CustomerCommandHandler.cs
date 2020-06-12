using CQRSDemo.API.Events;
using CQRSDemo.API.Models;
using CQRSDemo.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.Commands
{
    public interface ICommandHandler<T> where T : Command
    {
        void Execute(T command);
    }

	public class CustomerCommandHandler : ICommandHandler<Command>
	{
		private CustomerRepository _repository;
		private AMQPEventPublisher _eventPublisher;
		public CustomerCommandHandler(AMQPEventPublisher eventPublisher, CustomerRepository repository)
		{
			_eventPublisher = eventPublisher;
			_repository = repository;
		}

		public void Execute(Command command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command is null");
			}

			if (command is CreateCustomerCommand createCommand)
			{
				Customer created = _repository.Create(createCommand.ToCustomerRecord());
				_eventPublisher.PublishEvent(createCommand.ToCustomerEvent(created.Id));
			}
			else if (command is UpdateCustomerCommand updateCommand)
			{
				Customer record = _repository.GetById(updateCommand.Id);
				_repository.Update(updateCommand.ToCustomerRecord(record));
				_eventPublisher.PublishEvent(updateCommand.ToCustomerEvent());
			}
			else if (command is DeleteCustomerCommand deleteCommand)
			{
				_repository.Remove(deleteCommand.Id);
				_eventPublisher.PublishEvent(deleteCommand.ToCustomerEvent());
			}
		}
	}
}
