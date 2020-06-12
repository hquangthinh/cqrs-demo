using CQRSDemo.API.Events;
using CQRSDemo.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.Commands
{
	public abstract class Command
	{
		public long Id { get; set; }
	}

	public class CreateCustomerCommand : Command
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public int Age { get; set; }
		public List<CreatePhoneCommand> Phones { get; set; }

		public CustomerCreatedEvent ToCustomerEvent(long id)
		{
			return new CustomerCreatedEvent
			{
				Id = id,
				Name = this.Name,
				Email = this.Email,
				Age = this.Age,
				Phones = this.Phones.Select(phone => new PhoneCreatedEvent { AreaCode = phone.AreaCode, Number = phone.Number }).ToList()
			};
		}

		public Customer ToCustomerRecord()
		{
			return new Customer
			{
				Name = this.Name,
				Email = this.Email,
				Age = this.Age,
				Phones = this.Phones.Select(phone => new Phone { AreaCode = phone.AreaCode, Number = phone.Number }).ToList()
			};
		}
	}

	public class CreatePhoneCommand : Command
	{
		public PhoneType Type { get; set; }
		public int AreaCode { get; set; }
		public int Number { get; set; }
	}

	public class UpdateCustomerCommand : Command
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public List<CreatePhoneCommand> Phones { get; set; }

		public CustomerUpdatedEvent ToCustomerEvent()
		{
			return new CustomerUpdatedEvent
			{
				Id = this.Id,
				Name = this.Name,
				Age = this.Age,
				Phones = this.Phones.Select(phone => new PhoneCreatedEvent
				{
					Type = phone.Type,
					AreaCode = phone.AreaCode,
					Number = phone.Number
				}).ToList()
			};
		}

		public Customer ToCustomerRecord(Customer record)
		{
			record.Name = this.Name;
			record.Age = this.Age;
			record.Phones = this.Phones.Select(phone => new Phone
			{
				Type = phone.Type,
				AreaCode = phone.AreaCode,
				Number = phone.Number
			}).ToList();
			return record;
		}
	}

	public class DeleteCustomerCommand : Command
	{
		internal CustomerDeletedEvent ToCustomerEvent()
		{
			return new CustomerDeletedEvent
			{
				Id = this.Id
			};
		}
	}
}
