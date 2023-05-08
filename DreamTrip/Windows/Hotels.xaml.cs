using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
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

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Hotels.xaml
    /// </summary>
    public partial class Hotels : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        List<Hotel> hotelsList = new List<Hotel>();
        List<City> allCitiesList = new List<City>();
        #endregion

        #region Constructor
        public Hotels(TabClass tempTabItem)
        {
            InitializeComponent();
            parentTabItemLink = tempTabItem;
            LoadCities();
            LoadHotels();
        }

        #endregion

        #region LoadData
        private void LoadHotels()
        {
            hotelsList.Clear();
            dtgHotels.ItemsSource = null;

            DataTable hotelsData = MainFunctions.NewQuery($"SELECT * FROM Hotel ORDER BY name");
            for (int i = 0; i < hotelsData.Rows.Count; i++)
            {
                int starsCount = int.Parse(hotelsData.Rows[i][2].ToString());

                Hotel hotel = new Hotel()
                {
                    HotelId = int.Parse(hotelsData.Rows[i][0].ToString()),
                    HotelName = hotelsData.Rows[i][1].ToString(),
                    StarsCount = starsCount,
                    Star1 = 1 <= starsCount,
                    Star2 = 2 <= starsCount,
                    Star3 = 3 <= starsCount,
                    Star4 = 4 <= starsCount,
                    Star5 = 5 <= starsCount,
                    AllCities = allCitiesList,
                    HotelCity = GetCity(int.Parse(hotelsData.Rows[i][3].ToString()))
                };

                hotelsList.Add(hotel);
            }

            dtgHotels.ItemsSource = hotelsList;
        }

        private void LoadCities()
        {
            allCitiesList.Clear();

            DataTable citiesData = MainFunctions.NewQuery($"SELECT * FROM City ORDER BY name");
            for (int i = 0; i < citiesData.Rows.Count; i++)
            {

                City city = new City()
                {
                    CityId = int.Parse(citiesData.Rows[i][0].ToString()),
                    CityName = citiesData.Rows[i][1].ToString(),
                };

                allCitiesList.Add(city);
            }
        }
        #endregion

        #region Functions
        private City GetCity(int cityId)
        {
            City tempCity = new City(); ;
            for (int i = 0; i < allCitiesList.Count; i++)
            {
                if (allCitiesList[i].CityId == cityId)
                {
                    tempCity = allCitiesList[i];
                    break;
                }
            }
            return tempCity;
        }

        private Hotel GetHotel(int hotelId)
        {
            Hotel tempHotel = new Hotel(); ;
            for (int i = 0; i < hotelsList.Count; i++)
            {
                if (hotelsList[i].HotelId == hotelId)
                {
                    tempHotel = hotelsList[i];
                    break;
                }
            }
            return tempHotel;
        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            switch (MainFunctions.GetUserRole())
            {
                case "manager":
                    parentTabItemLink.ItemUserControl = new ManagerMenuUserControl(parentTabItemLink);
                    break;
                case "admin":
                    parentTabItemLink.ItemUserControl = new AdminMenuUserControl(parentTabItemLink);
                    break;
                case "analyst":
                    parentTabItemLink.ItemUserControl = new AnalystMenuUserControl(parentTabItemLink);
                    break;
            }
            parentTabItemLink.VerticalScrollBarVisibility = "Auto";
            parentTabItemLink.ItemHeaderText = "Меню";
            parentTabItemLink.ItemHeaderImageSource = "../Resources/list.png";
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int hotelId = (dtgHotels.SelectedItem as Hotel).HotelId;
            string hotelName = (dtgHotels.SelectedItem as Hotel).HotelName;

            if (hotelId == 0)
            {
                hotelsList.Remove(dtgHotels.SelectedItem as Hotel);
                dtgHotels.ItemsSource = null;
                dtgHotels.ItemsSource = hotelsList;
            }
            else
            {
                bool answer = false;
                Message deleteWindow = new Message("Вопрос", $"Вы уверены, что хотите удалить отель \"{hotelName}\"", true, true);
                deleteWindow.messageAnswer += value => answer = value;
                deleteWindow.ShowDialog();

                if (!answer)
                {
                    return;
                }


                // если отель привязан к туру - удалять нельзя
                DataTable linkedTours = MainFunctions.NewQuery($"SELECT name FROM tour WHERE id_hotel = {hotelId}");
                if (linkedTours.Rows.Count > 0)
                {
                    string tours = "";
                    for (int i = 0; i < linkedTours.Rows.Count; i++)
                    {
                        if (tours != "") tours += ", ";
                        tours += linkedTours.Rows[i][0].ToString();
                    }

                    new Message("Ошибка", $"Невозможно удалить отель \"{hotelName}\", " +
                        $"поскольку он связан с турами {tours}").ShowDialog();

                    MainFunctions.AddLogRecord($"Deleting hotel attempt" +
                        $"\n\tID: {hotelId}" +
                        $"\n\tName: {hotelName}" +
                        $"\n\tError: connected with tours {tours}");

                    return;
                }

                

                // удаление

                MainFunctions.NewQuery($"DELETE FROM Hotel_rooms WHERE id_hotel={hotelId}" +
                    $"\nDELETE FROM Hotel_feed_types WHERE id_hotel={hotelId}" +
                    $"\nDELETE FROM Hotel WHERE id_hotel = {hotelId}");
                new Message("Успех", "Отель успешно удален!").ShowDialog();
                MainFunctions.AddLogRecord($"Delete hotel success" +
                    $"\n\tID: {hotelId}" +
                    $"\n\tName: {hotelName}");

                LoadHotels();
            }
        }

        private void btnAddHotel_Click(object sender, RoutedEventArgs e)
        {
            dtgHotels.ItemsSource = null;
            hotelsList.Insert(0,new Hotel()
            {
                HotelId = 0,
                HotelName = "Новый отель",
                AllCities = allCitiesList,
                HotelCity = allCitiesList[0],
                StarsCount = 5,
                Star1 = true,
                Star2 = true,
                Star3 = true,
                Star4 = true,
                Star5 = true
            });
            dtgHotels.ItemsSource = hotelsList;
            dtgHotels.ScrollIntoView(hotelsList[0]);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //сохранение отелей
                int notSavedHotels = 0;
                for (int i = 0; i < hotelsList.Count; i++)
                {
                    if (hotelsList[i].HotelId == 0)
                    {
                        if (hotelsList[i].HotelName.Trim().Length > 0 && hotelsList[i].HotelName != "Новый отель")
                        {
                            MainFunctions.NewQuery($"INSERT INTO Hotel VALUES ('{hotelsList[i].HotelName}', {hotelsList[i].StarsCount} ,{hotelsList[i].HotelCity.CityId})");
                            int newHotelId = int.Parse(MainFunctions.NewQuery($"SELECT MAX(id_hotel) FROM Hotel").Rows[0][0].ToString());
                            MainFunctions.AddLogRecord($"Create hotel success" +
                                $"\n\tID: {newHotelId}" +
                                $"\n\tName: {hotelsList[i].HotelName}");

                            int[] roomTypes = new int[] { 1, 4, 5, 6, 9, 11, 12, 14, 16, 22, 25, 29, 33, 49, 55, 61, 63, 69 };
                            int[] feedTypes = new int[] {2, 6, 7, 8, 9, 10, 11, 12 };
                            int startRoomPrice = 1000 * hotelsList[i].StarsCount;
                            int startFeedPrice = 500 * hotelsList[i].StarsCount;

                            for (int k = 0; k < roomTypes.Length; k++)
                            {
                                MainFunctions.NewQuery($"INSERT INTO Hotel_rooms VALUES ({newHotelId},{roomTypes[k]}," +
                                    $" {startRoomPrice*(1 + 0.5*k)},{10})");
                            }

                            for (int k = 0; k < feedTypes.Length; k++)
                            {
                                MainFunctions.NewQuery($"INSERT INTO Hotel_feed_types VALUES ({newHotelId},{feedTypes[k]}," +
                                    $" {startFeedPrice * (1 + 0.5 * k) * Convert.ToInt32(k > 0)})");
                            }



                        }
                        else notSavedHotels++;
                    }
                    else
                    {
                        MainFunctions.NewQuery($"UPDATE Hotel SET name = '{hotelsList[i].HotelName}', stars = {hotelsList[i].StarsCount} , " +
                            $"id_city = {hotelsList[i].HotelCity.CityId} WHERE id_hotel = {hotelsList[i].HotelId}");
                    }
                }
                MainFunctions.AddLogRecord($"Update hotels success");

                //обновление таблиц и списков
                LoadHotels();

                //успешно сохранено
                string notSaved = "";
                if (notSavedHotels > 0) notSaved = "\nОднако некоторые данные имеют " +
                        "неверный формат (название \"Новый отель\" или отсутствие названия), поэтому не сохранено " + $"{notSavedHotels} отелей";

                new Message("Успех", $"Данные успешно сохранены!{notSaved}", false, false).ShowDialog();

            }
            //исключение
            catch (Exception ex)
            {
                new Message("Ошибка", "Что-то пошло не так... Данные не сохранены.").ShowDialog();
                MainFunctions.AddLogRecord($"Updating hotels error" +
                    $"\n\tError text: {ex.Message}");
            }

        }
        #endregion

        #region Changed
        private void dtgHotels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            borderDeleteButton.IsEnabled = dtgHotels.SelectedIndex != -1;
        }
        private void tbHotelName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length == 0) (sender as TextBox).Text = " ";
        }
        private void starCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                Hotel thisHotel = GetHotel(int.Parse((sender as CheckBox).Tag.ToString()));
                string cbName = (sender as CheckBox).Name;
                int starIndex = int.Parse(cbName.Substring(cbName.Length - 1, 1)) - 1;

                thisHotel.StarsCount = starIndex + 1;


                thisHotel.Star1 = 1 <= thisHotel.StarsCount;
                thisHotel.Star2 = 2 <= thisHotel.StarsCount;
                thisHotel.Star3 = 3 <= thisHotel.StarsCount;
                thisHotel.Star4 = 4 <= thisHotel.StarsCount;
                thisHotel.Star5 = 5 <= thisHotel.StarsCount;

            }
        }

        #endregion




    }
}
