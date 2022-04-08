using Microsoft.Extensions.Configuration;
using rsid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Konbini.RealsenseID
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RealsenseID realsenseID;
        public MainWindow()
        {
            InitializeComponent();
            string comport = GetConfigInfor();
            realsenseID = new RealsenseID(comport);
            var faceprints = realsenseID.GetUserFaceprintFromDevice();
            LoadFaceprintInLocalDevice(faceprints);
        }
        public void LoadFaceprintInLocalDevice(List<(Faceprints, string)> faceprints)
        {
            List<User> user = new List<User>();
            foreach (var (faceprintsDb, userIdDb) in faceprints)
            {

                user.Add(new User(userIdDb));
            }
            lvDataBinding.ItemsSource = user;
        }

        private string GetConfigInfor()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSetting.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string comport = configuration["Device:Comport"];

            WriteToFile(comport);

            return comport;
        }

        private void Enroll_Click(object sender, RoutedEventArgs e)
        {

        }

        /**
         * 
         * System processing:Write log into file
         * 
         * **/
        public void WriteToFile(string Message)
        {
            string path = @"C:\Logs";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = @"C:\Logs\RealsenseID_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            DateTime saveNow = DateTime.Now;
            DateTime localTime;
            localTime = saveNow.ToLocalTime();

            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }

            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }
        }
    }
    public class User
    {
        public string Name;
        public User(string Name)
        {
            this.Name = Name;
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
