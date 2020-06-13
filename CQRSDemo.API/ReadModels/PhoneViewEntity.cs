using CQRSDemo.API.Commons;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.Models.Mongo
{
	public partial class PhoneViewEntity
	{
		[BsonElement("Type")]
		public PhoneType Type { get; set; }
		
		[BsonElement("AreaCode")]
		public int AreaCode { get; set; }

		[BsonElement("Number")]
		public int Number { get; set; }
	}
}
