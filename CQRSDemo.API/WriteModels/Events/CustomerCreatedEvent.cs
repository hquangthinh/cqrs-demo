using CQRSDemo.API.WriteModels.VOs;
using System;
using System.Collections.Generic;

namespace CQRSDemo.API.WriteModels.Events
{
	public class CustomerCreatedEvent : AbstractEvent
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public List<Phone> Phones { get; set; }

		public CustomerCreatedEvent()
		{
		}

		public CustomerCreatedEvent(Guid id, string email, string name, int age, List<Phone> phones, int version)
		{
			Id = id;
			Email = email;
			Name = name;
			Age = age;
			Phones = phones;
			Version = version;
		}
	}
}
