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

namespace Konbi.RealsenseID.Pro
{
    /// <summary>
    /// Interaction logic for FwUpdateInput.xaml
    /// </summary>
    public partial class FwUpdateInput : Window
    {
        private bool excludeRecognition;

        public FwUpdateInput(string title, string message)
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();

            UserApprovalTitle.Text = title;
            UserApprovalMessage.Text = message;
        }

        public bool ExcludeRecognition()
        {
            return excludeRecognition;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            excludeRecognition = true;
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            excludeRecognition = false;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
