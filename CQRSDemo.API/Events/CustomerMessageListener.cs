using CQRSDemo.API.Repositories.Mongo;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSDemo.API.Events
{
	public class CustomerMessageListener
	{
		private readonly CustomerMongoRepository _repository;

		public CustomerMessageListener(CustomerMongoRepository repository)
		{
			_repository = repository;
		}

		public void Start(string contentRootPath)
		{
			ConnectionFactory connectionFactory = new ConnectionFactory();

			var builder = new ConfigurationBuilder()
				.SetBasePath(contentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddEnvironmentVariables();

			builder.Build().GetSection("amqp").Bind(connectionFactory);
			connectionFactory.AutomaticRecoveryEnabled = true;
			connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(15);

			using (IConnection conn = connectionFactory.CreateConnection())
			{
				using (IModel channel = conn.CreateModel())
				{
					DeclareQueues(channel);

					var customerCreatedEventConsumer = new EventingBasicConsumer(channel);
					var customerUpdatedEventConsumer = new EventingBasicConsumer(channel);
					var customerDeletedEventConsumer = new EventingBasicConsumer(channel);

					customerCreatedEventConsumer.Received += CustomerCreatedEventConsumer_Received;
					customerUpdatedEventConsumer.Received += CustomerUpdatedEventConsumer_Received;
					customerDeletedEventConsumer.Received += CustomerDeletedEventConsumer_Received;

                    //var subscriptionCreated = new Subscription(channel, Constants.QUEUE_CUSTOMER_CREATED, false);
                    //var subscriptionUpdated = new Subscription(channel, Constants.QUEUE_CUSTOMER_UPDATED, false);
                    //var subscriptionDeleted = new Subscription(channel, Constants.QUEUE_CUSTOMER_DELETED, false);

                    while (true)
                    {
                        // Sleeps for 5 sec before trying again
                        Thread.Sleep(5000);
                        new Thread(() =>
                        {
							//ListenCreated(subscriptionCreated);
							channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_CREATED,
								 autoAck: false,
								 consumer: customerCreatedEventConsumer
							);
						}).Start();
                        new Thread(() =>
                        {
							//ListenUpdated(subscriptionUpdated);
							channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_UPDATED,
								 autoAck: false,
								 consumer: customerUpdatedEventConsumer
							);
						}).Start();
                        new Thread(() =>
                        {
							//ListenDeleted(subscriptionDeleted);
							channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_DELETED,
								 autoAck: false,
								 consumer: customerDeletedEventConsumer
							);
						}).Start();
                    }
                }
			}
		}

        private void CustomerCreatedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsCreated)
        {
            if (eventArgsCreated != null)
            {
                string messageContent = Encoding.UTF8.GetString(eventArgsCreated.Body.ToArray());
                CustomerCreatedEvent _created = JsonConvert.DeserializeObject<CustomerCreatedEvent>(messageContent);
                _repository.Create(_created.ToCustomerEntity());
                ((EventingBasicConsumer)sender).Model.BasicAck(eventArgsCreated.DeliveryTag, false);
                //subscriptionCreated.Ack(eventArgsCreated);
            }
        }

        private void CustomerUpdatedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsUpdated)
		{
			if (eventArgsUpdated != null)
            {
                var messageContent = Encoding.UTF8.GetString(eventArgsUpdated.Body.ToArray());
                CustomerUpdatedEvent _updated = JsonConvert.DeserializeObject<CustomerUpdatedEvent>(messageContent);
                _repository.Update(_updated.ToCustomerEntity(_repository.GetCustomer(_updated.Id)));
				((EventingBasicConsumer)sender).Model.BasicAck(eventArgsUpdated.DeliveryTag, false);
				//subscriptionUpdated.Ack(eventArgsUpdated);
			}
        }

		private void CustomerDeletedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsDeleted)
		{
            if (eventArgsDeleted != null)
            {
                var messageContent = Encoding.UTF8.GetString(eventArgsDeleted.Body.ToArray());
                CustomerDeletedEvent _deleted = JsonConvert.DeserializeObject<CustomerDeletedEvent>(messageContent);
                _repository.Remove(_deleted.Id);
				((EventingBasicConsumer)sender).Model.BasicAck(eventArgsDeleted.DeliveryTag, false);
				//subscriptionDeleted.Ack(eventArgsDeleted);
			}
        }

		//private void ListenDeleted(Subscription subscriptionDeleted)
		//{
		//	BasicDeliverEventArgs eventArgsDeleted = subscriptionDeleted.Next();
		//	if (eventArgsDeleted != null)
		//	{
		//		string messageContent = Encoding.UTF8.GetString(eventArgsDeleted.Body);
		//		CustomerDeletedEvent _deleted = JsonConvert.DeserializeObject<CustomerDeletedEvent>(messageContent);
		//		_repository.Remove(_deleted.Id);
		//		subscriptionDeleted.Ack(eventArgsDeleted);
		//	}
		//}

		//private void ListenUpdated(Subscription subscriptionUpdated)
		//{
		//	BasicDeliverEventArgs eventArgsUpdated = subscriptionUpdated.Next();
		//	if (eventArgsUpdated != null)
		//	{
		//		string messageContent = Encoding.UTF8.GetString(eventArgsUpdated.Body);
		//		CustomerUpdatedEvent _updated = JsonConvert.DeserializeObject<CustomerUpdatedEvent>(messageContent);
		//		_repository.Update(_updated.ToCustomerEntity(_repository.GetCustomer(_updated.Id)));
		//		subscriptionUpdated.Ack(eventArgsUpdated);
		//	}
		//}

		//private void ListenCreated(Subscription subscriptionCreated)
		//{
		//	BasicDeliverEventArgs eventArgsCreated = subscriptionCreated.Next();
		//	if (eventArgsCreated != null)
		//	{
		//		string messageContent = Encoding.UTF8.GetString(eventArgsCreated.Body);
		//		CustomerCreatedEvent _created = JsonConvert.DeserializeObject<CustomerCreatedEvent>(messageContent);
		//		_repository.Create(_created.ToCustomerEntity());
		//		subscriptionCreated.Ack(eventArgsCreated);
		//	}
		//}

		private static void DeclareQueues(IModel channel)
		{
			channel.QueueDeclare(
				queue: Constants.QUEUE_CUSTOMER_CREATED,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);
			channel.QueueDeclare(
				queue: Constants.QUEUE_CUSTOMER_UPDATED,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);
			channel.QueueDeclare(
				queue: Constants.QUEUE_CUSTOMER_DELETED,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);
		}
	}
}
