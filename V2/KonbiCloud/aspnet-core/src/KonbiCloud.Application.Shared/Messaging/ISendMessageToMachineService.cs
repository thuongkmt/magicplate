using Abp.Dependency;
using Konbini.Messages;
using Konbini.Messages.Enums;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging
{
    public interface ISendMessageToMachineService   :ISingletonDependency
    {
        bool SendNotification(KeyValueMessage message);
        Task<bool> SendNotificationAsync(KeyValueMessage message);
    }
}
