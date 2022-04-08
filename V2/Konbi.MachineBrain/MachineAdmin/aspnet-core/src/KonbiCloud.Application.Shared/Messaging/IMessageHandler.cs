using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Konbini.Messages;

namespace KonbiCloud.Messaging
{
    public interface IMessageHandler 
    {
        Task<Boolean> Handle(KeyValueMessage keyValueMessage);
    }

    public interface IProductMessageHandler : IMessageHandler
    {

    }

    public interface ITransactionMessageHandler : IMessageHandler
    {

    }

    public interface IInventoryMessageHandler : IMessageHandler
    {

    }

    public interface ITopupMessageHandler : IMessageHandler
    {

    }

    public interface IUpdateInventoryListMessageHandler : IMessageHandler
    {

    }
}
