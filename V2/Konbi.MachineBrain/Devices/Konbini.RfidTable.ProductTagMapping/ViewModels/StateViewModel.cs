using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;
using Konbini.RfidFridge.TagManagement.Service;
using Screen = Caliburn.Micro.Screen;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class StateViewModel : Conductor<object>, IHandle<AppMessage>
    {
        protected IEventAggregator EventAggregator;
        protected ShellViewModel ShellView;
        private MachineState _currentState;
        protected Screen messageBox;
        public IWindowManager WM { get; set; }
        public MachineState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnStateChange(value);
            }
        }

        public StateViewModel(IEventAggregator events, ShellViewModel shellView = null)
        {
            EventAggregator = events;
            EventAggregator.Subscribe(this);

            ShellView = shellView;
            WM = new WindowManager();
            this.messageBox = (Screen)IoC.GetInstance(typeof(MessageBoxViewModel), null);
        }

        public virtual void ShowMessageDialog(string message)
        {
            messageBox.TryClose();
            ((MessageBoxViewModel)messageBox).Message = message;
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.None;
            settings.ShowInTaskbar = false;
            settings.ResizeMode = ResizeMode.NoResize;
            WM.ShowWindow(messageBox, null, settings);

            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(3 * 1000);
                Execute.OnUIThread(() => messageBox.TryClose());
            });
        }

        public void CheckBeforeHandle(Konbini.RfidFridge.TagManagement.Enums.Screen currentScreen)
        {
            if (ShellView.CurrentScreen == currentScreen)
            {
                return;
            }
        }
        public virtual void Handle(AppMessage message)
        {

        }

        public virtual void OnStateChange(MachineState state)
        {

        }

    
    }
}