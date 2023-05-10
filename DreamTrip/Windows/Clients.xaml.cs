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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DreamTrip.Classes;
using DreamTrip.Functions;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Clients.xaml
    /// </summary>
    public partial class Clients : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Клиенты", "../Resources/clients.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ComboBoxItem tempFirstItemPost;
        ComboBoxItem tempSecondItemPost;
        ComboBoxItem tempThirdItemPost;
        ObservableCollection<Client> ClientsList { get; set; } = new ObservableCollection<Client>();
        ObservableCollection<WorkField> FieldsList { get; set; } = new ObservableCollection<WorkField>();
        ObservableCollection<WorkPost> CurrentPostsList { get; set; } = new ObservableCollection<WorkPost>();

        int lastChosenField = -1;



        #endregion

        #region Constructor
        public Clients(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadClients("all");
            LoadFields();
            SecondItemCheckBoxField.IsChecked = true;
        }

        #endregion

        #region Load Data
        /// <summary>
        /// Загрузка данных клиентов
        /// </summary>
        /// <param name="clientsIds"></param>
        private void LoadClients(string clientsIds)
        {
            dtgClients.ItemsSource = null;
            ClientsList.Clear();

            tbNothingFound.Visibility = Visibility.Hidden;
            if (clientsIds == "")
            {
                tbNothingFound.Visibility = Visibility.Visible;
            }
            else
            {
                DataTable dataClients;

                if (clientsIds == "all")
                {
                    dataClients = MainFunctions.NewQuery("SELECT * FROM [dbo].[Client] ORDER BY surname, name, patronymic");
                }
                else
                {
                    dataClients = MainFunctions.NewQuery($"SELECT * FROM [dbo].[Client] WHERE id_client IN ({clientsIds}) ORDER BY surname, name, patronymic");
                }

                for (int i = 0; i < dataClients.Rows.Count; i++)
                {
                    DataTable clientContacts = MainFunctions.NewQuery($"SELECT phone, email FROM [dbo].[Client_contacts] WHERE id_client={int.Parse(dataClients.Rows[i][0].ToString())}");
                    string tempPhone = "отсутствует";
                    string tempEmail = "отсутствует";
                    if (clientContacts.Rows.Count != 0)
                    {
                        if (clientContacts.Rows[0][0].ToString() != "") tempPhone = "8" + clientContacts.Rows[0][0].ToString();
                        if (clientContacts.Rows[0][1].ToString() != "") tempEmail = clientContacts.Rows[0][1].ToString();
                    }

                    DataTable clientWorkInfo = MainFunctions.NewQuery($"SELECT cwi.id_client, cwi.id_work_field, wf.name, cwi.id_work_post, wp.name FROM Client_work_info as cwi " +
                        $"JOIN Work_field as wf ON wf.id_work_field = cwi.id_work_field " +
                        $"JOIN Work_post as wp ON wp.id_work_post = cwi.id_work_post " +
                        $"WHERE id_client = {int.Parse(dataClients.Rows[i][0].ToString())}");

                    int tempWorkFieldId = 0, tempWorkPostId = 0;
                    string tempWorkField = "отсутствует", tempWorkPost = "отсутствует";
                    if (clientWorkInfo.Rows.Count != 0)
                    {
                        tempWorkFieldId = int.Parse(clientWorkInfo.Rows[0][1].ToString());
                        tempWorkField = clientWorkInfo.Rows[0][2].ToString().ToLower();
                        tempWorkPostId = int.Parse(clientWorkInfo.Rows[0][3].ToString());
                        tempWorkPost = clientWorkInfo.Rows[0][4].ToString().ToLower();

                    }


                    Client dataClient = new Client()
                    {
                        ClientId = int.Parse(dataClients.Rows[i][0].ToString()),
                        Surname = dataClients.Rows[i][1].ToString(),
                        Name = dataClients.Rows[i][2].ToString(),
                        Patronymic = dataClients.Rows[i][3].ToString(),
                        PassportSeria = dataClients.Rows[i][4].ToString(),
                        PassportNumber = dataClients.Rows[i][5].ToString(),
                        Birthday = dataClients.Rows[i][6].ToString().Replace("0:00:00", "").Trim(),
                        Age = int.Parse(dataClients.Rows[i][8].ToString()),
                        Gender = dataClients.Rows[i][7].ToString(),
                        Phone = tempPhone,
                        Email = tempEmail,
                        WorkFieldId = tempWorkFieldId,
                        WorkFieldName = tempWorkField,
                        WorkPostId = tempWorkPostId,
                        WorkPostName = tempWorkPost,
                    };
                    ClientsList.Add(dataClient);
                }
                dtgClients.ItemsSource = ClientsList;
            }
        }

        /// <summary>
        /// Загрузка сфер деятельности
        /// </summary>
        private void LoadFields()
        {
            DataTable dataTableWithoutWork = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Безработный'");
            WorkField dataWithoutWork = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableWithoutWork.Rows[0][0].ToString()),
                Name = dataTableWithoutWork.Rows[0][1].ToString(),
                IsChecked = false,
            };
            lstField.Items.Add(dataWithoutWork);
            FieldsList.Add(dataWithoutWork);

            DataTable dataTableNullWork = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Отсутствует'");
            WorkField dataNullWork = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableNullWork.Rows[0][0].ToString()),
                Name = dataTableNullWork.Rows[0][1].ToString(),
                IsChecked = false,
            };
            lstField.Items.Add(dataNullWork);
            FieldsList.Add(dataNullWork);


            DataTable dataFields = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name NOT IN ('Безработный','Другое','Отсутствует') ORDER BY name");
            for (int i = 0; i < dataFields.Rows.Count; i++)
            {
                WorkField dataField = new WorkField()
                {
                    WorkFieldId = int.Parse(dataFields.Rows[i][0].ToString()),
                    Name = dataFields.Rows[i][1].ToString(),
                    IsChecked = false,
                };
                lstField.Items.Add(dataField);
                FieldsList.Add(dataField);
            }

            DataTable dataTableOther = MainFunctions.NewQuery("SELECT * FROM [dbo].[Work_field] WHERE name = 'Другое'");
            WorkField dataOther = new WorkField()
            {
                WorkFieldId = int.Parse(dataTableOther.Rows[0][0].ToString()),
                Name = dataTableOther.Rows[0][1].ToString(),
                IsChecked = false,
            };
            lstField.Items.Add(dataOther);
            FieldsList.Add(dataOther);
        }

        /// <summary>
        /// Загрузка специальностей
        /// </summary>
        private void LoadPosts()
        {
            int[] checkedFields = CommonFunctions.CheckedFields(FieldsList);
            int fieldListId = -1;
            if (checkedFields.Length == 1) fieldListId = checkedFields[0];
            if (fieldListId != lastChosenField)
            {
                SecondItemCheckBoxPost.IsChecked = false;

                lastChosenField = fieldListId;
                tempFirstItemPost = lstPost.Items[0] as ComboBoxItem;
                tempSecondItemPost = lstPost.Items[1] as ComboBoxItem;
                tempThirdItemPost = lstPost.Items[2] as ComboBoxItem;

                lstPost.Items.Clear();

                lstPost.Items.Add(tempFirstItemPost);
                lstPost.Items.Add(tempSecondItemPost);
                lstPost.Items.Add(tempThirdItemPost);

                lstPost.SelectedIndex = 0;

                CurrentPostsList.Clear();

                if (fieldListId == -1)
                {
                    (lstPost.Items[1] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (lstPost.Items[1] as ComboBoxItem).IsEnabled = false;
                    (lstPost.Items[2] as ComboBoxItem).Visibility = Visibility.Visible;
                    (lstPost.Items[2] as ComboBoxItem).IsEnabled = true;

                }
                else
                {
                    (lstPost.Items[2] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (lstPost.Items[2] as ComboBoxItem).IsEnabled = false;
                    (lstPost.Items[1] as ComboBoxItem).Visibility = Visibility.Visible;
                    (lstPost.Items[1] as ComboBoxItem).IsEnabled = true;

                    DataTable dataPosts = MainFunctions.NewQuery($"SELECT id_work_post, name FROM [dbo].[Work_post] WHERE id_work_field={fieldListId} ORDER BY name");

                    for (int j = 0; j < dataPosts.Rows.Count; j++)
                    {
                        WorkPost dataPost = new WorkPost()
                        {
                            WorkPostId = int.Parse(dataPosts.Rows[j][0].ToString()),
                            WorkFieldId = fieldListId,
                            Name = dataPosts.Rows[j][1].ToString(),
                            IsChecked = false,
                        };
                        lstPost.Items.Add(dataPost);
                        CurrentPostsList.Add(dataPost);
                    }

                    SecondItemCheckBoxPost.IsChecked = true;
                }
            }
        }

        #endregion

        #region Functions
        /// <summary>
        /// При изменении любого элемента фильтра
        /// </summary>
        private void FilterChanged()
        {
            if (tbxNameSearch != null && tbxAgeFrom != null && tbxAgeTo != null
                && cmbGender != null && SecondItemCheckBoxField != null && borderClear != null)
            {
                if (tbxNameSearch.Text.Length == 0
                    && tbxAgeFrom.Text.Length == 0
                    && tbxAgeTo.Text.Length == 0
                    && (cmbGender.SelectedIndex == 0 || cmbGender.SelectedIndex == 1)
                    && SecondItemCheckBoxField.IsChecked == true)
                {
                    borderClear.Visibility = Visibility.Hidden;
                }
                else
                {
                    borderClear.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta">значение скролла</param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            if (Delta < 0) Delta = Delta / 40 + 2;
            else Delta = Delta / 40 - 2;


            if (lstField.IsDropDownOpen && lstField.IsMouseOver)
            {

                ScrollViewer scv = lstField.Template.FindName("cmbScroll", lstField) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - Delta);
            }

            if (lstPost.IsDropDownOpen && lstPost.IsMouseOver)
            {
                ScrollViewer scv = lstPost.Template.FindName("cmbScroll", lstPost) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - Delta);
            }

            if (dtgClients.IsMouseOver)
            {
                ScrollViewer scv = dtgClients.Template.FindName("DG_ScrollViewer", dtgClients) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
            }


        }
        #endregion

        #region Fields List
        private void lstField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstField.SelectedIndex = 0;
        }

        private void lstField_DropDownOpened(object sender, EventArgs e)
        {
            lstField.SelectedIndex = 1;
        }

        private void CheckBoxAllFields_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < lstField.Items.Count; i++)
            {
                (lstField.Items[i] as WorkField).IsChecked = true;
            }
        }

        private void CheckBoxAllFields_UnChecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllFieldsCheck = true;

            for (int i = 0; i < FieldsList.Count; i++)
            {
                flag_AllFieldsCheck = FieldsList[i].IsChecked && flag_AllFieldsCheck;
            }


            if (flag_AllFieldsCheck)
            {
                for (int i = 2; i < lstField.Items.Count; i++)
                {
                    (lstField.Items[i] as WorkField).IsChecked = false;
                }
            }
        }

        private void CheckBoxField_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxField.IsChecked = false;
        }

        private void CheckBoxField_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxField.IsChecked == false)
            {
                bool flag_AllFieldsCheck = true;

                for (int i = 0; i < FieldsList.Count; i++)
                {
                    flag_AllFieldsCheck = FieldsList[i].IsChecked && flag_AllFieldsCheck;
                }

                if (flag_AllFieldsCheck) SecondItemCheckBoxField.IsChecked = true;
            }


        }

        private void lstField_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        #endregion

        #region Posts List
        private void lstPost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstPost.SelectedIndex = 0;
        }

        private void lstPost_DropDownOpened(object sender, EventArgs e)
        {
            lstPost.SelectedIndex = 1;
            LoadPosts();
        }

        private void CheckBoxAllPosts_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 3; i < lstPost.Items.Count; i++)
            {
                (lstPost.Items[i] as WorkPost).IsChecked = true;
            }
        }

        private void CheckBoxAllPosts_UnChecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllPostsCheck = true;

            for (int i = 0; i < CurrentPostsList.Count; i++)
            {
                flag_AllPostsCheck = CurrentPostsList[i].IsChecked && flag_AllPostsCheck;
            }


            if (flag_AllPostsCheck)
            {
                for (int i = 3; i < lstPost.Items.Count; i++)
                {
                    (lstPost.Items[i] as WorkPost).IsChecked = false;
                }
            }
        }

        private void CheckBoxPost_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxPost.IsChecked == false)
            {
                bool flag_AllPostsCheck = true;

                for (int i = 0; i < CurrentPostsList.Count; i++)
                {
                    flag_AllPostsCheck = CurrentPostsList[i].IsChecked && flag_AllPostsCheck;
                }

                if (flag_AllPostsCheck) SecondItemCheckBoxPost.IsChecked = true;
            }

        }

        private void CheckBoxPost_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxPost.IsChecked = false;
        }

        private void lstPost_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        #endregion

        #region ButtonsClick
        private void btnTrips_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new ClientsTrips(parentTabItemLink, this, thisPageParametres, (dtgClients.SelectedItem as Client).ClientId.ToString(), true);

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);



        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbxNameSearch.Text = "";
            tbxAgeFrom.Text = "";
            tbxAgeTo.Text = "";
            cmbGender.SelectedIndex = 0;
            SecondItemCheckBoxField.IsChecked = true;
            LoadPosts();

            borderClear.Visibility = Visibility.Hidden;

        }

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadPosts();
            if (borderClear.Visibility == Visibility.Hidden)
            {
                LoadClients("all");
            }
            else
            {
                string condition = "";

                if (tbxNameSearch.Text != "")
                {
                    string[] fullName = tbxNameSearch.Text.Split(' ');
                    string[] name = new string[3];
                    int currentNameIndex = 0;
                    for (int i = 0; i < fullName.Length; i++)
                    {
                        fullName[i] = fullName[i].Trim();
                        if (MainFunctions.validateName(fullName[i]) && currentNameIndex < 3)
                        {
                            name[currentNameIndex] = fullName[i];
                            currentNameIndex++;
                        }
                        if (currentNameIndex >= 3) break;
                    }

                    if (name[0] != null && MainFunctions.validateName(name[0].ToLower()))
                    {
                        condition += String.Format("WHERE (surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[0]);
                    }
                    if (name[1] != null && MainFunctions.validateName(name[1].ToLower()))
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("(surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[1]);
                    }
                    if (name[2] != null && MainFunctions.validateName(name[2].ToLower()))
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("(surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[2]);
                    }
                }

                if (cmbGender.SelectedIndex == 2 || cmbGender.SelectedIndex == 3)
                {
                    if (condition == "") condition += "WHERE ";
                    else condition += " AND ";

                    condition += String.Format("gender='{0}'", (cmbGender.SelectedItem as ComboBoxItem).Content.ToString().ToCharArray()[0].ToString());
                }

                string genderName = "";
                switch (cmbGender.SelectedIndex)
                {
                    case 2:
                        genderName = "мужской";
                        break;
                    case 3:
                        genderName = "женский";
                        break;
                    default:
                        genderName = "любой";
                        break;
                }


                if (tbxAgeFrom.Text != "")
                {
                    if (condition == "") condition += "WHERE ";
                    else condition += " AND ";

                    condition += String.Format("age>={0}", tbxAgeFrom.Text);
                }

                if (tbxAgeTo.Text != "")
                {
                    if (condition == "") condition += "WHERE ";
                    else condition += " AND ";

                    condition += String.Format("age<={0}", tbxAgeTo.Text);
                }

                if (SecondItemCheckBoxField.IsChecked == false)
                {
                    int[] checkedFields = CommonFunctions.CheckedFields(FieldsList);
                    if (checkedFields.Length > 0)
                    {
                        string checkedFieldsIds = "";
                        for (int i = 0; i < checkedFields.Length; i++)
                        {
                            if (checkedFieldsIds != "") checkedFieldsIds += ",";
                            checkedFieldsIds += checkedFields[i].ToString();
                        }

                        if (checkedFieldsIds != "")
                        {
                            if (condition == "") condition += "WHERE ";
                            else condition += " AND ";

                            condition += String.Format("id_client IN (SELECT id_client FROM [dbo].[Client_work_info] WHERE id_work_field IN ({0}))", checkedFieldsIds);
                        }
                    }
                    else
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += "id_client IN (SELECT id_client FROM [dbo].[Client_work_info] WHERE id_work_field IS NULL OR id_work_field = 37)";
                    }
                }



                if (SecondItemPost.Visibility == Visibility.Visible && CurrentPostsList.Count > 0)
                {
                    string checkedPosts = "";
                    for (int i = 0; i < CurrentPostsList.Count; i++)
                    {
                        if (CurrentPostsList[i].IsChecked)
                        {
                            if (checkedPosts != "") checkedPosts += ",";
                            checkedPosts += CurrentPostsList[i].WorkPostId.ToString();
                        }
                    }

                    if (checkedPosts != "")
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("id_client IN (SELECT id_client FROM [dbo].[Client_work_info] WHERE id_work_post IN ({0}", checkedPosts);
                        if (SecondItemCheckBoxPost.IsChecked == true) condition += ", 1471) OR id_work_post IS NULL)";
                        else condition += "))";
                    }
                    else
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("id_client IN (SELECT id_client FROM [dbo].[Client_work_info] WHERE id_work_field = {0} AND (id_work_post IS NULL OR id_work_post = 1471))", CurrentPostsList[0].WorkFieldId.ToString());
                    }
                }

                DataTable dataClients = MainFunctions.NewQuery($"SELECT id_client FROM [dbo].[Client] {condition}");
                string clientsIds = "";
                for (int i = 0;i < dataClients.Rows.Count; i++)
                {
                    if (clientsIds != "") clientsIds += ",";
                    clientsIds += dataClients.Rows[i][0].ToString();
                }

                MainFunctions.AddLogRecord($"Clients search with condition: " +
                    $"\n\tName: {tbxNameSearch.Text}" +
                    $"\n\tGender: {genderName}" +
                    $"\n\tAge from _{tbxAgeFrom.Text} to _{tbxAgeTo.Text}");
                LoadClients(clientsIds);
            }



        }
        
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new EditClient(parentTabItemLink, this, thisPageParametres, (dtgClients.SelectedItem as Client).ClientId);

        }
        #endregion

        #region FilterItemsChangedEvents
        private void tbxNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.validateName(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }
            FilterChanged();
        }

        private void tbxAgeFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] charList = tbxAgeFrom.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false)
                {
                    tbxAgeFrom.Text = tbxAgeFrom.Text.Remove(i, 1);
                }
            }
            if (tbxAgeFrom.Text.Length > 4) tbxAgeFrom.Text = tbxAgeFrom.Text.Substring(0, tbxAgeFrom.Text.Length - 1);

            FilterChanged();
        }

        private void tbxAgeTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] charList = tbxAgeTo.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false)
                {
                    tbxAgeTo.Text = tbxAgeTo.Text.Remove(i, 1);
                }
            }
            if (tbxAgeTo.Text.Length > 4) tbxAgeTo.Text = tbxAgeTo.Text.Substring(0, tbxAgeTo.Text.Length - 1);

            FilterChanged();
        }

        private void cmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterChanged();
        }

        #endregion

        #region DataGridEvents
        private void dtgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgClients.SelectedIndex!=-1)
            {
                borderTripsButton.IsEnabled = true;
                borderEdit.IsEnabled = true;
                borderDeleteButton.IsEnabled = true;
            }
            else
            {
                borderEdit.IsEnabled = false;
                borderTripsButton.IsEnabled = false;
                borderDeleteButton.IsEnabled = false;
            }
        }

        private void ShowHideDetails(object sender, RoutedEventArgs e)
        {
            Button tempBtn = sender as Button;
            if (tempBtn.Content.ToString() == "+") tempBtn.Content = "-";
            else tempBtn.Content = "+";

            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                    row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Client currentClient = dtgClients.SelectedItem as Client;
            int clientId = currentClient.ClientId;

            bool popupAnswer = false;    
            int currentTripsCount = Convert.ToInt32(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Trip WHERE id_client={clientId} AND end_date>=GETDATE()").Rows[0][0].ToString());
            string addMessage = "";
            if (currentTripsCount > 0) addMessage = $" Он имеет {currentTripsCount} неоконченных поездок.";
            
            Message messageDeleteWindow = new Message("Удаление", $"Вы точно хотите удалить даннного клиента?{addMessage}", true, true);
            messageDeleteWindow.messageAnswer += value => popupAnswer = value;
            messageDeleteWindow.ShowDialog();

            if (popupAnswer)
            {
                string clientDeletionProcess = "Start deletion. ";
                try
                {

                    DataTable clientTaskData = MainFunctions.NewQuery($"SELECT id_task FROM Task_call WHERE id_client = {clientId} ");
                    for (int i = 0; i < clientTaskData.Rows.Count; i++)
                    {
                        MainFunctions.NewQuery($"DELETE FROM Task_call WHERE id_task = {Convert.ToInt32(clientTaskData.Rows[i][0].ToString())}");
                        MainFunctions.NewQuery($"DELETE FROM Task WHERE id_task = {Convert.ToInt32(clientTaskData.Rows[i][0].ToString())}");
                    }
                    clientDeletionProcess += "Tasks deleted. ";


                    DataTable clientTripData = MainFunctions.NewQuery($"SELECT id_trip FROM Trip WHERE id_client = {clientId}");
                    for (int i = 0; i < clientTripData.Rows.Count; i++)
                    {
                        MainFunctions.NewQuery($"DELETE FROM Trip_docs WHERE id_trip = {Convert.ToInt32(clientTripData.Rows[i][0].ToString())}");
                        MainFunctions.NewQuery($"DELETE FROM Trip_feedback WHERE id_trip = {Convert.ToInt32(clientTripData.Rows[i][0].ToString())}");
                        MainFunctions.NewQuery($"DELETE FROM Trip_services WHERE id_trip = {Convert.ToInt32(clientTripData.Rows[i][0].ToString())}");
                        MainFunctions.NewQuery($"DELETE FROM Trip_status WHERE id_trip = {Convert.ToInt32(clientTripData.Rows[i][0].ToString())}");
                        MainFunctions.NewQuery($"DELETE FROM Trip WHERE id_trip = {Convert.ToInt32(clientTripData.Rows[i][0].ToString())}");
                    }
                    clientDeletionProcess += "Trips deleted. ";


                    MainFunctions.NewQuery($"DELETE FROM Client_work_info WHERE id_client = {clientId}");
                    MainFunctions.NewQuery($"DELETE FROM Client_login WHERE id_client = {clientId}");
                    MainFunctions.NewQuery($"DELETE FROM Client_favourites WHERE id_client = {clientId}");
                    MainFunctions.NewQuery($"DELETE FROM Client_contacts WHERE id_client = {clientId}");
                    clientDeletionProcess += "Add info deleted. ";


                    MainFunctions.NewQuery($"DELETE FROM Client WHERE id_client = {clientId}");
                    clientDeletionProcess += "Client deleted.";
                    clientDeletionProcess += "End deletion. ";



                    new Message("Успех", "Клиент успешно удален!").ShowDialog();
                    MainFunctions.AddLogRecord("Client delete success" +
                        $"\n\tID: {currentClient.ClientId}" +
                        $"\n\tName: {currentClient.Surname} {currentClient.Name} {currentClient.Patronymic}" +
                        $"\n\tPassport: {currentClient.PassportSeria} {currentClient.PassportNumber}");


                    btnSearch_Click(sender, e);
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так...");
                    MainFunctions.AddLogRecord("Client delete error." +
                        $"\n\tID: {currentClient.ClientId}" +
                        $"\n\tName: {currentClient.Surname} {currentClient.Name} {currentClient.Patronymic}" +
                        $"\n\tPassport: {currentClient.PassportSeria} {currentClient.PassportNumber}" +
                        $"\n\tProcess: {clientDeletionProcess} " +
                        $"\n\tError: " + ex.Message);
                }

            }

        }
    }
}
