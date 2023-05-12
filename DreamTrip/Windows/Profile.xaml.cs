using DreamTrip.Classes;
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
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Профиль", "../Resources/user_profile.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        Account currentAccount;
        string origPhone, origImagePath;
        #endregion

        #region Constructor
        public Profile(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            currentAccount = GetAccount(MainFunctions.СurrentSessionLogin);
            this.DataContext = currentAccount;
        }
        #endregion

        #region LoadData
        private Account GetAccount(string login)
        {
            Account account;

            DataTable accountData = MainFunctions.NewQuery($"SELECT w.id_worker, w.login, w.surname, w.name, w.patronymic, " +
                $" w.phone, ur.role, w.image_path  FROM worker w " +
                $" JOIN User_roles ur ON ur.id_role = w.id_role " +
                $" WHERE w.login = '{login}' ");

            string fullPath = MainFunctions.GetAppPath() + accountData.Rows[0][7].ToString();
            if (!File.Exists(fullPath))
            {
                fullPath = MainFunctions.GetAppPath() + "/Resources/AccountsPhotos/default.jpg";
            }

            account = new Account()
            {
                AccountId = int.Parse(accountData.Rows[0][0].ToString()),
                Login = login, 
                Surname = accountData.Rows[0][2].ToString(),
                Name = accountData.Rows[0][3].ToString(),
                Patronymic = accountData.Rows[0][4].ToString(),
                Phone = accountData.Rows[0][5].ToString(),
                Role = accountData.Rows[0][6].ToString(),
                OrigImagePath = fullPath
            };

            origImagePath = account.OrigImagePath;
            origPhone = account.Phone;

            return account;
        }

        private void changeImage()
        {
            string newImagePath = "";

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.FileName = null;
            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    newImagePath = openFileDialog.FileName;
                    string extenstion = System.IO.Path.GetExtension(newImagePath);
                    if (extenstion == ".png" || extenstion == ".jpg" || extenstion == ".jpeg")
                    {
                        currentAccount.OrigImagePath = newImagePath;

                        BitmapImage tempServiceImage = new BitmapImage();
                        tempServiceImage.BeginInit();
                        tempServiceImage.CacheOption = BitmapCacheOption.OnLoad;
                        FileStream stream = File.OpenRead(newImagePath);
                        tempServiceImage.StreamSource = stream;
                        tempServiceImage.EndInit();
                        stream.Close();

                        //imgPhoto. = tempServiceImage;

                        CheckButtonSaveIsEnabled();
                    }
                    else
                    {
                        Message message = new Message("Ошибка", "Неверный формат изображения (png,jpeg,jpg)", false, false);
                        message.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Возможно, неверно был выбран тип файла. Файл должен иметь формат изображения.", false, false);
                messageError.ShowDialog();
                MainFunctions.AddLogRecord("Change Profile Image Error Text: " + ex.Message);
            }
        }
        
        private void CheckButtonSaveIsEnabled()
        {
            borderSaveButton.IsEnabled = ((tbxPhone.Text != origPhone) || (currentAccount.OrigImagePath != origImagePath)) && (tbxPhone.Text.Length >= 10);
        }
        #endregion

        #region ButtonsClick

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            EnterOldPassword enterOldPassword = new EnterOldPassword();
            enterOldPassword.ShowDialog();

            //вызываем окно старого пароля (передаем login)
            
            //проверяем в нем соответствует ли хэш пароля тому что в БД
                //соответствует 
                    //открываем окно двойного ввода нового пароля (login)
                        //совпадают - записываем хэш в БД
                        //не сопадают - повторите попытку

                //не соответствует - повторите попытку
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show($"origPhone {origPhone} newPhone {currentAccount.Phone}" +
            //    $" \norigImage {origImagePath} \nnewImage {currentAccount.OrigImagePath}");

            string appFolder = MainFunctions.GetAppPath();
            string imagesFolder = "/Resources/AccountsPhotos/";

            try
            {
                //update телефон
                if (origPhone != currentAccount.Phone)
                {
                    MainFunctions.NewQuery($"UPDATE Worker SET phone = '{tbxPhone.Text}' WHERE id_worker = {currentAccount.AccountId}");
                    origPhone = tbxPhone.Text;
                }


                if (origImagePath != currentAccount.OrigImagePath)
                {

                    string extenstion = System.IO.Path.GetExtension(currentAccount.OrigImagePath);
                    string photoName = $"{currentAccount.AccountId} {currentAccount.Login}";

                    string lastFileDigit = "";
                    int counter = 0;
                    while (File.Exists(appFolder + imagesFolder + photoName + lastFileDigit + extenstion))
                    {
                        counter++;
                        lastFileDigit = "_" + counter.ToString();
                    }

                    File.Copy(currentAccount.OrigImagePath, appFolder + imagesFolder + photoName + lastFileDigit + extenstion, true);

                    MainFunctions.NewQuery($"UPDATE Worker  SET image_path = '{imagesFolder + photoName + lastFileDigit + extenstion}' WHERE id_worker = {currentAccount.AccountId}");

                    origImagePath = currentAccount.OrigImagePath;
                }

                CheckButtonSaveIsEnabled();

                new Message("Успех", $"Данные успешно сохранены!", false, false).ShowDialog();

                MainFunctions.AddLogRecord($"Profile updating success " +
                    $"\n\tID: {currentAccount.AccountId} " +
                    $"\n\tLogin: {currentAccount.Login}");


            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так...", false, false);
                messageError.ShowDialog();

                MainFunctions.AddLogRecord($"Profile saving error:" +
                            $"\n\tError: {ex.Message}");
            }

        }

        private void borderImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            changeImage();
        }
        #endregion

        #region Changed
        private void tbxPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbPhone = sender as TextBox;

            char[] charList = tbPhone.Text.Trim().ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false && charList[i].ToString() != "+")
                {
                    tbPhone.Text = tbPhone.Text.Remove(i, 1);
                }
            }

            if (tbPhone.Text.Length > 10) tbPhone.Text = tbPhone.Text.Substring(0, 10);

            CheckButtonSaveIsEnabled();
        }

        #endregion

    }
}
