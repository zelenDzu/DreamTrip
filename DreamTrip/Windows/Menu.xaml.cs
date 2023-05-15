using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DreamTrip.Classes;
using DreamTrip.Functions;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        #region Variables
        TabClass tabItemPlus;
        string userType;
        string login;
        #endregion

        #region Constructor
        public Menu(string tempUserType, string tempLogin) 
        {
            InitializeComponent();
            MainFunctions.DeleteTempFolders();
            MainFunctions.ClearLogs();


            MainFunctions.ClearAccountsPhotos();
            MainFunctions.ClearServicesPhotos();

            userType = tempUserType;
            login = tempLogin;

            switch (userType)
            {
                case "manager":
                    tbRoleName.Text = "Менеджер";
                    this.Title = "Окно менеджера";
                    break;

                case "admin":
                    tbRoleName.Text = "Администратор";
                    this.Title = "Окно администратора";
                    break;

                case "analyst":
                    tbRoleName.Text = "Аналитик";
                    this.Title = "Окно аналитика";
                    break;
            }

            MainFunctions.AddLogRecord($"{login} logged in");
            MainFunctions.MenuLink = this;

            tabItemPlus = new TabClass()
            {
                Index = "0",
                ItemHeaderImageSource = "../Resources/plus.png",
                CloseButtonVisibility = "Collapsed",
            };

            menuTabControl.Items.Add(tabItemPlus);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AddNewItem();
        }
        #endregion

        #region Functions
        /// <summary>
        /// Получить ширину и высоту текущего окна
        /// </summary>
        /// <returns></returns>
        public double[] GetWidthHeight()
        {
            return new double[2] { this.ActualWidth, this.ActualHeight };
        }

        /// <summary>
        /// Добавить новую вкладку
        /// </summary>
        private void AddNewItem()
        {
            if (menuTabControl.Items.Count >= 8)
            {
                Message messageWindow = new Message("Много вкладок", "Вкладок слишком много. Закройте одну или несколько, чтобы добавить новую", false, false);
                messageWindow.ShowDialog();
                return;
            }
            else
            {

                menuTabControl.Items.Remove(tabItemPlus);
                TabClass tabItem = new TabClass()
                {
                    CloseButtonVisibility = "Visible",
                    Index = menuTabControl.Items.Count.ToString(),
                    ItemHeaderText = "Меню",
                    ItemHeaderImageSource = "../Resources/list.png",
                };


                switch (userType)
                {
                    case "manager":
                        tabItem.ItemUserControl = new ManagerMenuUserControl(tabItem);
                        break;

                    case "admin":
                        tabItem.ItemUserControl = new AdminMenuUserControl(tabItem);
                        break;

                    case "analyst":
                        tabItem.ItemUserControl = new AnalystMenuUserControl(tabItem);
                        break;
                }

                tabItemPlus.Index = (menuTabControl.Items.Count + 1).ToString();

                menuTabControl.Items.Add(tabItem);
                menuTabControl.Items.Add(tabItemPlus);
                menuTabControl.SelectedIndex = menuTabControl.Items.Count - 2;
            }
        }
        #endregion

        #region Handlers
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            MainFunctions.DeleteTempFolders();
        }

        private void TabItemExit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool popupAnswer = false;
            Message messageWindow = new Message("Выход", "Вы уверены, что хотите выйти из учетной записи?", true, true);
            messageWindow.messageAnswer += value => popupAnswer = value;
            messageWindow.ShowDialog();

            if (popupAnswer)
            {
                MainWindow mainWindow = new MainWindow();
                this.Close();
                mainWindow.Show();
            }
        }

        private void TabItemProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TabClass selectedTab = menuTabControl.SelectedItem as TabClass;
            Profile profile = new Profile(selectedTab, selectedTab.ItemUserControl, new string[] {
                selectedTab.VerticalScrollBarVisibility, 
                selectedTab.ItemHeaderText,
                selectedTab.ItemHeaderImageSource});
            selectedTab.ItemUserControl = profile;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            (menuTabControl.SelectedItem as TabClass).ScrollEvent(e.Delta);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in (sender as StackPanel).Children)
            {
                if (item.GetType().Name == "TextBlock" && (item as TextBlock).Text == "")
                {
                    AddNewItem();
                }
            }
        }

        private void ButtonCloseTab_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse((sender as Button).Tag.ToString());
            menuTabControl.Items.RemoveAt(index);
            for (int i = index; i < menuTabControl.Items.Count; i++)
            {
                (menuTabControl.Items[i] as TabClass).Index = (i).ToString();
            }
        }

        private void menuTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menuTabControl.SelectedIndex == menuTabControl.Items.Count - 1)
            {
                menuTabControl.SelectedIndex = menuTabControl.Items.Count - 2;
            }

            SizeChangedInfo sifo = new SizeChangedInfo(this, new Size(0, 0), true, true);
            SizeChangedEventArgs ea = typeof(System.Windows.SizeChangedEventArgs).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).FirstOrDefault().Invoke(new object[] { (this as FrameworkElement), sifo }) as SizeChangedEventArgs;
            ea.RoutedEvent = Panel.SizeChangedEvent;
            this.RaiseEvent(ea);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl tempUserControl;
            if (menuTabControl.SelectedIndex != -1)
            {
                tempUserControl = (menuTabControl.SelectedItem as TabClass).ItemUserControl;
                if (tempUserControl as Tours != null)
                    (tempUserControl as Tours).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as ManagerMenuUserControl != null)
                    (tempUserControl as ManagerMenuUserControl).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as AnalystMenuUserControl != null)
                    (tempUserControl as AnalystMenuUserControl).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as AdminMenuUserControl != null)
                    (tempUserControl as AdminMenuUserControl).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as TourInfo != null)
                    (tempUserControl as TourInfo).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as CreateTrip != null)
                    (tempUserControl as CreateTrip).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as EditTrip != null)
                    (tempUserControl as EditTrip).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as NewTour != null)
                    (tempUserControl as NewTour).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);

                if (tempUserControl as Statistics != null)
                    (tempUserControl as Statistics).WindowSizeChanged(e.NewSize.Width, e.NewSize.Height);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MainFunctions.DeleteTempFolders();
            MainFunctions.AddHistoryRecord(login);
            MainFunctions.ClearLogs();
        }

        #endregion


    }
}
