using DreamTrip.Classes;
using DreamTrip.Functions;
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
using System.Windows.Threading;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для NewTour.xaml
    /// </summary>
    public partial class NewTour : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Новый тур", "../../Resources/new_tour.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ObservableCollection<Country> countriesList { get; set; } = new ObservableCollection<Country>();
        ObservableCollection<City> currentCitiesList { get; set; } = new ObservableCollection<City>();
        ObservableCollection<Hotel> currentHotelsList { get; set; } = new ObservableCollection<Hotel>();
        ObservableCollection<TourType> tourTypesList { get; set; } = new ObservableCollection<TourType>();
        ObservableCollection<TourService> tourServiceList { get; set; } = new ObservableCollection<TourService>();
        ComboBoxItem tempFirstItemCity;
        ComboBoxItem tempSecondItemCity;
        ComboBoxItem tempFistItemHotel;
        ComboBoxItem tempSecondItemHotel;
        Tour editedTour;
        string tourPhotoPath;
        BitmapImage tourphoto;
        #endregion

        #region Constructor
        public NewTour(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres, Tour tempEditedTour = null)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;

            tourPhotoPath = MainFunctions.GetAppPath() + "/Resources/ToursPhotos/default.png";
            tourphoto = GetBitmapImageFromPath(tourPhotoPath);
            editedTour = tempEditedTour;
            if (tempEditedTour == null)
                editedTour = new Tour()
                {
                    TourId = 0,
                    ImageSource = tourphoto
                };
            DataContext = editedTour;
            LoadTourTypes();
            LoadCountries();
            LoadServices();
            tempFirstItemCity = cmbCity.Items[0] as ComboBoxItem;
            tempSecondItemCity = cmbCity.Items[1] as ComboBoxItem;
            tempFistItemHotel = cmbHotel.Items[0] as ComboBoxItem;
            tempSecondItemHotel = cmbHotel.Items[1] as ComboBoxItem;
            if (tempEditedTour != null)
            {
                btnCreate.Content = "Применить";
                tbNewTour.Text = "Изменить тур";
                thisPageParametres[1] = tempEditedTour.Name;
                LoadTourInfo();
            }

            double[] sizes = MainFunctions.MenuLink.GetWidthHeight();
            WindowSizeChanged(sizes[0], sizes[1]);

            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

        }
        #endregion

        #region LoadData
        /// <summary>
        /// Загрузка информации о туре
        /// </summary>
        private void LoadTourInfo()
        {
            DataTable locationData = MainFunctions.NewQuery($"SELECT t.id_city,  c.id_country,  t.id_hotel " +
                $"FROM Tour t  JOIN City c ON t.id_city = c.id_city WHERE t.id_tour = {editedTour.TourId}");

            tourPhotoPath = "no";

            int tempCityId = int.Parse(locationData.Rows[0][0].ToString());
            int tempCountryId = int.Parse(locationData.Rows[0][1].ToString());
            int tempHotelId = int.Parse(locationData.Rows[0][2].ToString());

            tbxTourName.Text = editedTour.Name;
            tbxPrice.Text = editedTour.TicketPrice.ToString();
            tbxDescription.Text = editedTour.Description;
            tourphoto = editedTour.ImageSource;

            for (int i = 0; i < tourTypesList.Count; i++)
            {
                for (int j = 0; j < editedTour.TypesIds.Count; j++)
                {
                    if (tourTypesList[i].TourTypeId == editedTour.TypesIds[j])
                    {
                        tourTypesList[i].IsChecked = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < tourServiceList.Count; i++)
            {
                for (int j = 0; j < editedTour.ServicesIds.Count; j++)
                {
                    if (tourServiceList[i].ServiceId == editedTour.ServicesIds[j])
                    {
                        tourServiceList[i].IsChecked = true;
                        break;
                    }
                }
            }

            for (int i = 0;i < countriesList.Count; i++)
            {
                if (countriesList[i].CountryId == tempCountryId)
                {
                    cmbCountry.SelectedItem = countriesList[i];
                }
            }

            for (int i = 0; i < currentCitiesList.Count; i++)
            {
                if (currentCitiesList[i].CityId == tempCityId)
                {
                    cmbCity.SelectedItem = currentCitiesList[i];
                }
            }

            for (int i = 0; i < currentHotelsList.Count; i++)
            {
                if (currentHotelsList[i].HotelId == tempHotelId)
                {
                    cmbHotel.SelectedItem = currentHotelsList[i];
                }
            }

        }

        /// <summary>
        /// Загрузка сервисов
        /// </summary>
        private void LoadServices()
        {
            DataTable serviceData = MainFunctions.NewQuery($"SELECT id_service, [name] FROM [Service]");

            for (int i = 0; i < serviceData.Rows.Count; i++)
            {
                TourService tourService = new TourService()
                {
                    ServiceId = int.Parse(serviceData.Rows[i][0].ToString()),
                    ServiceName = serviceData.Rows[i][1].ToString(),
                    IsChecked = false
                };

                tourServiceList.Add(tourService);
                lstService.Items.Add(tourService);
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

                ObservableCollection<City> tempCitiesList = new ObservableCollection<City>();

                for (int j = 0; j < dataCities.Rows.Count; j++)
                {
                    City dataCity = new City()
                    {
                        CityId = int.Parse(dataCities.Rows[j][0].ToString()),
                        CityName = dataCities.Rows[j][1].ToString()
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

                countriesList.Add(dataCountry);
                cmbCountry.Items.Add(dataCountry);
            }
        }

        /// <summary>
        /// Загрузка городов
        /// </summary>
        void LoadCities()
        {
            currentCitiesList.Clear();
            cmbCity.Items.Clear();
            if (cmbCountry.SelectedIndex != -1 && cmbCountry.SelectedIndex != 0)
            {
                cmbCity.Items.Add(tempFirstItemCity);
                ObservableCollection<City> tempCitiesList = (cmbCountry.SelectedItem as Country).CitiesList;
                for (int i = 0; i < tempCitiesList.Count; i++)
                {
                    currentCitiesList.Add(tempCitiesList[i]);
                    cmbCity.Items.Add(tempCitiesList[i]);
                }
            }
            else
            {
                cmbCity.Items.Add(tempFirstItemCity);
                cmbCity.Items.Add(tempSecondItemCity);
            }
            cmbCity.SelectedIndex = 0;
        }

        /// <summary>
        /// Загрузка отелей
        /// </summary>
        void LoadHotels()
        {
            currentHotelsList.Clear();
            cmbHotel.Items.Clear();
            if (cmbCity.SelectedIndex != -1 && cmbCity.SelectedIndex != 0)
            {
                cmbHotel.Items.Add(tempFistItemHotel);
                DataTable hotelData = MainFunctions.NewQuery($"SELECT * FROM Hotel WHERE id_city = {(cmbCity.SelectedItem as City).CityId}");
                for (int i = 0; i < hotelData.Rows.Count; i++)
                {
                    Hotel hotel = new Hotel()
                    {
                        HotelId = int.Parse(hotelData.Rows[i][0].ToString()),
                        HotelName = hotelData.Rows[i][1].ToString()
                    };
                    currentHotelsList.Add(hotel);
                    cmbHotel.Items.Add(hotel);
                }
            }
            else
            {
                cmbHotel.Items.Add(tempFistItemHotel);
                cmbHotel.Items.Add(tempSecondItemHotel);
            }
            cmbHotel.SelectedIndex = 0;
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

                tourTypesList.Add(dataTourType);
                lstType.Items.Add(dataTourType);
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void FilterChanged()
        {
            if (tbxTourName != null && tbxPrice != null && cmbCountry != null
                && cmbCity != null && lstType != null && lstService != null && cmbHotel != null && 
                tbxDescription != null )
            {
                int[] checkedTourTypes = CommonFunctions.CheckedTourTypes(tourTypesList);
                if (tbxTourName.Text.Length > 1
                    && tbxPrice.Text.Length > 1
                    && checkedTourTypes.Length > 0 
                    && tbxDescription.Text.Length > 0
                    && tourPhotoPath != null
                    && cmbHotel.SelectedIndex != 0)
                {
                    borderCreateButton.IsEnabled = true;
                }
                else
                {
                    borderCreateButton.IsEnabled = false;
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

            if (cmbCountry.IsDropDownOpen && cmbCountry.IsMouseOver)
            {

                ScrollViewer scv = cmbCountry.Template.FindName("cmbScroll", cmbCountry) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (cmbCity.IsDropDownOpen && cmbCity.IsMouseOver)
            {

                ScrollViewer scv = cmbCity.Template.FindName("cmbScroll", cmbCity) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (cmbHotel.IsDropDownOpen && cmbHotel.IsMouseOver)
            {

                ScrollViewer scv = cmbHotel.Template.FindName("cmbScroll", cmbHotel) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (lstType.IsDropDownOpen && lstType.IsMouseOver)
            {

                ScrollViewer scv = lstType.Template.FindName("cmbScroll", lstType) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            if (lstService.IsDropDownOpen && lstService.IsMouseOver)
            {

                ScrollViewer scv = lstService.Template.FindName("cmbScroll", lstService) as ScrollViewer;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - lstDelta);
                return;
            }

            //scvMain.ScrollToVerticalOffset(scvMain.VerticalOffset - Delta);

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
        /// При изменении размеров окна
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void WindowSizeChanged(double width, double height)
        {

            //if (width > 1300)
            //{
            //    this.Width = 1300;
            //    mainGrid.Width = 1200;
            //    firstWrapPanel.Orientation = Orientation.Horizontal;
            //    secondWrapPanel.Orientation = Orientation.Horizontal;
            //    firstWrapPanel.Width = 1200;
            //    secondWrapPanel.Width = 1200;
            //}
            //else
            //{
            //    this.Width = width - 20;
            //    firstWrapPanel.Orientation = Orientation.Horizontal;
            //    secondWrapPanel.Orientation = Orientation.Horizontal;
            //    firstWrapPanel.Width = 1200;
            //    secondWrapPanel.Width = 1200;

            //    if (this.Width <= 1200)
            //    {
            //        mainGrid.Width = this.Width - 40;

            //        if (mainGrid.Width <= 1100)
            //        {
            //            firstWrapPanel.Orientation = Orientation.Vertical;
            //            secondWrapPanel.Orientation = Orientation.Vertical;
            //            firstWrapPanel.Width = 420;
            //            secondWrapPanel.Width = 420;
            //        }
            //    }

            //}

        }

        /// <summary>
        /// Внести информацию об отредактированном туре в БД
        /// </summary>
        private void EditTour()
        {
            DataTable clientServicesData = MainFunctions.NewQuery($"SELECT DISTINCT ts.id_service, s.name FROM Trip_services ts " +
                $"JOIN Service s ON s.id_service = ts.id_service " +
                $"WHERE id_trip IN " +
                $"(SELECT id_trip FROM Trip WHERE id_tour = {editedTour.TourId} AND start_date <= getdate() AND end_date >= getdate())");
            for (int i = 0; i < clientServicesData.Rows.Count; i++)
            {
                int currentClientService = int.Parse(clientServicesData.Rows[i][0].ToString());
                bool flag = false;
                for (int j = 0; j < tourServiceList.Count; j++)
                {
                    if (tourServiceList[j].IsChecked)
                        flag = flag || (tourServiceList[j].ServiceId == currentClientService);
                    if (flag) break;
                }
                if (!flag)
                {
                    Message serviceErrorMessage = new Message("Ошибка", "Невозможно изменить сервисы тура, " +
                        $"поскольку клиентами еще используется сервис {clientServicesData.Rows[i][1].ToString()}", false, false);
                    serviceErrorMessage.ShowDialog();
                    return;
                }
            }


            try
            {
                bool putImageResult = true;
                if (tourPhotoPath != "no")
                {
                    putImageResult = MainFunctions.PutTourImageInDb(tourPhotoPath, editedTour.TourId);
                }

                MainFunctions.NewQuery($"DELETE FROM Tour_services WHERE id_tour = {editedTour.TourId}");
                int[] checkedServices = CommonFunctions.CheckedServicesIds(tourServiceList);
                for (int i = 0; i < checkedServices.Length; i++)
                {
                    MainFunctions.NewQuery($"INSERT INTO Tour_services VALUES ({editedTour.TourId}, {checkedServices[i]})");
                }

                MainFunctions.NewQuery($"DELETE FROM Tour_types WHERE id_tour = {editedTour.TourId}");
                int[] checkedTourTypes = CommonFunctions.CheckedTourTypes(tourTypesList);
                for (int i = 0; i < checkedTourTypes.Length; i++)
                {
                    MainFunctions.NewQuery($"INSERT INTO Tour_types VALUES ({editedTour.TourId}, {checkedTourTypes[i]})");
                }

                MainFunctions.NewQuery($"UPDATE Tour SET name = '{tbxTourName.Text}', ticket_price = {tbxPrice.Text}, " +
                    $"description = '{tbxDescription.Text}', id_city = {(cmbCity.SelectedItem as City).CityId}, " +
                    $"id_hotel = {(cmbHotel.SelectedItem as Hotel).HotelId}" +
                    $" WHERE id_tour = {editedTour.TourId}");


                string imageError = "";
                if (!putImageResult) imageError = "Однако, в связи с неправильным форматом изображения, фотография тура изменена не была.";

                Message successMessage = new Message("Успех", $"Тур был успешно изменен. {imageError}", false, false);
                successMessage.ShowDialog();

                MainFunctions.AddLogRecord($"Tour edited:" +
                    $"\n\tID: {editedTour.TourId}" +
                    $"\n\tOld Name: {editedTour.Name}" +
                    $"\n\tNew Name: {tbxTourName.Text}" +
                    $"{(putImageResult ? "" : "\n\tPhoto error: wrong format")}");


                parentTabItemLink.ItemUserControl = previousPage;
                MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
                (previousPage as TourInfo).LoadAllData();
                
            }
            catch (Exception ex)
            {
                MainFunctions.AddLogRecord($"Tour editing ERROR text: " + ex.Message);

                Message errorMessage = new Message("Ошибка", $"Что-то пошло не так.", false, false);
                errorMessage.ShowDialog();
            }

        }

        /// <summary>
        /// Внести информацию о новом туре в БД
        /// </summary>
        private void CreateNewTour()
        {
            int currentTourId = -1;
            try
            {
                MainFunctions.NewQuery($"INSERT INTO Tour VALUES ('{tbxTourName.Text}', " +
                    $"{tbxPrice.Text}, '{tbxDescription.Text}', " +
                    $"{(cmbCity.SelectedItem as City).CityId}, {(cmbHotel.SelectedItem as Hotel).HotelId})");

                currentTourId = int.Parse(MainFunctions.NewQuery($"SELECT MAX(id_tour) FROM Tour").Rows[0][0].ToString());
                int[] checkedTourTypes = CommonFunctions.CheckedTourTypes(tourTypesList);
                for (int i = 0; i < checkedTourTypes.Length; i++)
                {
                    MainFunctions.NewQuery($"INSERT INTO Tour_types VALUES ({currentTourId}, {checkedTourTypes[i]})");
                }

                int[] checkedServices = CommonFunctions.CheckedServicesIds(tourServiceList);
                for (int i = 0; i < checkedServices.Length; i++)
                {
                    MainFunctions.NewQuery($"INSERT INTO Tour_services VALUES ({currentTourId}, {checkedServices[i]})");
                }

                bool putImageResult = MainFunctions.PutTourImageInDb(tourPhotoPath, currentTourId);

                string imageError = "";
                if (!putImageResult) imageError = "Однако в связи с неправильным форматом изображения фотография тура не была добавлена.";
                Message successMessage = new Message("Успех", $"Новый тур был успешно добавлен. {imageError}", false, false);
                successMessage.ShowDialog();

                MainFunctions.AddLogRecord($"Tour created:" +
                    $"\n\tID: {currentTourId}" +
                    $"\n\tName: {tbxTourName.Text}" +
                    $"{(putImageResult ? "" : "\n\tPhoto error: wrong format")}");


                parentTabItemLink.ItemUserControl = previousPage;
                MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
            }
            catch (Exception ex)
            {
                MainFunctions.AddLogRecord($"Tour creating ERROR text: "  + ex.Message);
                MainFunctions.NewQuery($"DELETE FROM Tour_photos WHERE id_tour = {currentTourId} " +
                    $"DELETE FROM Tour_services WHERE id_tour = {currentTourId} " +
                    $"DELETE FROM Tour_types WHERE id_tour = {currentTourId} " +
                    $"DELETE FROM Tour WHERE id_tour = {currentTourId}");
                Message errorMessage = new Message("Ошибка", "Что-то пошло не так. Данные о новом туре не сохранены", false, false);
                errorMessage.ShowDialog();
            }

        }

        /// <summary>
        /// Получить объект (изображение) типа BitmapImage
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private BitmapImage GetBitmapImageFromPath(string imagePath)
        {
            BitmapImage tempTourImage = new BitmapImage();
            tempTourImage.BeginInit();
            tempTourImage.CacheOption = BitmapCacheOption.OnLoad;
            FileStream stream = File.OpenRead(imagePath);
            tempTourImage.StreamSource = stream;
            tempTourImage.EndInit();
            stream.Close();

            return tempTourImage;
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
            if (int.Parse(MainFunctions.NewQuery($"SELECT COUNT(*) FROM Tour WHERE name = '{tbxTourName.Text}' AND id_tour != {editedTour.TourId}").Rows[0][0].ToString()) > 0)
            {
                new Message($"Ошибка","Тур с таким названием уже есть").ShowDialog();
            }
            else
            {
                tbFileUpload.Text = "Сохранение данных...";
                gridFileLoad.Visibility = Visibility.Visible;

                DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
                dispatcherTimer.Start();
                dispatcherTimer.Tick += new EventHandler((object c, EventArgs eventArgs) =>
                {
                    if (editedTour.TourId == 0)
                        CreateNewTour();
                    else
                        EditTour();

                    gridFileLoad.Visibility = Visibility.Hidden;
                    ((DispatcherTimer)c).Stop();
                });
            }
        }

        private void imgTourPhoto_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.FileName = null;
            tbFileUpload.Text = "Загрузка файла";
            gridFileLoad.Visibility = Visibility.Visible;
            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tourPhotoPath = openFileDialog.FileName;
                    tourphoto = GetBitmapImageFromPath(tourPhotoPath);
                    editedTour.ImageSource = tourphoto;
                }
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Возможно, неверно был выбран тип файла. Файл должен иметь формат изображения.", false, false);
                messageError.ShowDialog();
                MainFunctions.AddLogRecord("Choose New Tour Image Error: " + ex.Message);
            }
            gridFileLoad.Visibility = Visibility.Hidden;
            FilterChanged();
        }
        #endregion

        #region TextChanged
        private void tbxTourName_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbxPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbTop = sender as TextBox;

            char[] charList = tbTop.Text.Trim().ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out int tempOut) == false)
                {
                    tbTop.Text = tbTop.Text.Remove(i, 1);
                }
            }

            if (tbTop.Text.Length > 9) tbTop.Text = tbTop.Text.Substring(0, 9);

            PriceChanged(sender);
        }

        private void tbxDescription_TextChanged(object sender, TextChangedEventArgs e)
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

        #region SelectionChanged
        private void lstType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void lstService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 0);
        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCity != null) LoadCities();
            FilterChanged();
        }

        private void cmbCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHotel != null) LoadHotels();
            FilterChanged();
        }

        private void cmbHotel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterChanged();
        }
        #endregion

        #region DropDowns
        private void lstType_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);
        }

        private void lstType_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();
        }

        private void lstService_DropDownOpened(object sender, EventArgs e)
        {
            CommonFunctions.ComboBoxSelectionChanged(sender, 1);
        }

        private void lstService_DropDownClosed(object sender, EventArgs e)
        {
            FilterChanged();

        }
        #endregion

    }
}
