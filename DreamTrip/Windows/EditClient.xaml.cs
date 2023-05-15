using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для EditClient.xaml
    /// </summary>
    public partial class EditClient : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Данные клиента", "../Resources/user_profile.png" };
        UserControl previousPage;
        string[] previousPageParametres;
        bool isWorkChanged = false;

        Client currentClient;
        int currentClientId;

        ObservableCollection<WorkField> FieldsList { get; set; } = new ObservableCollection<WorkField>();
        ObservableCollection<WorkPost> CurrentPostsList { get; set; } = new ObservableCollection<WorkPost>();
        //int lastChosenField = -1;
        #endregion

        #region Constructor
        public EditClient(TabClass tempTabItem , UserControl tempPreviousPage, string[] tempPreviousPageParametres, int clientId = 0)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            currentClientId = clientId;
            if (currentClientId == 0) borderTripsButton.Visibility = Visibility.Hidden;

            LoadFields();
            LoadPosts();
            LoadClient();
            LoadAddInfo();

            DataContext = currentClient;

        }

        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка данных клиента
        /// </summary>
        private void LoadClient()
        {

            if (currentClientId != 0)
            {
                DataTable clientData = MainFunctions.NewQuery($"SELECT c.id_client, " +
                    $"c.surname, " +
                    $"c.name, " +
                    $"c.patronymic, " +
                    $"c.passport_seria, " +
                    $"c.passport_number, " +
                    $"CONVERT(date, c.birthday), " +
                    $"c.gender, " +
                    $"ISNULL(cc.phone, 'нет'), " +
                    $"ISNULL(cc.email, 'нет'), " +
                    $"ISNULL(cw.id_work_field, 0), " +
                    $"ISNULL(cw.id_work_post, 0) " +
                    $"FROM Client c " +
                    $"LEFT JOIN Client_contacts cc ON cc.id_client = c.id_client " +
                    $"LEFT JOIN Client_work_info cw ON cw.id_client = c.id_client " +
                    $"WHERE c.id_client = {currentClientId}");

                if (clientData.Rows.Count == 0)
                {
                    new Message("Ошибка", "Не удалось получить данные клиента :(").ShowDialog();
                    btnCancel_Click(btnCancel, new RoutedEventArgs());
                }

                currentClient = new Client()
                {
                    ClientId = int.Parse(clientData.Rows[0][0].ToString()),
                    Surname = clientData.Rows[0][1].ToString(),
                    Name = clientData.Rows[0][2].ToString(),
                    Patronymic = clientData.Rows[0][3].ToString(),
                    PassportSeria = clientData.Rows[0][4].ToString(),
                    PassportNumber = clientData.Rows[0][5].ToString(),
                    Birthday = Convert.ToDateTime(clientData.Rows[0][6].ToString()).ToShortDateString(),
                    GenderNum = clientData.Rows[0][7].ToString() == "М" ? 0 : 1,
                    Phone = clientData.Rows[0][8].ToString(),
                    Email = clientData.Rows[0][9].ToString(),
                    ClientWorkField = GetWorkFieldById(int.Parse(clientData.Rows[0][10].ToString())),
                };


                lstField.SelectedItem = currentClient.ClientWorkField;
                LoadPosts();
                currentClient.ClientWorkPost = GetWorkPostById(int.Parse(clientData.Rows[0][11].ToString()));
                lstPost.SelectedItem = currentClient.ClientWorkPost;


            }
            else
            {
                currentClient = new Client()
                {
                    ClientId = 0,
                    Surname = "",
                    Name = "",
                    Patronymic = "",
                    PassportSeria = "",
                    PassportNumber = "",
                    Phone = "",
                    Email = "",
                    Birthday = "",
                    GenderNum = 0,
                    ClientWorkField = FieldsList[0],
                    ClientWorkPost = CurrentPostsList[0]
                };

                lstField.SelectedItem = currentClient.ClientWorkField;
                lstPost.SelectedItem = currentClient.ClientWorkPost;


            }

            
            
        }

        /// <summary>
        /// Загрузка статистической информации
        /// </summary>
        private void LoadAddInfo()
        {
            tbCallCount.Text = MainFunctions.NewQuery($"SELECT id_task FROM Task_call WHERE id_client={currentClient.ClientId} AND id_task IN (SELECT id_task FROM Task WHERE is_done=1)").Rows.Count.ToString();
            tbTripsCount.Text = MainFunctions.NewQuery($"SELECT id_trip FROM Trip WHERE  id_client={currentClient.ClientId}").Rows.Count.ToString();
            tbTripsPrice.Text = MainFunctions.NewQuery($"SELECT ISNULL(SUM(total_price),0) FROM Trip WHERE id_client={currentClient.ClientId}").Rows[0][0].ToString() + " руб.";
            
            
            DataTable LastCallData = MainFunctions.NewQuery($"SELECT TOP 1 CONVERT(date,date_done) FROM Task WHERE (is_done = 1) AND (date_done is not NULL)  AND (id_task_type=1) AND id_task IN" +
                $" (SELECT id_task FROM Task_call WHERE id_client = {currentClient.ClientId}) ORDER BY date_done DESC");
            if (LastCallData.Rows.Count == 0) tbLastCall.Text = "нет";
            else tbLastCall.Text = Convert.ToDateTime(LastCallData.Rows[0][0].ToString()).ToShortDateString();


            DataTable LastTripData = MainFunctions.NewQuery($"SELECT TOP 1 CONVERT(date,start_date), CONVERT(date, end_date) " +
                $"FROM Trip WHERE id_client = {currentClient.ClientId} AND start_date < getdate() ORDER BY start_date DESC");
            if (LastTripData.Rows.Count == 0) tbLastTrip.Text = "нет";
            else
            {
                string startTripDate = Convert.ToDateTime(LastTripData.Rows[0][0].ToString()).ToShortDateString();
                string endTripDate = Convert.ToDateTime(LastTripData.Rows[0][1].ToString()).ToShortDateString();
                tbLastTrip.Text = startTripDate.Substring(0, 6) + startTripDate.Substring(8, 2) +
                        " - " + endTripDate.Substring(0, 6) + endTripDate.Substring(8, 2);
            }


        }

        /// <summary>
        /// Загрузка сфер деятельности
        /// </summary>
        private void LoadFields()
        {
            FieldsList.Clear();
            lstField.ItemsSource = null;

            DataTable dataTableWithoutWork = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Безработный'");
            WorkField dataWithoutWork = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableWithoutWork.Rows[0][0].ToString()),
                Name = dataTableWithoutWork.Rows[0][1].ToString()
            };

            FieldsList.Add(dataWithoutWork);

            DataTable dataTableNullWork = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Отсутствует'");
            WorkField dataNullWork = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableNullWork.Rows[0][0].ToString()),
                Name = dataTableNullWork.Rows[0][1].ToString()
            };

            FieldsList.Add(dataNullWork);


            DataTable dataFields = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name NOT IN ('Безработный','Другое','Отсутствует') ORDER BY name");
            for (int i = 0; i < dataFields.Rows.Count; i++)
            {
                WorkField dataField = new WorkField()
                {
                    WorkFieldId = int.Parse(dataFields.Rows[i][0].ToString()),
                    Name = dataFields.Rows[i][1].ToString()
                };

                FieldsList.Add(dataField);
            }

            DataTable dataTableOther = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Другое'");
            WorkField dataOther = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableOther.Rows[0][0].ToString()),
                Name = dataTableOther.Rows[0][1].ToString()
            };

            FieldsList.Add(dataOther);

            lstField.ItemsSource = FieldsList;
            lstField.SelectedIndex = 0;
        }

        /// <summary>
        /// Загрузка специальностей
        /// </summary>
        private void LoadPosts()
        {
            CurrentPostsList.Clear();
            lstPost.ItemsSource = null;

            int currentFieldId = (lstField.SelectedItem as WorkField).WorkFieldId;

            DataTable dataPosts = MainFunctions.NewQuery($"SELECT id_work_post, name FROM Work_post WHERE id_work_field={currentFieldId} ORDER BY name");

            for (int j = 0; j < dataPosts.Rows.Count; j++)
            {
                WorkPost dataPost = new WorkPost()
                {
                    WorkPostId = int.Parse(dataPosts.Rows[j][0].ToString()),
                    WorkFieldId = currentFieldId,
                    Name = dataPosts.Rows[j][1].ToString(),
                };
                CurrentPostsList.Add(dataPost);
            }

            if (CurrentPostsList.Count == 0)
                CurrentPostsList.Add(new WorkPost()
                {
                    WorkPostId = 0,
                    WorkFieldId = currentFieldId,
                    Name = "Нет"
                }) ;

            lstPost.ItemsSource = CurrentPostsList;

            if (true) lstPost.SelectedIndex = 0;
        }
        #endregion

        #region Functions
        private static int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int age = today.Year - birthDate.Year;
            if (birthDate.AddYears(age) > today)
            {
                age--;
            }
            return age;
        }

        private WorkPost GetWorkPostById(int workPostId)
        {
            WorkPost workPost = new WorkPost() { WorkPostId = 0};
            for (int i = 0; i < CurrentPostsList.Count; i++)
            {
                if (CurrentPostsList[i].WorkPostId == workPostId)
                {
                    workPost = CurrentPostsList[i];
                    break;
                }

            }
            if (workPost.WorkPostId == 0) workPost = CurrentPostsList[0];

            return workPost;
        }

        private WorkField GetWorkFieldById(int workFieldId)
        {
            WorkField workField = new WorkField() { WorkFieldId = 0};
            int noneWorkId = 0;

            for (int i = 0; i < FieldsList.Count; i++)
            {
                if (FieldsList[i].Name.ToLower() == "отсутствует") noneWorkId = i;
                if (FieldsList[i].WorkFieldId == workFieldId)
                {
                    workField = FieldsList[i];
                    break;
                }
            }
            
            
            if (workField.WorkFieldId == 0) workField = FieldsList[noneWorkId];


            return workField;
        }

        private void PageDataChanged()
        {
            bool isSaveButtonActivated = MainFunctions.validateName(tbxSurname.Text) &&
                MainFunctions.validateName(tbxName.Text) &&
                MainFunctions.validateName(tbxPatronymic.Text) &&
                CommonFunctions.isTextDateValid(tbxBirth.Text) &&
                tbxSeria.Text.Length == 4 &&
                tbxNumber.Text.Length == 6 &&
                cmbGender.SelectedIndex != -1 &&
                tbxPhone.Text.Length == 10 &&
                Regex.IsMatch(tbxEmail.Text, @"(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)") &&
                lstField.SelectedIndex != -1 &&
                lstPost.SelectedIndex != -1;

            borderSave.IsEnabled = isSaveButtonActivated;
        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string clientUpdateProcess = "Start update.";
            try
            {
                string gender = currentClient.GenderNum == 0 ? "М" : "Ж";
                string endMessage = "Данные успешно изменены";

                if (currentClient.ClientId == 0)
                {
                    MainFunctions.NewQuery($"INSERT INTO Client VALUES ('temp','temp','temp','1234','123456',getdate(),'М')");
                    currentClient.ClientId = Convert.ToInt32(MainFunctions.NewQuery($"SELECT MAX(id_client) FROM Client").Rows[0][0].ToString());

                    MainFunctions.NewQuery($"INSERT INTO Client_contacts VALUES ({currentClient.ClientId}, '9180000000','temp@mail.ru')");
                    MainFunctions.NewQuery($"INSERT INTO Client_work_info VALUES ({currentClient.ClientId}, 37,1471)");
                    endMessage = "Клиент успешно добавлен!";
                    clientUpdateProcess += "New client data inserted.";
                }

                MainFunctions.NewQuery($"UPDATE Client SET surname = '{currentClient.Surname}', name = '{currentClient.Name}', patronymic = '{currentClient.Patronymic}', " +
                    $"passport_seria = '{currentClient.PassportSeria}', passport_number = '{currentClient.PassportNumber}', birthday = '{currentClient.Birthday}', " +
                    $"gender = '{gender}' WHERE id_client = {currentClient.ClientId}");
                clientUpdateProcess += "Main data updated.";

                MainFunctions.NewQuery($"UPDATE Client_contacts SET phone = '{currentClient.Phone}', email = '{currentClient.Email}' WHERE id_client = {currentClient.ClientId}");
                clientUpdateProcess += "Contacts data updated.";


                MainFunctions.NewQuery($"DELETE FROM Client_work_info  WHERE id_client = {currentClient.ClientId}");
                MainFunctions.NewQuery($"INSERT INTO Client_work_info VALUES ({currentClient.ClientId}, " +
                    $"{(lstField.SelectedItem as WorkField).WorkFieldId}, {(lstPost.SelectedItem as WorkPost).WorkPostId})");
                clientUpdateProcess += "Work data updated.";


                new Message($"Успех", endMessage).ShowDialog();

                btnCancel_Click(sender, e);

                if (currentClientId != 0)
                {
                    MainFunctions.AddLogRecord("Client update success" +
                    $"\n\tID: {currentClient.ClientId}" +
                    $"\n\tName: {currentClient.Surname} {currentClient.Name} {currentClient.Patronymic}" +
                    $"\n\tPassport: {currentClient.PassportSeria} {currentClient.PassportNumber}");
                    if ((previousPage as Clients) != null) (previousPage as Clients).btnSearch_Click(sender, e);
                    if ((previousPage as Tasks) != null) (previousPage as Tasks).LoadTasks();
                    if ((previousPage as EditTasks) != null) (previousPage as EditTasks).LoadTasks();
                }
                else
                {
                    MainFunctions.AddLogRecord("Client create success" +
                    $"\n\tID: {currentClient.ClientId}" +
                    $"\n\tName: {currentClient.Surname} {currentClient.Name} {currentClient.Patronymic}" +
                    $"\n\tPassport: {currentClient.PassportSeria} {currentClient.PassportNumber}");
                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так...");
                string operation = currentClientId == 0 ? "Client create error" : "Client update error";
                MainFunctions.AddLogRecord(operation +
                    $"\n\tID: {currentClient.ClientId}" +
                    $"\n\tName: {currentClient.Surname} {currentClient.Name} {currentClient.Patronymic}" +
                    $"\n\tPassport: {currentClient.PassportSeria} {currentClient.PassportNumber}" +
                    $"\n\tProcess: {clientUpdateProcess} " +
                    $"\n\tError: " + ex.Message);
            }
        }

        private void btnTrips_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new ClientsTrips(parentTabItemLink, this, thisPageParametres, currentClientId.ToString(), false);
        }
        #endregion

        #region Changed
        private void tbxBirth_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxBirth.Text))
            {
                tbWrongStartDate.Visibility = Visibility.Hidden;

                int r = CalculateAge(Convert.ToDateTime(tbxBirth.Text));
                currentClient.Age = r;
                tbAge.Text = r.ToString() + " лет";
            }

            else
                tbWrongStartDate.Visibility = Visibility.Visible;

            if (tbxBirth.Text.Length > 10) tbxBirth.Text = tbxBirth.Text.Substring(0, 10);



            
            PageDataChanged();
        }

        private void tbxSeria_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            TextBox tbSeria = sender as TextBox;

            char[] charList = tbSeria.Text.Trim().ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false)
                {
                    tbSeria.Text = tbSeria.Text.Remove(i, 1);
                }
            }
            
            
            if (tbSeria.Text.Length > 4) tbSeria.Text = tbSeria.Text.Substring(0, 4);
            

            PageDataChanged();
        }

        private void tbxNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbSeria = sender as TextBox;

            char[] charList = tbSeria.Text.Trim().ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false)
                {
                    tbSeria.Text = tbSeria.Text.Remove(i, 1);
                }
            }

            if (tbSeria.Text.Length > 6) tbSeria.Text = tbSeria.Text.Substring(0, 6);

            PageDataChanged();
        }

        private void cmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageDataChanged();

        }

        private void tbxSurname_TextChanged(object sender, TextChangedEventArgs e)
        {
            PageDataChanged();
        }

        private void tbxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            PageDataChanged();

        }

        private void tbxPatronymic_TextChanged(object sender, TextChangedEventArgs e)
        {
            PageDataChanged();

        }

        private void tbxPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbPhone = sender as TextBox;

            char[] charList = tbPhone.Text.Trim().ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false)
                {
                    tbPhone.Text = tbPhone.Text.Remove(i, 1);
                }
            }

            if (tbPhone.Text.Length > 10) tbPhone.Text = tbPhone.Text.Substring(0, 10);

            PageDataChanged();
        }

        private void tbxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            PageDataChanged();

        }



        #endregion

        private void lstField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPosts();
            PageDataChanged();

        }

        private void lstField_DropDownClosed(object sender, EventArgs e)
        {
            isWorkChanged = true;
            PageDataChanged();

        }
    }
}
