using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using Konbini.RfidFridge.TagManagement.Views;
using Screen = Konbini.RfidFridge.TagManagement.Enums.Screen;
using Konbini.RfidFridge.TagManagement.Service;
using Konbini.RfidFridge.TagManagement.Data;
using System.Linq;
using System.Configuration;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public sealed class ShellViewModel : StateViewModel
    {
        #region Properties

        public Screen CurrentScreen = Screen.None;
        public Dictionary<Enums.Screen, StateViewModel> AllViews = new Dictionary<Enums.Screen, StateViewModel>();
        public bool ToggleMenu { get; set; }
        public List<MenuItemModel> DisplayedMenuItemCollection { get; set; }
        private MenuItemModel _selectedMenuItem;
        public MenuItemModel SelectedMenuItem
        {
            get => _selectedMenuItem;

            set
            {
                if (value == null || value.Equals(_selectedMenuItem)) return;
                _selectedMenuItem = value;
                this.ActivateItem(_selectedMenuItem.ScreenName);
                NotifyOfPropertyChange(() => SelectedMenuItem);
            }
        }

        public bool CotfRunning { get; set; }
        public bool TARunning { get; set; }

        #endregion Properties

        public ShellViewModel() : base(new EventAggregator())
        {
            RegisterViewModel();
            InitMenu();
            InitDefaultData();

            try
            {
                Process[] cotfs = Process.GetProcessesByName("KonbiBrain.WindowServices.CotfPad");
                foreach (Process worker in cotfs)
                {
                    CotfRunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
                Process[] tas = Process.GetProcessesByName("KonbiBrain.RfidTable.TakeAwayPrice");
                foreach (Process worker in tas)
                {
                    TARunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        public void InitDefaultData()
        {
            using (var context = new KDbContext())
            {
                var config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.CloudUrl);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key =  Enums.SettingKey.CloudUrl, Value = "http://localhost:22743/" });
                    context.SaveChanges();
                }
                config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.UserName);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key = Enums.SettingKey.UserName, Value = "admin" });
                    context.SaveChanges();
                }
                config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.Password);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key = Enums.SettingKey.Password, Value = "!23Qwe" });
                    context.SaveChanges();
                }
            }
        }


        public void InitMenu()
        {
            DisplayedMenuItemCollection = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    DisplayName = "Tag Management",
                    ScreenName = Screen.Main,
                    IsDisplay =  true
                },
                new MenuItemModel
                {
                    DisplayName = "Setting",
                    ScreenName = Screen.Setting,
                    IsDisplay =  true
                }
            };
        }

        public void RegisterViewModel()
        {
            AllViews.Add(Screen.Main, new MainViewModel(this.EventAggregator, this));
            AllViews.Add(Screen.Setting, new SettingViewModel(this.EventAggregator));
        }

        protected override void OnActivate()
        {
            SeriLogService.CreateLoggers();

            base.OnActivate();
            ActivateItem(Screen.Main);
        }

        public override void ActivateItem(object item)
        {
            if (item == null) return;
            var nextScreen = ((Screen?)item).Value;
            if ((nextScreen == Screen.None) || (nextScreen == CurrentScreen)) return;
            IoC.BuildUp(AllViews[nextScreen]);
            base.ActivateItem(AllViews[nextScreen]);
            CurrentScreen = nextScreen;
            Debug.WriteLine(CurrentScreen);
        }

        public void OnKeyPress(object sender, KeyEventArgs e)
        {
            EventAggregator.PublishOnUIThread(new KeyPressedMessage { Key = e.Key });
        }

        public override void Handle(AppMessage message)
        {
            switch (message)
            {
                case StateChangeMessage state:
                    CurrentState = state.State;
                    break;

                case ActiveScreenMessage screen:
                    ActivateItem(screen.Screen);
                    break;

                case KeyPressedMessage key:
                    Debug.WriteLine($"Key pressed: {key.Key}");
                    OnKeyPressed(key.Key);
                    break;
            }
        }

        private void OpenCustomerScreen()
        {
            try
            {
                //var window = IoC.Get<IWindowManager>();
                //var vm = new CustomerViewModel(EventAggregator);
                //window.ShowWindow(vm);
            }
            catch (System.InvalidOperationException)
            {
                // Skip this, to force close customer screen
            }
        }

        public void OnKeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Decimal:
                    ToggleMenu = !ToggleMenu;
                    NotifyOfPropertyChange(() => ToggleMenu);
                    break;

                case Key.NumPad0:
                case Key.D0:
                    ActivateItem(Screen.Main);
                    break;

                case Key.NumPad1:
                case Key.D1:
                    if (ToggleMenu)
                        OpenCustomerScreen();
                    break;

                //case Key.NumPad2:
                //case Key.D2:
                //    if (ToggleMenu)
                //        ActivateItem(Screen.Float);
                //    break;

                //case Key.NumPad3:
                //case Key.D3:
                //    if (ToggleMenu)
                //        ActivateItem(Screen.SalesLog);
                //    break;

                case Key.NumPad4:
                case Key.D4:
                    if (ToggleMenu)
                        ActivateItem(Screen.Setting);
                    break;

            }
        }

        public void FormClosing()
        {
            EventAggregator.PublishOnUIThread(new ClosingFormMessage());
            try
            {
                if (CotfRunning)
                {
                    var path = ConfigurationManager.AppSettings["COTFPath"].ToString();
                    Process.Start(path);
                }
                if (TARunning)
                {
                    var path = ConfigurationManager.AppSettings["TakeAwayPath"].ToString();
                    Process.Start(path);
                }
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}