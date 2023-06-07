using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Data;
using System.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Tours.xaml
    /// </summary>
    public partial class Tours : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Disabled", "Туры", "../../Resources/tours.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ObservableCollection<Tour> ToursList { get; set; } = new ObservableCollection<Tour>();
        ObservableCollection<Country> CountriesList { get; set; } = new ObservableCollection<Country>();
        ObservableCollection<City> CurrentCitiesList { get; set; } = new ObservableCollection<City>();
        ObservableCollection<TourType> TourTypesList { get; set; } = new ObservableCollection<TourType>();
        ObservableCollection<FeedType> FeedTypesList { get; set; } = new ObservableCollection<FeedType>();
        ObservableCollection<HotelStars> HotelStarsList { get; set; } = new ObservableCollection<HotelStars>();

        ComboBoxItem tempFirstItemCity;
        ComboBoxItem tempSecondItemCity;
        ComboBoxItem tempThirdItemCity;
        int lastChosenCountry = -1;
        bool isToCreateTrip;


        #endregion

        #region Constructor
        public Tours(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, bool tempIsToCreateTrip)
        {
            InitializeComponent();
            isToCreateTrip = tempIsToCreateTrip;
            LoadTours("all");
            LoadFeedTypes();
            LoadTourTypes();
            LoadCountries();
            LoadHotelStars();
            SecondItemCheckBoxCountry.IsChecked = true;
            SecondItemCheckBoxFeed.IsChecked = true;
            SecondItemCheckBoxHotel.IsChecked = true;
            SecondItemCheckBoxType.IsChecked = true;
            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);

            if (isToCreateTrip) thisPageParametres[1] = "Выбор тура";
            
            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            if (MainFunctions.GetShowPrompts()) btnHelpInfo.Visibility = Visibility.Visible;
            else btnHelpInfo.Visibility = Visibility.Hidden;

        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка уровней комфорта отеля
        /// </summary>
        void LoadHotelStars()
        {
            for (int i = 1; i <= 5; i++)
            {
                string stars = "";
                for (int j = 0; j < i; j++)
                {
                    stars += "★";
                }

                HotelStars hotelStars = new HotelStars()
                {
                    StarsCount = i,
                    StarsString = stars,
                    IsChecked = false
                };

                HotelStarsList.Add(hotelStars);
                lstHotel.Items.Add(hotelStars);
            }
        }

        /// <summary>
        /// Загрузка типов тура
        /// </summary>
        void LoadTourTypes()
        {
            DataTable dataTourTypes = MainFunctions.NewQuery($"SELECT * FROM Type_of_tour");
            for (int i = 0; i < dataTourTypes.Rows.Count; i++)
            {
                TourType dataTourType = new TourType()
                {
                    TourTypeId = int.Parse(dataTourTypes.Rows[i][0].ToString()),
                    Name = dataTourTypes.Rows[i][1].ToString(),
                    IsChecked = false,
                };

                TourTypesList.Add(dataTourType);
                lstType.Items.Add(dataTourType);
            }
        }

        /// <summary>
        /// Загрузка типов питания
        /// </summary>
        void LoadFeedTypes()
        {
            DataTable dataFeedTypes = MainFunctions.NewQuery($"SELECT * FROM Feed_type");
            for (int i = 0;i< dataFeedTypes.Rows.Count; i++)
            {
                FeedType dataFeedType = new FeedType()
                {
                    FeedTypeId = int.Parse(dataFeedTypes.Rows[i][0].ToString()),
                    FeedTypeName = dataFeedTypes.Rows[i][1].ToString(),
                    IsChecked = false,
                };

                FeedTypesList.Add(dataFeedType);
                lstFeed.Items.Add(dataFeedType);
            }

        }

        /// <summary>
        /// Загрузка стран
        /// </summary>
        void LoadCountries()
        {
            DataTable dataCountries = MainFunctions.NewQuery($"SELECT * FROM Country");
            for (int i = 0; i < dataCountries.Rows.Count; i++)
            {
                int tempCountryId = int.Parse(dataCountries.Rows[i][0].ToString());
                DataTable dataCities = MainFunctions.NewQuery($"SELECT id_city,[name] FROM City WHERE id_country={tempCountryId}");

                ObservableCollection <City> tempCitiesList = new ObservableCollection<City>();

                for (int j = 0; j < dataCities.Rows.Count; j++)
                {
                    City dataCity = new City()
                    {
                        CityId = int.Parse(dataCountries.Rows[j][0].ToString()),
                        CityName = dataCountries.Rows[j][1].ToString()
                    };
                    tempCitiesList.Add(dataCity);
                }

                Country dataCountry = new Country()
                {
                    CountryId = tempCountryId,
                    CountryName = dataCountries.Rows[i][1].ToString(),
                    IsChecked = false,
                    CitiesList = tempCitiesList,
                };

                CountriesList.Add(dataCountry);
                lstCountry.Items.Add(dataCountry);
            }
        }

        /// <summary>
        /// Загрузка городов
        /// </summary>
        void LoadCities()
        {
            int[] checkedCountries = CommonFunctions.CheckedCountries(CountriesList);
            int countryListId = -1;
            if (checkedCountries.Length == 1) countryListId = checkedCountries[0];
            if (countryListId != lastChosenCountry)
            {
                SecondItemCheckBoxCity.IsChecked = false;

                lastChosenCountry = countryListId;
                tempFirstItemCity = lstCity.Items[0] as ComboBoxItem;
                tempSecondItemCity = lstCity.Items[1] as ComboBoxItem;
                tempThirdItemCity = lstCity.Items[2] as ComboBoxItem;

                lstCity.Items.Clear();

                lstCity.Items.Add(tempFirstItemCity);
                lstCity.Items.Add(tempSecondItemCity);
                lstCity.Items.Add(tempThirdItemCity);

                lstCity.SelectedIndex = 0;

                CurrentCitiesList.Clear();

                if (countryListId == -1)
                {
                    (lstCity.Items[1] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (lstCity.Items[1] as ComboBoxItem).IsEnabled = false;
                    (lstCity.Items[2] as ComboBoxItem).Visibility = Visibility.Visible;
                    (lstCity.Items[2] as ComboBoxItem).IsEnabled = true;

                }
                else
                {
                    (lstCity.Items[2] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (lstCity.Items[2] as ComboBoxItem).IsEnabled = false;
                    (lstCity.Items[1] as ComboBoxItem).Visibility = Visibility.Visible;
                    (lstCity.Items[1] as ComboBoxItem).IsEnabled = true;

                    int CountryId = countryListId;

                    DataTable dataCities = MainFunctions.NewQuery($"SELECT id_city, name FROM [dbo].[City] WHERE id_country={CountryId} ORDER BY name");

                    for (int j = 0; j < dataCities.Rows.Count; j++)
                    {
                        City dataCity = new City()
                        {
                            CityId = int.Parse(dataCities.Rows[j][0].ToString()),
                            IdCountry = CountryId,
                            CityName = dataCities.Rows[j][1].ToString(),
                            IsChecked = false,
                        };
                        lstCity.Items.Add(dataCity);
                        CurrentCitiesList.Add(dataCity);
                    }

                    SecondItemCheckBoxCity.IsChecked = true;
                }
            }
        }

        
        
        


        /// <summary>
        /// Загрузка туров
        /// </summary>
        /// <param name="toursIds"></param>
        public void LoadTours(string toursIds)
        {

            lvTours.ItemsSource = null;
            ToursList.Clear();

            tbNothingFound.Visibility = Visibility.Hidden;
            if (toursIds == "")
            {
                tbNothingFound.Visibility = Visibility.Visible;
            }
            else
            {
                DataTable dataTours;

                if (toursIds == "all")
                {
                    dataTours = MainFunctions.NewQuery("SELECT * FROM Tour ORDER BY id_tour");
                }
                else
                {
                    dataTours = MainFunctions.NewQuery($"SELECT * FROM Tour WHERE id_tour IN ({toursIds}) ORDER BY id_tour");
                }


                for (int i = 0; i < dataTours.Rows.Count; i++)
                {
                    int tempTourId = int.Parse(dataTours.Rows[i][0].ToString());
                    //DataTable dataPhoto = MainFunctions.NewQuery($"SELECT photo_path FROM Tour_photos WHERE id_tour = {tempTourId}");
                    //string photoPath = "\\Resources\\ToursPhotos\\default.png";
                    //if (dataPhoto.Rows.Count > 0) photoPath = dataPhoto.Rows[0][0].ToString();

                    int tempPrice = int.Parse(dataTours.Rows[i][2].ToString()) + int.Parse(MainFunctions.NewQuery($"SELECT price_per_day FROM Hotel_rooms " +
                        $"WHERE id_hotel={int.Parse(dataTours.Rows[i][5].ToString())} ORDER BY price_per_day").Rows[0][0].ToString());


                    int tempCityId = int.Parse(dataTours.Rows[i][4].ToString());
                    string tempCity = MainFunctions.NewQuery($"SELECT [name] FROM City WHERE id_city = {tempCityId}").Rows[0][0].ToString();
                    string tempCountry = MainFunctions.NewQuery($"SELECT [name] FROM Country WHERE id_country = " +
                        $"(SELECT id_country FROM City WHERE id_city = {tempCityId})").Rows[0][0].ToString();

                    DataTable typeData = MainFunctions.NewQuery($"SELECT [name] FROM Type_of_tour WHERE id_type IN (SELECT id_type FROM Tour_types WHERE id_tour={tempTourId})");
                    string tourTypes = "";
                    for (int j = 0; j < typeData.Rows.Count; j++)
                    {
                        if (j != 0) tourTypes += ", ";
                        tourTypes += typeData.Rows[j][0].ToString();
                    }

                    if (tourTypes.Length > 80) tourTypes = tourTypes.Substring(0, 80) + "...";


                    //currentDirectory = new DirectoryInfo(Path.Combine(currentDirectory.FullName, folder));
                    //if (!currentDirectory.Exists) currentDirectory.Create();

                    string tourName = dataTours.Rows[i][1].ToString();

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;

                    string appPath = MainFunctions.GetAppPath();
                    if (!appPath.Contains("DreamTrip")) appPath = MainFunctions.GetCurrentExePath() + "/DreamTrip";
                    string imageFolderPath = $"/Resources/ToursPhotos/ID{tempTourId}_{tourName.ToLower().Replace(" ", "-")}";

                    DataTable imageExtension = MainFunctions.NewQuery($"SELECT format FROM Tour_photos " +
                        $" WHERE id_tour = {tempTourId}");
                    if (imageExtension.Rows.Count != 0)
                    {
                        FileStream stream = null;
                        if (!Directory.Exists(appPath + imageFolderPath))
                        {
                            string fullImagePath = appPath + imageFolderPath +
                                $"/photo" + $".{imageExtension.Rows[0][0].ToString()}";

                            Directory.CreateDirectory(appPath + imageFolderPath);
                            MainFunctions.SaveImage(tempTourId, fullImagePath);
                            stream = File.OpenRead(fullImagePath);
                            image.StreamSource = stream;
                            image.EndInit();
                        }
                        else
                        {
                            string[] tempFiles = Directory.GetFiles(appPath + imageFolderPath);
                            if (tempFiles.Length != 0)
                            {
                                stream = File.OpenRead(tempFiles[0]);
                                image.StreamSource = stream;
                                image.EndInit();
                            }
                        }
                        stream.Close();
                    }
                    else
                    {
                        image.UriSource = new Uri("pack://application:,,,/DreamTrip;component/Resources/ToursPhotos/default.png");
                        image.EndInit();
                    }


                    Tour newTour = new Tour()
                    {
                        TourId = tempTourId,
                        Name = dataTours.Rows[i][1].ToString(),
                        StartPrice = $"от {tempPrice.ToString("### ### ###")} руб.",
                        TourTypes = tourTypes,
                        Location = $"{tempCountry}, {tempCity}",
                        ImageSource = image
                    };
                    ToursList.Add(newTour);
                }

                lvTours.ItemsSource = ToursList;

            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// При измении размеров окна
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void WindowSizeChanged(double width, double height)
        {
            this.Width = width - 48.8;
            mainGrid.Width = this.Width - 270;
        }

        /// <summary>
        /// При изменении цены
        /// </summary>
        /// <param name="sender"></param>
        private void PriceChanged(object sender)
        {
            TextBox textBoxPrice = sender as TextBox;
            char[] charList = textBoxPrice.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false)
                {
                    textBoxPrice.Text = textBoxPrice.Text.Remove(i, 1);
                }
            }
            if (textBoxPrice.Text.Length > 12) textBoxPrice.Text = textBoxPrice.Text.Substring(0, textBoxPrice.Text.Length - 1);

            FilterChanged();
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


            if (lstFeed.IsDropDownOpen && lstFeed.IsMouseOver)
            {

                ScrollViewer scv = lstFeed.Template.FindName("cmbScroll", lstFeed) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (lstCountry.IsDropDownOpen && lstCountry.IsMouseOver)
            {

                ScrollViewer scv = lstCountry.Template.FindName("cmbScroll", lstCountry) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (lstType.IsDropDownOpen && lstType.IsMouseOver)
            {

                ScrollViewer scv = lstType.Template.FindName("cmbScroll", lstType) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            
            ScrollViewer listViewScv = MainFunctions.GetChildOfType<ScrollViewer>(lvTours);
            if (listViewScv != null)
            {
                listViewScv.ScrollToVerticalOffset(listViewScv.VerticalOffset - Delta);
            }
        }

        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void FilterChanged()
        {
            if (tbxTourNameSearch != null && tbxPriceFrom != null && tbxPriceTo != null
                && lstCountry != null && lstCity != null && lstFeed != null && lstHotel != null && lstType != null && SecondItemCheckBoxCountry != null && borderClear != null)
            {
                if (tbxTourNameSearch.Text.Length == 0
                    && tbxPriceFrom.Text.Length == 0
                    && tbxPriceTo.Text.Length == 0
                    && SecondItemCheckBoxCountry.IsChecked == true
                    && SecondItemCheckBoxFeed.IsChecked == true
                    && SecondItemCheckBoxHotel.IsChecked == true
                    && SecondItemCheckBoxType.IsChecked == true)
                {
                    borderClear.Visibility = Visibility.Hidden;
                }
                else
                {
                    borderClear.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region ButtonsClick
        private void TourButton_Click(object sender, RoutedEventArgs e)
        {
            int tourId = int.Parse((sender as Button).Tag.ToString());


            //Tour thisTour = new Tour();

            //for (int i = 0; i < ToursList.Count; i++)
            //{
            //    if (ToursList[i].TourId == tourId)
            //    {
            //        thisTour = ToursList[i];
            //        break;
            //    }
            //}

            //BitmapImage image = new BitmapImage();
            //image.BeginInit();
            //image.CacheOption = BitmapCacheOption.OnLoad;

            //string appPath = MainFunctions.GetAppPath();
            //if (!appPath.Contains("DreamTrip")) appPath = MainFunctions.GetCurrentExePath() + "/DreamTrip";
            //string imageFolderPath = $"/Resources/ToursPhotos/ID{tourId}_{thisTour.Name.ToLower().Replace(" ", "-")}";

            //DataTable imageExtension = MainFunctions.NewQuery($"SELECT format FROM Tour_photos " +
            //    $" WHERE id_tour = {tourId}");
            //if (imageExtension.Rows.Count != 0)
            //{
            //    FileStream stream = null;
            //    if (!Directory.Exists(appPath + imageFolderPath))
            //    {
            //        string fullImagePath = appPath + imageFolderPath +
            //            $"/photo" + $".{imageExtension.Rows[0][0].ToString()}";

            //        Directory.CreateDirectory(appPath + imageFolderPath);
            //        MainFunctions.SaveImage(tourId, fullImagePath);
            //        stream = File.OpenRead(fullImagePath);

            //        image.StreamSource = stream;
            //        image.EndInit();
            //        thisTour.ImageSource = image;
            //    }
            //    else
            //    {
            //        string[] tempFiles = Directory.GetFiles(appPath + imageFolderPath);
            //        if (tempFiles.Length != 0)
            //        {
            //            stream = File.OpenRead(tempFiles[0]);
            //            image.StreamSource = stream;
            //            image.EndInit();
            //            thisTour.ImageSource = image;

            //        }
            //    }
            //    stream.Close();
            //}



            if (isToCreateTrip)
            {
                DataTable baseTourData = MainFunctions.NewQuery($"SELECT * FROM Tour where id_tour={tourId}");

                int tempCityId = int.Parse(baseTourData.Rows[0][4].ToString());
                string tempCity = MainFunctions.NewQuery($"SELECT [name] FROM City WHERE id_city = {tempCityId}").Rows[0][0].ToString();
                string tempCountry = MainFunctions.NewQuery($"SELECT [name] FROM Country WHERE id_country = " +
                    $"(SELECT id_country FROM City WHERE id_city = {tempCityId})").Rows[0][0].ToString();

                Tour currentTour = new Tour()
                {
                    TourId = tourId,
                    HotelId = int.Parse(baseTourData.Rows[0][5].ToString()),
                    Name = baseTourData.Rows[0][1].ToString(),
                    TicketPrice = int.Parse(baseTourData.Rows[0][2].ToString()),
                    Location = $"{tempCountry}, {tempCity}",
                };


                parentTabItemLink.ItemUserControl = new ChooseClient(parentTabItemLink, this, thisPageParametres, currentTour, true);
            }
            else
            {
                string tourName = MainFunctions.NewQuery($"SELECT name FROM Tour WHERE id_tour = {tourId}").Rows[0][0].ToString();

                try
                {
                    parentTabItemLink.ItemUserControl = new TourInfo(parentTabItemLink, this, thisPageParametres, tourId);
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
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbxTourNameSearch.Text = "";
            tbxPriceFrom.Text = "";
            tbxPriceTo.Text = "";
            SecondItemCheckBoxCountry.IsChecked = true;
            SecondItemCheckBoxFeed.IsChecked = true;
            SecondItemCheckBoxType.IsChecked = true;
            SecondItemCheckBoxHotel.IsChecked = true;
            LoadCities();

            borderClear.Visibility = Visibility.Hidden;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadCities();
            if (borderClear.Visibility == Visibility.Hidden)
            {
                LoadTours("all");
            }
            else
            {

                string nameCondition = "";

                if (tbxTourNameSearch.Text != "")
                {
                    string[] fullTourName = tbxTourNameSearch.Text.Split(' ');

                    for (int i = 0; i < fullTourName.Length; i++)
                    {
                        fullTourName[i] = fullTourName[i].Trim();
                        if (fullTourName[i] != null && fullTourName[i] != "" && fullTourName[i] != " "
                            && MainFunctions.ValidateString_RuEngNumSpec(fullTourName[i].ToLower()))
                        {
                            if (nameCondition == "") nameCondition += " WHERE ";
                            else nameCondition += " OR ";

                            nameCondition += String.Format(" (t.name LIKE '%{0}%') ", fullTourName[i]);
                        }
                    }
                }

                string priceFromCondition = "";

                if (tbxPriceFrom.Text != "")
                {
                    priceFromCondition += $" HAVING MIN(hr.price_per_day) + t.ticket_price >= {int.Parse(tbxPriceFrom.Text)} ";
                }

                string priceToCondition = "";

                if (tbxPriceTo.Text != "")
                {
                    string priceToStart = " HAVING";
                    if (priceFromCondition != "") priceToStart = " AND";
                    priceToCondition += $" {priceToStart} (MIN(hr.price_per_day) + t.ticket_price <= {int.Parse(tbxPriceTo.Text)}) ";
                }

                string countriesCondition = "";
                string citiesCondition = "";
                if (SecondItemCheckBoxCountry.IsChecked == false)
                {
                    int[] checkedCountries = CommonFunctions.CheckedCountries(CountriesList);
                    countriesCondition = string.Join(",", checkedCountries);
                    string countriesStart = " AND ";
                    if (nameCondition == "") countriesStart = " WHERE ";
                    if (checkedCountries.Length > 0) countriesCondition = $" {countriesStart} (t.id_city IN (SELECT id_city FROM City WHERE id_country IN ({countriesCondition})))";
                    
                    if (checkedCountries.Length == 1)
                    {
                        int[] checkedCities = CommonFunctions.CheckedCities(CurrentCitiesList);
                        citiesCondition = string.Join(",", checkedCities);
                        string citiesStart = " AND ";
                        if (nameCondition == "" && countriesCondition == "") citiesStart = " WHERE ";
                        if (checkedCities.Length > 0) citiesCondition = $" {citiesStart} (t.id_city IN ({citiesCondition}))";
                    }
                }


                string feedCondition = "";
                if (SecondItemCheckBoxFeed.IsChecked == false)
                {
                    int[] checkedFeeds = CommonFunctions.CheckedFeedTypes(FeedTypesList);
                    feedCondition = string.Join(",", checkedFeeds);
                    string feedStart = " AND ";
                    if (nameCondition == "" && countriesCondition == "" && citiesCondition == "") feedStart = " WHERE ";
                    if (checkedFeeds.Length > 0) feedCondition = $" {feedStart} hft.id_feed_type IN ({feedCondition})";
                }


                string starsCondition = "";
                if (SecondItemCheckBoxHotel.IsChecked == false)
                {
                    int[] checkedHotelStars = CommonFunctions.CheckedHotelStars(HotelStarsList);
                    starsCondition = string.Join(",", checkedHotelStars);
                    string hotelStart = " AND ";
                    if (nameCondition == "" && countriesCondition == "" && citiesCondition == "" && feedCondition == "") hotelStart = " WHERE ";
                    if (checkedHotelStars.Length > 0) starsCondition = $" {hotelStart} h.stars IN ({starsCondition})";
                }


                string tourTypeCondition = "";
                if (SecondItemCheckBoxType.IsChecked == false)
                {
                    int[] checkedTourTypes = CommonFunctions.CheckedTourTypes(TourTypesList);
                    tourTypeCondition = string.Join(",", checkedTourTypes);
                    string typeStart = " AND ";
                    if (nameCondition == "" && countriesCondition == ""
                        && citiesCondition == "" && feedCondition == "" && starsCondition == "") typeStart = " WHERE ";
                    if (checkedTourTypes.Length > 0) tourTypeCondition = $" {typeStart} tt.id_type IN ({tourTypeCondition})";
                }


                string condition = $" SELECT DISTINCT t.id_tour FROM Tour as t " +
                    $" JOIN Hotel_feed_types as hft ON t.id_hotel = hft.id_hotel " +
                    $" JOIN Hotel as h ON h.id_hotel = t.id_hotel " +
                    $" JOIN Hotel_rooms as hr ON hr.id_hotel = t.id_hotel " +
                    $" JOIN Tour_types as tt ON tt.id_tour = t.id_tour " +
                    $" {nameCondition} {countriesCondition} {citiesCondition} {feedCondition} {starsCondition} {tourTypeCondition} " +
                    $" GROUP BY t.id_tour, t.ticket_price, tt.id_type " +
                    $" {priceFromCondition} {priceToCondition} ";



                DataTable dataTours = MainFunctions.NewQuery(condition);


                string toursIds = "";
                for (int i = 0; i < dataTours.Rows.Count; i++)
                {
                    if (toursIds != "") toursIds += ",";
                    toursIds += dataTours.Rows[i][0].ToString();
                }

                LoadTours(toursIds);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);

            
        }
        #endregion

        #region TextChanged
        private void tbxPriceFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            PriceChanged(sender);
        }

        private void tbxPriceTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            PriceChanged(sender);
        }

        private void tbxTourNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_RuEngNumSpec(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }

            FilterChanged();
        }
        #endregion

        #region lstCountry
        private void lstCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);

        }

        private void lstCountry_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);

        }

        private void lstCountry_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void CheckBoxAllCountries_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < lstCountry.Items.Count; i++)
            {
                (lstCountry.Items[i] as Country).IsChecked = true;
            }
        }

        private void CheckBoxAllCountries_UnChecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllCountriesCheck = true;

            for (int i = 0; i < CountriesList.Count; i++)
            {
                flag_AllCountriesCheck = CountriesList[i].IsChecked && flag_AllCountriesCheck;
            }


            if (flag_AllCountriesCheck)
            {
                for (int i = 2; i < lstCountry.Items.Count; i++)
                {
                    (lstCountry.Items[i] as Country).IsChecked = false;
                }
            }
        }

        private void CheckBoxCountry_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxCountry.IsChecked == false)
            {
                bool flag_AllCountriesCheck = true;

                for (int i = 0; i < CountriesList.Count; i++)
                {
                    flag_AllCountriesCheck = CountriesList[i].IsChecked && flag_AllCountriesCheck;
                }

                if (flag_AllCountriesCheck) SecondItemCheckBoxCountry.IsChecked = true;
            }
        }

        private void CheckBoxCountry_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxCountry.IsChecked = false;
        }
        #endregion

        #region lstCity
        private void lstCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void lstCity_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);
            LoadCities();
        }

        private void lstCity_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();

        }

        private void CheckBoxAllCities_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 3; i < lstCity.Items.Count; i++)
            {
                (lstCity.Items[i] as City).IsChecked = true;
            }
        }

        private void CheckBoxAllCities_UnChecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllCitiesCheck = true;

            for (int i = 0; i < CurrentCitiesList.Count; i++)
            {
                flag_AllCitiesCheck = CurrentCitiesList[i].IsChecked && flag_AllCitiesCheck;
            }


            if (flag_AllCitiesCheck)
            {
                for (int i = 3; i < lstCity.Items.Count; i++)
                {
                    (lstCity.Items[i] as City).IsChecked = false;
                }
            }
        }

        private void CheckBoxCity_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxCity.IsChecked == false)
            {
                bool flag_AllCitiesCheck = true;

                for (int i = 0; i < CurrentCitiesList.Count; i++)
                {
                    flag_AllCitiesCheck = CurrentCitiesList[i].IsChecked && flag_AllCitiesCheck;
                }

                if (flag_AllCitiesCheck) SecondItemCheckBoxCity.IsChecked = true;
            }
        }

        private void CheckBoxCity_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxCity.IsChecked = false;
        }
        #endregion

        #region lstStars
        private void lstStars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }
        #endregion

        #region lstFeed
        private void lstFeed_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);

        }

        private void lstFeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);

        }
        
        private void lstFeed_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void SecondItemCheckBoxFeed_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < lstFeed.Items.Count; i++)
            {
                (lstFeed.Items[i] as FeedType).IsChecked = true;
            }
        }

        private void SecondItemCheckBoxFeed_Unchecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllFeedsCheck = true;

            for (int i = 0; i < FeedTypesList.Count; i++)
            {
                flag_AllFeedsCheck = FeedTypesList[i].IsChecked && flag_AllFeedsCheck;
            }


            if (flag_AllFeedsCheck)
            {
                for (int i = 2; i < lstFeed.Items.Count; i++)
                {
                    (lstFeed.Items[i] as FeedType).IsChecked = false;
                }
            }
        }

        private void CheckBoxFeed_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxFeed.IsChecked == false)
            {
                bool flag_AllFeedsCheck = true;

                for (int i = 0; i < FeedTypesList.Count; i++)
                {
                    flag_AllFeedsCheck = FeedTypesList[i].IsChecked && flag_AllFeedsCheck;
                }

                if (flag_AllFeedsCheck) SecondItemCheckBoxFeed.IsChecked = true;
            }
        }

        private void CheckBoxFeed_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxFeed.IsChecked = false;

        }
        #endregion

        #region lstType
        private void lstType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void lstType_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);
        }

        private void lstType_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void SecondItemCheckBoxType_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < lstType.Items.Count; i++)
            {
                (lstType.Items[i] as TourType).IsChecked = true;
            }
        }

        private void SecondItemCheckBoxType_Unchecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllTypesCheck = true;

            for (int i = 0; i < TourTypesList.Count; i++)
            {
                flag_AllTypesCheck = TourTypesList[i].IsChecked && flag_AllTypesCheck;
            }


            if (flag_AllTypesCheck)
            {
                for (int i = 2; i < lstType.Items.Count; i++)
                {
                    (lstType.Items[i] as TourType).IsChecked = false;
                }
            }
        }

        private void CheckBoxType_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxType.IsChecked == false)
            {
                bool flag_AllTypesCheck = true;

                for (int i = 0; i < TourTypesList.Count; i++)
                {
                    flag_AllTypesCheck = TourTypesList[i].IsChecked && flag_AllTypesCheck;
                }

                if (flag_AllTypesCheck) SecondItemCheckBoxType.IsChecked = true;
            }
        }

        private void CheckBoxType_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxType.IsChecked = false;
        }
        #endregion

        #region lstHotel
        private void lstHotel_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);
        }

        private void lstHotel_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();

        }

        private void SecondItemCheckBoxHotel_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < lstHotel.Items.Count; i++)
            {
                (lstHotel.Items[i] as HotelStars).IsChecked = true;
            }
        }

        private void SecondItemCheckBoxHotel_Unchecked(object sender, RoutedEventArgs e)
        {
            bool flag_AllHotelsCheck = true;

            for (int i = 0; i < HotelStarsList.Count; i++)
            {
                flag_AllHotelsCheck = HotelStarsList[i].IsChecked && flag_AllHotelsCheck;
            }


            if (flag_AllHotelsCheck)
            {
                for (int i = 2; i < lstHotel.Items.Count; i++)
                {
                    (lstHotel.Items[i] as HotelStars).IsChecked = false;
                }
            }
        }
        
        private void CheckBoxHotel_Checked(object sender, RoutedEventArgs e)
        {
            if (SecondItemCheckBoxHotel.IsChecked == false)
            {
                bool flag_AllHotelsCheck = true;

                for (int i = 0; i < HotelStarsList.Count; i++)
                {
                    flag_AllHotelsCheck = HotelStarsList[i].IsChecked && flag_AllHotelsCheck;
                }

                if (flag_AllHotelsCheck) SecondItemCheckBoxHotel.IsChecked = true;
            }
        }

        private void CheckBoxHotel_Unchecked(object sender, RoutedEventArgs e)
        {
            SecondItemCheckBoxHotel.IsChecked = false;

        }
        #endregion

    }
}
