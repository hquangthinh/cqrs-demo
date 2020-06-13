using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQRSDemo.API.Models;
using CQRSDemo.API.Models.Mongo;
using CQRSDemo.API.Repositories;
using CQRSDemo.API.Services;
using CQRSDemo.API.WriteModels.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CQRSDemo.API.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public Task<IActionResult> Get([FromQuery] string email)
        {
            return Task.Run(async () =>
            {
                if (email != null)
                {
                    IActionResult result = NotFound();
                    var customer = await _customerService.GetCustomersByEmailAsync(email);
                    if (customer != null)
                    {
                        result = new ObjectResult(customer);
                    }
                    return result;
                }
                else
                {
                    return new ObjectResult(await _customerService.GetAllCustomersAsync());
                }
            });
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public Task<IActionResult> GetById(Guid id)
        {
            return Task.Run(async () =>
            {
                IActionResult result = NotFound();
                var customer = await _customerService.GetCustomerAsync(id);
                if (customer != null)
                {
                    result = new ObjectResult(customer);
                }
                return result;
            });
        }

        [HttpPost]
        public Task<IActionResult> Post([FromBody] CreateCustomerCommand customer)
        {
            customer.Id = Guid.NewGuid();
            return Task.Run(async () =>
            {
                IActionResult result = NotFound();
                bool created = await _customerService.IssueCommandAsync(customer);
                if (created)
                {
                    result = CreatedAtRoute("GetCustomer", new { id = customer.Id }, customer);
                }
                return result;
            });
        }

        [HttpPut("{id}")]
        public Task<IActionResult> Put(Guid id, [FromBody] UpdateCustomerCommand customer)
        {
            return Task.Run(async () =>
            {
                IActionResult result = NotFound();
                var record = await _customerService.GetCustomerAsync(id);
                if (record != null)
                {
                    customer.Id = id;
                    bool updated = await _customerService.IssueCommandAsync(customer);
                    if (updated)
                    {
                        result = NoContent();
                    }
                }
                return result;
            });
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(Guid id)
        {
            return Task.Run(async () =>
            {
                IActionResult result = NotFound();
                var record = await _customerService.GetCustomerAsync(id);
                if (record != null)
                {
                    bool updated = await _customerService.IssueCommandAsync(new DeleteCustomerCommand()
                    {
                        Id = id
                    });
                    if (updated)
                    {
                        result = NoContent();
                    }
                }
                return result;
            });
        }
    }
}
