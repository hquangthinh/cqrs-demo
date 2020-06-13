using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.Models.Mongo
{
	public class CustomerViewEntity
	{
		[BsonElement("Id")]
		public string Id { get; set; }

		[BsonElement("Email")]
		public string Email { get; set; }

		[BsonElement("Name")]
		public string Name { get; set; }

		[BsonElement("Age")]
		public int Age { get; set; }

		[BsonElement("Phones")]
		public List<PhoneViewEntity> Phones { get; set; }
	}
}
