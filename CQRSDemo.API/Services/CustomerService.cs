using CQRSDemo.API.Commons;
using CQRSDemo.API.Models.Mongo;
using CQRSDemo.API.ReadModels.Repositories;
using CQRSDemo.API.WriteModels.Commands;
using CQRSDemo.API.WriteModels.Commands.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CQRSDemo.API.Services
{
    //[ServiceContract]
    public interface ICustomerService
    {
        //[OperationContract]
        Task<bool> IssueCommandAsync(Command cmd);
        //[OperationContract]
        Task<List<CustomerViewEntity>> GetAllCustomersAsync();
        //[OperationContract]
        Task<CustomerViewEntity> GetCustomerAsync(Guid custId);
        //[OperationContract]
        Task<List<CustomerViewEntity>> GetCustomersByEmailAsync(string email);
    }

    public class CustomerService : ICustomerService
    {
        private readonly CustomerCommandHandler _commandHandlers;
        private readonly CustomerReadModelRepository _readModelRepository;

        public CustomerService(CustomerCommandHandler commandHandlers, CustomerReadModelRepository readModelRepository)
        {
            this._commandHandlers = commandHandlers;
            this._readModelRepository = readModelRepository;
        }

        public async Task<bool> IssueCommandAsync(Command cmd)
        {
            await Task.Run(() =>
            {
                var method = (from meth in typeof(CustomerCommandHandler)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              let @params = meth.GetParameters()
                              where @params.Count() == 1 && @params[0].ParameterType == cmd.GetType()
                              select meth).FirstOrDefault();
                if (method == null)
                {
                    var name = cmd.GetType().Name;
                    throw new ServiceException(string.Format("Command handler of {0} not found", name));
                }
                method.Invoke(_commandHandlers, new[] { cmd });
            });
            return true;
        }

        public async Task<List<CustomerViewEntity>> GetAllCustomersAsync()
        {
            return await _readModelRepository.GetCustomers();
        }

        public async Task<CustomerViewEntity> GetCustomerAsync(Guid orderId)
        {
            return await _readModelRepository.GetCustomer(orderId);
        }

        public async Task<List<CustomerViewEntity>> GetCustomersByEmailAsync(string email)
        {
            return await _readModelRepository.GetCustomerByEmail(email);
        }
    }
}
