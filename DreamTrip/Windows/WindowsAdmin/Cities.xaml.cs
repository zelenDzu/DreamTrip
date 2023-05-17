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
    /// Логика взаимодействия для Cities.xaml
    /// </summary>
    public partial class Cities : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Города", "../../Resources/city.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<Country> countriesList = new List<Country>();
        List<City> citiesList = new List<City>();
        #endregion

        #region Constructor
        public Cities(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadCountries();
            LoadCities();
        }
        #endregion

        #region LoadData
        private void LoadCountries()
        {
            countriesList.Clear();
            dtgCountries.ItemsSource = null;

            DataTable countriesData = MainFunctions.NewQuery($"SELECT * FROM Country");
            for (int i = 0; i < countriesData.Rows.Count; i++)
            {
                Country country = new Country()
                {
                    CountryId = int.Parse(countriesData.Rows[i][0].ToString()),
                    CountryName = countriesData.Rows[i][1].ToString(),
                    CountryVisibility = Visibility.Visible
                };
                countriesList.Add(country);
            };

            dtgCountries.ItemsSource = countriesList;
        }

        private void LoadCities()
        {
            citiesList.Clear();
            dtgCities.ItemsSource = null;

            DataTable citiesData = MainFunctions.NewQuery($"SELECT * FROM City");
            for (int i = 0; i < citiesData.Rows.Count; i++)
            {

                City city = new City()
                {
                    CityId = int.Parse(citiesData.Rows[i][0].ToString()),
                    CityName = citiesData.Rows[i][1].ToString(),
                    AllCountries = countriesList,
                    CityCountry = GetCountry(int.Parse(citiesData.Rows[i][2].ToString()))
                };

                citiesList.Add(city);
            }

            dtgCities.ItemsSource = citiesList;
        }
        #endregion

        #region Functions
        private Country GetCountry(int countryId)
        {
            Country tempCountry = new Country(); ;
            for (int i = 0; i < countriesList.Count; i++)
            {
                if (countriesList[i].CountryId == countryId)
                {
                    tempCountry = countriesList[i];
                    break;
                }
            }
            return tempCountry;
        }

        private bool isDataDuplicated()
        {
            bool isDuplicated = false;
            List<string> duplicated = new List<string>();

            for (int i = 0; i < countriesList.Count - 1; i++)
            {
                for (int j = i+1; j < countriesList.Count; j++)
                {
                    if (countriesList[i].CountryName == countriesList[j].CountryName)
                        duplicated.Add(countriesList[i].CountryName);
                }
            }

            string error = "";
            if (duplicated.Count > 0)
            {
                isDuplicated = true;
                error += $"Повторяются названия стран {string.Join(",", duplicated)}. ";
            }

            duplicated.Clear();

            for (int i = 0; i < citiesList.Count - 1; i++)
            {
                for (int j = i + 1; j < citiesList.Count; j++)
                {
                    if (citiesList[i].CityName == citiesList[j].CityName)
                        duplicated.Add(citiesList[i].CityName);
                }
            }

            if (duplicated.Count > 0)
            {
                isDuplicated = true;
                error += $"Повторяются названия городов {string.Join(",", duplicated)}";
            }

            if (isDuplicated) new Message("Ошибка", error).ShowDialog();

            return isDuplicated;
        }
        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isDataDuplicated())
            {
                try
                {
                    //сохранение стран
                    int notSavedCountries = 0;
                    for (int i = 0; i < countriesList.Count; i++)
                    {
                        if (countriesList[i].CountryId == 0)
                        {
                            if (countriesList[i].CountryName.Trim().Length > 0 && countriesList[i].CountryName != "Новая страна")
                            {
                                MainFunctions.NewQuery($"INSERT INTO Country VALUES ('{countriesList[i].CountryName}')");
                                MainFunctions.AddLogRecord($"Create country success" +
                                    $"\n\tID: {int.Parse(MainFunctions.NewQuery($"SELECT MAX(id_country) FROM Country").Rows[0][0].ToString())}" +
                                    $"\n\tName: {countriesList[i].CountryName}");
                            }
                            else notSavedCountries++;
                        }
                        else
                        {
                            MainFunctions.NewQuery($"UPDATE Country SET name = '{countriesList[i].CountryName}' WHERE id_country = {countriesList[i].CountryId}");
                        }

                    }
                    MainFunctions.AddLogRecord($"Update countries success");


                    //сохранение городов
                    int notSavedCities = 0;
                    for (int i = 0; i < citiesList.Count; i++)
                    {
                        if (citiesList[i].CityId == 0)
                        {
                            if (citiesList[i].CityName.Trim().Length > 0 && citiesList[i].CityName != "Новый город")
                            {
                                MainFunctions.NewQuery($"INSERT INTO City VALUES ('{citiesList[i].CityName}', {citiesList[i].CityCountry.CountryId})");
                                MainFunctions.AddLogRecord($"Create city success" +
                                    $"\n\tID: {int.Parse(MainFunctions.NewQuery($"SELECT MAX(id_city) FROM City").Rows[0][0].ToString())}" +
                                    $"\n\tName: {citiesList[i].CityName}");
                            }
                            else notSavedCities++;
                        }
                        else
                        {
                            MainFunctions.NewQuery($"UPDATE City SET name = '{citiesList[i].CityName}', id_country = {citiesList[i].CityCountry.CountryId} WHERE id_city = {citiesList[i].CityId}");
                        }
                    }
                    MainFunctions.AddLogRecord($"Update cities success");

                    //обновление таблиц и списков
                    LoadCountries();
                    LoadCities();

                    //успешно сохранено
                    string notSaved = "";
                    if (notSavedCities > 0 || notSavedCountries > 0) notSaved = "\nОднако некоторые данные имеют " +
                            "неверный формат (название \"Новый город\"\\\"Новая страна\" или отсутствие названия), поэтому не сохранено: " +
                            $"\n{notSavedCountries} стран" +
                            $"\n{notSavedCities} городов";

                    new Message("Успех", $"Данные успешно сохранены!{notSaved}", false, false).ShowDialog();

                }
                //исключение
                catch (Exception ex)
                {
                    new Message("Ошибка", "Что-то пошло не так... Данные не сохранены.").ShowDialog();
                    MainFunctions.AddLogRecord($"Updating cities and countries error" +
                        $"\n\tError text: {ex.Message}");
                }
            }
        }
        
        private void btnAddCity_Click(object sender, RoutedEventArgs e)
        {
            dtgCities.ItemsSource = null;
            City tempCity = new City()
            {
                CityId = 0,
                CityName = "Новый город",
                AllCountries = countriesList,
                CityCountry = countriesList[0],
            };
            citiesList.Add(tempCity);
            dtgCities.ItemsSource = citiesList;
            dtgCities.ScrollIntoView(tempCity);
        }

        private void btnAddCountry_Click(object sender, RoutedEventArgs e)
        {
            dtgCountries.ItemsSource = null;
            Country temoCountry = new Country()
            {
                CountryId = 0,
                CountryName = "Новая страна",
                CountryVisibility = Visibility.Hidden
            };
            countriesList.Add(temoCountry);
            dtgCountries.ItemsSource = countriesList;
            dtgCities.ScrollIntoView(temoCountry);

        }

        private void btnDeleteCity_Click(object sender, RoutedEventArgs e)
        {
            
                int cityId = (dtgCities.SelectedItem as City).CityId;
                string cityName = (dtgCities.SelectedItem as City).CityName;
            try { 
                if (cityId == 0)
                {
                    citiesList.Remove(dtgCities.SelectedItem as City);
                    dtgCities.ItemsSource = null;
                    dtgCities.ItemsSource = citiesList;
                }
                else
                {
                    bool answer = false;
                    Message deleteWindow = new Message("Вопрос", $"Вы уверены, что хотите удалить город \"{cityName}\"", true, true);
                    deleteWindow.messageAnswer += value => answer = value;
                    deleteWindow.ShowDialog();

                    if (!answer)
                    {
                        return;
                    }



                    // если город привязан к туру - удалять нельзя
                    DataTable linkedTours = MainFunctions.NewQuery($"SELECT name FROM tour WHERE id_city = {cityId}");
                    if (linkedTours.Rows.Count > 0)
                    {
                        string tours = "";
                        for (int i = 0; i < linkedTours.Rows.Count; i++)
                        {
                            if (tours != "") tours += ", ";
                            tours += linkedTours.Rows[i][0].ToString();
                        }

                        new Message("Ошибка", $"Невозможно удалить город \"{cityName}\", " +
                            $"поскольку он связан с турами {tours}").ShowDialog();

                        MainFunctions.AddLogRecord($"Deleting city attempt" +
                            $"\n\tID: {cityId}" +
                            $"\n\tName: {cityName}" +
                            $"\n\tError: connected with tours {tours}");

                        return;
                    }

                    // если город привязан к отелям - предупреждение о том что удаление с отелями
                    string linkedHotels = "";
                    DataTable hotels = MainFunctions.NewQuery($"SELECT name FROM Hotel WHERE id_city = {cityId}");
                    for (int i = 0; i < hotels.Rows.Count; i++)
                    {
                        if (linkedHotels != "") linkedHotels += ", ";
                        linkedHotels += hotels.Rows[i][0].ToString();
                    }


                    if (linkedHotels != "")
                    {
                        bool popupAnswer = false;
                        Message messageDeleteWindow = new Message("Предупреждение", $"Город \"{cityName}\" " +
                            $"связан с отелями {linkedHotels}. Вы уверены, что хотите удалить его? " +
                            $"Все относящиеся к городу отели будут также удалены.", true, true);
                        messageDeleteWindow.messageAnswer += value => popupAnswer = value;
                        messageDeleteWindow.ShowDialog();

                        if (!popupAnswer)
                        {
                            return;
                        }
                        else
                        {
                            //удаление с отелями
                            MainFunctions.NewQuery($"DELETE FROM Hotel_feed_types WHERE id_hotel IN " +
                                $"(SELECT id_hotel FROM Hotel WHERE id_city = {cityId})");
                            MainFunctions.NewQuery($"DELETE FROM Hotel_rooms WHERE id_hotel IN " +
                                $"(SELECT id_hotel FROM Hotel WHERE id_city = {cityId})");
                            MainFunctions.NewQuery($"DELETE FROM Hotel WHERE id_city = {cityId}");
                            MainFunctions.AddLogRecord($"Delete city {cityName} (ID: {cityId}) cause deleting hotels {linkedHotels}");
                        }
                    }

                    // удаление
                    MainFunctions.NewQuery($"DELETE FROM City WHERE id_city = {cityId}");
                    new Message("Успех", "Город успешно удален!").ShowDialog();
                    MainFunctions.AddLogRecord($"Delete city success" +
                        $"\n\tID: {cityId}" +
                        $"\n\tName: {cityName}");

                    LoadCountries();
                    LoadCities();

                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Ошибка удаления города").ShowDialog();
                MainFunctions.AddLogRecord($"Delete city error " +
                    $"\n\tID: {cityId}" +
                    $"\n\tName: {cityName}" +
                    $"\n\tError: {ex.Message}");
            }
        }

        private void btnDeleteCountry_Click(object sender, RoutedEventArgs e)
        {
            int countryId = (dtgCountries.SelectedItem as Country).CountryId;
            string countryName = (dtgCountries.SelectedItem as Country).CountryName;
            try
            {
                if (countryId == 0)
                {
                    countriesList.Remove(dtgCountries.SelectedItem as Country);
                    dtgCountries.ItemsSource = null;
                    dtgCountries.ItemsSource = countriesList;
                }
                else
                {
                    bool answer = false;
                    Message deleteWindow = new Message("Вопрос", $"Вы уверены, что хотите удалить страну \"{countryName}\"", true, true);
                    deleteWindow.messageAnswer += value => answer = value;
                    deleteWindow.ShowDialog();

                    if (!answer)
                    {
                        return;
                    }


                    // если страна привязана к туру - удалять нельзя
                    DataTable linkedTours = MainFunctions.NewQuery($"SELECT name FROM tour WHERE id_city IN (SELECT id_city FROM City WHERE id_country = {countryId})");
                    if (linkedTours.Rows.Count > 0)
                    {
                        string tours = "";
                        for (int i = 0; i < linkedTours.Rows.Count; i++)
                        {
                            if (tours != "") tours += ", ";
                            tours += linkedTours.Rows[i][0].ToString();
                        }

                        new Message("Ошибка", $"Невозможно удалить страну \"{countryName}\", " +
                            $"поскольку она связана с турами {tours}").ShowDialog();
                        MainFunctions.AddLogRecord($"Deleting country attempt" +
                            $"\n\tID: {countryId}" +
                            $"\n\tName: {countryName}" +
                            $"\n\tError: connected with tours {tours}");
                        return;
                    }

                    // если страна привязана к городу - предупреждение о том что удаление с городами
                    string linkedCities = "";
                    for (int i = 0; i < citiesList.Count; i++)
                    {
                        if (citiesList[i].CityCountry.CountryId == countryId)
                        {
                            if (linkedCities != "") linkedCities += ", ";
                            linkedCities += citiesList[i].CityName;
                        }
                    }
                    if (linkedCities != "")
                    {
                        bool popupAnswer = false;
                        Message messageDeleteWindow = new Message("Предупреждение", $"Страна \"{countryName}\" " +
                            $"связана с городами {linkedCities}. Вы уверены, что хотите удалить ее? " +
                            $"Все относящиеся к стране города будут также удалены.", true, true);
                        messageDeleteWindow.messageAnswer += value => popupAnswer = value;
                        messageDeleteWindow.ShowDialog();

                        if (!popupAnswer)
                        {
                            return;
                        }
                        else
                        {
                            MainFunctions.NewQuery($"DELETE FROM Hotel_feed_types WHERE id_hotel IN " +
                                    $"(SELECT id_hotel FROM Hotel WHERE id_city IN (SELECT id_city FROM City WHERE id_country = {countryId}))");
                            MainFunctions.NewQuery($"DELETE FROM Hotel_rooms WHERE id_hotel IN " +
                                $"(SELECT id_hotel FROM Hotel WHERE id_city IN (SELECT id_city FROM City WHERE id_country = {countryId}))");

                            MainFunctions.NewQuery($"DELETE FROM Hotel WHERE id_city IN (SELECT id_city FROM City WHERE id_country = {countryId})");
                            MainFunctions.NewQuery($"DELETE FROM City WHERE id_country = {countryId}");

                            MainFunctions.AddLogRecord($"Delete country {countryName} (ID: {countryId}) cause deleting cities {linkedCities}");


                        }
                    }

                    // удаление
                    MainFunctions.NewQuery($"DELETE FROM Country WHERE id_country = {countryId}");
                    new Message("Успех", "Страна успешно удалена!").ShowDialog();
                    MainFunctions.AddLogRecord($"Delete country success" +
                        $"\n\tID: {countryId}" +
                        $"\n\tName: {countryName}");

                    LoadCountries();
                    LoadCities();
                }
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Ошибка удаления страны").ShowDialog();
                MainFunctions.AddLogRecord($"Delete country error " +
                    $"\n\tID: {countryId}" +
                    $"\n\tName: {countryName}" +
                    $"\n\tError: {ex.Message}");
            }
        }
        #endregion

        #region Changed
        private void dtgCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            borderDeleteCity.IsEnabled = dtgCities.SelectedIndex != -1;
        }

        private void dtgCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            borderDeleteCountry.IsEnabled = dtgCountries.SelectedIndex != -1;
        }

        private void tbCountryName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_RuEng(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }

            if ((sender as TextBox).Text.Length == 0) (sender as TextBox).Text = " ";
        }

        private void tbCityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_RuEng(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }

            if ((sender as TextBox).Text.Length == 0) (sender as TextBox).Text = " ";
        }
        #endregion

    }
}
