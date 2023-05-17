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
using System.Windows.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChooseClient.xaml
    /// </summary>
    public partial class ChooseClient : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Выбор клиента", "../../Resources/clients.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ObservableCollection<Client> ClientsList { get; set; } = new ObservableCollection<Client>();
        Tour chosenTour;
        bool isToCreateTrip;
        #endregion

        #region Constructor
        public ChooseClient(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, Tour tempTour, bool tempIsToCreateTrip)
        {
            InitializeComponent();
            isToCreateTrip = tempIsToCreateTrip;
            chosenTour = tempTour;

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadClients("all");
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка клиентов
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
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridTourLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                parentTabItemLink.ItemUserControl = previousPage;
                MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);

                gridTourLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (tbxNameSearch.Text.Length == 0)
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
                        if (MainFunctions.ValidateString_RuEng(fullName[i]) && currentNameIndex < 3)
                        {
                            name[currentNameIndex] = fullName[i];
                            currentNameIndex++;
                        }
                        if (currentNameIndex >= 3) break;
                    }

                    if (name[0] != null && MainFunctions.ValidateString_RuEng(name[0].ToLower()))
                    {
                        condition += String.Format("WHERE (surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[0]);
                    }
                    if (name[1] != null && MainFunctions.ValidateString_RuEng(name[1].ToLower()))
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("(surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[1]);
                    }
                    if (name[2] != null && MainFunctions.ValidateString_RuEng(name[2].ToLower()))
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("(surname LIKE '%{0}%' OR name LIKE '%{0}%' OR patronymic LIKE '%{0}%')", name[2]);
                    }
                }

                DataTable dataClients = MainFunctions.NewQuery($"SELECT id_client FROM [dbo].[Client] {condition}");
                string clientsIds = "";
                for (int i = 0; i < dataClients.Rows.Count; i++)
                {
                    if (clientsIds != "") clientsIds += ",";
                    clientsIds += dataClients.Rows[i][0].ToString();
                }


                LoadClients(clientsIds);
            }


        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new CreateTrip(parentTabItemLink, this, thisPageParametres, chosenTour, dtgClients.SelectedItem as Client, isToCreateTrip);
        }
        #endregion

        #region DataGridEvents
        private void dtgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgClients.SelectedIndex != -1)
            {
                borderChooseButton.IsEnabled = true;
            }
            else
            {
                borderChooseButton.IsEnabled = false;
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

        #region Handlers
        private void tbxNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_RuEng(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta"></param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            if (Delta < 0) Delta = Delta / 40 + 2;
            else Delta = Delta / 40 - 2;

            if (dtgClients.IsMouseOver)
            {
                ScrollViewer scv = dtgClients.Template.FindName("DG_ScrollViewer", dtgClients) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
            }
        }
        #endregion

    }
}
