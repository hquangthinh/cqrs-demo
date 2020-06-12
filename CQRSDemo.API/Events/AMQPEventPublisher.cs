using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSDemo.API.Events
{
	public class Constants
	{
		public const string QUEUE_CUSTOMER_CREATED = "customer_created";
		public const string QUEUE_CUSTOMER_UPDATED = "customer_updated";
		public const string QUEUE_CUSTOMER_DELETED = "customer_deleted";
	}

	public interface IEvent
	{
	}

	public class AMQPEventPublisher
	{
		private readonly ConnectionFactory connectionFactory;

		public AMQPEventPublisher(IWebHostEnvironment env)
		{
			connectionFactory = new ConnectionFactory();
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddEnvironmentVariables();

			builder.Build().GetSection("amqp").Bind(connectionFactory);
		}

		public void PublishEvent<T>(T @event) where T : IEvent
		{
			using (IConnection conn = connectionFactory.CreateConnection())
			{
				using (IModel channel = conn.CreateModel())
				{
					var queue = @event is CustomerCreatedEvent ?
						Constants.QUEUE_CUSTOMER_CREATED : @event is CustomerUpdatedEvent ?
							Constants.QUEUE_CUSTOMER_UPDATED : Constants.QUEUE_CUSTOMER_DELETED;
					channel.QueueDeclare(
						queue: queue,
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null
					);
					var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
					channel.BasicPublish(
						exchange: "",
						routingKey: queue,
						basicProperties: null,
						body: body
					);
				}
			}
		}
	}
}
