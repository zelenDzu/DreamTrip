using DreamTrip.Classes;
using DreamTrip.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для Services.xaml
    /// </summary>
    public partial class Services : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Сервисы", "../Resources/service.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<TourService> servicesList = new List<TourService>();
        #endregion

        #region Constructor
        public Services(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadServices();
        }
        #endregion

        #region LoadData
        private void LoadServices()
        {
            dtgServices.ItemsSource = null;
            servicesList.Clear();

            DataTable servicesData = MainFunctions.NewQuery($"SELECT * FROM Service ORDER BY name");

            for (int i = 0; i < servicesData.Rows.Count; i++)
            {
                string imagePath = servicesData.Rows[i][4].ToString();


                TourService service = new TourService()
                {
                    ServiceId = int.Parse(servicesData.Rows[i][0].ToString()),
                    ServiceName = servicesData.Rows[i][1].ToString(),
                    Price = int.Parse(servicesData.Rows[i][2].ToString()),
                    PerDay = Convert.ToBoolean(servicesData.Rows[i][3].ToString()),
                    ImagePath = imagePath,
                    OrigImagePath = MainFunctions.GetAppPath() + imagePath,
                    IsPhotoUpdated = false
                };
                servicesList.Add(service);
            }


            dtgServices.ItemsSource = servicesList;
        }
        #endregion

        #region ButtonsClick
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = (dtgServices.SelectedItem as TourService).ServiceName;
            int serviceId = (dtgServices.SelectedItem as TourService).ServiceId;
            if (serviceId == 0)
            {
                servicesList.Remove(dtgServices.SelectedItem as TourService);
                dtgServices.ItemsSource = null;
                dtgServices.ItemsSource = servicesList;
            }
            else
            {

                bool popupAnswer = false;
                Message messageDeleteWindow = new Message("Удаление", "Вы точно хотите удалить данный сервис?", true, true);
                messageDeleteWindow.messageAnswer += value => popupAnswer = value;
                messageDeleteWindow.ShowDialog();

                if (popupAnswer)
                {
                    if (MainFunctions.NewQuery($"SELECT * FROM Trip_services " +
                        $"WHERE id_service = {serviceId} " +
                        $"AND  id_trip IN(SELECT id_trip FROM Trip WHERE end_date >= GETDATE())").Rows.Count > 0)
                    {
                        Message message = new Message("Ошибка", "Удаление невозможно, поскольку существуют незавершеные поездки с подключением данного сервиса", false, false);
                        message.ShowDialog();

                        MainFunctions.AddLogRecord($"Service deleting error:" +
                            $"\n\tID: {serviceId}" +
                            $"\n\tName: {serviceName}" +
                            $"\n\tError: not-ended trips");
                    }
                    else
                    {
                        try
                        {
                            MainFunctions.NewQuery($"DELETE FROM Trip_services WHERE id_service = {serviceId}");
                            MainFunctions.NewQuery($"DELETE FROM Tour_services WHERE id_service = {serviceId}");
                            MainFunctions.NewQuery($"DELETE FROM Service WHERE id_service = {serviceId}");

                            new Message("Успех", "Сервис успешно удален", false, false).ShowDialog();

                            MainFunctions.AddLogRecord($"Service deleting success:" +
                            $"\n\tID: {serviceId}" +
                            $"\n\tName: {serviceName}");

                            LoadServices();
                        }
                        catch (Exception ex)
                        {
                            new Message("Ошибка", "Ошибка удаления сервиса", false, false).ShowDialog();

                            MainFunctions.AddLogRecord($"Service deleting error:" +
                            $"\n\tID: {serviceId}" +
                            $"\n\tName: {serviceName}" +
                            $"\n\tError: {ex.Message}");
                        }

                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string appFolder = MainFunctions.GetAppPath();
            string imagesFolder = "/Resources/ServicesPhotos/";

            try
            {
                int notSavedServices = 0;
                for (int i = 0; i < servicesList.Count; i++)
                {
                    if (servicesList[i].ServiceId == 0)
                    {
                        if (servicesList[i].ServiceName.Trim().Length > 0 && servicesList[i].ServiceName != "Новый сервис" && servicesList[i].Price > 0)
                        {
                            MainFunctions.NewQuery($"INSERT INTO Service VALUES ('{servicesList[i].ServiceName}', " +
                                $"{servicesList[i].Price} ," +
                                $"{Convert.ToInt32(servicesList[i].PerDay)} , " +
                                $"'/Resources/service.png')");

                            servicesList[i].ServiceId = Convert.ToInt32(MainFunctions.NewQuery($"SELECT MAX(id_service) FROM Service").Rows[0][0].ToString());

                            MainFunctions.AddLogRecord($"Service creation success: " +
                                $"\n\tID: {servicesList[i].ServiceId}" +
                                $"\n\tName: {servicesList[i].ServiceName}");
                        }
                        else notSavedServices++;
                    }


                    if (servicesList[i].IsPhotoUpdated && servicesList[i].ServiceId != 0)
                    {
                        string extenstion = System.IO.Path.GetExtension(servicesList[i].ImagePath);
                        string photoName = $"{servicesList[i].ServiceId} {servicesList[i].ServiceName}";

                        string lastFileDigit = "";
                        int counter = 0;
                        while (File.Exists(appFolder + imagesFolder + photoName + lastFileDigit + extenstion))
                        {
                            counter++;
                            lastFileDigit = "_" + counter.ToString();
                        }

                        //if (File.Exists(destinationFolder + photoName)) File.Delete(destinationFolder + photoName);
                        File.Copy(servicesList[i].ImagePath, appFolder + imagesFolder + photoName + lastFileDigit + extenstion, true);

                        MainFunctions.NewQuery($"UPDATE Service SET icon_path = '{imagesFolder + photoName + lastFileDigit + extenstion}' WHERE id_service = {servicesList[i].ServiceId}");


                    }

                    if (servicesList[i].ServiceId == 0) continue;

                    MainFunctions.NewQuery($"UPDATE Service SET name = '{servicesList[i].ServiceName}', " +
                        $"price = {servicesList[i].Price}, per_day = {Convert.ToInt32(servicesList[i].PerDay)} " +
                        $"WHERE id_service = {servicesList[i].ServiceId}");
                }

                LoadServices();

                //успешно сохранено
                string notSaved = "";
                if (notSavedServices > 0) notSaved = "\nОднако некоторые данные имеют " +
                        "неверный формат (название \"Новый сервис\"\\отсутствие названия\\цена не является натуральным числом), поэтому не сохранено " + $"{notSavedServices} сервисов";

                new Message("Успех", $"Данные успешно сохранены!{notSaved}", false, false).ShowDialog();

                MainFunctions.AddLogRecord($"Services updating success");
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так...", false, false);
                messageError.ShowDialog();

                MainFunctions.AddLogRecord($"Service saving error:" +
                            $"\n\tError: {ex.Message}");

            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int serviceId = Convert.ToInt32((sender as Image).Tag.ToString());
            
            string newImagePath = "";

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.FileName = null;
            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    newImagePath = openFileDialog.FileName;
                    string extenstion = System.IO.Path.GetExtension(newImagePath);
                    if (extenstion == ".png" || extenstion == ".jpg" || extenstion == ".jpeg")
                    {

                        for (int i = 0; i < servicesList.Count; i++)
                        {
                            if (servicesList[i].ServiceId == serviceId)
                            {
                                if (servicesList[i].OrigImagePath != newImagePath)
                                    servicesList[i].IsPhotoUpdated = true;
                                else
                                    servicesList[i].IsPhotoUpdated = false;

                                servicesList[i].ImagePath = newImagePath;

                                break;
                            }
                        }

                        BitmapImage tempServiceImage = new BitmapImage();
                        tempServiceImage.BeginInit();
                        tempServiceImage.CacheOption = BitmapCacheOption.OnLoad;
                        FileStream stream = File.OpenRead(newImagePath);
                        tempServiceImage.StreamSource = stream;
                        tempServiceImage.EndInit();
                        stream.Close();

                        (sender as Image).Source = tempServiceImage;
                    }
                    else
                    {
                        Message message = new Message("Ошибка","Неверный формат изображения (png,jpeg,jpg)",false,false);
                        message.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Message messageError = new Message("Ошибка", "Что-то пошло не так. Возможно, неверно был выбран тип файла. Файл должен иметь формат изображения.", false, false);
                messageError.ShowDialog();
                MainFunctions.AddLogRecord("Service Image Choose Error Text: " + ex.Message);
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            dtgServices.ItemsSource = null;

            servicesList.Insert(0,new TourService()
            {
                ServiceId = 0,
                ServiceName = "Новый сервис",
                Price = 0,
                OrigImagePath = MainFunctions.GetAppPath() + "/Resources/plus.png",
                ImagePath = "/Resources/service.png",
                IsPhotoUpdated = false,
                PerDay = false,
            });

            dtgServices.ItemsSource = servicesList;
            dtgServices.ScrollIntoView(servicesList[0]);

        }
        #endregion

        #region Changed
        private void dtgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            borderDeleteButton.IsEnabled = dtgServices.SelectedIndex != -1;
        }

        private void tbServicePrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbServicePrice = sender as TextBox;

            char[] charList = tbServicePrice.Text.ToCharArray();
            int tempOut;
            for (int i = 0; i < charList.Length; i++)
            {
                if (Int32.TryParse(charList[i].ToString(), out tempOut) == false)
                {
                    tbServicePrice.Text = tbServicePrice.Text.Remove(i, 1);
                }
            }

            if (tbServicePrice.Text.Length >= 12) tbServicePrice.Text = tbServicePrice.Text.Substring(0, 12);
        }
        
        private void tbServiceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length == 0) (sender as TextBox).Text = " ";
        }
        #endregion

    }
}
