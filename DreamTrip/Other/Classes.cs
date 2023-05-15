using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using DreamTrip.Windows;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows;

/// <summary>
/// Здесь описываются все базовые сущности
/// </summary>
namespace DreamTrip.Classes
{

    /// <summary>
    /// Вкладка в меню менеджера
    /// </summary>
    public class TabClass : INotifyPropertyChanged
    {
        private string index;

        public string Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged("Index");
            }

        }

        private string itemHeaderImageSource;
        public string ItemHeaderImageSource
        {
            get { return itemHeaderImageSource; }
            set
            {
                itemHeaderImageSource = value;
                OnPropertyChanged("ItemHeaderImageSource");
            }
        }


        private string itemHeaderText;
        public string ItemHeaderText
        {
            get { return itemHeaderText; }
            set
            {
                itemHeaderText = value;
                OnPropertyChanged("ItemHeaderText");
            }
        }
        public string CloseButtonVisibility { get; set; }
        private UserControl itemUserControl;
        public UserControl ItemUserControl
        {
            get { return itemUserControl; }
            set
            {
                itemUserControl = value;
                OnPropertyChanged("ItemUserControl");
            }

        }
        public void ScrollEvent(int delta)
        {
            if (ItemUserControl as Clients != null)
            {
                (ItemUserControl as Clients).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as Tours != null)
            {
                (ItemUserControl as Tours).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as TourInfo != null)
            {
                (ItemUserControl as TourInfo).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as ChooseClient != null)
            {
                (ItemUserControl as ChooseClient).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as CreateTrip != null)
            {
                (ItemUserControl as CreateTrip).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as ClientsTrips != null)
            {
                (ItemUserControl as ClientsTrips).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as EditTrip != null)
            {
                (ItemUserControl as EditTrip).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as NewTour != null)
            {
                (ItemUserControl as NewTour).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as Statistics != null)
            {
                (ItemUserControl as Statistics).ScrollEvent(delta);
                return;
            }
            if (ItemUserControl as Tasks != null)
            {
                (ItemUserControl as Tasks).ScrollEvent(delta);
                return;
            }
        }
        private string verticalScrollBarVisibility { get; set; } = "Auto";
        public string VerticalScrollBarVisibility
        {
            get { return verticalScrollBarVisibility; }
            set
            {
                verticalScrollBarVisibility = value;
                OnPropertyChanged("VerticalScrollBarVisibility");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// Аккаунт пользователя
    /// </summary>
    public class Account : INotifyPropertyChanged
    {
        public int AccountId { get; set; }
        private string login;
        public string Login
        {
            get { return this.login; }
            set
            {
                this.login = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Login"));
                }
            }
        }
        public string Role { get; set; }
        public string ImagePath { get; set; }

        private string origImagePath;
        public string OrigImagePath
        {
            get { return this.origImagePath; }
            set
            {
                this.origImagePath = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("OrigImagePath"));
                }
            }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Password"));
                }
            }
        }
        public Visibility IsNew { get; set; }
        private string surname;
        public string Surname
        {
            get { return this.surname; }
            set
            {
                this.surname = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Surname"));
                }
            }
        }
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }
        private string patronymic;
        public string Patronymic
        {
            get { return this.patronymic; }
            set
            {
                this.patronymic = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Patronymic"));
                }
            }
        }

        private string phone;
        public string Phone
        {
            get { return this.phone; }
            set
            {
                this.phone = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Phone"));
                }
            }
        }
        private AccountType type;
        public AccountType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Type"));
                }
            }
        }

        private bool isActivated;
        public bool IsActivated
        {
            get { return this.isActivated; }
            set
            {
                this.isActivated = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsActivated"));
                }
            }
        }
        public List<AccountType> AllTypes { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class AccountType
    {
        public int AccountTypeId { get; set; }
        public string TypeName { get; set; }
    }

    /// <summary>
    /// Клиент
    /// </summary>
    public class Client : INotifyPropertyChanged
    {
        public int ClientId { get; set; }
        public int ClientLogin { get; set; }
        private string surname;
        public string Surname
        {
            get { return this.surname; }
            set
            {
                this.surname = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Surname"));
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        private string patronymic;
        public string Patronymic
        {
            get { return this.patronymic; }
            set
            {
                this.patronymic = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Patronymic"));
                }
            }
        }

        public string FullName {
            get { return this.Surname + " " + this.Name + " " + this.Patronymic; }
        }

        private string passportSeria;
        public string PassportSeria
        {
            get { return this.passportSeria; }
            set
            {
                this.passportSeria = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("PassportSeria"));
                }
            }
        }

        private string passportNumber;
        public string PassportNumber
        {
            get { return this.passportNumber; }
            set
            {
                this.passportNumber = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("PassportNumber"));
                }
            }
        }

        private string birthday;
        public string Birthday
        {
            get { return this.birthday; }
            set
            {
                this.birthday = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Birthday"));
                }
            }
        }
        public int Age { get; set; }
        public string Gender { get; set; }
        private int genderNum; //0 - М, 1 - Ж
        public int GenderNum //0 - М, 1 - Ж
        {
            get { return this.genderNum; }
            set
            {
                this.genderNum = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("GenderNum"));
                }
            }
        }

        public string phone;
        public string Phone
        {
            get { return this.phone; }
            set
            {
                this.phone = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Phone"));
                }
            }
        }

        public string email;
        public string Email
        {
            get { return this.email; }
            set
            {
                this.email = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Email"));
                }
            }
        }

        private WorkField clientWorkField;
        public WorkField ClientWorkField
        {
            get { return this.clientWorkField; }
            set
            {
                this.clientWorkField = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ClientWorkField"));
                }
            }
        }
        private WorkPost clientWorkPost;
        public WorkPost ClientWorkPost
        {
            get { return this.clientWorkPost; }
            set
            {
                this.clientWorkPost = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ClientWorkPost"));
                }
            }
        }
        public int WorkFieldId { get; set; }
        public string WorkFieldName { get; set; }
        public string WorkPostName { get; set; }
        public int WorkPostId { get; set; }
        public int[] FavoriteTours { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Сфера деятельности клиента
    /// </summary>
    public class WorkField : INotifyPropertyChanged
    {
        public int WorkFieldId { get; set; }
        public string Name { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Рабочая специальность клиента
    /// </summary>
    public class WorkPost : INotifyPropertyChanged
    {
        public int WorkPostId { get; set; }
        public int WorkFieldId { get; set; }
        public string Name { get; set; }

        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Задача менеджера
    /// </summary>
    public class MTask : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int TaskTypeId { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
        private bool isDone { get; set; }

        public bool IsDone
        {
            get { return this.isDone; }
            set
            {
                this.isDone = value;
                if (this.isDone) this.Color = "#FF9EE8E1";
                else this.Color = "#FFB1B1B1";
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsDone"));
                }
            }
        }

        private string imageSource;
        public string ImageSource
        {
            get { return this.imageSource; }
            set
            {
                this.imageSource = value;
                if (this.isDone) this.Color = "#FF9EE8E1";
                else this.Color = "#FFB1B1B1";
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ImageSource"));
                }
            }
        }
        private Visibility clientVisible;
        public Visibility ClientVisible
        {
            get { return this.clientVisible; }
            set
            {
                this.clientVisible = value;
                if (this.isDone) this.Color = "#FF9EE8E1";
                else this.Color = "#FFB1B1B1";
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ClientVisible"));
                }
            }
        }
        public Client taskClient { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientContact { get; set; }
        private string color { get; set; }
        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Тур
    /// </summary>
    public class Tour : INotifyPropertyChanged
    {
        public int TourId { get; set; }
        public string Name { get; set; }
        public int TicketPrice { get; set; }
        public string StartPrice { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string TourTypes { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int StarsCount { get; set; }
        public int HotelId { get; set; }
        public List<int> ServicesIds { get; set; } = new List<int>();
        public List<int> TypesIds { get; set; } = new List<int>();



        private BitmapImage imageSource;
        public BitmapImage ImageSource
        {
            get { return this.imageSource; }
            set
            {
                this.imageSource = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ImageSource"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Отель
    /// </summary>
    public class Hotel : INotifyPropertyChanged
    {
        public int HotelId { get; set; }

        private bool star1;
        public bool Star1
        {
            get { return this.star1; }
            set
            {
                this.star1 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Star1"));
                }
            }
        }
        private bool star2;
        public bool Star2
        {
            get { return this.star2; }
            set
            {
                this.star2 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Star2"));
                }
            }
        }
        private bool star3;
        public bool Star3
        {
            get { return this.star3; }
            set
            {
                this.star3 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Star3"));
                }
            }
        }
        private bool star4;
        public bool Star4
        {
            get { return this.star4; }
            set
            {
                this.star4 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Star4"));
                }
            }
        }
        private bool star5;
        public bool Star5
        {
            get { return this.star5; }
            set
            {
                this.star5 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Star5"));
                }
            }
        }
        public int StarsCount { get; set; }

        private string hotelName;
        public string HotelName
        {
            get { return this.hotelName; }
            set
            {
                this.hotelName = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("HotelName"));
                }
            }
        }

        private List<City> allCities;
        public List<City> AllCities
        {
            get { return this.allCities; }
            set
            {
                this.allCities = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AllCities"));
                }
            }
        }
        private City hotelCity;
        public City HotelCity
        {
            get { return this.hotelCity; }
            set
            {
                this.hotelCity = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("HotelCity"));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Поездка клиента (оформленное бронирование тура)
    /// </summary>
    public class Trip
    {
        public int TripId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }

        public int TourId { get; set; }
        public string TourName { get; set; }
        public int HotelId { get; set; }
        public int TicketPrice { get; set; }
        public string Location { get; set; }

        public int RoomTypeId { get; set; }
        public string RoomType { get; set; }
        public int FeedTypeId { get; set; }
        public string FeedType { get; set; }
        public int[] ServicesIds { get; set; }
        public string Services { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public float TotalPrice { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Оконченная поездка 
    /// </summary>
    public class EndedTrip : Trip
    {
        public string Comment { get; set; }
        public int TourRate { get; set; }
        public int HotelRate { get; set; }
        public int ServiceRate { get; set; }
    }

    /// <summary>
    /// Тип питания в отеле
    /// </summary>
    public class FeedType : INotifyPropertyChanged
    {
        public int FeedTypeId { get; set; }
        public string FeedTypeName { get; set; }
        public int PricePerDay { get; set; }

        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Тип тра
    /// </summary>
    public class TourType : INotifyPropertyChanged
    {
        public int TourTypeId { get; set; }
        public string Name { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Страна
    /// </summary>
    public class Country : INotifyPropertyChanged
    {
        public int CountryId { get; set; }
        private string countryName;
        public string CountryName
        {
            get { return this.countryName; }
            set
            {
                this.countryName = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CountryName"));
                }
            }
        }
        public Visibility CountryVisibility { get; set; }
        public ObservableCollection<City> CitiesList { get; set; } = new ObservableCollection<City>();
        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Город
    /// </summary>
    public class City : INotifyPropertyChanged
    {
        public int CityId { get; set; }
        private string cityName;
        public string CityName
        {
            get { return this.cityName; }
            set
            {
                this.cityName = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CityName"));
                }
            }
        }
        private List<Country> allCountries;
        public List<Country> AllCountries
        {
            get { return this.allCountries; }
            set
            {
                this.allCountries = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AllCountries"));
                }
            }
        }
        private Country cityCountry;
        public Country CityCountry
        {
            get { return this.cityCountry; }
            set
            {
                this.cityCountry = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CityCountry"));
                }
            }
        }
        public int IdCountry { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Уровень комфорта отеля
    /// </summary>
    public class HotelStars : INotifyPropertyChanged
    {
        public int StarsCount { get; set; }
        public string StarsString { get; set; }

        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Отзыв на тур
    /// </summary>
    public class TourComment
    {
        public int TourId { get; set; }
        public int CommentId { get; set; }
        public string Name { get; set; }
        public int Rate { get; set; }
        public string CommentDateTime { get; set; }
        public string CommentText { get; set; }
        public string StarVisibility1 { get; set; } = "Hidden";
        public string StarVisibility2 { get; set; } = "Hidden";
        public string StarVisibility3 { get; set; } = "Hidden";
        public string StarVisibility4 { get; set; } = "Hidden";
        public string StarVisibility5 { get; set; } = "Hidden";

    }

    /// <summary>
    /// Комната отеля
    /// </summary>
    public class TourHotelRoom
    {
        public int RoomTypeId { get; set; }
        public string RoomName { get; set; }
        public int PricePerDay { get; set; }
        public int FreeRoomsAmount { get; set; }
    }

    /// <summary>
    /// Сервис тура
    /// </summary>
    public class TourService : INotifyPropertyChanged
    {
        public int ServiceId { get; set; }

        private string serviceName;
        public string ServiceName
        {
            get { return this.serviceName; }
            set
            {
                this.serviceName = value;
                if (this.PropertyChanged != null){
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ServiceName"));
                }
            }
        }

        private int price;
        public int Price
        {
            get { return this.price; }
            set
            {
                this.price = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Price"));
                }
            }
        }

        private bool perDay;
        public bool PerDay
        {
            get { return this.perDay; }
            set
            {
                this.perDay = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("PerDay"));
                }
            }
        }

        private BitmapImage serviceImage;
        public BitmapImage ServiceImage
        {
            get { return this.serviceImage; }
            set
            {
                this.serviceImage = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ServiceImage"));
                }
            }
        }

        public string ImagePath { get; set; }
        public string OrigImagePath { get; set; }
        public bool IsPhotoUpdated { get; set; }


        private bool isChecked;
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// Запись в истории авторизации
    /// </summary>
    public class HistoryRecord
    {
        public int RecordId { get; set; }
        public string Login { get; set; }
        public string LoginDatetime { get; set; }
        public string LogoutDatetime { get; set; }
        public string Role { get; set; }
        public string Logs { get; set; }
    }

}
