using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Interface;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class MessageBoxViewModel : Conductor<object>, IShell, IHandle<object>
    {
        public MessageBoxViewModel()
        {
            DisplayName = "Konbini";
        }
        public IEventAggregator EventAggregator { get; set; }

        public void Handle(object message)
        {
            
        }

        public string Message { get; set; }

        public void OKButton()
        {
            this.TryClose();
        }
    }
}
