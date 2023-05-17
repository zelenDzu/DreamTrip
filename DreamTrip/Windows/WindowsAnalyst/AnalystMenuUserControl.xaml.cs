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
    /// Логика взаимодействия для AnalystMenuUserControl.xaml
    /// </summary>
    public partial class AnalystMenuUserControl : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;

        string[] thisPageParametres = new string[] { "Auto", "Меню", "../../Resources/list.png" };
        #endregion

        #region Constructor
        public AnalystMenuUserControl(TabClass tempTabItem)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion

        #region MenuButtonsClick
        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new Clients(parentTabItemLink, this, thisPageParametres);

        }

        private void btnNewTask_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new EditTasks(parentTabItemLink, this, thisPageParametres);

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

        private void btnAnalyzeClients_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new AnalyzeClients(parentTabItemLink, this, thisPageParametres);

        }

        private void btnAnalyzeTours_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new AnalyzeTrips(parentTabItemLink, this, thisPageParametres);
            //parentTabItemLink.ItemUserControl = new Statistics(parentTabItemLink, this, thisPageParametres);

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
