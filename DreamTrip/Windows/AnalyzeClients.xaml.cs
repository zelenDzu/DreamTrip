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
    /// Логика взаимодействия для AnalyzeClients.xaml
    /// </summary>
    public partial class AnalyzeClients : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Аналитика клиентов", "../Resources/analyze_client.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        #endregion

        #region Constructor
        public AnalyzeClients(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadNewClientsCount();
            LoadClientsABC();
        }
        #endregion

        #region LoadData
        void LoadNewClientsCount()
        {
            List<int> countPercent = Analytics.GetCurrentClients_CountPercent();
            tbNewClientCount.Text = countPercent[0].ToString();

            int percent = countPercent[1];
            if (percent >= 0)
            {
                tbNewClientPercent.Text = percent.ToString() + "%";
                tbNewClientPercent.Foreground = plgGreen.Fill;
                plgGreen.Visibility = Visibility.Visible;
                plgRed.Visibility = Visibility.Hidden;
            }
            else
            {
                tbNewClientPercent.Text = (-percent).ToString() + "%";
                tbNewClientPercent.Foreground = plgRed.Fill;
                plgGreen.Visibility = Visibility.Hidden;
                plgRed.Visibility = Visibility.Visible;
            }
        }

        void LoadPieChart(int groupType = 1)
        {
            switch (groupType)
            {
                case 1: //пол

                    break;

                case 2: //возраст

                    break;

                case 3: //работа

                    break;
            }
        }

        void LoadClientsABC()
        {

        }
        #endregion

        #region Functions

        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        #endregion

        #region Changed

        #endregion

        private void cmbClientGroupType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //index : type
            //1 : пол
            //2 : возраст
            //3 : работа

            if (cmbClientGroupType.SelectedIndex != 0) LoadPieChart();
        }
    }
}
