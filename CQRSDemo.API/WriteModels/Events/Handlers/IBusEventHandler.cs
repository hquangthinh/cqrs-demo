using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.Events.Handlers
{
    public interface IBusEventHandler
    {
        Type HandlerType { get; }
        Task Handle(IEvent @event);
    }
}
