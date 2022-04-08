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
    /// Interaction logic for EnrollInput.xaml
    /// </summary>
    public partial class EnrollInput : Window
    {
        public EnrollInput()
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();

            UserNameInput.Text = "New User";
            UserNameInput.Focus();
            UserNameInput.SelectAll();
        }

        public string Username
        {
            get { return UserNameInput.Text; }
            set { UserNameInput.Text = value; }
        }

        private static bool IsValidUserId(string user)
        {
            return !string.IsNullOrEmpty(user) && user.All((ch) => ch <= 127) && user.Length <= 30;
        }

        private void UserNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserInputErrorLabel == null || EnrollOKButton == null)
                return;

            if (IsValidUserId(UserNameInput.Text) || UserNameInput.Text.Length == 0)
            {
                // show error label and disable ok button on invalid input
                UserInputErrorLabel.Visibility = Visibility.Hidden;
                EnrollOKButton.IsEnabled = UserNameInput.Text.Any();
            }
            else
            {
                // hide error label and enable ok button on valid input
                UserInputErrorLabel.Visibility = Visibility.Visible;
                EnrollOKButton.IsEnabled = false;
            }

        }

        private void EnrollCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void EnrollOKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
