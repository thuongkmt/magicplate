﻿using System;
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
    /// Interaction logic for DeleteUserInput.xaml
    /// </summary>
    public partial class DeleteUserInput : Window
    {
        public DeleteUserInput()
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        public IEnumerable<string> UserIds { get; private set; }

        public string SelectedUser { get; set; }

        public bool DeleteAll { get; set; } = false;

        private void DeleteUserOKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void DeleteUserCancelButton_Click(object sender, RoutedEventArgs e)
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
