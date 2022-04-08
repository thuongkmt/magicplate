using System;
using System.Windows;
using System.Windows.Controls;

namespace Konbini.RfidFridge.TagManagement.Views
{
    public partial class MessageBoxView : UserControl
    {
        public MessageBoxView()
        {
            InitializeComponent();
            
        }
        public void HideOKButton()
        {
            OKButton.Visibility = Visibility.Collapsed;
        }
        public void ShowOKButotn()
        {
            OKButton.Visibility = Visibility.Visible;
        }

        public Action OkPressed { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (OkPressed != null) OkPressed();
        }
    }
}
