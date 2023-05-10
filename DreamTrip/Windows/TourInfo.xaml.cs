using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Логика взаимодействия для TourInfo.xaml
    /// </summary>
    public partial class TourInfo : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Disabled", "Статистика", "../Resources/tours.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        int currentTourId;
        Tour currentTour;
        Image[] starsList;
        Image[] servicesImageList;
        TextBlock[] servicesTextList;
        ObservableCollection<TourComment> tourCommentsList = new ObservableCollection<TourComment>();
        ObservableCollection<TourHotelRoom> tourRoomsList = new ObservableCollection<TourHotelRoom>();
        ObservableCollection<FeedType> tourFeedsList = new ObservableCollection<FeedType>();
        ObservableCollection<TourService> tourServiceList = new ObservableCollection<TourService>();
        #endregion

        #region Constructor
        public TourInfo(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, int tempTourId)
        {
            InitializeComponent();
            starsList = new Image[] { imgStar1, imgStar2, imgStar3, imgStar4, imgStar5 };
            servicesImageList = new Image[] {imgService1, imgService2, imgService3, imgService4, imgService5, imgService6 };
            servicesTextList = new TextBlock[] {tbService1, tbService2, tbService3, tbService4, tbService5, tbService6 };
            currentTourId = tempTourId;

            switch (MainFunctions.GetUserRole())
            {
                case "manager":
                    borderDeleteButton.Visibility = Visibility.Hidden;
                    borderEditButton.Visibility = Visibility.Hidden;
                    borderBuyButton.Visibility = Visibility.Visible;
                    tbPriceText.Visibility = Visibility.Hidden;
                    break;

                case "admin":
                    borderDeleteButton.Visibility = Visibility.Visible;
                    borderEditButton.Visibility = Visibility.Visible;
                    borderBuyButton.Visibility = Visibility.Hidden;
                    tbPriceText.Visibility = Visibility.Visible;
                    break;

                case "analyst":
                    borderDeleteButton.Visibility = Visibility.Hidden;
                    borderEditButton.Visibility = Visibility.Hidden;
                    borderBuyButton.Visibility = Visibility.Hidden;
                    tbPriceText.Visibility = Visibility.Visible;
                    break;
            }

            LoadAllData();
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);

            thisPageParametres[1] = currentTour.Name;

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);
        }
        #endregion

        #region LoadData

        public void LoadAllData()
        {
            LoadTourInfo();
            LoadTourOnPage();
            LoadTourServices();
            LoadRoomTypes();
            LoadFeedTypes();
            LoadServices();
            LoadComments();
        }

        /// <summary>
        /// Загрузка комнат отеля
        /// </summary>
        private void LoadRoomTypes()
        {
            ComboBoxItem tempItem = cmbRoom.Items[0] as ComboBoxItem;

            tourRoomsList.Clear();
            cmbRoom.Items.Clear();

            cmbRoom.Items.Add(tempItem);
            cmbRoom.SelectedIndex = 0;

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
        /// Загрузка типов питания
        /// </summary>
        private void LoadFeedTypes()
        {
            ComboBoxItem tempItem = cmbFeed.Items[0] as ComboBoxItem;

            tourFeedsList.Clear();
            cmbFeed.Items.Clear();

            cmbFeed.Items.Add(tempItem);
            cmbFeed.SelectedIndex = 0;

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
        /// Загрузка сервисов
        /// </summary>
        private void LoadServices()
        {
            ComboBoxItem tempItem = lstService.Items[0] as ComboBoxItem;

            tourServiceList.Clear();
            lstService.Items.Clear();

            lstService.Items.Add(tempItem);
            lstService.SelectedIndex = 0;

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
        /// Загрузка информации о туре
        /// </summary>
        private void LoadTourInfo()
        {
            currentTour = null;

            DataTable baseTourData = MainFunctions.NewQuery($"SELECT * FROM Tour where id_tour={currentTourId}");

            int tempPrice = int.Parse(baseTourData.Rows[0][2].ToString()) + int.Parse(MainFunctions.NewQuery($"SELECT price_per_day FROM Hotel_rooms " +
                $"WHERE id_hotel={int.Parse(baseTourData.Rows[0][5].ToString())} ORDER BY price_per_day").Rows[0][0].ToString());

            int tempCityId = int.Parse(baseTourData.Rows[0][4].ToString());

            string tempCity = MainFunctions.NewQuery($"SELECT [name] FROM City WHERE id_city = {tempCityId}").Rows[0][0].ToString();

            string tempCountry = MainFunctions.NewQuery($"SELECT [name] FROM Country WHERE id_country = " +
                $"(SELECT id_country FROM City WHERE id_city = {tempCityId})").Rows[0][0].ToString();

            DataTable typeData = MainFunctions.NewQuery($"SELECT [name],id_type  FROM Type_of_tour WHERE id_type IN (SELECT id_type FROM Tour_types WHERE id_tour={currentTourId})");

            List<int> tourTypesIds = new List<int>();
            string tourTypes = "";
            for (int j = 0; j < typeData.Rows.Count; j++)
            {
                tourTypesIds.Add(int.Parse(typeData.Rows[j][1].ToString()));
                if (j != 0) tourTypes += ", ";
                tourTypes += typeData.Rows[j][0].ToString();
            }

            DataTable hotelInfo = MainFunctions.NewQuery($"SELECT * FROM Hotel WHERE id_hotel = (SELECT id_hotel FROM Tour WHERE id_tour = {currentTourId})");

            BitmapImage image;
            if (MainFunctions.NewQuery($"SELECT id_rec FROM Tour_photos " +
                $" WHERE id_tour = {currentTourId}").Rows.Count == 0)
            {

                image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/DreamTrip;component/Resources/ToursPhotos/default.png");
                image.EndInit();
            }
            else
            {
                image = MainFunctions.GetTourImageSource(currentTourId);
            }

            currentTour = new Tour()
            {
                TourId = currentTourId,
                Name = baseTourData.Rows[0][1].ToString(),
                TicketPrice = int.Parse(baseTourData.Rows[0][2].ToString()),
                StartPrice = $"от {tempPrice.ToString("### ### ###")} руб.",
                TourTypes = tourTypes,
                TypesIds = tourTypesIds,
                ImageSource = image,
                Location = $"{tempCountry}, {tempCity}",
                HotelId = int.Parse(baseTourData.Rows[0][5].ToString()),
                Description = baseTourData.Rows[0][3].ToString().Replace('¥', '\\'),
                StarsCount = int.Parse(hotelInfo.Rows[0][2].ToString())
            };

        }

        /// <summary>
        /// Загрузка информации о туре на страницу
        /// </summary>
        private void LoadTourOnPage()
        {
            for (int i = 0; i < starsList.Length; i++)
            {
                starsList[i].Visibility = Visibility.Hidden;
            }


            tbxTourName.Text = currentTour.Name;
            tbxLocation.Text = currentTour.Location;
            tbPriceFrom.Text = currentTour.StartPrice;

            imgTourPhoto.Source = currentTour.ImageSource;

            for (int i = 0; i < currentTour.StarsCount; i++)
            {
                starsList[i].Visibility = Visibility.Visible;
            }

            tbxType.Text = $"Тип: {currentTour.TourTypes}";
            tbxTourDescription.Text = $"Описание: " + currentTour.Description;

            tbCalculatedPrice.Visibility = Visibility.Hidden;
            tbxDays.Text = "";
        }

        /// <summary>
        /// Загрузка сервисов тура
        /// </summary>
        private void LoadTourServices()
        {
            for (int i = 0; i < servicesImageList.Length; i++)
            {
                servicesImageList[i].Source = null;
            }

            for (int i = 0; i < servicesTextList.Length; i++)
            {
                servicesTextList[i].Text = null;
            }

            DataTable dataServices = MainFunctions.NewQuery($"SELECT * FROM Tour_services WHERE id_tour={currentTourId}");
            int rowsCount = dataServices.Rows.Count;
            if (rowsCount > 6) rowsCount = 6;
            for (int i = 0;i < rowsCount; i++)
            {
                int serviceId = int.Parse(dataServices.Rows[i][1].ToString());
                currentTour.ServicesIds.Add(serviceId);
                DataTable serviceData = MainFunctions.NewQuery($"SELECT * FROM Service WHERE id_service={serviceId}");

                //servicesImageList[i].Source = new BitmapImage(new Uri($"pack://application:,,,/DreamTrip;component/{serviceData.Rows[0][4].ToString()}"));
                servicesImageList[i].Source = new BitmapImage(new Uri($"{MainFunctions.GetAppPath()}{serviceData.Rows[0][4].ToString()}"));

                servicesTextList[i].Text = serviceData.Rows[0][1].ToString();

                if (servicesTextList[i].Text.Length >11 && servicesTextList[i].Text.Length <= 23)
                {
                    servicesTextList[i].FontSize = 16;
                    servicesImageList[i].Width = 80;
                    servicesImageList[i].Height = 80;
                }

                if (servicesTextList[i].Text.Length > 23)
                {
                    servicesTextList[i].FontSize = 15;
                    servicesImageList[i].Width = 80;
                    servicesImageList[i].Height = 80;
                }
            }
        }

        /// <summary>
        /// Загрузка отзывов о туре
        /// </summary>
        private void LoadComments()
        {
            DataTable commentsData = MainFunctions.NewQuery($"SELECT tf.id_feedback, tf.id_trip, tf.comment, tf.rate, tf.date, " +
                $"c.surname, c.name, c.patronymic FROM Trip_feedback as tf " +
                $"JOIN Trip ON Trip.id_trip = tf.id_trip " +
                $"JOIN Client as c ON c.id_client = Trip.id_client " +
                $"WHERE Trip.id_tour ={currentTour.TourId}");

            for (int i = 0;i< commentsData.Rows.Count; i++)
            {
                TourComment comment = new TourComment()
                {
                    CommentId = int.Parse(commentsData.Rows[i][0].ToString()),
                    TourId = currentTour.TourId,
                    Rate = int.Parse(commentsData.Rows[i][3].ToString()),
                    Name = commentsData.Rows[i][5].ToString() + " " + commentsData.Rows[i][6].ToString() + " " + commentsData.Rows[i][7].ToString(),
                    CommentDateTime = commentsData.Rows[i][4].ToString().Substring(0,10),
                    CommentText = commentsData.Rows[i][2].ToString()
                };

                int rate = int.Parse(commentsData.Rows[i][3].ToString());
                if (rate >= 1) comment.StarVisibility1 = "Visible";
                if (rate >= 2) comment.StarVisibility2 = "Visible";
                if (rate >= 3) comment.StarVisibility3 = "Visible";
                if (rate >= 4) comment.StarVisibility4 = "Visible";
                if (rate >= 5) comment.StarVisibility5 = "Visible";

                tourCommentsList.Add(comment);
            }


            lvComments.ItemsSource = tourCommentsList;

            if (lvComments.Items.Count==0) tbxNoComments.Visibility = Visibility.Visible;

        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridTourLoad.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
            {

                parentTabItemLink.ItemUserControl = previousPage;
                MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
                (previousPage as Tours).LoadTours("all");


                gridTourLoad.Visibility = Visibility.Hidden;
                ((DispatcherTimer)c).Stop();
            });
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            string error = "";
            if (cmbRoom.SelectedIndex == -1 || cmbRoom.SelectedIndex == 0) error += "Вы не выбрали тип комнаты.\n";
            if (cmbFeed.SelectedIndex == -1 || cmbFeed.SelectedIndex == 0) error += "Вы не выбрали тип питания.\n";
            if (tbxDays.Text.Length == 0) error += "Вы не указали количество дней\n";

            if (error != "")
            {
                error += "Укажите необходимые параметры тура и повторите попытку.";
                Message messageWindow = new Message("Ошибка", error, false, false);
                messageWindow.ShowDialog();
                return;
            }

            int totalPrice = 0;

            totalPrice += currentTour.TicketPrice;
            totalPrice += (cmbRoom.SelectedItem as TourHotelRoom).PricePerDay * int.Parse(tbxDays.Text);
            totalPrice += (cmbFeed.SelectedItem as FeedType).PricePerDay * int.Parse(tbxDays.Text);

            for (int i = 0; i < tourServiceList.Count; i++)
            {
                if (tourServiceList[i].IsChecked)
                {
                    if (tourServiceList[i].PerDay)
                        totalPrice += tourServiceList[i].Price * int.Parse(tbxDays.Text);
                    else
                        totalPrice += tourServiceList[i].Price;
                }
            }


            tbCalculatedPrice.Visibility = Visibility.Visible;
            tbCalculatedPrice.Text = $"Стоимость: {totalPrice.ToString("### ### ###")} руб.";

        }

        private void btnCreateTrip_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = new ChooseClient(parentTabItemLink, this, thisPageParametres, currentTour, false);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Tour editedTour = new Tour()
            {
                TourId = currentTour.TourId,
                Name = currentTour.Name,
                Description = currentTour.Description,
                TicketPrice = currentTour.TicketPrice,
                TypesIds = currentTour.TypesIds,
                ServicesIds = currentTour.ServicesIds,
                ImageSource = currentTour.ImageSource
            };
            parentTabItemLink.ItemUserControl = new NewTour(parentTabItemLink, this, thisPageParametres, editedTour);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool popupAnswer = false;
            Message messageDeleteWindow = new Message("Удаление", "Вы точно хотите удалить  данный тур? Действие невозможно отменить?", true, true);
            messageDeleteWindow.messageAnswer += value => popupAnswer = value;
            messageDeleteWindow.ShowDialog();

            if (popupAnswer)
            {
                MainFunctions.NewQuery($"DELETE FROM Tour WHERE id_tour = {currentTourId}");

                Message messageSuccessWindow = new Message("Успех", "Тур был успешно удален!", false, false);
                messageSuccessWindow.ShowDialog();
                MainFunctions.AddLogRecord($"Tour deleted:" +
                    $"\n\tID: {currentTourId}" +
                    $"\n\tName: {currentTour.Name}");

                btnCancel_Click(sender, e);
            }

        }
        #endregion

        #region TextChanged
        private void tbxDays_TextChanged(object sender, TextChangedEventArgs e)
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

            if (textBox.Text.Length > 3) textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
        }
        #endregion

        #region Functions
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

            scvTourInfo.ScrollToVerticalOffset(scvTourInfo.VerticalOffset - Delta);
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
                stackPanelMainInfo.Orientation = Orientation.Horizontal;
                this.Width = 1300;
                borderTourMainInfo.Width = borderTourMainInfo.MaxWidth - 230;
                borderBuy.Width = borderTourMainInfo.MaxWidth - 230;
                borderBuy.HorizontalAlignment = HorizontalAlignment.Right;
                borderBuyButton.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                this.Width = width - 20;

                double tempMainInfoWIdth = (borderTourMainInfo.MaxWidth - 230) - (1300 - this.Width);


                if (tempMainInfoWIdth < borderTourMainInfo.MinWidth)
                {
                    stackPanelMainInfo.Orientation = Orientation.Vertical;
                    borderTourMainInfo.Width = borderTourImage.MaxWidth;
                    borderBuy.Width = borderTourImage.MaxWidth;
                    borderBuy.HorizontalAlignment = HorizontalAlignment.Center;
                    borderBuyButton.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    stackPanelMainInfo.Orientation = Orientation.Horizontal;
                    borderTourMainInfo.Width = tempMainInfoWIdth;
                    borderBuy.Width = tempMainInfoWIdth;
                    borderBuy.HorizontalAlignment = HorizontalAlignment.Right;
                    borderBuyButton.HorizontalAlignment = HorizontalAlignment.Left;
                }

            }

            tbPriceFrom.Width = borderBuy.Width - 270;
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

        #endregion

        
    }
}
