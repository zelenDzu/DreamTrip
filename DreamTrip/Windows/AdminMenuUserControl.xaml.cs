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
using System.Windows.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для AdminMenuUserControl.xaml
    /// </summary>
    public partial class AdminMenuUserControl : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        #endregion

        #region Constructor
        public AdminMenuUserControl(TabClass tempTabItem)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion


        #region Functions
        /// <summary>
        /// При изменении размеров окна
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void WindowSizeChanged(double width, double height)
        {

        }
        #endregion

        #region MenuButtonsClick
        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new LoginHistory(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "История входа";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/login_history.png";
        }

        private void btnNewTour_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new NewTour(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Новый тур";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/new_tour.png";
        }

        private void btnTours_Click(object sender, RoutedEventArgs e)
        {
            gridTourLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                parentTabItemLink.ItemUserControl = new Tours(parentTabItemLink, false);
                parentTabItemLink.VerticalScrollBarVisibility = "Disabled";
                parentTabItemLink.ItemHeaderText = "Туры";
                parentTabItemLink.ItemHeaderImageSource = "../Resources/tours.png";
                gridTourLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }

        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Accounts(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Аккаунты";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/accounts.png";
        }

        private void btnServices_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Services(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Сервисы";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/service.png";
        }

        private void btnCities_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Cities(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Города";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/city.png";
        }

        private void btnHotels_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Hotels(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Отели";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/hotel.png";
        }
        #endregion
    }
}
