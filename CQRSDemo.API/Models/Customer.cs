using System.Collections.Generic;

namespace CQRSDemo.API.Models
{
    public class Customer
	{
		public long Id { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public List<Phone> Phones { get; set; }
	}
}
