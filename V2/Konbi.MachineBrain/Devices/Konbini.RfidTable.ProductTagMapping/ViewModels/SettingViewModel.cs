using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Data;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class SettingViewModel : StateViewModel
    {
        private string cloudUrl;
        private string userName;
        private string password;
        public IMbCloudService MbCloudService { get; set; }
        public string CloudUrl
        {
            get => cloudUrl;
            set
            {
                cloudUrl = value;
                SaveConfig(SettingKey.CloudUrl, value);
                MbCloudService.BASE_URL = value;
                MbCloudService.Token = null;
            }
        }
        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
                SaveConfig(SettingKey.UserName, value);
                MbCloudService.USER_NAME = value;
                MbCloudService.Token = null;
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                SaveConfig(SettingKey.Password, value);
                MbCloudService.PASSWORD = value;
                MbCloudService.Token = null;
            }
        }
        public SettingViewModel(IEventAggregator events) : base(events)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadConfig();
        }

        private void LoadConfig()
        {
            using (var context = new KDbContext())
            {
                cloudUrl = context.Settings.FirstOrDefault(x => x.Key == SettingKey.CloudUrl)?.Value;
                password = context.Settings.FirstOrDefault(x => x.Key == SettingKey.Password)?.Value;
                userName = context.Settings.FirstOrDefault(x => x.Key == SettingKey.UserName)?.Value;
            }
        }

        private void SaveConfig(SettingKey key, string value)
        {
            using (var context = new KDbContext())
            {
                var config = context.Settings.FirstOrDefault(x => x.Key == key);
                if(config != null)
                {
                    config.Value = value;
                    context.SaveChanges();
                }
            }
        }
    }
}
