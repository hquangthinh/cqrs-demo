using CQRSDemo.API.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.VOs
{
	[DataContract]
	public class Phone
	{
		[DataMember]
		public PhoneType Type { get; set; }
		[DataMember]
		public int AreaCode { get; set; }
		[DataMember]
		public int Number { get; set; }
	}
}
