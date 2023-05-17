using DreamTrip.Functions;
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

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для EnterOldPassword.xaml
    /// </summary>
    public partial class EnterOldPassword : Window
    {
        Brush borderBrush;
        bool isOldPasswordEntered;
        bool isPasswordChanged;

        public EnterOldPassword()
        {
            InitializeComponent();
            borderBrush = borderPassword.BorderBrush;
            isOldPasswordEntered = false;
            isPasswordChanged = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordChanged)
            {
                this.Close();
            }
            else
            {
                if (isOldPasswordEntered)
                {
                    if (pwbPassword.Password == pwbPassword2.Password)
                    {
                        MainFunctions.NewQuery($"UPDATE User_login_data SET password_hash  = '{MainFunctions.GetHash(pwbPassword.Password)}' " +
                            $" WHERE login = '{MainFunctions.СurrentSessionLogin}' ");

                        MainFunctions.AddLogRecord($"Password change success");


                        tbTitle.Text = "Пароль успешно изменен!";
                        this.Height = 250;
                        this.MinHeight = 250;
                        this.MaxHeight = 250;
                        tbTitle.FontSize = 26;
                        pwbPassword.Visibility = Visibility.Hidden;
                        pwbPassword2.Visibility = Visibility.Hidden;
                        borderPassword.Visibility = Visibility.Hidden;
                        borderPassword2.Visibility = Visibility.Hidden;
                        borderCancel.Visibility = Visibility.Hidden;
                        borderOk.Margin = new Thickness(0,0,0,0);
                        borderOk.VerticalAlignment = VerticalAlignment.Bottom;
                        borderOk.SetValue(Grid.RowProperty,2);
                        isPasswordChanged = true;



                    }
                    else
                    {
                        tbWrongPassword2.Visibility = Visibility.Visible;
                        borderPassword2.BorderBrush = Brushes.Red;
                    }
                }
                else
                {
                    string passwordHash = MainFunctions.GetHash(pwbPassword.Password);
                    if (IsOldPasswordCorrect(passwordHash))
                    {
                        isOldPasswordEntered = true;
                        MakePasswordStandart();
                        pwbPassword.Password = "";
                        borderPassword2.Visibility = Visibility.Visible;
                        tbTitle.Text = "Введите новый пароль";
                        this.Height = 350;
                        this.MinHeight = 350;
                        this.MaxHeight = 350;
                        borderCancel.SetValue(Grid.RowProperty, 4);
                        borderOk.SetValue(Grid.RowProperty, 4);

                    }
                    else
                    {
                        tbWrongPassword.Visibility = Visibility.Visible;
                        borderPassword.BorderBrush = Brushes.Red;
                    }
                }
            }
        }

        private bool IsOldPasswordCorrect(string hash)
        {
            bool result = false;
            if (MainFunctions.NewQuery($"SELECT * FROM User_login_data WHERE login = '{MainFunctions.СurrentSessionLogin}' and  password_hash = '{hash}'").Rows.Count > 0)
            {
                result = true;
            }

            return result;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MakePasswordStandart()
        {
            tbWrongPassword.Visibility = Visibility.Hidden;
            tbWrongPassword2.Visibility = Visibility.Hidden;
            borderPassword.BorderBrush = borderBrush;
            borderPassword2.BorderBrush = borderBrush;
        }

        private void pwbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox textBox = sender as PasswordBox;
            char[] charList = textBox.Password.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_RuEngNumSpec(charList[i].ToString()))
                {
                    textBox.Password = textBox.Password.Remove(i, 1);
                }
            }

            borderOk.IsEnabled = pwbPassword.Password.Length > 0
                && MainFunctions.ValidateString_RuEngNumSpec(pwbPassword.Password)
                && ((pwbPassword2.Password.Length > 0 && MainFunctions.ValidateString_RuEngNumSpec(pwbPassword2.Password)) || !isOldPasswordEntered);
            MakePasswordStandart();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && borderOk.IsEnabled)
            {
                btnOk_Click(sender, e);
            }
        }
    }
}
