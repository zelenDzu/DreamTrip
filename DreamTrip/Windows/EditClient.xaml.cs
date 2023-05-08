using DreamTrip.Classes;
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
        Client currentClient;
        UserControl previousControl;
        #endregion

        #region Constructor
        public EditClient(TabClass tempTabItem , UserControl tempPreviousControl,Client tempClient = null)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            previousControl = tempPreviousControl;
            currentClient = tempClient;
            if (currentClient == null)
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
                    WorkFieldId = int.Parse(MainFunctions.NewQuery($"SELECT id_work_field FROM Work_field WHERE name = 'Отсутствует'").Rows[0][0].ToString()),
                    WorkPostId = int.Parse(MainFunctions.NewQuery($"SELECT id_work_post FROM Work_post WHERE name = 'Отсутствует'").Rows[0][0].ToString()),
                    WorkFieldName = "Отсутствует",
                    WorkPostName = "Отсутствует",

                };
            }

            DataContext = currentClient;
        }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousControl;
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Меню";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/list.png";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTrips_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
