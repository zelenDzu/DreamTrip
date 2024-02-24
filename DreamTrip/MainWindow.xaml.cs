using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using DreamTrip.Functions;
using DreamTrip.Windows;
using System.Security.Cryptography;

namespace DreamTrip
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor
        /// <summary>
        /// Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            EnableLoginButton();
            MainFunctions.LoadConnectionString();
        }
        #endregion


        #region Functions
        

        /// <summary>
        /// Активирует кнопку Войти
        /// </summary>
        private void EnableLoginButton()
        {
            try
            {
                if (tbxLogin.Text.Length >= 1 && pwbPassword.Password.Length >= 1
                    && MainFunctions.ValidateString_EngNum(tbxLogin.Text)
                    && MainFunctions.ValidateString_RuEngNumSpec(pwbPassword.Password))
                {
                    borderLoginButton.IsEnabled = true;
                }
                else
                {
                    borderLoginButton.IsEnabled = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// Приводит стиль поля ввода логина в стандартное состояние
        /// </summary>
        private void LoginMakeStandard()
        {
            borderLogin.BorderBrush = borderShowPassword.BorderBrush;
            tbWrongLogin.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Приводит стиль поля ввода пароля в стандартное состояние
        /// </summary>
        private void PasswordMakeStandard()
        {
            borderPWBPassword.BorderBrush = borderShowPassword.BorderBrush;
            borderTBXPassword.BorderBrush = borderShowPassword.BorderBrush;
            tbWrongPassword.Visibility = Visibility.Hidden;
        }

        private void PasswordMakeWrong()
        {
            borderPWBPassword.BorderBrush = tbWrongPassword.Foreground;
            borderTBXPassword.BorderBrush = tbWrongPassword.Foreground;
            tbWrongPassword.Visibility = Visibility.Visible;
        }

        private void LoginMakeWrong()
        {
            borderLogin.BorderBrush = tbWrongPassword.Foreground;
            borderLogin.BorderBrush = tbWrongPassword.Foreground;
            tbWrongLogin.Visibility = Visibility.Visible;
        }
        #endregion

        #region PasswordBox
        private void btnShowPassword_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbxPassword.Text = pwbPassword.Password;
            borderTBXPassword.Visibility = Visibility.Visible;
            borderPWBPassword.Visibility = Visibility.Hidden;
        }

        private void btnShowPassword_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pwbPassword.Password = tbxPassword.Text;
            borderPWBPassword.Visibility = Visibility.Visible;
            borderTBXPassword.Visibility = Visibility.Hidden;
        }

        private void pwbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
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
            }
            catch { }

            EnableLoginButton();
        }

        private void pwbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordMakeStandard();
        }
        #endregion

        #region TextBoxLogin
        private void tbxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                switch (tbxLogin.Text)
                {
                    case "m":
                        tbxLogin.Text = "manager";
                        pwbPassword.Password = "manager1234";
                        break;

                    case "ad":
                        tbxLogin.Text = "admin";
                        pwbPassword.Password = "admin1234";
                        break;

                    case "an":
                        tbxLogin.Text = "analyst";
                        pwbPassword.Password = "analyst1234";
                        break;
                }

                TextBox textBox = sender as TextBox;
                char[] charList = textBox.Text.ToCharArray();
                for (int i = 0; i < charList.Length; i++)
                {
                    if (!MainFunctions.ValidateString_EngNum(charList[i].ToString()))
                    {
                        textBox.Text = textBox.Text.Remove(i, 1);
                    }
                }
            }
            catch { }

            EnableLoginButton();
        }

        private void tbxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginMakeStandard();
        }
        #endregion

        #region ButtonsClick
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginMakeStandard();
            PasswordMakeStandard();

            try
            {
                string login = tbxLogin.Text;
                string passwordHash = MainFunctions.GetHash(pwbPassword.Password);

                if (!MainFunctions.Authorize_LoginExists(login))
                {
                    LoginMakeWrong();
                }
                else
                {
                    if (!MainFunctions.Authorize_CheckPassword(login, passwordHash))
                    {
                        PasswordMakeWrong();
                    }
                    else
                    {
                        if (!MainFunctions.Authorize_IsAccountActivated(login))
                        {
                            new Message($"Предупреждение", $"Ваш аккаунт заблокирован. Вход запрещен. Свяжитесь с администратором.").ShowDialog();
                        }
                        else
                        {
                            string role = MainFunctions.GetUserRole(login);

                            MainFunctions.LogInSystemEvent(login, role);

                            switch (role)
                            {
                                case "1":
                                    Windows.Menu adminMenuWindow = new Windows.Menu("admin", login);
                                    this.Close();
                                    adminMenuWindow.Show();
                                    break;

                                case "2":
                                    Windows.Menu managerMenuWindow = new Windows.Menu("manager", login);
                                    this.Close();
                                    managerMenuWindow.Show();
                                    break;

                                case "3":
                                    Windows.Menu analystMenuWindow = new Windows.Menu("analyst", login);
                                    this.Close();
                                    analystMenuWindow.Show();
                                    break;

                                default:
                                    new Message($"Ошибка", $"Ваш тип учетной записи не поддерживается. " +
                                    $"Если вы увидели это окно, свяжитесь с администратором.").ShowDialog();
                                    break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && btnLogin.IsEnabled == true)
            {
                btnLogin_Click(sender, e);
            }
        }
        #endregion


    }
}
