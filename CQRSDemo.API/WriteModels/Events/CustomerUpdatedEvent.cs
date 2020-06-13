using CQRSDemo.API.WriteModels.VOs;
using System;
using System.Collections.Generic;

namespace CQRSDemo.API.WriteModels.Events
{
	public class CustomerUpdatedEvent : AbstractEvent
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public List<Phone> Phones { get; set; }

		public CustomerUpdatedEvent()
		{
		}

		public CustomerUpdatedEvent(Guid id, string name, int age, List<Phone> phones, int version)
		{
			Id = id;
			Name = name;
			Age = age;
			Phones = phones;
			Version = version;
		}
	}
}
