using DreamTrip.Classes;
using DreamTrip.Functions;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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
    /// Логика взаимодействия для ClientsTrips.xaml
    /// </summary>
    public partial class ClientsTrips : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Поездки клиентов", "../Resources/trips.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ObservableCollection<Trip> TripsList { get; set; } = new ObservableCollection<Trip>();
        ObservableCollection<Country> CountriesList { get; set; } = new ObservableCollection<Country>();
        ObservableCollection<City> CurrentCitiesList { get; set; } = new ObservableCollection<City>();
        ObservableCollection<TourType> TourTypesList { get; set; } = new ObservableCollection<TourType>();

        ComboBoxItem tempFirstItemCity;
        ComboBoxItem tempSecondItemCity;
        ComboBoxItem tempThirdItemCity;
        public bool IsFromClients;
        int lastChosenCountry = -1;
        #endregion

        #region Constructor
        public ClientsTrips(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, string clientId = "", bool tempIsFromClients = false)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            IsFromClients = tempIsFromClients;
            LoadTrips("all", clientId);
            LoadCountries();
            LoadTourTypes();
            SecondItemCheckBoxCountry.IsChecked = true;
            SecondItemCheckBoxType.IsChecked = true;
        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка поездок
        /// </summary>
        /// <param name="tripsIds">id загружаемых поездок</param>
        /// <param name="clientId">id клиента (если загружаем поездки только 1 клиента)</param>
        public void LoadTrips(string tripsIds, string clientId = "")
        {
            dtgTrips.ItemsSource = null;
            TripsList.Clear();

            tbNothingFound.Visibility = Visibility.Hidden;
            if (tripsIds == "")
            {
                tbNothingFound.Visibility = Visibility.Visible;
            }
            else
            {
                DataTable dataTrips;

                if (clientId != "")
                {
                    dataTrips = MainFunctions.NewQuery($"SELECT * FROM Trip WHERE id_client = {clientId} ORDER BY id_trip DESC");
                }
                else
                {
                    if (tripsIds == "all")
                    {
                        dataTrips = MainFunctions.NewQuery("SELECT * FROM Trip ORDER BY id_trip DESC");
                    }
                    else
                    {
                        dataTrips = MainFunctions.NewQuery($"SELECT * FROM Trip WHERE id_trip IN ({tripsIds}) ORDER BY id_trip DESC");
                    }
                }

                for (int i = 0; i < dataTrips.Rows.Count; i++)
                {
                    string tempTourName = MainFunctions.NewQuery($"SELECT name FROM Tour WHERE id_tour = {dataTrips.Rows[i][2].ToString()}").Rows[0][0].ToString();
                    string tempClientName = 
                        MainFunctions.NewQuery($"SELECT surname FROM Client WHERE id_client = {dataTrips.Rows[i][1].ToString()}").Rows[0][0].ToString() + " " +
                        MainFunctions.NewQuery($"SELECT name FROM Client WHERE id_client = {dataTrips.Rows[i][1].ToString()}").Rows[0][0].ToString() + " " + 
                        MainFunctions.NewQuery($"SELECT patronymic FROM Client WHERE id_client = {dataTrips.Rows[i][1].ToString()}").Rows[0][0].ToString();

                    int tempCityId = int.Parse(MainFunctions.NewQuery($"SELECT id_city FROM Tour WHERE id_tour={dataTrips.Rows[i][2].ToString()}").Rows[0][0].ToString());
                    string tempCity = MainFunctions.NewQuery($"SELECT [name] FROM City WHERE id_city = {tempCityId}").Rows[0][0].ToString();
                    string tempCountry = MainFunctions.NewQuery($"SELECT [name] FROM Country WHERE id_country = " +
                        $"(SELECT id_country FROM City WHERE id_city = {tempCityId})").Rows[0][0].ToString();

                    DataTable feedData = MainFunctions.NewQuery($"SELECT name,id_feed_type FROM Feed_type WHERE id_feed_type={dataTrips.Rows[i][4].ToString()}");
                    string tempFeedType = feedData.Rows[0][0].ToString();
                    int tempFeedTypeId = int.Parse(feedData.Rows[0][1].ToString());

                    DataTable roomData = MainFunctions.NewQuery($"SELECT name,id_room_type FROM Room_type WHERE id_room_type={dataTrips.Rows[i][3].ToString()}");
                    string tempRoomType = roomData.Rows[0][0].ToString();
                    int tempRoomTypeId = int.Parse(roomData.Rows[0][1].ToString());




                    string tempStartDate = dataTrips.Rows[i][5].ToString().Replace("0:00:00", "").Trim();
                    string tempEndDate = dataTrips.Rows[i][6].ToString().Replace("0:00:00", "").Trim();
                    string tempStatus = MainFunctions.NewQuery($"SELECT name FROM Trip_status WHERE id_trip_status = {dataTrips.Rows[i][8].ToString()}").Rows[0][0].ToString();
                    if (Convert.ToDateTime(tempStartDate).Date <= DateTime.Now)
                    {
                        if (Convert.ToDateTime(tempEndDate).Date < DateTime.Now)
                            tempStatus = "Окончена";
                        else
                            tempStatus = "В процессе";
                    }


                    string tempServices = "";
                    int[] tempServicesIds = new int[0];

                    DataTable servicesData = MainFunctions.NewQuery($"SELECT [Service].name, [Service].id_service FROM Trip_services  " +
                        $"JOIN[Service] ON[Service].id_service = Trip_services.id_service " +
                        $"WHERE id_trip = { dataTrips.Rows[i][0].ToString()}");
                    
                    for (int j = 0; j < servicesData.Rows.Count; j++)
                    {
                        Array.Resize(ref tempServicesIds, tempServicesIds.Length + 1);
                        tempServicesIds[tempServicesIds.Length - 1] = int.Parse(servicesData.Rows[j][1].ToString());

                        if (tempServices != "") tempServices += ", ";
                        tempServices += servicesData.Rows[j][0].ToString();
                    }

                    if (tempServices == "") tempServices = "Отсутствуют";


                    DataTable tourData = MainFunctions.NewQuery($"SELECT ticket_price, id_hotel FROM Tour WHERE id_tour = {dataTrips.Rows[i][2].ToString()}");


                    Trip dataTrip= new Trip()
                    {
                        TripId = int.Parse(dataTrips.Rows[i][0].ToString()),
                        ClientId = int.Parse(dataTrips.Rows[i][1].ToString()),
                        ClientName = tempClientName,
                        TourId = int.Parse(dataTrips.Rows[i][2].ToString()),
                        TourName = tempTourName,
                        Location = $"{tempCountry}, {tempCity}",
                        RoomType = tempRoomType,
                        FeedType = tempFeedType,
                        Services = tempServices,
                        StartDate = tempStartDate,
                        EndDate = tempEndDate,
                        TotalPrice = int.Parse(dataTrips.Rows[i][7].ToString()),
                        Status = tempStatus,
                        HotelId = int.Parse(tourData.Rows[0][1].ToString()),
                        TicketPrice = int.Parse(tourData.Rows[0][0].ToString()),
                        RoomTypeId = tempRoomTypeId,
                        FeedTypeId = tempFeedTypeId,
                        ServicesIds = tempServicesIds
                    };
                    TripsList.Add(dataTrip);
                }
                dtgTrips.ItemsSource = TripsList;

                if (TripsList.Count == 0) tbNothingFound.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Загрузка стран
        /// </summary>
        private void LoadCountries()
        {
            DataTable dataCountries = MainFunctions.NewQuery($"SELECT * FROM Country");
            for (int i = 0; i < dataCountries.Rows.Count; i++)
            {
                DataTable dataCities = MainFunctions.NewQuery($"SELECT id_city,[name] FROM City WHERE id_country=1");

                ObservableCollection<City> tempCitiesList = new ObservableCollection<City>();

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
                    CountryId = int.Parse(dataCountries.Rows[i][0].ToString()),
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
        private void LoadCities()
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
        /// Загрузка типов тура
        /// </summary>
        private void LoadTourTypes()
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
        #endregion

        #region Functions

        /// <summary>
        /// Событие "Скролл мыши"
        /// </summary>
        /// <param name="Delta"></param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            int lstDelta;

            if (Delta < 0) lstDelta = Delta / 60;
            else lstDelta = Delta / 60;


            if (lstCity.IsDropDownOpen && lstCity.IsMouseOver)
            {

                ScrollViewer scv = lstCity.Template.FindName("cmbScroll", lstCity) as ScrollViewer;
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

            if (dtgTrips.IsMouseOver)
            {
                ScrollViewer scv = dtgTrips.Template.FindName("DG_ScrollViewer", dtgTrips) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
            }
        }

        /// <summary>
        /// При изменении фильтра поиска
        /// </summary>
        private void FilterChanged()
        {
            if (tbxTourNameSearch != null && tbxTourNameSearch != null && tbxStartDate != null && tbxEndDate != null
                && lstCountry != null && lstCity != null && lstType != null && SecondItemCheckBoxCountry != null && borderClear != null)
            {
                if (tbxClientNameSearch.Text.Length == 0
                    && tbxTourNameSearch.Text.Length == 0
                    && tbxStartDate.Text.Length == 0
                    && tbxEndDate.Text.Length == 0
                    && SecondItemCheckBoxCountry.IsChecked == true
                    && SecondItemCheckBoxType.IsChecked == true
                    && cbAllDocsFilled.IsChecked == true
                    && cbSomeDocsFilled.IsChecked == true
                    && cbNoDocsFilled.IsChecked == true
                    && cbStatusWait.IsChecked == true
                    && cbStatusProgress.IsChecked == true
                    && cbStatusEnded.IsChecked == true)
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
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Client currentClient = new Client()
            {
                ClientId = (dtgTrips.SelectedItem as Trip).ClientId,
                Surname = (dtgTrips.SelectedItem as Trip).ClientName.Split(' ')[0],
                Name = (dtgTrips.SelectedItem as Trip).ClientName.Split(' ')[1],
                Patronymic = (dtgTrips.SelectedItem as Trip).ClientName.Split(' ')[2]
            };


            Tour currentTour = new Tour()
            {
                TourId = (dtgTrips.SelectedItem as Trip).TourId,
                Name = (dtgTrips.SelectedItem as Trip).TourName,
                Location = (dtgTrips.SelectedItem as Trip).Location,
                HotelId = (dtgTrips.SelectedItem as Trip).HotelId,
                TicketPrice = (dtgTrips.SelectedItem as Trip).TicketPrice
            };

            parentTabItemLink.ItemUserControl = new EditTrip(parentTabItemLink, this, thisPageParametres, currentTour, currentClient, (dtgTrips.SelectedItem as Trip));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool popupAnswer = false;
            Message messageDeleteWindow = new Message("Удаление", "Вы точно хотите удалить данную поездку? Действие невозможно отменить?", true, true);
            messageDeleteWindow.messageAnswer += value => popupAnswer = value;
            messageDeleteWindow.ShowDialog();

            if (popupAnswer)
            {
                int currentTripId = (dtgTrips.SelectedItem as Trip).TripId;
                MainFunctions.NewQuery($"DELETE FROM Trip_services WHERE id_trip = {currentTripId} " +
                    $"DELETE FROM Trip_docs WHERE id_trip = {currentTripId} " +
                    $"DELETE FROM Trip_feedback WHERE id_trip = {currentTripId} " +
                    $"DELETE FROM Trip WHERE id_trip = {currentTripId} ");

                Message messageSuccessWindow = new Message("Успех", "Поездка была успешно удалена!", false, false);
                messageSuccessWindow.ShowDialog();
                MainFunctions.AddLogRecord($"Deleted trip with ID {currentTripId}");

                TripsList.Remove(dtgTrips.SelectedItem as Trip);
            }

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearSearch();
        }

        public void ClearSearch()
        {
            tbxClientNameSearch.Text = "";
            tbxStartDate.Text = "";
            tbxEndDate.Text = "";
            cbAllDocsFilled.IsChecked = true;
            cbSomeDocsFilled.IsChecked = true;
            cbNoDocsFilled.IsChecked = true;
            cbStatusWait.IsChecked = true;
            cbStatusProgress.IsChecked = true;
            cbStatusEnded.IsChecked = true;
            tbxTourNameSearch.Text = "";
            SecondItemCheckBoxCountry.IsChecked = true;
            SecondItemCheckBoxType.IsChecked = true;
            LoadCities();

            borderClear.Visibility = Visibility.Hidden;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadCities();
            if (borderClear.Visibility == Visibility.Hidden)
            {
                LoadTrips("all");
            }
            else
            {
                if (!CheckDatesAreValid())
                {
                    Message messageErrorDate = new Message("Ошибка", "Неверно введены даты", false, false);
                    messageErrorDate.ShowDialog();
                }
                else
                {

                    string condition = "";

                    if (tbxClientNameSearch.Text != "")
                    {
                        string[] fullName = tbxClientNameSearch.Text.Split(' ');
                        string[] name = new string[3];
                        int currentNameIndex = 0;
                        for (int i = 0; i < fullName.Length; i++)
                        {
                            fullName[i] = fullName[i].Trim();
                            if (MainFunctions.validateName(fullName[i]) && currentNameIndex < 3)
                            {
                                name[currentNameIndex] = fullName[i];
                                currentNameIndex++;
                            }
                            if (currentNameIndex >= 3) break;
                        }

                        if (name[0] != null && MainFunctions.validateName(name[0].ToLower()))
                        {
                            condition += String.Format("WHERE (Client.surname LIKE '%{0}%' OR Client.name LIKE '%{0}%' OR Client.patronymic LIKE '%{0}%')", name[0]);
                        }
                        if (name[1] != null && MainFunctions.validateName(name[1].ToLower()))
                        {
                            if (condition == "") condition += "WHERE ";
                            else condition += " AND ";

                            condition += String.Format("(Client.surname LIKE '%{0}%' OR Client.name LIKE '%{0}%' OR Client.patronymic LIKE '%{0}%')", name[1]);
                        }
                        if (name[2] != null && MainFunctions.validateName(name[2].ToLower()))
                        {
                            if (condition == "") condition += "WHERE ";
                            else condition += " AND ";

                            condition += String.Format("(Client.surname LIKE '%{0}%' OR Client.name LIKE '%{0}%' OR Client.patronymic LIKE '%{0}%')", name[2]);
                        }
                    }

                    if (tbxTourNameSearch.Text != "")
                    {
                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("Tour.name LIKE '%{0}%'", tbxTourNameSearch.Text);
                    }

                    if (tbxStartDate.Text != "")
                    {
                        string startDate = tbxStartDate.Text.Remove(4, 1).Remove(6, 1);

                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("Trip.start_date >= '{0}'", startDate);
                    }

                    if (tbxEndDate.Text != "")
                    {
                        string endDate = tbxEndDate.Text.Remove(4, 1).Remove(6, 1);

                        if (condition == "") condition += "WHERE ";
                        else condition += " AND ";

                        condition += String.Format("Trip.end_date <= '{0}'", endDate);
                    }

                    string countriesCondition = "";
                    string citiesCondition = "";
                    if (SecondItemCheckBoxCountry.IsChecked == false)
                    {
                        int[] checkedCountries = CommonFunctions.CheckedCountries(CountriesList);
                        countriesCondition = string.Join(",", checkedCountries);
                        string countriesStart = " AND ";
                        if (condition == "") countriesStart = " WHERE ";
                        if (checkedCountries.Length > 0) countriesCondition = $"{countriesStart} (Country.id_country IN ({countriesCondition}))";

                        if (checkedCountries.Length == 1)
                        {
                            int[] checkedCities = CommonFunctions.CheckedCities(CurrentCitiesList);
                            citiesCondition = string.Join(",", checkedCities);
                            string citiesStart = " AND ";
                            if (condition == "" && countriesCondition == "") citiesStart = " WHERE ";
                            if (checkedCities.Length > 0) citiesCondition = $"{citiesStart} (City.id_city IN ({citiesCondition}))";
                        }
                    }
                    condition += countriesCondition + citiesCondition;

                    string tourTypeCondition = "";
                    if (SecondItemCheckBoxType.IsChecked == false)
                    {
                        int[] checkedTourTypes = CommonFunctions.CheckedTourTypes(TourTypesList);
                        tourTypeCondition = string.Join(",", checkedTourTypes);
                        string typeStart = " AND ";
                        if (condition == "") typeStart = " WHERE ";
                        if (checkedTourTypes.Length > 0) tourTypeCondition = $"{typeStart} Tour_types.id_type IN ({tourTypeCondition})";
                    }
                    condition += tourTypeCondition;

                    string tripStatusName = "";
                    if (cbStatusWait.IsChecked == true || cbStatusProgress.IsChecked == true || cbStatusEnded.IsChecked == true)
                    {
                        string startWord = " AND ";
                        if (condition == "") startWord = " WHERE ";

                        string statusCondition = "";
                        if (cbStatusWait.IsChecked == true)
                        {
                            statusCondition += "(Trip.start_date > GETDATE())";
                            tripStatusName += "принята";
                        }
                        if (cbStatusProgress.IsChecked == true)
                        {
                            if (statusCondition != "") statusCondition += " OR ";
                            if (tripStatusName != "") tripStatusName += ", ";
                            statusCondition += "(Trip.start_date <= GETDATE() AND Trip.end_date >= GETDATE())";
                            tripStatusName += "в процессе";
                        }
                        if (cbStatusEnded.IsChecked == true)
                        {
                            if (statusCondition != "") statusCondition += "OR";
                            if (tripStatusName != "") tripStatusName += ", ";
                            statusCondition += "(Trip.end_date < GETDATE())";
                            tripStatusName += "окончена";
                        }


                        condition += $"{startWord} ({statusCondition})";
                    }

                    string docsStatusName = "";
                    if ((cbAllDocsFilled.IsChecked == true || cbNoDocsFilled.IsChecked == true || cbSomeDocsFilled.IsChecked == true)
                        && !(cbAllDocsFilled.IsChecked == true && cbNoDocsFilled.IsChecked == true && cbSomeDocsFilled.IsChecked == true))
                    {
                        string startWord = " AND ";
                        if (condition == "") startWord = " WHERE ";

                        string docCondition = "";

                        string allDocs = string.Format(" (Trip_docs.transfer_doc {1} NULL " +
                            "{0} Trip_docs.ticket_doc {1} NULL " +
                            "{0} Trip_docs.visa_doc {1} NULL " +
                            "{0} Trip_docs.checkin_doc {1} NULL " +
                            "{0} Trip_docs.insurance_doc {1} NULL) ", "AND", "IS NOT");
                        string noDocs = string.Format(" (Trip_docs.transfer_doc {1} NULL " +
                            "{0} Trip_docs.ticket_doc {1} NULL " +
                            "{0} Trip_docs.visa_doc {1} NULL " +
                            "{0} Trip_docs.checkin_doc {1} NULL " +
                            "{0} Trip_docs.insurance_doc {1} NULL) ", "AND", "IS");
                        string someDocs = string.Format(" (Trip_docs.transfer_doc {1} NULL " +
                            "{0} Trip_docs.ticket_doc {1} NULL " +
                            "{0} Trip_docs.visa_doc {1} NULL " +
                            "{0} Trip_docs.checkin_doc {1} NULL " +
                            "{0} Trip_docs.insurance_doc {1} NULL) ", "OR ", "IS NOT");

                        if (cbAllDocsFilled.IsChecked == true)
                        {
                            docCondition += allDocs;
                            docsStatusName += "заполнено";
                        }
                        if (cbNoDocsFilled.IsChecked == true)
                        {
                            if (docCondition != "") docCondition += " OR ";
                            if (docsStatusName != "") docsStatusName += ", ";
                            docCondition += noDocs;
                            docsStatusName += "частично заполнено";
                        }
                        if (cbSomeDocsFilled.IsChecked == true)
                        {
                            if (docCondition != "") docCondition += " OR ";
                            if (docsStatusName != "") docsStatusName += ", ";
                            docCondition += $"({someDocs} AND NOT {allDocs})";
                            docsStatusName += "не заполнено";
                        }

                        condition += $"{startWord} ({docCondition})";
                    }
                    else
                    {
                        docsStatusName = "все";
                    }

                    string tempstartWord = " AND ";
                    if (condition == "") tempstartWord = " WHERE ";
                    condition += $"{tempstartWord} Trip.id_status = 2";


                    string MainCondition = "SELECT DISTINCT Trip.id_trip FROM Trip " +
                        "JOIN Tour ON Tour.id_tour = Trip.id_tour " +
                        "JOIN City ON Tour.id_city = City.id_city " +
                        "JOIN Country ON Country.id_country = City.id_country " +
                        "JOIN Trip_docs ON Trip_docs.id_trip = Trip.id_trip " +
                        "JOIN Client ON Client.id_client = Trip.id_client " +
                        "JOIN Tour_types ON Tour_types.id_tour = Tour.id_tour " +
                        "JOIN Trip_status ON Trip_status.id_trip_status = Trip.id_status " +
                        $"{condition}";

                    DataTable dataTrips = MainFunctions.NewQuery($"{MainCondition}");
                    string tripsIds = "";
                    for (int i = 0; i < dataTrips.Rows.Count; i++)
                    {
                        if (tripsIds != "") tripsIds += ",";
                        tripsIds += dataTrips.Rows[i][0].ToString();
                    }

                    LoadTrips(tripsIds);

                    MainFunctions.AddLogRecord($"Search trip with condition:" +
                        $"\n\tClient name: {tbxClientNameSearch.Text}" +
                        $"\n\tTour name: {tbxTourNameSearch.Text}" +
                        $"\n\tTrip date: from _{tbxStartDate.Text} to _{tbxEndDate.Text}" +
                        $"\n\tClient docs status: {docsStatusName}" +
                        $"\n\tTrip status: {tripStatusName}");
                }
            }
        }

        private void btnFormPdfDoc_Click(object sender, RoutedEventArgs e)
        {
            //создание pdf документа


            //System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            //saveFileDialog.FileName = null;
            //saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //saveFileDialog.Filter = $"Pdf-документ pdf(*.pdf)|*.pdf|Все файлы (*.*)|*.*";
            //saveFileDialog.ShowDialog();
            //if (saveFileDialog.FileName != null && saveFileDialog.FileName != "")
            //{
            //    string pdfFilePath = saveFileDialog.FileName;
            //    //string fileName = $"TripID{(dtgTrips.SelectedItem as Trip).TripId} " +
            //        //$"TourId{(dtgTrips.SelectedItem as Trip).TourId} ClientID{(dtgTrips.SelectedItem as Trip).ClientId}";

            //    var document = new iTextSharp.text.Document();
            //    using (var writer = PdfWriter.GetInstance(document, new FileStream(pdfFilePath, FileMode.Create)))
            //    {
            //        document.Open();



            //        writer.DirectContent.MoveTo(35, 780);
            //        writer.DirectContent.LineTo(430, 780);

            //        iTextSharp.text.Font helvetica = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12);
            //        BaseFont helveticaBase = helvetica.GetCalculatedBaseFont(false);
            //        writer.DirectContent.BeginText();
            //        writer.DirectContent.SetFontAndSize(helveticaBase, 12f);
            //        writer.DirectContent.ShowTextAligned(iTextSharp.text.Element.ALIGN_LEFT, "Подтверждение бронирования", 35, 766, 0);
            //        writer.DirectContent.EndText();



            //        document.Close();
            //        writer.Close();
            //    }

            //    (new Message("ok","ok",false,false)).ShowDialog();

            //}

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ChooseSaveTripsFormat saveFormatWindow = new ChooseSaveTripsFormat(TripsList);
            saveFormatWindow.ShowDialog();
        }
        #endregion

        #region TextChanges
        private void tbxClientNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.validateName(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }
            FilterChanged();
        }

        private void tbxTourNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.validateTourName(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }

            FilterChanged();
        }

        private void tbxStartDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            FilterChanged();
        }

        private void tbxEndDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            FilterChanged();
        }

        private bool CheckDatesAreValid()
        {
            bool valid = true;
            if (tbxStartDate.Text.Length != 0)
                valid = valid && CommonFunctions.isTextDateValid(tbxStartDate.Text);

            if (tbxEndDate.Text.Length != 0)
            {
                valid = valid && CommonFunctions.isTextDateValid(tbxEndDate.Text);

                if (valid && tbxStartDate.Text.Length != 0)
                    valid = valid && (Convert.ToDateTime(tbxEndDate.Text) > Convert.ToDateTime(tbxStartDate.Text));
            }

            return valid;
        }
        #endregion

        #region lstDocs
        private void lstDocs_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void lstDocs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }
        #endregion

        #region lstStatus
        private void lstStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void lstStatus_DropDownClosed(object sender, EventArgs e)
        {
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

        #region DataGridEvents
        private void dtgTrips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgTrips.SelectedIndex != -1)
            {
                borderDeleteButton.IsEnabled = true;

                string status = (dtgTrips.SelectedItem as Trip).Status;
                if (status == "Принята" || status == "Заявка")
                    borderEditButton.IsEnabled = true;
                else
                    borderEditButton.IsEnabled = false;
            }
            else
            {
                borderEditButton.IsEnabled = false;
                borderDeleteButton.IsEnabled = false;

            }
        }

        private void ShowHideDetails(object sender, RoutedEventArgs e)
        {
            Button tempBtn = sender as Button;
            if (tempBtn.Content.ToString() == "+") tempBtn.Content = "-";
            else tempBtn.Content = "+";

            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                    row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }
        #endregion

        
    }
}
