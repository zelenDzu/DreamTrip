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
        string[] thisPageParametres = new string[] { "Auto", "Меню", "../../Resources/list.png" };
        #endregion

        #region Constructor
        public AdminMenuUserControl(TabClass tempTabItem)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

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
            parentTabItemLink.ItemUserControl = new LoginHistory(parentTabItemLink, this, thisPageParametres);
        }

        private void btnNewTour_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                parentTabItemLink.ItemUserControl = new NewTour(parentTabItemLink, this, thisPageParametres);
            }
            catch (Exception ex)
            {
                //ЭТОТ TRY CATCH НЕ ОТРАБАТЫВАЕТ НОРМАЛЬНО И ПРОГРАММА ВЫЛЕТАЕТ ПОСЛЕ ПОКАЗА СООБЩЕНИЯ
                //САМ TRY CATCH НУЖЕН ПРИ ОШИБКАХ С ПУТЯМИ КАРТИНОК
                //ТАКОЙ БЛОК НАХОДИТСЯ В ОКНАХ AdminMenuUserControl (btnNewTour_Click), TourInfo (btnEdit_CLick), Tours (TourButton_Click)
                new Message("Ошибка", "Что-то пошло не так...").ShowDialog();
                MainFunctions.AddLogRecord("Unknown error: " + ex.Message);
                parentTabItemLink.ItemUserControl = this;
                MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);
            }
        }

        private void btnTours_Click(object sender, RoutedEventArgs e)
        {
            gridTourLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                parentTabItemLink.ItemUserControl = new Tours(parentTabItemLink, this, thisPageParametres, false);
                gridTourLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }

        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Accounts(parentTabItemLink, this, thisPageParametres);
        }

        private void btnServices_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Services(parentTabItemLink, this, thisPageParametres);
        }

        private void btnCities_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Cities(parentTabItemLink, this, thisPageParametres);
        }

        private void btnHotels_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Hotels(parentTabItemLink, this, thisPageParametres);
        }
        #endregion
    }
}
