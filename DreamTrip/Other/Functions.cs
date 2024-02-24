using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using DreamTrip.Classes;
using System.Security.Cryptography;
using DreamTrip.Windows;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace DreamTrip.Functions
{
    /// <summary>
    /// Главные функции и поля, используемые приложением
    /// </summary>
    public static class MainFunctions
    {
        #region Menu&Tabs
        /// <summary>
        /// Ссылка на меню менеджера - родительский элемент всех вкладок
        /// </summary>
        public static Windows.Menu MenuLink;

        /// <summary>
        /// Поменять параметры текущей вкладки (название, изображение)
        /// </summary>
        /// <param name="tabItem"></param>
        /// <param name="parametres"></param>
        public static void ChangeTabParametres(TabClass tabItem, string[] parametres)
        {
            tabItem.VerticalScrollBarVisibility = parametres[0];
            tabItem.ItemHeaderText = parametres[1];
            tabItem.ItemHeaderImageSource = parametres[2];
        }

        #endregion

        #region CurrentSession

        #region Variables
        /// <summary>
        /// Дата и время входа в систему для данной сессии
        /// </summary>
        private static DateTime logInDateTime;

        /// <summary>
        /// Роль пользователя в текущей сессии
        /// </summary>
        private static string currentSessionUserRole;

        /// <summary>
        /// Логин пользователя в текущей сессии
        /// </summary>
        public static string СurrentSessionLogin;

        /// <summary>
        /// Логи текущей сессии
        /// </summary>
        private static string Logs;

        #endregion

        #region Functions Login&History
        /// <summary>
        /// Событие входа в систему
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        public static void LogInSystemEvent(string login, string userRole)
        {
            logInDateTime = DateTime.Now;
            ClearLogs();
            СurrentSessionLogin = login;

            switch (userRole)
            {
                case "1":
                    currentSessionUserRole = "admin";
                    break;
                case "2":
                    currentSessionUserRole = "manager";
                    break;
                case "3":
                    currentSessionUserRole = "analyst";
                    break;
                case "4":
                    currentSessionUserRole = "client";
                    break;
            }
        }

        /// <summary>
        /// Изменение логов Logs
        /// </summary>
        /// <param name="record">Запись о действии</param>
        public static void AddLogRecord(string record)
        {
            Logs += $"\n{DateTime.Now} {record}";
        }

        /// <summary>
        /// Добавление новой записи БД в историю действий Login History
        /// </summary>
        /// <param name="login"></param>
        public static void AddHistoryRecord(string login)
        {
            NewQuery($"INSERT INTO Login_history VALUES ('{login}', '{logInDateTime}' ,'{DateTime.Now}', '{Logs}\n{DateTime.Now} {login} logged out')");
        }

        /// <summary>
        /// Очистить логи текущей сессиии
        /// </summary>
        public static void ClearLogs()
        {
            Logs = "";
        }
        #endregion

        #region Functions CurrentUserInfo
        /// <summary>
        /// Получить роль пользователя текущей сессии
        /// </summary>
        /// <returns></returns>
        public static string GetUserRole()
        {
            return currentSessionUserRole;
        }

        /// <summary>
        /// Нужно ли показывать подсказки для текущего пользователя
        /// </summary>
        /// <returns></returns>
        public static bool GetShowPrompts()
        {
            return Convert.ToBoolean(NewQuery($"SELECT ISNULL(show_prompts,1) FROM User_login_data WHERE login = '{СurrentSessionLogin}'").Rows[0][0]);
        }

        #endregion

        #endregion

        #region Connection&Query

        /// <summary>
        /// Строка подключения
        /// </summary>
        private static string connectionString = "";

        /// <summary>
        /// Загрузить строку подключения
        /// </summary>
        public static void LoadConnectionString()
        {
            INIManager manager = new INIManager(GetAppPath() + "\\server.ini");

            connectionString += $"server={manager.GetPrivateString("connectionString", "server")};";

            connectionString += $"Trusted_Connection={manager.GetPrivateString("connectionString", "Trusted_Connection")};";
            if (manager.GetPrivateString("connectionString", "Trusted_Connection") == "No")
            {
                connectionString += $"user={manager.GetPrivateString("connectionString", "user")};";
                connectionString += $"password={manager.GetPrivateString("connectionString", "password")};";
            }
            connectionString += $"DataBase={manager.GetPrivateString("connectionString", "DataBase")};";
        }


        /// <summary>
        /// Инициирует запрос к БД
        /// </summary>
        /// <param name="selectSQL">запрос</param>
        /// <returns>таблица, сформированная запросом</returns>
        public static DataTable NewQuery(string selectSQL)
        {
            DataTable dataTable = new DataTable("dataBase");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = selectSQL;
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(dataTable);
            SqlConnection.ClearAllPools();
            return dataTable;
        }
        #endregion

        #region Auth
        /// <summary>
        /// Проверка существования логина
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool Authorize_LoginExists(string login)
        {
            bool exists = false;

            exists = MainFunctions.NewQuery($"SELECT login FROM User_login_data WHERE login = '{login}'").Rows.Count != 0;

            return exists;
        }

        /// <summary>
        /// Проверка корректности пароля (по хэшу)
        /// </summary>
        /// <param name="login"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public static bool Authorize_CheckPassword(string login, string passwordHash)
        {
            bool passwordCorrect = false;

            passwordCorrect = passwordHash == MainFunctions.NewQuery($"SELECT password_hash FROM User_login_data WHERE login = '{login}'").Rows[0][0].ToString();

            return passwordCorrect;
        }

        /// <summary>
        /// Проверка статуса аккаунта (активен или заблокирован)
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool Authorize_IsAccountActivated(string login)
        {
            bool isActivated = false;

            isActivated = Convert.ToBoolean(MainFunctions.NewQuery($"SELECT is_activated FROM worker WHERE login = '{login}'").Rows[0][0].ToString());

            return isActivated;
        }

        #endregion

        #region Gets
        /// <summary>
        /// Получить роль пользователя по логину
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static string GetUserRole(string login)
        {
            string role = "";

            role = MainFunctions.NewQuery($"SELECT id_role FROM User_login_data WHERE login = '{login}'").Rows[0][0].ToString();

            return role;
        }

        /// <summary>
        /// Получить путь приложения
        /// </summary>
        /// <returns>путь к основной папке приложения</returns>
        public static string GetAppPath()
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
        }

        /// <summary>
        /// Получить текущую папку exe файла
        /// </summary>
        /// <returns>путь к папке с exe файлом данного запущенного приложения</returns>
        public static string GetCurrentExePath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Возвращает хэш указанной строки. Нужно для проверки корректности логина и пароля 
        /// (в БД хранится хэш данных авторизации, а не сами данные)
        /// </summary>
        /// <param name="word">строка</param>
        /// <returns>хэш</returns>
        public static string GetHash(string hashing_string)
        {
            byte[] tmpSource = ASCIIEncoding.ASCII.GetBytes(hashing_string);
            byte[] tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);

            int i;
            StringBuilder sOutput = new StringBuilder(tmpHash.Length);
            for (i = 0; i < tmpHash.Length; i++)
            {
                sOutput.Append(tmpHash[i].ToString("X2"));
            }

            return sOutput.ToString();
        }

        /// <summary>
        /// Возвращает дочерний элемент указанного типа у предоставленного родительского элемента
        /// </summary>
        /// <typeparam name="T">тип дочернего элемента</typeparam>
        /// <param name="depObj">родительский элемент</param>
        /// <returns>ссылка на дочерний элемент</returns>
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        #endregion

        #region Validate
        /// <summary>
        /// Проверка корректности названия тура
        /// </summary>
        /// <param name="tourName">название тура</param>
        /// <returns>результат проверки корректности</returns>
        public static bool ValidateString_RuEngNumSpec(string tourName)
        {
            if (tourName.Length == 0) return false;
            tourName = tourName.ToLower();
            char[] letters = { 'a','b','c','d','e','f','g','h','i','j','k',
                'l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж','з', 'и', 'й', 'к',
                'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц',
                'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я', '-', '—',
                '1','2','3','4','5','6','7','8','9','0','!','?','&','$',
                ' ',',','.','\\','/','\"', '№','#', '(', ')'};
            foreach (char ch in tourName)
                if (Array.IndexOf(letters, ch, 0) < 0)
                {
                    return false;
                }
            return true;
        }

        /// <summary>
        /// Проверка корректности логина
        /// </summary>
        /// <param name="login">логин</param>
        /// <returns>результат проверки корректности</returns>
        public static bool ValidateString_EngNum(string login)
        {
            if (login.Length == 0) return false;
            login = login.ToLower();
            char[] letters = { 'a','b','c','d','e','f','g','h','i','j','k',
                'l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                '-', '—', '_', '1','2','3','4','5','6','7','8','9','0'};
            foreach (char ch in login)
                if (Array.IndexOf(letters, ch, 0) < 0)
                {
                    return false;
                }
            return true;
        }

        /// <summary>
        /// Проверка корректности имени
        /// </summary>
        /// <param name="name">имя</param>
        /// <returns>результат проверки корректности</returns>
        public static bool ValidateString_RuEng(string name)
        {
            if (name.Length == 0) return false;
            name = name.ToLower();
            char[] letters = { 'a','b','c','d','e','f','g','h','i','j','k',
                           'l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                           'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж',
                           'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о',
                           'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц',
                           'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я', '-', '—',' ' };
            foreach (char ch in name)
                if (Array.IndexOf(letters, ch, 0) < 0)
                {
                    return false;
                }
            return true;
        }
        #endregion

        #region TourImages&DOcs
        /// <summary>
        /// Загрузить изображение тура в БД
        /// </summary>
        /// <param name="iFile">путь к изображению</param>
        /// <param name="tourId">id тура</param>
        /// <returns>результат загрузки (true - успешно загружено,false - ошибка)</returns>
        public static bool PutTourImageInDb(string iFile, int tourId)
        {
            try
            {
                string base64String = null;
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(iFile))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }
                string iImageExtension = (Path.GetExtension(iFile)).Replace(".", "").ToLower();

                if (int.Parse(NewQuery($"SELECT COUNT(id_rec) FROM Tour_photos WHERE id_tour={tourId}").Rows[0][0].ToString()) == 0)
                {
                    NewQuery($"INSERT INTO Tour_photos (id_tour, photo, format) VALUES({tourId}, '{base64String}', '{iImageExtension}')");
                }
                else
                {
                    NewQuery($"UPDATE Tour_photos SET photo = '{base64String}', format = '{iImageExtension}' WHERE id_tour = {tourId}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Возможно, неверно был выбран тип файла. Файл должен иметь формат изображения.", false, false);
                messageError.ShowDialog();
                AddLogRecord("PutTourImageInDb Error text: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Загрузить документ поездки в БД
        /// </summary>
        /// <param name="iFile">путь к документу</param>
        /// <param name="tripId">id поездки</param>
        /// <param name="docType">тип документа</param>
        public static void PutTripDocumentInDb(string iFile, int tripId, string docType)
        {
            try
            {
                string base64String = null;
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(iFile))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }
                string iImageExtension = (Path.GetExtension(iFile)).Replace(".", "").ToLower();

                if (int.Parse(NewQuery($"SELECT COUNT(id_trip) FROM Trip_docs WHERE id_trip={tripId}").Rows[0][0].ToString()) == 0)
                {
                    NewQuery($"INSERT INTO Trip_docs (id_trip, {docType}_doc, {docType}_format) VALUES({tripId}, '{base64String}', '{iImageExtension}')");
                }
                else
                {
                    NewQuery($"UPDATE Trip_docs SET {docType}_doc = '{base64String}', {docType}_format = '{iImageExtension}' WHERE id_trip = {tripId}");
                }

                Message messageSucces = new Message("Успех", "Документ был успешно загружен!", false, false);
                messageSucces.ShowDialog();
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Возможно, неверно был выбран тип файла. Файл должен иметь формат изображения.", false, false);
                messageError.ShowDialog();
                AddLogRecord("PutTourImageInDb Error text: " + ex.Message);

            }
        }

        /// <summary>
        /// Скачать документ поездки из БД
        /// </summary>
        /// <param name="tripId">id поездки</param>
        /// <param name="docType">тип документа</param>
        public static void GetTripDocumentFromDb(int tripId, string docType)
        {
            try
            {
                List<string> iScreen = new List<string>();
                List<string> iScreen_format = new List<string>();
                using (SqlConnection sqlConnection = new SqlConnection("server=BONJOVI\\SQLEXPRESS;Trusted_Connection=Yes;DataBase=DreamTrip_Project;"))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = $@"SELECT [{docType}_doc] , [{docType}_format] FROM Trip_docs WHERE id_trip = {tripId}";
                    SqlDataReader sqlReader = sqlCommand.ExecuteReader();
                    string iTrimText = null;
                    while (sqlReader.Read())
                    {
                        iTrimText = sqlReader[$"{docType}_doc"].ToString().TrimStart().TrimEnd();
                        iScreen.Add(iTrimText);
                        iTrimText = sqlReader[$"{docType}_format"].ToString().TrimStart().TrimEnd();
                        iScreen_format.Add(iTrimText);
                    }
                    sqlConnection.Close();
                }

                string base64StringImage = iScreen[0];
                byte[] imageData = Convert.FromBase64String(base64StringImage);
                MemoryStream ms = new MemoryStream(imageData);
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(ms);

                string iImageExtension = iScreen_format[0];
                string iImageName;

                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.FileName = null;
                saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                saveFileDialog.Filter = $"Изображение {iImageExtension}(*.{iImageExtension})|*.{iImageExtension}|Все файлы (*.*)|*.*";
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName != null && saveFileDialog.FileName != "")
                {
                    iImageName = saveFileDialog.FileName;
                    if (!iImageName.Contains($".{iImageExtension}"))
                    {
                        if (iImageName.Contains(".")) iImageName = iImageName.Remove(iImageName.IndexOf('.'));
                        iImageName += $".{iImageExtension}";
                    }
                    newImage.Save(iImageName);
                }


            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Попробуйте выбрать другое расположение и название файла.", false, false);
                messageError.ShowDialog();
                AddLogRecord("GetTripDocumentFromDb Error text: " + ex.Message);

            }
        }

        /// <summary>
        /// Возвращает скачанное из БД изображение тура в виде BitmapImage
        /// </summary>
        /// <param name="tourId">id тура</param>
        /// <returns> изображение тура в виде BitmapImage</returns>
        public static BitmapImage GetTourImageSource(int tourId)
        {
            List<string> iScreen = new List<string>();
            List<string> iScreen_format = new List<string>();
            using (SqlConnection sqlConnection = new SqlConnection("server=BONJOVI\\SQLEXPRESS;Trusted_Connection=Yes;DataBase=DreamTrip_Project;"))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = $@"SELECT [photo] , [format] FROM Tour_photos WHERE id_tour = {tourId}";
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();
                string iTrimText = null;
                while (sqlReader.Read())
                {
                    iTrimText = sqlReader[$"photo"].ToString().TrimStart().TrimEnd();
                    iScreen.Add(iTrimText);
                    iTrimText = sqlReader[$"format"].ToString().TrimStart().TrimEnd();
                    iScreen_format.Add(iTrimText);
                }
                sqlConnection.Close();
            }

            string base64StringImage = iScreen[0];
            byte[] imageData = Convert.FromBase64String(base64StringImage);
            MemoryStream ms = new MemoryStream(imageData);
            BitmapImage ix = new BitmapImage();
            ix.BeginInit();
            ix.CacheOption = BitmapCacheOption.OnLoad;
            ix.StreamSource = ms;
            ix.EndInit();
            return ix;
        }

        /// <summary>
        /// Сохраняет изображение тура во временную папку во время работы программы
        /// </summary>
        /// <param name="tourId">id тура</param>
        /// <param name="localFilePath">путь для сохранения изображения</param>
        public static void SaveImage(int tourId, string localFilePath)
        {
            try
            {
                List<string> iScreen = new List<string>();
                List<string> iScreen_format = new List<string>();
                using (SqlConnection sqlConnection = new SqlConnection("server=BONJOVI\\SQLEXPRESS;Trusted_Connection=Yes;DataBase=DreamTrip_Project;"))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = $@"SELECT photo , format FROM Tour_photos WHERE id_tour = {tourId}";
                    SqlDataReader sqlReader = sqlCommand.ExecuteReader();
                    string iTrimText = null;
                    while (sqlReader.Read())
                    {
                        iTrimText = sqlReader[$"photo"].ToString().TrimStart().TrimEnd();
                        iScreen.Add(iTrimText);
                        iTrimText = sqlReader[$"format"].ToString().TrimStart().TrimEnd();
                        iScreen_format.Add(iTrimText);
                    }
                    sqlConnection.Close();
                }

                string base64StringImage = iScreen[0];
                byte[] imageData = Convert.FromBase64String(base64StringImage);
                MemoryStream ms = new MemoryStream(imageData);
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(ms);

                string iImageExtension = iScreen_format[0];
                string iImageName;

                iImageName = localFilePath;
                if (!iImageName.Contains($".{iImageExtension}"))
                {
                    if (iImageName.Contains(".")) iImageName = iImageName.Remove(iImageName.IndexOf('.'));
                    iImageName += $".{iImageExtension}";
                }
                newImage.Save(iImageName);
            }
            catch (Exception ex)
            {
                AddLogRecord("Save image Error text: " + ex.Message);

            }
        }
        #endregion

        #region ClearFiles
        /// <summary>
        /// Удаляет созданные во время работы программы временные папки с изображениями туров
        /// </summary>
        public static void DeleteTempFolders()
        {
            //try
            //{
            //    string resourcesPath;
            //    if (GetAppPath().Contains("DreamTrip"))
            //    {
            //        resourcesPath = GetAppPath() + "/Resources/ToursPhotos";
            //        foreach (string folder in Directory.GetDirectories(resourcesPath))
            //        {
            //            foreach (string file in Directory.GetFiles(folder))
            //            {
            //                File.Delete(file);
            //            }
            //            Directory.Delete(folder);
            //        }
            //    }
            //    else
            //    {
            //        resourcesPath = GetCurrentExePath() + "/DreamTrip";

            //        Directory.Delete(resourcesPath,true);

            //    }
            //}
            //catch (Exception ex)
            //{
            //    AddLogRecord("DeleteTempFolders Error text: " + ex.Message);

            //}
        }

        /// <summary>
        /// Удалить временные фото аккаунтов
        /// </summary>
        public static void ClearAccountsPhotos()
        {
            try
            {
                DataTable photosData = MainFunctions.NewQuery($"SELECT image_path FROM Worker");
                List<string> photoPaths = new List<string>();

                for (int i = 0; i < photosData.Rows.Count; i++)
                {
                    string path = photosData.Rows[i][0].ToString();
                    path = path.Substring(path.LastIndexOf('/') + 1, path.Length - path.LastIndexOf('/') - 1);
                    photoPaths.Add(path);
                }

                if (!photoPaths.Contains("default.jpg")) photoPaths.Add("default.jpg");

                foreach (string file in Directory.GetFiles(GetAppPath() + "/Resources/AccountsPhotos"))
                {
                    string fileName = Path.GetFileName(file);

                    try
                    {
                        if (!photoPaths.Contains(fileName))
                            File.Delete(file);
                    }
                    catch
                    {
                        //файл не удалился, может удалится в следующий раз
                    }

                }
            }
            catch (Exception ex)
            {
                MainFunctions.AddLogRecord($"Clear account photos error: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить временные фото сервисов
        /// </summary>
        public static void ClearServicesPhotos()
        {
            try
            {
                DataTable photosData = MainFunctions.NewQuery($"SELECT icon_path FROM Service");
                List<string> photoPaths = new List<string>();

                for (int i = 0; i < photosData.Rows.Count; i++)
                {
                    string path = photosData.Rows[i][0].ToString();
                    path = path.Substring(path.LastIndexOf('/') + 1, path.Length - path.LastIndexOf('/') - 1);
                    photoPaths.Add(path);
                }

                foreach (string file in Directory.GetFiles(GetAppPath() + "/Resources/ServicesPhotos"))
                {
                    string fileName = Path.GetFileName(file);

                    try
                    {
                        if (!photoPaths.Contains(fileName))
                            File.Delete(file);
                    }
                    catch
                    {
                        //файл не удалился, может удалится в следующий раз
                    }

                }
            }
            catch (Exception ex)
            {
                MainFunctions.AddLogRecord($"Clear Services photos error: {ex.Message}");
            }
        }
        #endregion
    }



    /// <summary>
    /// Основные функции, часто используемые приложением
    /// </summary>
    public static class CommonFunctions
    {
        #region GetCheckedItems
        /// <summary>
        /// Возвращает список id сфер деятельности, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список сфер деятельности</param>
        /// <returns>список id сфер деятельности, отмеченных галочкой </returns>
        public static int[] CheckedFields(ObservableCollection<WorkField> FieldsList)
        {

            int[] checkedFields = new int[0];

            for (int i = 0; i < FieldsList.Count; i++)
            {
                if (FieldsList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedFields, checkedFields.Length + 1);
                    checkedFields[checkedFields.Length - 1] = FieldsList[i].WorkFieldId;
                }
            }

            return checkedFields;
        }

        /// <summary>
        /// Возвращает список id стран, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список стран</param>
        /// <returns>список id стран, отмеченных галочкой </returns>
        public static int[] CheckedCountries(ObservableCollection<Country> CountriesList)
        {
            int[] checkedCountries = new int[0];

            for (int i = 0; i < CountriesList.Count; i++)
            {
                if (CountriesList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedCountries, checkedCountries.Length + 1);
                    checkedCountries[checkedCountries.Length - 1] = CountriesList[i].CountryId;
                }
            }

            return checkedCountries;
        }

        /// <summary>
        /// Возвращает список id городов, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список городов</param>
        /// <returns>список id городов, отмеченных галочкой </returns>
        public static int[] CheckedCities(ObservableCollection<City> CurrentCitiesList)
        {

            int[] checkedCities = new int[0];

            for (int i = 0; i < CurrentCitiesList.Count; i++)
            {
                if (CurrentCitiesList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedCities, checkedCities.Length + 1);
                    checkedCities[checkedCities.Length - 1] = CurrentCitiesList[i].CityId;
                }
            }

            return checkedCities;
        }

        /// <summary>
        /// Возвращает список id типов тура, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список типов тура</param>
        /// <returns>список id типов тура, отмеченных галочкой </returns>
        public static int[] CheckedTourTypes(ObservableCollection<TourType> TourTypesList)
        {

            int[] checkedTourTypes = new int[0];

            for (int i = 0; i < TourTypesList.Count; i++)
            {
                if (TourTypesList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedTourTypes, checkedTourTypes.Length + 1);
                    checkedTourTypes[checkedTourTypes.Length - 1] = TourTypesList[i].TourTypeId;
                }
            }

            return checkedTourTypes;
        }

        /// <summary>
        /// Возвращает список id сервисов, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список сервисов</param>
        /// <returns>список id сервисов, отмеченных галочкой </returns>
        public static int[] CheckedServicesIds(ObservableCollection<TourService> tourServiceList)
        {
            int[] checkedServices = new int[0];

            for (int i = 0; i < tourServiceList.Count; i++)
            {
                if (tourServiceList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedServices, checkedServices.Length + 1);
                    checkedServices[checkedServices.Length - 1] = tourServiceList[i].ServiceId;
                }
            }

            return checkedServices;
        }

        /// <summary>
        /// Возвращает список id типов питания, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список питания</param>
        /// <returns>список id питания, отмеченных галочкой </returns>
        public static int[] CheckedFeedTypes(ObservableCollection<FeedType> FeedTypesList)
        {

            int[] checkedFeedTypes = new int[0];

            for (int i = 0; i < FeedTypesList.Count; i++)
            {
                if (FeedTypesList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedFeedTypes, checkedFeedTypes.Length + 1);
                    checkedFeedTypes[checkedFeedTypes.Length - 1] = FeedTypesList[i].FeedTypeId;
                }
            }

            return checkedFeedTypes;
        }

        /// <summary>
        /// Возвращает список уровней комфорта отеля, отмеченных галочкой 
        /// </summary>
        /// <param name="FieldsList">список уровней комфорта отеля</param>
        /// <returns>список уровней комфорта отеля, отмеченных галочкой </returns>
        public static int[] CheckedHotelStars(ObservableCollection<HotelStars> HotelStarsList)
        {

            int[] checkedHotelStars = new int[0];

            for (int i = 0; i < HotelStarsList.Count; i++)
            {
                if (HotelStarsList[i].IsChecked == true)
                {
                    Array.Resize(ref checkedHotelStars, checkedHotelStars.Length + 1);
                    checkedHotelStars[checkedHotelStars.Length - 1] = HotelStarsList[i].StarsCount;
                }
            }

            return checkedHotelStars;
        }
        #endregion

        #region Date
        /// <summary>
        /// Не дает изменить дату при неправильном форматировании 
        /// </summary>
        /// <param name="sender">ссылка на textBox с датой</param>
        public static void CheckDate(object sender)
        {
            TextBox date = sender as TextBox;
            char[] charList = date.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false && charList[i].ToString() != "-" && charList[i].ToString() != ".")
                {
                    date.Text = date.Text.Remove(i, 1);
                }
            }
        }

        /// <summary>
        /// Проверка корректности введенной даты
        /// </summary>
        /// <param name="textDate">дата</param>
        /// <returns>результат проверки корректности</returns>
        public static bool isTextDateValid(string textDate)
        {
            DateTime temp;
            if (textDate.Length == 10 && DateTime.TryParse(textDate, out temp))
                return true;
            else return false;
        }

        /// <summary>
        /// Конвертирует объект DateTime в строку с необходимым форматом
        /// </summary>
        /// <param name="datetime">объект DateTime</param>
        /// <returns>дата в виде строки с необходимым форматом</returns>
        public static string ConvertDateToText(DateTime datetime)
        {
            string tempCurrentDate = datetime.ToString();
            tempCurrentDate = tempCurrentDate.Replace("0:00:00", "").Trim();
            string[] CurrentDate = tempCurrentDate.Split('.');
            tempCurrentDate = CurrentDate[0];
            CurrentDate[0] = CurrentDate[2];
            CurrentDate[2] = tempCurrentDate;
            tempCurrentDate = string.Join("-", CurrentDate);

            return tempCurrentDate;
        }
        #endregion

        #region Changed
        /// <summary>
        /// Изменяет выбранный пункт в выпадающем списке
        /// </summary>
        /// <param name="sender">выпадающий список</param>
        /// <param name="index">индекс выбранного пункта</param>
        public static void ComboBoxSelectionChanged(object sender, int index)
        {
            (sender as ComboBox).SelectedIndex = index;
        }
        #endregion
    }

    /// <summary>
    /// Функции, используемые в аналитике
    /// </summary>
    public static class Analytics
    {
        #region Tour
        /// <summary>
        /// Получить текущий топ 3 туров
        /// Формат: {"[Название тура1],[Количество поездок1]", "[Название тура2],[Количество поездок2]"....}
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTopTours()
        {
            List<string> topTours = new List<string>();

            DataTable toursData = Requests.SelectTop3Tours();

            for (int i = 0; i < toursData.Rows.Count; i++)
            {
                topTours.Add(toursData.Rows[i][0].ToString() + "," + toursData.Rows[i][1].ToString());
            }


            return topTours;
        }

        /// <summary>
        /// Получить выручку, полученную по данному туру monthAgo месяцев назад (0 - текущий месяц)
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="mongthAgo"></param>
        /// <returns></returns>
        public static int GetTourIncome(int tourId, int monthAgo)
        {
            int tourIncome = 0;

            tourIncome = Convert.ToInt32(MainFunctions.NewQuery($"SELECT ISNULL(SUM(total_price),0) FROM Trip " +
                $"WHERE id_tour = {tourId} AND " +
                $"start_date BETWEEN DATEADD(month, {-1 - monthAgo}, eomonth(getdate())) AND  DATEADD(month, {-monthAgo}, eomonth(getdate()))").Rows[0][0].ToString());

            return tourIncome;
        }
        #endregion

        #region Trip
        /// <summary>
        /// Получить количество поездок в данный момент времени и процент, показывающий динамика изменения этого показателя
        /// Формат: {[количество],[процент]}
        /// </summary>
        /// <returns></returns>
        public static List<int> GetCurrentTrips_CountPercent()
        {
            List<int> countPercent = new List<int>();

            int currentCount = MainFunctions.NewQuery($"SELECT * FROM Trip WHERE start_date <= getdate() AND end_date >= GETDATE()").Rows.Count;
            int prevCount = MainFunctions.NewQuery($"SELECT * FROM Trip WHERE start_date <= DATEADD(month,-1,getdate()) " +
                $"AND end_date >= DATEADD(month, -1, getdate())").Rows.Count;

            countPercent.Add(currentCount);

            if (prevCount == 0)
            {
                if (currentCount == 0) countPercent.Add(0);
                else countPercent.Add(100);
            }
            else
            {
                int diff = currentCount - prevCount;
                int percent = 0;

                if (diff >= 0)
                {
                    percent = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(diff) / Convert.ToDouble(prevCount) * 100)));
                }
                else
                {
                    percent = -Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(-diff) / Convert.ToDouble(prevCount) / prevCount * 100)));
                }

                countPercent.Add(percent);
            }


            return countPercent;
        }

        /// <summary>
        /// Получить выручку на текущий месяц и процент, показывающий ее динамику изменения
        /// Формат: {[выручка],[процент]}
        /// </summary>
        /// <returns></returns>
        public static List<int> GetCurrentTrips_IncomePercent()
        {
            List<int> incomePercent = new List<int>();

            int currentIncome = Requests.SelectThisMonthIncome();
            int prevIncome = Requests.SelectPreviousMonthIncome();

            incomePercent.Add(currentIncome);

            if (prevIncome == 0)
            {
                if (currentIncome == 0) incomePercent.Add(0);
                else incomePercent.Add(100);
            }
            else
            {
                int diff = currentIncome - prevIncome;
                int percent = 0;

                if (diff >= 0)
                {
                    percent = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(diff) / Convert.ToDouble(prevIncome) * 100)));
                }
                else
                {
                    percent = -Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(-diff) / Convert.ToDouble(prevIncome) * 100)));
                }

                incomePercent.Add(percent);
            }

            return incomePercent;
        }

        /// <summary>
        /// Получить количество поездок, совершенных по данному туру monthAgo месяцев назад (0 - текущий месяц)
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="mongthAgo"></param>
        /// <returns></returns>
        public static int GetTourTripsCount(int tourId, int monthAgo)
        {
            int tripsCount = 0;

            tripsCount = Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Trip " +
                $"WHERE id_tour = {tourId} AND " +
                $"start_date BETWEEN DATEADD(month, {-1 - monthAgo}, eomonth(getdate())) AND  DATEADD(month, {-monthAgo}, eomonth(getdate()))").Rows[0][0].ToString());

            return tripsCount;
        }
        #endregion

        #region Client
        /// <summary>
        /// Получить количество новых клиентов за этот месяц и процент, показывающий динамику изменения этого показателя
        /// Формат: "{[количество],[процент]}"
        /// </summary>
        /// <returns></returns>
        public static List<int> GetCurrentClients_CountPercent()
        {
            List<int> countPercent = new List<int>();

            int currentCount = MainFunctions.NewQuery($"SELECT * FROM Client " +
                $"WHERE date_creation BETWEEN(SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date)) " +
                $"AND DATEADD(month, 1, (SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date)))").Rows.Count;

            int prevCount = MainFunctions.NewQuery($"SELECT * FROM Client " +
                $"WHERE date_creation BETWEEN DATEADD(month, -1, (SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date))) " +
                $"AND(SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date))").Rows.Count;

            countPercent.Add(currentCount);

            if (prevCount == 0)
            {
                if (currentCount == 0) countPercent.Add(0);
                else countPercent.Add(100);
            }
            else
            {
                int diff = currentCount - prevCount;
                int percent = 0;

                if (diff >= 0)
                {
                    percent = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(diff) / Convert.ToDouble(prevCount) * 100)));
                }
                else
                {
                    percent = -Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(-diff) / Convert.ToDouble(prevCount) * 100)));
                }

                countPercent.Add(percent);
            }

            return countPercent;
        }

        /// <summary>
        /// Сформировать список клиентов с категориями по результатам АВС-анализа
        /// </summary>
        /// <returns></returns>
        public static List<ClientABC> GetClientABCs()
        {
            List<ClientABC> clients = new List<ClientABC>();
            DataTable clientsData = Requests.SelectClientAbcData();

            double previousPercent = 0;

            for (int i = 0; i < clientsData.Rows.Count; i++)
            {
                previousPercent += Convert.ToDouble(clientsData.Rows[i][3].ToString());
                string category = "";
                if (previousPercent < 80) category = "A";
                if (previousPercent >= 80 && previousPercent < 95) category = "B";
                if (previousPercent >= 95) category = "C";

                int clientId = Convert.ToInt32(clientsData.Rows[i][0].ToString());
                DataTable lastTripData = Requests.SelectLastClientTrip(clientId);
                string tempLastTripDates = "";
                string tempLastTripPrice = "";

                if (lastTripData.Rows[0][1].ToString() == "")
                {
                    tempLastTripDates = "нет";
                    tempLastTripPrice = "0₽";
                }
                else
                {
                    tempLastTripPrice = Convert.ToInt32(lastTripData.Rows[0][3]).ToString("### ### ###") + "₽";
                    string tempDate1 = Convert.ToDateTime(lastTripData.Rows[0][1].ToString()).ToShortDateString();
                    string tempDate2 = Convert.ToDateTime(lastTripData.Rows[0][2].ToString()).ToShortDateString();
                    tempDate1 = tempDate1.Substring(0, tempDate1.LastIndexOf(".") + 1) + tempDate1.Substring(tempDate1.Length - 2, 2);
                    tempDate2 = tempDate2.Substring(0, tempDate2.LastIndexOf(".") + 1) + tempDate2.Substring(tempDate2.Length - 2, 2);
                    tempLastTripDates = $"{tempDate1} - {tempDate2}";
                }

                ClientABC tempClient = new ClientABC()
                {
                    ClientId = clientId,
                    FullName = clientsData.Rows[i][1].ToString(),
                    TotalIncome = Convert.ToInt32(clientsData.Rows[i][2].ToString()),
                    TotalIncomeStr = clientsData.Rows[i][2].ToString() + "₽",
                    CategoryABC = category,
                    LastTripDates = tempLastTripDates,
                    LastTripPrice = tempLastTripPrice
                };
                clients.Add(tempClient);
            }

            return clients;
        }
        #endregion

        #region OtherHelpFunctions
        /// <summary>
        /// Получить название месяца, который был monthAgo месяцев назад (0 - текущий месяц)
        /// </summary>
        /// <param name="monthAgo"></param>
        /// <returns></returns>
        public static string GetMonthName(int monthAgo)
        {
            string month = DateTime.Today.AddMonths(-monthAgo).ToString("MMMM", new CultureInfo("ru-RU")) + " " +
                DateTime.Today.AddMonths(-monthAgo).ToString("yy");
            return month;
        }
        #endregion

    }

    /// <summary>
    /// Класс для запросов к БД
    /// </summary>
    public static class Requests
    {
        #region MonthIndicators
        /// <summary>
        /// Прибыль за текущий месяц
        /// </summary>
        /// <returns></returns>
        public static int SelectThisMonthIncome()
        {
            int currentIncome = Convert.ToInt32(MainFunctions.NewQuery($"SELECT ISNULL(SUM(total_price),0) FROM Trip WHERE booking_datetime BETWEEN " +
                $"(SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date)) AND " +
                $"DATEADD(month, 1, (SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date)))").Rows[0][0].ToString());

            return currentIncome;
        }
        
        /// <summary>
        /// Прибыль за предыдущий месяц
        /// </summary>
        /// <returns></returns>
        public static int SelectPreviousMonthIncome()
        {
            int prevIncome = Convert.ToInt32(MainFunctions.NewQuery($"SELECT ISNULL(SUM(total_price),0) FROM Trip WHERE booking_datetime BETWEEN " +
                $"DATEADD(month, -1, (SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date))) AND " +
                $"(SELECT cast(format(GETDATE(), 'yyyy-MM-01') as Date))").Rows[0][0].ToString());

            return prevIncome;
        }

        #endregion

        #region Tours
        /// <summary>
        /// Топ 3 популярных тура на текущий момент
        /// </summary>
        /// <returns></returns>
        public static DataTable SelectTop3Tours()
        {
            DataTable top3ToursData = MainFunctions.NewQuery($"SELECT TOP 3 " +
                $"(SELECT name FROM Tour t WHERE t.id_tour = tr.id_tour), " +
                $"COUNT(*) as tripCount, " +
                $"AVG(ABS(DAY(getdate() - tr.start_date))) as daysDistance " +
                $"FROM Trip as tr " +
                $"WHERE ABS(DAY(getdate() - tr.start_date)) <= 15 " +
                $"GROUP BY id_tour " +
                $"ORDER BY tripCount DESC, daysDistance ASC");

            return top3ToursData;
        }

        #endregion

        #region Clients
        /// <summary>
        /// Данные для АВС-анализа клиентов
        /// </summary>
        /// <returns></returns>
        public static DataTable SelectClientAbcData()
        {
            DataTable clientsData = MainFunctions.NewQuery($"SELECT c.id_client, CONCAT(c.surname, ' ', c.name, ' ', c.patronymic), " +
                $"ISNULL(SUM(t.total_price), 0) as total, ROUND(ISNULL(SUM(t.total_price), 0) / (SELECT SUM(total_price) FROM Trip) * 100, 0) " +
                $"FROM Client c LEFT JOIN Trip t ON t.id_client = c.id_client " +
                $"GROUP BY c.id_client, CONCAT(c.surname, ' ', c.name, ' ', c.patronymic) ORDER BY total DESC");

            return clientsData;
        }

        /// <summary>
        /// Данные последней поездки клиента
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static DataTable SelectLastClientTrip(int clientId)
        {
            DataTable lastTrip = MainFunctions.NewQuery($"SELECT TOP 1 c.id_client, t.start_date, t.end_date, t.total_price " +
                    $"FROM Client c LEFT JOIN Trip t ON t.id_client = c.id_client WHERE c.id_client = {clientId} ORDER BY t.start_date DESC");

            return lastTrip;
        }

        #endregion

    }

    /// <summary>
    /// Класс для чтения/записи INI-файлов
    /// </summary>
    public class INIManager
    {
        //Конструктор, принимающий путь к INI-файлу
        public INIManager(string aPath)
        {
            path = aPath;
        }

        //Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
        public INIManager() : this("") { }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aSection, string aKey)
        {
            //Для получения значения
            StringBuilder buffer = new StringBuilder(SIZE);

            //Получить значение в buffer
            GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

            //Вернуть полученное значение
            return buffer.ToString();
        }

        //Пишет значение в INI-файл (по указанным секции и ключу) 
        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
            //Записать значение в INI-файл
            WritePrivateString(aSection, aKey, aValue, path);
        }

        //Возвращает или устанавливает путь к INI файлу
        public string Path { get { return path; } set { path = value; } }

        //Поля класса
        private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
        private string path = null; //Для хранения пути к INI-файлу

        //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

        //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateString(string section, string key, string str, string path);
    }

}

