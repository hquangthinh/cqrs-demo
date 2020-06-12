using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.Models.SQLServer
{
	public class AppDataContext : DbContext
	{
		public AppDataContext(DbContextOptions<AppDataContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Customer>()
						.HasMany(x => x.Phones);
		}

		public DbSet<Customer> Customers { get; set; }
	}
}
