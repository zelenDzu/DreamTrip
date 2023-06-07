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
    /// Логика взаимодействия для Tasks.xaml
    /// </summary>
    public partial class Tasks : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Задачи", "../../Resources/task.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<MTask> tasks = new List<MTask>();

        #endregion

        #region Constructor
        public Tasks(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            SortTasks();

            if (MainFunctions.GetShowPrompts()) btnHelpInfo.Visibility = Visibility.Visible;
            else btnHelpInfo.Visibility = Visibility.Hidden;
        }
        #endregion

        #region LoadData 
        public void LoadTasks(string orderCondition = "")
        {
            tasks.Clear();
            lvTasks.ItemsSource = null;

            DataTable tasksData = MainFunctions.NewQuery($"SELECT t.id_task, " +
                $"t.text, " +
                $"t.date_creation, " +
                $"t.is_done, " +
                $"tt.icon_path, " +
                $"t.id_task_type, " +
                $"tc.id_client, " +
                $"CONCAT(c.surname, ' ', c.name, ' ', c.patronymic), " +
                $"cc.phone, " +
                $"cc.email " +

                $"FROM Task t " +
                $"JOIN Task_type tt ON tt.id_task_type = t.id_task_type " +
                $"LEFT JOIN Task_call tc ON tc.id_task = t.id_task " +
                $"LEFT JOIN Client c ON c.id_client = tc.id_client " +
                $"LEFT JOIN Client_contacts cc ON cc.id_client = tc.id_client " +
                $"WHERE t.is_done = 0 or (t.is_done = 1 and DATEDIFF(hour,date_done,getdate()) <= 6 ) {orderCondition}");

            for (int i = 0; i < tasksData.Rows.Count; i++)
            {
                MTask task = new MTask()
                {
                    Id = int.Parse(tasksData.Rows[i][0].ToString()),
                    Text = tasksData.Rows[i][1].ToString(),
                    Date = Convert.ToDateTime(tasksData.Rows[i][2]).ToShortDateString().ToString(),
                    IsDone = Convert.ToBoolean(tasksData.Rows[i][3]),
                    ImageSource = tasksData.Rows[i][4].ToString(),
                    TaskTypeId = int.Parse(tasksData.Rows[i][5].ToString()),
                    ClientId = 0,
                    ClientName = "",
                    ClientContact = "",
                    ClientVisible = Visibility.Hidden,
                };

                if (task.IsDone) task.Color = "#FF9EE8E1";
                else task.Color = "#FFB1B1B1";


                if (task.TaskTypeId == 1 || task.TaskTypeId == 2)
                {
                    task.ClientVisible = Visibility.Visible;
                    task.ClientName = tasksData.Rows[i][7].ToString();
                    task.ClientId = int.Parse(tasksData.Rows[i][6].ToString());


                    switch (task.TaskTypeId)
                    {
                        case 1:
                            string phone = tasksData.Rows[i][8].ToString();
                            task.ClientContact = "8 (" + phone.Substring(0,3) + ") " + phone.Substring(3,3) +"-"+ phone.Substring(6, 2) + "-" + phone.Substring(8, 2);
                            break;

                        case 2:
                            task.ClientContact = tasksData.Rows[i][9].ToString();
                            break;
                    }
                }

                tasks.Add(task);

            }

            lvTasks.ItemsSource = tasks;
        }
        #endregion

        #region Functions

        private void SortTasks()
        {
            if (lvTasks != null)
            {
                string orderType = "";
                switch (cmbSort.SelectedIndex)
                {
                    case 0: //по дате
                        orderType = "t.date_creation";
                        break;

                    case 1: //по типу
                        orderType = "t.id_task_type";
                        break;

                    case 2: //по выполнению
                        orderType = "t.is_done";
                        break;
                }

                LoadTasks($" ORDER BY {orderType} ");
            }
        }

        /// <summary>
        /// При событии "Скролл мыши"
        /// </summary>
        /// <param name="Delta">значение скролла</param>
        public void ScrollEvent(int Delta)
        {
            int dtgDelta = Delta * 2 / 7;
            if (Delta < 0) Delta = Delta / 40 + 2;
            else Delta = Delta / 40 - 2;


           

            if (lvTasks.IsMouseOver)
            {
                ScrollViewer scv = GetScroll(lvTasks);
                if (scv != null)
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - dtgDelta);
                }
                else
                {
                    //System.Windows.Forms.MessageBox.Show("null");
                }
            }


        }

        /// <summary>
        /// Получить скрол внутри listview
        /// </summary>
        /// <param name="listView"></param>
        /// <returns></returns>
        private ScrollViewer GetScroll(ListView listView)
        {
            bool indexException = false;
            DependencyObject timeReference = listView;
            while (!indexException)
            {
                try
                {
                    timeReference = VisualTreeHelper.GetChild(timeReference, 0);
                    ScrollViewer viewer = timeReference as ScrollViewer;
                    if (viewer != null)
                    {
                        return viewer;
                    }
                }
                catch // set correct exception handling
                {
                    indexException = true;
                }
            }
            return null;
        }

        #endregion

        #region ButtonsClick
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            parentTabItemLink.ItemUserControl = previousPage;
            MainFunctions.ChangeTabParametres(parentTabItemLink, previousPageParametres);
        }


        private void tbClientName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int clientId = int.Parse((sender as TextBlock).Tag.ToString());
            parentTabItemLink.ItemUserControl = new EditClient(parentTabItemLink, this, thisPageParametres, clientId);
        }

        #endregion

        #region Changed
        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortTasks();
        }
        
        private void cbTaskDone_Checked(object sender, RoutedEventArgs e)
        {
            int taskId = int.Parse((sender as CheckBox).Tag.ToString());
            MainFunctions.NewQuery($"UPDATE Task SET is_done = 1, date_done = getdate(), " +
                $"id_manager = (SELECT id_worker FROM Worker WHERE login = '{MainFunctions.СurrentSessionLogin}') WHERE id_task = {taskId}");

            MainFunctions.AddLogRecord($"Task {taskId} marked as \"done\"");
            SortTasks();
        }

        private void cbTaskDone_Unchecked(object sender, RoutedEventArgs e)
        {
            int taskId = int.Parse((sender as CheckBox).Tag.ToString());
            MainFunctions.NewQuery($"UPDATE Task SET is_done = 0, date_done = NULL, id_manager = NULL WHERE id_task = {taskId}");
            MainFunctions.AddLogRecord($"Task {taskId} marked as \"not done\"");
            SortTasks();
        }
        #endregion


    }
}
