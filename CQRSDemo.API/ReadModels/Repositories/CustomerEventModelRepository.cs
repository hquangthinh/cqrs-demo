using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.ReadModels.Repositories
{
    public class CustomerEventModelRepository
    {
		private const string _customerDB = "CustomerDB";
		private const string _customerEventCollection = "CustomerEvents";
		private IMongoDatabase _db;

		public CustomerEventModelRepository()
		{
			MongoClient _client = new MongoClient("mongodb://mongoadmin:123Qwe@localhost:27017/");
			_db = _client.GetDatabase(_customerDB);
		}

		public Task<bool> AddEvent(CustomerEventEntity customerEvent)
		{
			return Task.Run(() =>
			{
				_db.GetCollection<CustomerEventEntity>(_customerEventCollection).InsertOne(customerEvent);
				return true;
			});
		}
	}
}
