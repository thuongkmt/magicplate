using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.ViewModels;

namespace Konbini.RfidFridge.TagManagement.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = true;
            Title = "Tag Management v1.0";
        }


        public void ActiveMainWindow()
        {
            Activate();
            Focus();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Caliburn.Micro.Action.Invoke(DataContext, "OnKeyPress", null, null, null, new [] { sender, e });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Caliburn.Micro.Action.Invoke(DataContext, "FormClosing");
        }
    }
}
