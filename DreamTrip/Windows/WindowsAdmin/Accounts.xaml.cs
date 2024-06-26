﻿using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Accounts.xaml
    /// </summary>
    public partial class Accounts : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;

        string[] thisPageParametres = new string[] { "Auto", "Аккаунты", "../../Resources/accounts.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<AccountType> accountTypes = new List<AccountType>();
        List<Account> accountsList = new List<Account>();
        #endregion

        #region Constructor
        public Accounts(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            try
            {
                LoadAccountTypes();
                LoadAccounts();
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown load error: {ex.Message}");
            }

            if (MainFunctions.GetShowPrompts()) btnHelpInfo.Visibility = Visibility.Visible;
            else btnHelpInfo.Visibility = Visibility.Hidden;
        }
        #endregion

        #region LoadData
        private void LoadAccountTypes()
        {
            accountTypes.Clear();

            DataTable typesData = MainFunctions.NewQuery($"SELECT * FROM User_roles WHERE id_role != 4 ORDER BY role");

            for (int i = 0; i < typesData.Rows.Count; i++)
            {
                accountTypes.Add(new AccountType()
                {
                    AccountTypeId = int.Parse(typesData.Rows[i][0].ToString()),
                    TypeName = typesData.Rows[i][1].ToString()
                });
            }
        }

        private void LoadAccounts()
        {
            accountsList.Clear();
            dtgAccounts.ItemsSource = null;

            DataTable accountsData = MainFunctions.NewQuery($"SELECT * FROM Worker");

            for (int i = 0; i < accountsData.Rows.Count; i++)
            {
                if (accountsData.Rows[i][5].ToString() != MainFunctions.СurrentSessionLogin)
                {
                    string fullPath = MainFunctions.GetAppPath() + accountsData.Rows[i][8].ToString();
                    string path = accountsData.Rows[i][8].ToString();
                    if (!File.Exists(fullPath))
                    {
                        path = "/Resources/AccountsPhotos/default.jpg";
                        fullPath =  MainFunctions.GetAppPath() + path;
                    }

                    accountsList.Add(new Account()
                    {
                        AccountId = int.Parse(accountsData.Rows[i][0].ToString()),
                        Surname = accountsData.Rows[i][1].ToString(),
                        Name = accountsData.Rows[i][2].ToString(),
                        Patronymic = accountsData.Rows[i][3].ToString(),
                        Phone = accountsData.Rows[i][4].ToString(),
                        Login = accountsData.Rows[i][5].ToString(),
                        AllTypes = accountTypes,
                        Type = GetAccountType(int.Parse(accountsData.Rows[i][6].ToString())),
                        IsActivated = Convert.ToBoolean(accountsData.Rows[i][7].ToString()),
                        IsNew = Visibility.Collapsed,
                        ImagePath = path,
                        OrigImagePath = fullPath
                    });
                }
            }

            dtgAccounts.ItemsSource = accountsList;
        }
        #endregion

        #region Functions
        private AccountType GetAccountType(int typeId)
        {
            AccountType tempType = new AccountType();

            for (int i = 0; i < accountTypes.Count; i++)
            {
                if (accountTypes[i].AccountTypeId == typeId)
                {
                    tempType = accountTypes[i];
                    break;
                }
            }

            return tempType;
        }

        private bool CheckDataIsValid()
        {
            bool isValid = true;
            List<string> errors = new List<string>();

            for (int i = 0; i < accountsList.Count; i++)
            {
                if (accountsList[i].Surname.Trim().Length == 0) 
                    errors.Add($"Фамилия пользователя с логином {accountsList[i].Login} нулевой длины");
                if (!MainFunctions.ValidateString_RuEng(accountsList[i].Surname))
                    errors.Add($"Неверный формат фамилии {accountsList[i].Surname} пользователя с логином {accountsList[i].Login}");

                if (accountsList[i].Name.Trim().Length == 0)
                    errors.Add($"Имя пользователя с логином {accountsList[i].Login} нулевой длины");
                if (!MainFunctions.ValidateString_RuEng(accountsList[i].Name))
                    errors.Add($"Неверный формат имени {accountsList[i].Name} пользователя с логином {accountsList[i].Login}");

                if (accountsList[i].Patronymic.Trim().Length == 0)
                    errors.Add($"Отчество пользователя с логином {accountsList[i].Login} нулевой длины");
                if (!MainFunctions.ValidateString_RuEng(accountsList[i].Patronymic))
                    errors.Add($"Неверный формат отчества {accountsList[i].Patronymic} пользователя с логином {accountsList[i].Login}");

                if (accountsList[i].Login.Trim().Length == 0)
                    errors.Add($"Логин пользователя \"{accountsList[i].Surname} {accountsList[i].Name}\" нулевой длины");
                if (!MainFunctions.ValidateString_EngNum(accountsList[i].Login))
                    errors.Add($"Неверный формат логина {accountsList[i].Login}");

                if (accountsList[i].Phone.Trim().Length == 0)
                    errors.Add($"Телефон пользователя с логином {accountsList[i].Login} нулевой длины");
            }

            if (errors.Count > 0)
            {
                isValid = false;
                new Message("Ошибка", string.Join(",", errors)).ShowDialog();
            }

            return isValid;
        }

        private bool CheckRepeatedLogins()
        {
            bool isRepeated = false;
            SortedSet<string> loginsSet = new SortedSet<string>();

            for (int i = 0; i < accountsList.Count - 1; i++)
            {
                for (int j = i+1; j < accountsList.Count; j++)
                {
                    if (accountsList[i].Login == accountsList[j].Login)
                    {
                        loginsSet.Add(accountsList[i].Login);
                        break;
                    }
                }
            }


            if (loginsSet.Count != 0)
            {
                string[] logins = new string[loginsSet.Count];

                new Message($"Ошибка", $"Логины не могут повторяться. В число повторяющихся логинов входят {string.Join(",", loginsSet.ToArray())}").ShowDialog();
                isRepeated = true;
            }
            return isRepeated;
        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);

            
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Account thisAccount = (dtgAccounts.SelectedItem as Account);
            int accountId = thisAccount.AccountId;
            string accountLogin = thisAccount.Login;

            if (accountId == 0)
            {
                accountsList.Remove(thisAccount);
                dtgAccounts.ItemsSource = null;
                dtgAccounts.ItemsSource = accountsList;
            }
            else
            {
                bool answer = false;
                Message deleteWindow = new Message("Вопрос", $"Вы уверены, что хотите удалить аккаунт с логином \"{accountLogin}\"? \n" +
                    $"Вместе с ним удалятся все связанные логи (история действий).", true, true);
                deleteWindow.messageAnswer += value => answer = value;
                deleteWindow.ShowDialog();

                if (!answer)
                {
                    return;
                }

                // удаление
                try
                {
                    MainFunctions.NewQuery($"DELETE FROM Login_history WHERE login = '{accountLogin}'");
                    MainFunctions.NewQuery($"DELETE FROM Worker WHERE login = '{accountLogin}'");
                    if (accountLogin != "TEMP")
                        MainFunctions.NewQuery($"DELETE FROM User_login_data WHERE login = '{accountLogin}'");
                    new Message("Успех", "Аккаунт успешно удален!").ShowDialog();
                    MainFunctions.AddLogRecord($"Delete account success" +
                        $"\n\tID: {accountId}" +
                        $"\n\tLogin: {accountLogin}" +
                        $"\n\tFull name: {thisAccount.Surname} {thisAccount.Name} {thisAccount.Patronymic}" +
                        $"\n\tPhone: {thisAccount.Phone}," +
                        $"\n\tRole: {thisAccount.Type.TypeName}");
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                    MainFunctions.AddLogRecord($"Account deletion error: {ex.Message}");
                }

                LoadAccounts();
            }
        }

        private void btnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            dtgAccounts.ItemsSource = null;
            accountsList.Insert(0, new Account()
            {
                AccountId = 0,
                Surname = "Фамилия",
                Name = "Имя",
                Patronymic = "Отчество",
                Login = "Login" + accountsList.Count.ToString(),
                Password = "Password",
                Phone = "9180000000",
                AllTypes = accountTypes,
                Type = accountTypes[2],
                IsActivated = true,
                IsNew = Visibility.Visible,
                ImagePath = "/Resources/AccountsPhotos/default.jpg",
                OrigImagePath = MainFunctions.GetAppPath() + "/Resources/AccountsPhotos/default.jpg",

            }) ;
            dtgAccounts.ItemsSource = accountsList;
            dtgAccounts.ScrollIntoView(accountsList[0]);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckRepeatedLogins() && CheckDataIsValid())
            {
                try
                {
                    //сохранение аккаунтов
                    for (int i = 0; i < accountsList.Count; i++)
                    {
                        accountsList[i].Login = accountsList[i].Login.Trim();
                        accountsList[i].Surname = accountsList[i].Surname.Trim();
                        accountsList[i].Name = accountsList[i].Name.Trim();
                        accountsList[i].Patronymic = accountsList[i].Patronymic.Trim();
                        accountsList[i].Phone = accountsList[i].Phone.Trim();

                        if (accountsList[i].AccountId == 0)
                        {
                            accountsList[i].Password = accountsList[i].Password.Trim();

                            MainFunctions.NewQuery($"INSERT INTO User_login_data VALUES ('{accountsList[i].Login}', " +
                                $"'{MainFunctions.GetHash(accountsList[i].Password)}', " +
                                $" {accountsList[i].Type.AccountTypeId}, 1)");
                            MainFunctions.NewQuery($"INSERT INTO Worker VALUES ('{accountsList[i].Surname}'," +
                                $"'{accountsList[i].Name}', " +
                                $"'{accountsList[i].Patronymic}', " +
                                $"'{accountsList[i].Phone}', " +
                                $"'{accountsList[i].Login}', " +
                                $" {accountsList[i].Type.AccountTypeId}," +
                                $" {1}, " +
                                $" '{accountsList[i].ImagePath}')");


                            MainFunctions.AddLogRecord($"Create account success" +
                                $"\n\tID: {int.Parse(MainFunctions.NewQuery($"SELECT MAX(id_worker) FROM Worker").Rows[0][0].ToString())}" +
                                $"\n\tLogin: {accountsList[i].Login}," +
                                $"\n\tFull Name: {accountsList[i].Surname} {accountsList[i].Name} {accountsList[i].Patronymic}," +
                                $"\n\tPhone: {accountsList[i].Phone}," +
                                $"\n\tRole: {accountsList[i].Type.TypeName}");
                        }
                        else
                        {

                            string previousLogin = MainFunctions.NewQuery($"SELECT login FROM Worker WHERE id_worker = {accountsList[i].AccountId}").Rows[0][0].ToString();
                            if (previousLogin != accountsList[i].Login)
                            {
                                if (Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM User_login_data " +
                                    $"WHERE login = '{accountsList[i].Login}'").Rows[0][0].ToString()) == 0)
                                {
                                    MainFunctions.NewQuery($"UPDATE Worker SET login = 'TEMP' WHERE id_worker = {accountsList[i].AccountId}");
                                    MainFunctions.NewQuery($"INSERT INTO User_login_data VALUES ('{accountsList[i].Login}', " +
                                        $"(SELECT password_hash FROM User_login_data WHERE login = '{previousLogin}'), " +
                                        $"(SELECT id_role FROM User_login_data WHERE login = '{previousLogin}'), 1)");
                                    MainFunctions.NewQuery($"UPDATE Login_history SET login = '{accountsList[i].Login}' WHERE login = '{previousLogin}'");
                                    MainFunctions.NewQuery($"UPDATE Worker SET login = '{accountsList[i].Login}' WHERE id_worker = {accountsList[i].AccountId}");
                                    MainFunctions.NewQuery($"DELETE FROM User_login_data WHERE login = '{previousLogin}'");
                                }
                                else
                                {
                                    new Message("Ошибка", $"Ошибка смены логина \"{previousLogin}\" на \"{accountsList[i].Login}\". " +
                                        $"Пользователь с логином \"{accountsList[i].Login}\" уже существует.").ShowDialog();
                                }
                            }

                            MainFunctions.NewQuery($"UPDATE Worker SET " +
                                $"surname = '{accountsList[i].Surname}', " +
                                $"name = '{accountsList[i].Name}', " +
                                $"patronymic = '{accountsList[i].Patronymic}'," +
                                $"phone = '{accountsList[i].Phone}'," +
                                $"id_role = {accountsList[i].Type.AccountTypeId}," +
                                $"is_activated = {Convert.ToInt32(accountsList[i].IsActivated)} " +
                                $"WHERE id_worker = {accountsList[i].AccountId}");
                        }

                    }
                    MainFunctions.AddLogRecord($"Update accounts success");

                    //обновление таблиц и списков
                    LoadAccounts();

                    //успешно сохранено

                    new Message("Успех", $"Данные успешно сохранены!", false, false).ShowDialog();

                }
                //исключение
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так... Данные не сохранены.").ShowDialog();
                    MainFunctions.AddLogRecord($"Updating accounts error" +
                        $"\n\tError text: {ex.Message}");

                    
                }
            }
        }
        #endregion

        #region Changed
        private void dtgAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            borderDeleteButton.IsEnabled = dtgAccounts.SelectedIndex != -1;
        }

        private void tbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            try
            {
                char[] charList = textBox.Text.ToCharArray();
                for (int i = 0; i < charList.Length; i++)
                {
                    if (!MainFunctions.ValidateString_EngNum(charList[i].ToString()) && charList[i].ToString() != " ")
                    {
                        textBox.Text = textBox.Text.Remove(i, 1);
                    }
                }

            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }

            if (textBox.Text.Length == 0) textBox.Text = " ";
            
        }

        private void tbPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            try
            {
                char[] charList = textBox.Text.ToCharArray();
                for (int i = 0; i < charList.Length; i++)
                {
                    if (!MainFunctions.ValidateString_RuEngNumSpec(charList[i].ToString()))
                    {
                        textBox.Text = textBox.Text.Remove(i, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }



            if (textBox.Text.Length == 0) textBox.Text = " ";
        }

        private void FullNameTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            try
            {
                char[] charList = textBox.Text.ToCharArray();
                for (int i = 0; i < charList.Length; i++)
                {
                    if (!MainFunctions.ValidateString_RuEng(charList[i].ToString()))
                    {
                        textBox.Text = textBox.Text.Remove(i, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }

            if (textBox.Text.Length == 0) textBox.Text = " ";
        }

        private void tbPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbPhone = sender as TextBox;

            try
            {
                char[] charList = tbPhone.Text.Trim().ToCharArray();
                for (int i = 0; i < charList.Length; i++)
                {
                    if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false)
                    {
                        tbPhone.Text = tbPhone.Text.Remove(i, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                btnCancel_Click(btnCancel, new RoutedEventArgs());
                MainFunctions.AddLogRecord($"Unknown error: {ex.Message}");
            }

            if (tbPhone.Text.Length > 12) tbPhone.Text = tbPhone.Text.Substring(0, 12);
            if (tbPhone.Text.Length == 0) tbPhone.Text = " ";
        }




        #endregion

    }
}
