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

namespace DreamTrip.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginHistory.xaml
    /// </summary>
    public partial class LoginHistory : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "История входа", "../../Resources/login_history.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        ObservableCollection<HistoryRecord> HistoryRecordsList { get; set; } = new ObservableCollection<HistoryRecord>();
        #endregion

        #region Constructor
        public LoginHistory(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            LoadDate();
            btnSearch_Click(btnSearch, new RoutedEventArgs());
        }
        #endregion

        #region Load Data
        /// <summary>
        /// Загрузить даты
        /// </summary>
        private void LoadDate()
        {
            tbxStartDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today.AddDays(-7));

            tbxEndDate.Text = CommonFunctions.ConvertDateToText(DateTime.Today);
        }

        /// <summary>
        /// Загрузить историю авторизации
        /// </summary>
        /// <param name="recordsIds"></param>
        private void LoadHistory(string recordsIds)
        {
            dtgHistory.ItemsSource = null;
            HistoryRecordsList.Clear();

            tbNothingFound.Visibility = Visibility.Hidden;
            if (recordsIds == "")
            {
                tbNothingFound.Visibility = Visibility.Visible;
            }
            else
            {
                DataTable dataHistory;

                if (recordsIds == "all")
                {
                    dataHistory = MainFunctions.NewQuery("SELECT  TOP 100 * FROM Login_history ORDER BY log_in_datetime DESC");
                }
                else
                {
                    dataHistory = MainFunctions.NewQuery($"SELECT TOP 100 * FROM Login_history WHERE id_rec IN ({recordsIds}) ORDER BY log_in_datetime DESC");
                }

                for (int i = 0; i < dataHistory.Rows.Count; i++)
                {
                    string login = dataHistory.Rows[i][1].ToString();
                    string role = MainFunctions.NewQuery($"SELECT role FROM User_roles WHERE id_role = " +
                        $"(SELECT id_role FROM User_login_data WHERE login='{login}')").Rows[0][0].ToString();


                    HistoryRecord historyRecord = new HistoryRecord()
                    {
                        RecordId = int.Parse(dataHistory.Rows[i][0].ToString()),
                        Login = login,
                        LoginDatetime = dataHistory.Rows[i][2].ToString().Replace("0:00:00", "").Trim(),
                        LogoutDatetime = dataHistory.Rows[i][3].ToString().Replace("0:00:00", "").Trim(),
                        Logs = dataHistory.Rows[i][4].ToString(),
                        Role = role,
                    };
                    HistoryRecordsList.Add(historyRecord);
                }
                dtgHistory.ItemsSource = HistoryRecordsList;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// При изменении фильтра
        /// </summary>
        private void FilterChanged()
        {
            if (tbxLoginSearch != null && tbxStartDate != null && tbxEndDate != null && tbxTop != null && tbxEndDate != null && borderClear!=null)
            {
                if (tbxLoginSearch.Text.Length == 0
                    && tbxStartDate.Text.Length == 0
                    && tbxLogsSearch.Text.Length == 0
                    && tbxTop.Text.Length == 0
                    && tbxEndDate.Text.Length == 0)
                {
                    borderClear.Visibility = Visibility.Hidden;
                }
                else
                {
                    borderClear.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Проверка корректности введенных дат
        /// </summary>
        /// <returns></returns>
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

        #region ButtonsClick
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool popupAnswer = false;
            Message messageDeleteWindow = new Message("Удаление", "Вы точно хотите удалить данную запись истории? Действие невозможно отменить", true, true);
            messageDeleteWindow.messageAnswer += value => popupAnswer = value;
            messageDeleteWindow.ShowDialog();

            if (popupAnswer)
            {
                MainFunctions.NewQuery($"DELETE FROM Login_history WHERE id_rec = {(dtgHistory.SelectedItem as HistoryRecord).RecordId}");
                Message succesMessage = new Message("Успех", "Запись была успешно удалена!", false, false);
                succesMessage.ShowDialog();

                HistoryRecord currentRecord = dtgHistory.SelectedItem as HistoryRecord;
                MainFunctions.AddLogRecord($"History record deleted:" +
                    $"\n\tID record: {currentRecord.RecordId}" +
                    $"\n\tUser login: {currentRecord.Login}" +
                    $"\n\tLog In datetime: {currentRecord.LoginDatetime}" +
                    $"\n\tLog out datetime: {currentRecord.LogoutDatetime}" +
                    $"\n\tLogs: {currentRecord.Logs}");

                HistoryRecordsList.Remove(dtgHistory.SelectedItem as HistoryRecord);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            tbxLoginSearch.Text = "";
            tbxStartDate.Text = "";
            tbxEndDate.Text = "";
            tbxLogsSearch.Text = "";
            tbxTop.Text = "";
            LoadHistory("all");
            borderClear.Visibility = Visibility.Hidden;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (borderClear.Visibility == Visibility.Hidden)
            {
                LoadHistory("all");
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
                    if (tbxLoginSearch.Text != "")
                    {
                        condition += $" WHERE LOWER(login) LIKE '%{tbxLoginSearch.Text.ToLower()}%'";
                    }

                    if (tbxLogsSearch.Text != "")
                    {
                        if (condition == "") condition += " WHERE ";
                        else condition += " AND ";

                        condition += $" LOWER(logs) LIKE '%{tbxLogsSearch.Text.ToLower()}%'";
                    }

                    if (tbxStartDate.Text != "")
                    {
                        string startDate = tbxStartDate.Text.Remove(4, 1).Remove(6, 1);

                        if (condition == "") condition += " WHERE ";
                        else condition += " AND ";

                        condition += String.Format("log_in_datetime >= '{0}'", startDate);
                    }

                    if (tbxEndDate.Text != "")
                    {
                        string endDate = tbxEndDate.Text.Remove(4, 1).Remove(6, 1);

                        if (condition == "") condition += " WHERE ";
                        else condition += " AND ";

                        condition += String.Format("log_in_datetime <= DATEADD(day,1, '{0}')", endDate);
                    }

                    int top = 100;
                    if (tbxTop.Text.Length > 0) top = Convert.ToInt32(tbxTop.Text);

                    string mainCondition = $"SELECT TOP {top} id_rec FROM Login_history {condition} ORDER BY log_in_datetime DESC";
                    DataTable loginData = MainFunctions.NewQuery(mainCondition);

                    string recordsIds = "";
                    for (int i = 0; i < loginData.Rows.Count; i++)
                    {
                        if (recordsIds != "") recordsIds += ",";
                        recordsIds += loginData.Rows[i][0].ToString();
                    }

                    LoadHistory(recordsIds);

                }
            }
        }
        #endregion

        #region TextChanged
        private void tbxLoginSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            char[] charList = textBox.Text.ToCharArray();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!MainFunctions.ValidateString_EngNum(charList[i].ToString()))
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                }
            }

            FilterChanged();
        }

        private void tbxStartDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
            {
                tbWrongStartDate.Visibility = Visibility.Hidden;
            }
            else
                tbWrongStartDate.Visibility = Visibility.Visible;

            FilterChanged();
        }

        private void tbxEndDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonFunctions.CheckDate(sender);

            if (CommonFunctions.isTextDateValid(tbxEndDate.Text))
            {
                tbWrongEndDate.Visibility = Visibility.Hidden;

                if (CommonFunctions.isTextDateValid(tbxStartDate.Text))
                {
                    if (!(Convert.ToDateTime(tbxEndDate.Text) >= Convert.ToDateTime(tbxStartDate.Text)))
                        tbWrongEndDate.Visibility = Visibility.Visible;
                }
            }
            else
                tbWrongEndDate.Visibility = Visibility.Visible;

            FilterChanged();
        }
        
        private void tbxLogsSearch_TextChanged(object sender, TextChangedEventArgs e)
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

        #region DataGridEvents
        private void dtgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgHistory.SelectedIndex != -1)
                borderDeleteButton.IsEnabled = true;
            else
                borderDeleteButton.IsEnabled = false;
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

        private void tbxTop_TextChanged(object sender, TextChangedEventArgs e)
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

            if (tbTop.Text.Length > 4) tbTop.Text = tbTop.Text.Substring(0, 4);

            FilterChanged();
        }
    }
}
