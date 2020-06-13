using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using CQRSDemo.API.Models.SQLServer;
using CQRSDemo.API.ReadModels.Repositories;
using CQRSDemo.API.Repositories;
using CQRSDemo.API.Services;
using CQRSDemo.API.WriteModels.Commands.Handlers;
using CQRSDemo.API.WriteModels.Domain.Bus;
using CQRSDemo.API.WriteModels.Events.Handlers;
using CQRSDemo.API.WriteModels.EventStore;
using CQRSlite.Caching;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddControllers();
            services.AddOptions();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<CustomerRepository>();
            builder.RegisterType<CustomerReadModelRepository>();
            builder.RegisterType<CustomerEventModelRepository>(); 

            builder.RegisterType<AMQPEventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<AMQPEventSubscriber>().SingleInstance();
            builder.RegisterType<MemoryCache>().As<ICache>().SingleInstance();
            builder.RegisterType<CustomerEventStore>().As<IEventStore>().SingleInstance();

            builder.RegisterType<CustomerCreatedEventHandler>().As<IBusEventHandler>().SingleInstance();
            builder.RegisterType<CustomerUpdatedEventHandler>().As<IBusEventHandler>().SingleInstance();
            builder.RegisterType<CustomerDeletedEventHandler>().As<IBusEventHandler>().SingleInstance();

            builder.RegisterType<CustomerCommandHandler>();
            builder.RegisterType<CustomerService>().As<ICustomerService>();
            builder.RegisterType<Session>().As<ISession>();

            builder
                .Register(c => new CacheRepository(
                    new Repository(c.Resolve<IEventStore>()), 
                    c.Resolve<IEventStore>(), c.Resolve<ICache>()
                ))
                .As<IRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            //{
            //    var context = serviceScope.ServiceProvider.GetRequiredService<AppDataContext>();
            //    context.Database.EnsureCreated();
            //}

            //new Thread(() =>
            //{
            //    messageListener.Start(env.ContentRootPath);
            //}).Start();

        }
    }
}
