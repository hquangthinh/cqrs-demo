﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Domain;
using CQRSCode.WriteModel;
using CQRSlite.Caching;
using CQRSCode.WriteModel.Handlers;
using System.Reflection;
using System.Linq;
using CQRSlite.Messages;
using CQRSlite.Queries;
using CQRSlite.Routing;
using Microsoft.AspNetCore.Http;
using ISession = CQRSlite.Domain.ISession;

namespace CQRSWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            //Add Cqrs services
            services.AddSingleton<Router>(new Router());
            services.AddSingleton<ICommandSender>(y => y.GetService<Router>());
            services.AddSingleton<IEventPublisher>(y => y.GetService<Router>());
            services.AddSingleton<IHandlerRegistrar>(y => y.GetService<Router>());
            services.AddSingleton<IQueryProcessor>(y => y.GetService<Router>());
            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<ISession, Session>();

            //Scan for commandhandlers and eventhandlers
            services.Scan(scan => scan
                .FromAssemblies(typeof(InventoryCommandHandlers).GetTypeInfo().Assembly)
                    .AddClasses(classes => classes.Where(x => {
                        var allInterfaces = x.GetInterfaces();
                        return
                            allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IHandler<>)) ||
                            allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICancellableHandler<>)) ||
                            allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IQueryHandler<,>)) ||
                            allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICancellableQueryHandler<,>));
                    }))
                    .AsSelf()
                    .WithTransientLifetime()
            );
            services.AddControllersWithViews();
            services.AddHttpContextAccessor(); // No longer registered by default in ASP.NET Core 2.1
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
