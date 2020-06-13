using CQRSDemo.API.Models;
using CQRSDemo.API.Models.SQLServer;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace CQRSDemo.API.Repositories
{
	public class CustomerRepository
	{
		private readonly AppDataContext _context;

		public CustomerRepository(AppDataContext context)
		{
			_context = context;
		}

		public Customer Create(Customer customer)
		{
			EntityEntry<Customer> entry = _context.Customers.Add(customer);
			_context.SaveChanges();
			return entry.Entity;
		}

		public void Update(Customer customer)
		{
			_context.SaveChanges();
		}

		public void Remove(long id)
		{
			_context.Customers.Remove(GetById(id));
			_context.SaveChanges();
		}

		public IQueryable<Customer> GetAll()
		{
			return _context.Customers;
		}

		public Customer GetById(long id)
		{
			return _context.Customers.Find(id);
		}
	}
}
