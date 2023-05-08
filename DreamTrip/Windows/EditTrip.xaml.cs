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
using System.Drawing;
using System.Windows.Threading;
using System.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditTrip.xaml
    /// </summary>
    public partial class EditTrip : UserControl
    {
        #region Variables
        Tour currentTour;
        TabClass parentTabItemLink;
        Client chosenClient;
        Trip currentTrip;

        ObservableCollection<TourHotelRoom> tourRoomsList = new ObservableCollection<TourHotelRoom>();
        ObservableCollection<FeedType> tourFeedsList = new ObservableCollection<FeedType>();
        ObservableCollection<TourService> tourServiceList = new ObservableCollection<TourService>();
        #endregion

        #region Constructor
        public EditTrip(TabClass tempTabItem, Tour tempTour, Client tempClient, Trip tempTrip)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            currentTour = tempTour;
            chosenClient = tempClient;
            currentTrip = tempTrip;
            LoadDates();
            LoadServices();
            LoadFeedTypes();
            LoadRoomTypes();
            LoadTripInfo();
            CheckDocsAreUploaded();
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка информации о поездке
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
                int tempCurrentServiceId = int.Parse(serviceData.Rows[i][1].ToString());
                bool tempIsChecked = false;
                for (int j = 0;j < currentTrip.ServicesIds.Length; j++)
                {
                    tempIsChecked = tempCurrentServiceId == currentTrip.ServicesIds[j];
                    if (tempIsChecked) break;
                }

                TourService tourService = new TourService()
                {
                    ServiceId = tempCurrentServiceId,
                    ServiceName = serviceData.Rows[i][3].ToString(),
                    Price = int.Parse(serviceData.Rows[i][4].ToString()),
                    PerDay = Convert.ToBoolean(serviceData.Rows[i][5].ToString()),
                    IsChecked = tempIsChecked
                };

                tourServiceList.Add(tourService);
                lstService.Items.Add(tourService);
            }
        }

        /// <summary>
        /// Загрузка типов питания
        /// </summary>
        private void LoadFeedTypes()
        {
            DataTable feedData = MainFunctions.NewQuery($"SELECT * FROM Hotel_feed_types " +
                $" JOIN Feed_type ON Feed_type.id_feed_type = Hotel_feed_types.id_feed_type" +
                $" WHERE id_hotel = {currentTour.HotelId}");

            int selectedIndex = 0;

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

                if (tourFeedType.FeedTypeId == currentTrip.FeedTypeId)
                    selectedIndex = i + 1;
            }

            cmbFeed.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Загрузка комнат отеля
        /// </summary>
        private void LoadRoomTypes()
        {
            DataTable RoomsData = MainFunctions.NewQuery($"SELECT * FROM Hotel_rooms " +
                $" JOIN Room_type ON Room_type.id_room_type = Hotel_rooms.id_room_type" +
                $" WHERE id_hotel = {currentTour.HotelId}");

            int selectedIndex = 0;

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

                if (tourHotelRoom.RoomTypeId == currentTrip.RoomTypeId)
                    selectedIndex = i + 1;
            }

            cmbRoom.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Загрузка дат
        /// </summary>
        private void LoadDates()
        {
            tbxStartDate.Text = CommonFunctions.ConvertDateToText(Convert.ToDateTime(currentTrip.StartDate));

            tbxEndDate.Text = CommonFunctions.ConvertDateToText(Convert.ToDateTime(currentTrip.EndDate));

            tbxDays.Text = (Convert.ToDateTime(tbxEndDate.Text) - Convert.ToDateTime(tbxStartDate.Text)).TotalDays.ToString();
        }
        #endregion

        #region Functions
        /// <summary>
        /// Проверяет, какие документы по поездке загружены в БД
        /// </summary>
        private void CheckDocsAreUploaded()
        {
            DataTable tripDocsData =  MainFunctions.NewQuery($"SELECT transfer_doc,checkin_doc,ticket_doc,visa_doc,insurance_doc " +
                $"FROM Trip_docs WHERE id_trip={currentTrip.TripId}");

            if (tripDocsData.Rows.Count != 0)
            {
                ChangeTextDocColor(tripDocsData.Rows[0][0].ToString(), tbTransfer);
                ChangeTextDocColor(tripDocsData.Rows[0][1].ToString(), tbCheckin);
                ChangeTextDocColor(tripDocsData.Rows[0][2].ToString(), tbTicket);
                ChangeTextDocColor(tripDocsData.Rows[0][3].ToString(), tbVisa);
                ChangeTextDocColor(tripDocsData.Rows[0][4].ToString(), tbInsurance);
            }
        }

        /// <summary>
        /// Изменяет цвет надписи документа по поездке в зависимости от того, загружен он в БД или нет
        /// </summary>
        /// <param name="columnValue"></param>
        /// <param name="docTextBox"></param>
        private void ChangeTextDocColor(string columnValue, TextBlock docTextBox)
        {
            if (columnValue == "")
                docTextBox.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            else
                docTextBox.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
        }

        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void TourOptionsChanged()
        {
            if (tbxStartDate != null && tbxEndDate != null && tbxDays != null)
            {
                if (cmbRoom.SelectedIndex != 0 && cmbFeed.SelectedIndex != 0
                && CommonFunctions.isTextDateValid(tbxStartDate.Text) && CommonFunctions.isTextDateValid(tbxEndDate.Text) && tbxDays.Text.Length > 0)
                    borderEditButton.IsEnabled = true;
                else
                    borderEditButton.IsEnabled = false;

                int totalPrice = CalculateTotalPrice();

                tbTripPrice.Text = $"{totalPrice.ToString("### ### ###")} руб.";
            }
        }

        /// <summary>
        /// Вычислить итоговую стоимость
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
        /// Скачать документ поездки
        /// </summary>
        /// <param name="doc_type"></param>
        private void DownloadDoc(string doc_type)
        {
            tbFileUpload.Text = "Сохранение файла";
            gridFileLoad.Visibility = Visibility.Visible;
            if (int.Parse(MainFunctions.NewQuery($"SELECT COUNT(id_trip) FROM Trip_docs WHERE " +
                $"id_trip={currentTrip.TripId} AND {doc_type}_doc IS NOT null").Rows[0][0].ToString()) != 0)
                MainFunctions.GetTripDocumentFromDb(currentTrip.TripId, $"{doc_type}");
            else
            {
                Message message = new Message("Ошибка", "Файл еще не был загружен", false, false);
                message.ShowDialog();
            }
            gridFileLoad.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Загрузить документ поездки в БД
        /// </summary>
        /// <param name="doc_type"></param>
        private void UploadDoc(string doc_type)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.FileName = null;
            tbFileUpload.Text = "Загрузка файла";
            gridFileLoad.Visibility = Visibility.Visible;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string route = openFileDialog.FileName;

                MainFunctions.PutTripDocumentInDb(route, currentTrip.TripId, $"{doc_type}");
                CheckDocsAreUploaded();
            }
            gridFileLoad.Visibility = Visibility.Hidden;
        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new ClientsTrips(parentTabItemLink);
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Поездки клиентов";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/trips.png";
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
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

                string updateTripString = $"UPDATE Trip SET id_client = {chosenClient.ClientId}, id_tour = {currentTour.TourId}, " +
                    $"id_room_type = {(cmbRoom.SelectedItem as TourHotelRoom).RoomTypeId}, " +
                    $"id_feed_type = {(cmbFeed.SelectedItem as FeedType).FeedTypeId}, " +
                    $"start_date = '{startDate}', end_date = '{endDate}', total_price = {CalculateTotalPrice()} WHERE id_trip = {currentTrip.TripId}";

                MainFunctions.NewQuery(updateTripString);
                MainFunctions.NewQuery($"DELETE FROM Trip_services WHERE id_trip = {currentTrip.TripId}");

                for (int i = 0; i < tourServiceList.Count; i++)
                {
                    if (tourServiceList[i].IsChecked)
                    {
                        MainFunctions.NewQuery($"INSERT INTO Trip_services VALUES ({currentTrip.TripId}, {tourServiceList[i].ServiceId})");
                    }
                }

                MainFunctions.AddLogRecord($"Edited Trip ID: {currentTrip.TripId}");

                Message endMessage = new Message("Успех", "Поездка была успешно изменена!", false, false);
                endMessage.ShowDialog();

                parentTabItemLink.ItemUserControl = new ClientsTrips(parentTabItemLink);
                parentTabItemLink.VerticalScrollBarVisibility = "Auto";
                parentTabItemLink.ItemHeaderText = "Поездки клиентов";
                parentTabItemLink.ItemHeaderImageSource = "../Resources/trips.png";
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

        #region DownloadDocs
        private void btnDownloadTransfer_Click(object sender, RoutedEventArgs e)
        {
            DownloadDoc("transfer");
        }

        private void btnDownloadCheckin_Click(object sender, RoutedEventArgs e)
        {
            DownloadDoc("checkin");
        }

        private void btnDownloadVisa_Click(object sender, RoutedEventArgs e)
        {
            DownloadDoc("visa");
        }

        private void btnDownloadTickets_Click(object sender, RoutedEventArgs e)
        {
            DownloadDoc("ticket");
        }

        private void btnDownloadInsurance_Click(object sender, RoutedEventArgs e)
        {
            DownloadDoc("insurance");
        }
        #endregion

        #region UploadDocs
        private void btnUploadTransfer_Click(object sender, RoutedEventArgs e)
        {
            UploadDoc("transfer");
        }

        private void btnUploadVisa_Click(object sender, RoutedEventArgs e)
        {
            UploadDoc("visa");
        }

        private void btnUploadTickets_Click(object sender, RoutedEventArgs e)
        {
            UploadDoc("ticket");
        }

        private void btnUploadCheckin_Click(object sender, RoutedEventArgs e)
        {
            UploadDoc("checkin");
        }

        private void btnUploadInsurance_Click(object sender, RoutedEventArgs e)
        {
            UploadDoc("insurance");
        }
        #endregion
    }
}