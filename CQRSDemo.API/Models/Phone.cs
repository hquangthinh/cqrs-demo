namespace CQRSDemo.API.Models
{
    public class Phone
	{
		public long Id { get; set; }
		public PhoneType Type { get; set; }
		public int AreaCode { get; set; }
		public int Number { get; set; }
	}
}
