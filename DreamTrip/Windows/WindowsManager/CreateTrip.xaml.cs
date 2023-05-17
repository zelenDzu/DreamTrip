using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для CreateTrip.xaml
    /// </summary>
    public partial class CreateTrip : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Disabled", "Создание поездки", "../../Resources/new_trip.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        Tour currentTour;
        Client chosenClient;
        bool isToCreateTrip;

        ObservableCollection<TourHotelRoom> tourRoomsList = new ObservableCollection<TourHotelRoom>();
        ObservableCollection<FeedType> tourFeedsList = new ObservableCollection<FeedType>();
        ObservableCollection<TourService> tourServiceList = new ObservableCollection<TourService>();
        #endregion

        #region Constructor
        public CreateTrip(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, Tour tempTour, Client tempClient, bool tempIsToCreateTrip)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);


            isToCreateTrip = tempIsToCreateTrip;
            currentTour = tempTour;
            chosenClient = tempClient;
            LoadDates();
            LoadServices();
            LoadFeedTypes();
            LoadRoomTypes();
            LoadTripInfo();
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка основной информации о поездке
        /// </summary>
        private void LoadTripInfo()
        {
            tbClientName.Text = $"Клиент: {chosenClient.Surname} {chosenClient.Name} {chosenClient.Patronymic}";
            tbTourInfo.Text = $"Тур: {currentTour.Name} - {currentTour.Location}";
        }

        /// <summary>
        /// Загрузка сервисов
        /// </summary>
        private void LoadServices()
        {
            DataTable serviceData = MainFunctions.NewQuery($"SELECT * FROM Tour_services" +
               $" JOIN [Service] ON [Service].id_service = Tour_services.id_service " +
               $" WHERE id_tour = {currentTour.TourId}");

            for (int i = 0; i < serviceData.Rows.Count; i++)
            {
                TourService tourService = new TourService()
                {
                    ServiceId = int.Parse(serviceData.Rows[i][1].ToString()),
                    ServiceName = serviceData.Rows[i][3].ToString(),
                    Price = int.Parse(serviceData.Rows[i][4].ToString()),
                    PerDay = Convert.ToBoolean(serviceData.Rows[i][5].ToString()),
                    IsChecked = false
                };

                tourServiceList.Add(tourService);
                lstService.Items.Add(tourService);
            }
        }

        /// <summary>
        /// Загрузка типов тура
        /// </summary>
        private void LoadFeedTypes()
        {
            DataTable feedData = MainFunctions.NewQuery($"SELECT * FROM Hotel_feed_types " +
                $" JOIN Feed_type ON Feed_type.id_feed_type = Hotel_feed_types.id_feed_type" +
                $" WHERE id_hotel = {currentTour.HotelId}");

            for (int i = 0; i < feedData.Rows.Count; i++)
            {
                FeedType tourFeedType = new FeedType()
                {
                    FeedTypeId = int.Parse(feedData.Rows[i][1].ToString()),
                    FeedTypeName = feedData.Rows[i][4].ToString(),
                    PricePerDay = int.Parse(feedData.Rows[i][2].ToString()),
                };

                tourFeedsList.Add(tourFeedType);
                cmbFeed.Items.Add(tourFeedType);
            }
        }

        /// <summary>
        /// Загрузка комнат отеля
        /// </summary>
        private void LoadRoomTypes()
        {
            DataTable RoomsData = MainFunctions.NewQuery($"SELECT * FROM Hotel_rooms " +
                $" JOIN Room_type ON Room_type.id_room_type = Hotel_rooms.id_room_type" +
                $" WHERE id_hotel = {currentTour.HotelId}");

            for (int i = 0; i < RoomsData.Rows.Count; i++)
            {
                TourHotelRoom tourHotelRoom = new TourHotelRoom()
                {
                    RoomTypeId = int.Parse(RoomsData.Rows[i][1].ToString()),
                    RoomName = RoomsData.Rows[i][5].ToString(),
                    PricePerDay = int.Parse(RoomsData.Rows[i][2].ToString()),
                    FreeRoomsAmount = int.Parse(RoomsData.Rows[i][3].ToString())
                };
                tourRoomsList.Add(tourHotelRoom);
                cmbRoom.Items.Add(tourHotelRoom);
            }
        }

        /// <summary>
        /// Загрузка дат
        /// </summary>
        private void LoadDates()
        {
            tbxStartDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today.AddDays(1));

            tbxEndDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today.AddDays(8));

            tbxDays.Text = "7";
        }
        #endregion

        #region Functions
        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void TourOptionsChanged()
        {
            if (tbxStartDate != null && tbxEndDate != null && tbxDays != null)
            {

                if (cmbRoom.SelectedIndex != 0 && cmbFeed.SelectedIndex != 0
                && CommonFunctions.isTextDateValid(tbxStartDate.Text) && CommonFunctions.isTextDateValid(tbxEndDate.Text) && tbxDays.Text.Length > 0)
                    borderCreateButton.IsEnabled = true;
                else
                    borderCreateButton.IsEnabled = false;


                int totalPrice = CalculateTotalPrice();

                tbTripPrice.Text = $"{totalPrice.ToString("### ### ###")} руб.";
            }
        }

        /// <summary>
        /// Вычислить итоговую стоимость поездки
        /// </summary>
        /// <returns></returns>
        private int CalculateTotalPrice()
        {
            int totalPrice = 0;
            totalPrice += currentTour.TicketPrice;

            for (int i = 0; i < tourServiceList.Count; i++)
            {
                if (tourServiceList[i].IsChecked)
                {
                    int daysCount = 1;
                    if (tourServiceList[i].PerDay && tbxDays.Text.Length > 0)
                        daysCount = int.Parse(tbxDays.Text);

                    totalPrice += tourServiceList[i].Price * daysCount;
                }
            }

            if (tbxDays.Text.Length > 0)
            {
                if (cmbRoom.SelectedIndex != 0)
                {
                    totalPrice += (cmbRoom.SelectedItem as TourHotelRoom).PricePerDay * int.Parse(tbxDays.Text);
                }

                if (cmbFeed.SelectedIndex != 0)
                {
                    totalPrice += (cmbFeed.SelectedItem as FeedType).PricePerDay * int.Parse(tbxDays.Text);
                }
            }

            return totalPrice;
        }

        /// <summary>
        /// При изменении размеров окна
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void WindowSizeChanged(double width, double height)
        {

            if (width > 1300)
            {
                this.Width = 1300;
                mainGrid.Width = 900;
                wrapPanelOne.Orientation = Orientation.Horizontal;
                wrapPanelOne.Width = 900;
            }
            else
            {
                this.Width = width - 20;
                wrapPanelOne.Orientation = Orientation.Horizontal;
                wrapPanelOne.Width = 900;

                if (this.Width <= 920)
                {
                    mainGrid.Width = this.Width - 40;

                    if (mainGrid.Width <= 880)
                    {
                        wrapPanelOne.Orientation = Orientation.Vertical;
                        wrapPanelOne.Width = 440;
                    }
                }

            }

        }

        /// <summary>
        /// Проверка корректности введенных дат
        /// </summary>
        /// <param name="sender"></param>
        private void CheckDays(object sender)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false)
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }
        }

        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta"></param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            int lstDelta;

            if (Delta < 0) lstDelta = Delta / 60;
            else lstDelta = Delta / 60;

            if (cmbRoom.IsDropDownOpen && cmbRoom.IsMouseOver)
            {

                ScrollViewer scv = cmbRoom.Template.FindName("cmbScroll", cmbRoom) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (cmbFeed.IsDropDownOpen && cmbFeed.IsMouseOver)
            {

                ScrollViewer scv = cmbFeed.Template.FindName("cmbScroll", cmbFeed) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (lstService.IsDropDownOpen && lstService.IsMouseOver)
            {

                ScrollViewer scv = lstService.Template.FindName("cmbScroll", lstService) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            scvMain.ScrollToVerticalOffset(scvMain.VerticalOffset - Delta);

        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDateTime(tbxStartDate.Text) <= DateTime.Now.Date)
            {
                Message messageWindow = new Message("Ошибка", "Дата начала должна быть больше текущей даты.", false, false);
                messageWindow.ShowDialog();
            }
            else
            {
                string startDate = tbxStartDate.Text.Remove(4, 1).Remove(6, 1);
                string endDate = tbxEndDate.Text.Remove(4, 1).Remove(6, 1);

                string insertTripString = $"INSERT INTO Trip VALUES ({chosenClient.ClientId},{currentTour.TourId}, " +
                    $"{(cmbRoom.SelectedItem as TourHotelRoom).RoomTypeId}, {(cmbFeed.SelectedItem as FeedType).FeedTypeId}, " +
                    $"'{startDate}', '{endDate}', {CalculateTotalPrice()}, 2, '{DateTime.Now}')";

                MainFunctions.NewQuery(insertTripString);

                DataTable tempDate = MainFunctions.NewQuery("SELECT MAX(id_trip) FROM Trip");
                int currentTripId = int.Parse(tempDate.Rows[0][0].ToString());

                MainFunctions.NewQuery($"INSERT INTO Trip_docs (id_trip) VALUES ({currentTripId})");

                for (int i = 0; i < tourServiceList.Count; i++)
                {
                    if (tourServiceList[i].IsChecked)
                    {
                        MainFunctions.NewQuery($"INSERT INTO Trip_services VALUES ({currentTripId}, {tourServiceList[i].ServiceId})");
                    }
                }

                MainFunctions.AddLogRecord($"Trip created:" +
                    $"\n\tTrip ID: {currentTripId}" +
                    $"\n\tTour ID: {currentTour.TourId}" +
                    $"\n\tTour Name: {currentTour.Name}" +
                    $"\n\tClient ID: {chosenClient.ClientId}" +
                    $"\n\tClient Name: {chosenClient.Surname} {chosenClient.Name} {chosenClient.Patronymic}");

                Message endMessage = new Message("Успех", "Новая поездка была успешно создана!", false, false);
                endMessage.ShowDialog();


                parentTabItemLink.ItemUserControl = new ManagerMenuUserControl(parentTabItemLink);
                //parentTabItemLink.ItemUserControl = previousPage;
                //MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);

            }
        }
        #endregion

        #region TextChanged
        private void tbxDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDays(sender);

            if (tbxDays.Text.Length > 0 && CommonFunctions.isTextDateValid(tbxStartDate.Text))
                if (int.Parse(tbxDays.Text) != 0)
                    tbxEndDate.Text = CommonFunctions.ConvertDateToText(Convert.ToDateTime(tbxStartDate.Text).AddDays(int.Parse(tbxDays.Text)));
                else
                    tbxDays.Text = "";

            TourOptionsChanged();
        }

        private void tbxEndDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxEndDate.Text))
            {
                tbWrongEndDate.Visibility = Visibility.Hidden;

                if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
                {
                    if (Convert.ToDateTime(tbxEndDate.Text) >= Convert.ToDateTime(tbxStartDate.Text))
                        tbxDays.Text = (Convert.ToDateTime(tbxEndDate.Text) - Convert.ToDateTime(tbxStartDate.Text)).TotalDays.ToString();
                    else
                        tbWrongEndDate.Visibility = Visibility.Visible;
                }
            }
            else
                tbWrongEndDate.Visibility = Visibility.Visible;


            TourOptionsChanged();
        }

        private void tbxStartDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
            {
                tbWrongStartDate.Visibility = Visibility.Hidden;

                int days = 0;
                if (tbxDays.Text != "") days = int.Parse(tbxDays.Text);

                tbxEndDate.Text = CommonFunctions.ConvertDateToText(Convert.ToDateTime(tbxStartDate.Text).AddDays(days));
            }
            else
                tbWrongStartDate.Visibility = Visibility.Visible;

            TourOptionsChanged();
        }
        #endregion

        #region cmbRoom
        private void cmbRoom_DropDownOpened(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                ScrollViewer scv = cmbRoom.Template.FindName("cmbScroll", cmbRoom) as ScrollViewer;
                scv.ScrollToTop();
                ((DispatcherTimer)c).Stop();
            });
        }

        private void cmbRoom_DropDownClosed(object sender, EventArgs e)
        {
            TourOptionsChanged();
        }
        #endregion

        #region cmbFeed
        private void cmbFeed_DropDownOpened(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                ScrollViewer scv = cmbFeed.Template.FindName("cmbScroll", cmbFeed) as ScrollViewer;
                scv.ScrollToTop();
                ((DispatcherTimer)c).Stop();
            });
        }

        private void cmbFeed_DropDownClosed(object sender, EventArgs e)
        {
            TourOptionsChanged();
        }
        #endregion

        #region lstService
        private void lstService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void lstService_DropDownOpened(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {
                ScrollViewer scv = lstService.Template.FindName("cmbScroll", lstService) as ScrollViewer;
                scv.ScrollToTop();
                ((DispatcherTimer)c).Stop();
            });
        }

        private void lstService_DropDownClosed(object sender, EventArgs e)
        {
            TourOptionsChanged();
        }
        #endregion

    }
}
