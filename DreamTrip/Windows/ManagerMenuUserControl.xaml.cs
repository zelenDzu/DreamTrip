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
    /// Логика взаимодействия для ManagerMenuUserControl.xaml
    /// </summary>
    public partial class ManagerMenuUserControl : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        #endregion

        #region Constructor
        public ManagerMenuUserControl(TabClass tempTabItem)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion

        #region MenuButtonsClick
        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Clients(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Клиенты";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/clients.png";
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

        private void btnNewTrip_Click(object sender, RoutedEventArgs e)
        {
            gridTourLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                parentTabItemLink.ItemUserControl = new Tours(parentTabItemLink, true);
                parentTabItemLink.VerticalScrollBarVisibility = "Disabled";
                parentTabItemLink.ItemHeaderText = "Выбор тура";
                parentTabItemLink.ItemHeaderImageSource = "../Resources/tours.png";
                gridTourLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }

        private void btnTrips_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new ClientsTrips(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Поездки клиентов";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/trips.png";
        }
        
        //удалено, переброшено к администратору
        private void btnStatistics_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Statistics(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Статистика";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/statistics.png";
        }

        //сделать
        private void btnNewClient_Click(object sender, RoutedEventArgs e)
        {

        }
        
        //сделать
        private void btnTasks_Click(object sender, RoutedEventArgs e)
        {

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

    }
}
