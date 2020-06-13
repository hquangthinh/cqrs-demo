using CQRSDemo.API.Models.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.ReadModels.Repositories
{
	public class CustomerReadModelRepository
	{
		private const string _customerDB = "CustomerDB";
		private const string _customerCollection = "Customers";
		private IMongoDatabase _db;

		public CustomerReadModelRepository()
		{
			MongoClient _client = new MongoClient("mongodb://mongoadmin:123Qwe@localhost:27017/");
			_db = _client.GetDatabase(_customerDB);
        }

		public Task<List<CustomerViewEntity>> GetCustomers()
		{
			return Task.Run(() =>
			{
				return _db.GetCollection<CustomerViewEntity>(_customerCollection).Find(_ => true).ToList();
			});
		}

		public Task<CustomerViewEntity> GetCustomer(Guid id)
		{
			return Task.Run(() =>
			{
				return _db.GetCollection<CustomerViewEntity>(_customerCollection).Find(customer => customer.Id.Equals(id)).SingleOrDefault();
			});
		}

		public Task<List<CustomerViewEntity>> GetCustomerByEmail(string email)
		{
			return Task.Run(() =>
			{
				return _db.GetCollection<CustomerViewEntity>(_customerCollection).Find(customer => customer.Email == email).ToList();
			});
		}

		public Task<bool> Create(CustomerViewEntity customer)
		{
			return Task.Run(() =>
			{
				_db.GetCollection<CustomerViewEntity>(_customerCollection).InsertOne(customer);
				return true;
			});
		}

		public Task<bool> Update(CustomerViewEntity customer)
		{
			return Task.Run(() =>
			{
				var filter = Builders<CustomerViewEntity>.Filter.Where(_ => _.Id == customer.Id);
				_db.GetCollection<CustomerViewEntity>(_customerCollection).ReplaceOne(filter, customer);
				return true;
			});
		}

		public Task<bool> Remove(Guid id)
		{
			return Task.Run(() =>
			{
				var filter = Builders<CustomerViewEntity>.Filter.Where(_ => _.Id.Equals(id));
				var operation = _db.GetCollection<CustomerViewEntity>(_customerCollection).DeleteOne(filter);
				return true;
			});
		}
	}
}
