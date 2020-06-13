using CQRSDemo.API.Models;
using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CQRSDemo.API.WriteModels.Commands
{
	[DataContract]
	[KnownType(typeof(CreateCustomerCommand))]
	[KnownType(typeof(UpdateCustomerCommand))]
	[KnownType(typeof(DeleteCustomerCommand))]
	public abstract class Command : ICommand
	{
		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public int ExpectedVersion { get; set; }
	}

	[DataContract]
	public class CreateCustomerCommand : Command
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Email { get; set; }
		[DataMember]
		public int Age { get; set; }
		[DataMember]
		public List<Phone> Phones { get; set; }
	}

	[DataContract]
	public class UpdateCustomerCommand : Command
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public int Age { get; set; }
		[DataMember]
		public List<Phone> Phones { get; set; }
	}

	[DataContract]
	public class DeleteCustomerCommand : Command
	{

	}
}
