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
    /// Логика взаимодействия для AnalystMenuUserControl.xaml
    /// </summary>
    public partial class AnalystMenuUserControl : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        #endregion

        #region Constructor
        public AnalystMenuUserControl(TabClass tempTabItem)
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

        }

        private void btnNewTask_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTours_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAnalyzeClients_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAnalyzeTours_Click(object sender, RoutedEventArgs e)
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
