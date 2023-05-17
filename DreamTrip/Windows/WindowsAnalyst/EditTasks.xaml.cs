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
    /// Логика взаимодействия для EditTasks.xaml
    /// </summary>
    public partial class EditTasks : UserControl
    {
        #region Variables
        TabClass parentTabItemLink;
        string[] thisPageParametres = new string[] { "Auto", "Задачи", "../../Resources/task.png" };
        UserControl previousPage;
        string[] previousPageParametres;

        List<string> taskTypePaths = new List<string>();
        List<Client> currentClients = new List<Client>();
        List<MTask> tasks = new List<MTask>();

        MTask newTask;
        ComboBoxItem cmiNoMatches;

        #endregion

        #region Constructor
        public EditTasks(TabClass tempTabItem, UserControl tempPreviousPage, string[] tempPreviousPageParametres)
        {
            InitializeComponent();

            parentTabItemLink = tempTabItem;
            previousPage = tempPreviousPage;
            previousPageParametres = tempPreviousPageParametres;
            MainFunctions.ChangeTabParametres(parentTabItemLink, thisPageParametres);

            cmiNoMatches = cmiNothingFound;

            LoadTaskTypes();
            LoadTasks();
            LoadNewTask();
            LoadClients();

            cmbClient.DataContext = currentClients;
        }
        #endregion

        #region LoadData
        public void LoadTasks()
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
                $"ISNULL(cc.phone,'нет'), " +
                $"ISNULL(cc.email,'нет')  " +

                $"FROM Task t " +
                $"JOIN Task_type tt ON tt.id_task_type = t.id_task_type " +
                $"LEFT JOIN Task_call tc ON tc.id_task = t.id_task " +
                $"LEFT JOIN Client c ON c.id_client = tc.id_client " +
                $"LEFT JOIN Client_contacts cc ON cc.id_client = tc.id_client " +
                $"WHERE t.is_done = 0 " +
                $"ORDER BY t.date_creation DESC");

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
                    Color = "#FFB1B1B1",
                };


                if (task.TaskTypeId == 1 || task.TaskTypeId == 2)
                {
                    task.ClientVisible = Visibility.Visible;
                    task.ClientName = tasksData.Rows[i][7].ToString();
                    task.ClientId = int.Parse(tasksData.Rows[i][6].ToString());

                    switch (task.TaskTypeId)
                    {
                        case 1:
                            string phone = tasksData.Rows[i][8].ToString();
                            try
                            {
                                task.ClientContact = "8 (" + phone.Substring(0, 3) + ") " + phone.Substring(3, 3) + "-" + phone.Substring(6, 2) + "-" + phone.Substring(8, 2);
                            }
                            catch
                            {
                                task.ClientContact = "нет";
                            }
                            
                            
                            break;

                        case 2:
                            try
                            {
                                task.ClientContact = tasksData.Rows[i][9].ToString();
                            }
                            catch
                            {
                                task.ClientContact = "нет";
                            }


                            break;
                    }
                }

                tasks.Add(task);
            }

            lvTasks.ItemsSource = tasks;
        }

        void LoadNewTask()
        {
            newTask = new MTask()
            {
                Id = 0,
                Text = "",
                TaskTypeId = 0,
                ImageSource = taskTypePaths[0],
                ClientVisible = Visibility.Visible,
            };

            brdNewTask.DataContext = newTask;

        }

        void LoadTaskTypes()
        {
            taskTypePaths.Clear();

            DataTable taskTypesData = MainFunctions.NewQuery($"SELECT icon_path FROM Task_type");

            for (int i = 0; i < taskTypesData.Rows.Count; i++)
            {
                taskTypePaths.Add(taskTypesData.Rows[i][0].ToString());
            }
        }
       
        void LoadClients(string nameSearch = "")
        {
            //for (int i = 2; i < cmbClient.Items.Count; i++)
            //{
            //    cmbClient.Items.RemoveAt(i);
            //}

            currentClients.Clear();
            cmbClient.ItemsSource = null;
            if (cmbClient.Items.Count > 0) cmbClient.Items.Clear();

            DataTable clientsData = MainFunctions.NewQuery($"SELECT id_client, " +
                $"surname, " +
                $"name, " +
                $"patronymic " +
                $"FROM Client " +
                $"WHERE(lower(surname) LIKE '%{nameSearch.ToLower()}%') " +
                $"OR(lower(name) LIKE '%{nameSearch.ToLower() }% ') " +
                $"OR(lower(patronymic) LIKE '%{nameSearch.ToLower() }% ')");

            for (int i = 0; i < clientsData.Rows.Count; i++)
            {
                Client newClient = new Client()
                {
                    ClientId = Convert.ToInt32(clientsData.Rows[i][0].ToString()),
                    Surname = clientsData.Rows[i][1].ToString(),
                    Name = clientsData.Rows[i][2].ToString(),
                    Patronymic = clientsData.Rows[i][3].ToString()
                };

                currentClients.Add(newClient);
            }

            


            if (currentClients.Count == 0)
            {
                cmbClient.Items.Add(cmiNoMatches);
            }
            else
            {
                cmbClient.ItemsSource = currentClients;

                bool selectedUpdated = false;

                for (int i = 0; i < currentClients.Count; i++)
                {
                    if (currentClients[i].ClientId == newTask.ClientId)
                    {
                        cmbClient.SelectedItem = currentClients[i];
                        selectedUpdated = true;
                        break;
                    }
                }

                if (!selectedUpdated) cmbClient.SelectedIndex = 0;
            }

        }

        #endregion

        #region Functions
        void DataChanged()
        {
            if (cmbClient != null && newTask != null && brdAddTask != null && tbxText != null)
            {
                try
                {
                    if (newTask.ImageSource.Contains("call") || newTask.ImageSource.Contains("email"))
                    {
                        if ((cmbClient.SelectedItem as Client) != null)
                            brdAddTask.IsEnabled = tbxText.Text.Length > 0 && (cmbClient.SelectedItem as Client).ClientId > 0;
                        else
                            brdAddTask.IsEnabled = false;
                    }
                    else
                    {
                        brdAddTask.IsEnabled = tbxText.Text.Length > 0;
                    }


                }
                catch
                {
                    if (brdAddTask != null) brdAddTask.IsEnabled = false;
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
        
        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            bool popupAnswer = false;
            Message messageDeleteWindow = new Message("Удаление", "Вы точно хотите удалить данную задачу?", true, true);
            messageDeleteWindow.messageAnswer += value => popupAnswer = value;
            messageDeleteWindow.ShowDialog();

            if (popupAnswer)
            {

                int taskId = Convert.ToInt32((sender as Button).Tag.ToString());

                try
                {
                    MainFunctions.NewQuery($"DELETE FROM Task_call WHERE id_task = {taskId}");
                    MainFunctions.NewQuery($"DELETE FROM Task WHERE id_task = {taskId}");
                    //new Message("Успех", $"Задача успешно удалена!").ShowDialog();

                    MainFunctions.AddLogRecord("Task deletion success" +
                        $"\n\tID: {taskId}");
                }
                catch (Exception ex)
                {
                    new Message("Ошибка", $"Что-то пошло не так...").ShowDialog();
                    MainFunctions.AddLogRecord($"Task deletion error" +
                        $"\n\tID: {taskId}" +
                        $"\n\tError text: {ex.Message}");
                }

                LoadTasks();
            }

        }

        private void tbClientName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int clientId = int.Parse((sender as TextBlock).Tag.ToString());
            parentTabItemLink.ItemUserControl = new EditClient(parentTabItemLink, this, thisPageParametres, clientId);


        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (newTask.TaskTypeId >= (taskTypePaths.Count - 1))
            {
                newTask.TaskTypeId = 0;
            }
            else
            {
                newTask.TaskTypeId += 1;
            }

            newTask.ImageSource = taskTypePaths[newTask.TaskTypeId];

            if (newTask.ImageSource.Contains("call") || newTask.ImageSource.Contains("email"))
            {
                newTask.ClientVisible = Visibility.Visible;
            }
            else
            {
                newTask.ClientVisible = Visibility.Hidden;
            }

            DataChanged();
        }

        private void btnAddTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idTaskType;
                try
                {
                    idTaskType = Convert.ToInt32(MainFunctions.NewQuery($"SELECT id_task_type FROM Task_type WHERE icon_path = '{newTask.ImageSource}'").Rows[0][0].ToString());
                }
                catch
                {
                    idTaskType = 3;
                }

                string text = tbxText.Text;

                MainFunctions.NewQuery($"INSERT INTO Task VALUES ({idTaskType},'{text}',getdate(),NULL,NULL,0)");
                if (idTaskType == 1 || idTaskType == 2) MainFunctions.NewQuery($"INSERT INTO Task_call " +
                    $" VALUES ((SELECT MAX(id_task) FROM Task), {newTask.ClientId})");

                MainFunctions.AddLogRecord("Task creating success" +
                    $"\n\tID: {MainFunctions.NewQuery("SELECT MAX(id_task) FROM Task").Rows[0][0].ToString()}");

                LoadNewTask();
                LoadTasks();
            }
            catch (Exception ex)
            {
                new Message("Ошибка", "Ошибка добавления задачи", false, false).ShowDialog();

                MainFunctions.AddLogRecord("Task creating error" +
                    $"\n\tError text: {ex.Message}");
            }

        }
        #endregion

        #region Changed
        private void tbxText_TextChanged(object sender, TextChangedEventArgs e)
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

            DataChanged();
        }

        private void tbxClientName_TextChanged(object sender, TextChangedEventArgs e)
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
        }
        
        private void cmbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cmbClient.SelectedItem as Client) != null && (cmbClient.SelectedItem as Client).ClientId > 0)
            {
                newTask.ClientId = (cmbClient.SelectedItem as Client).ClientId;
            }

            DataChanged();
        }







        #endregion

        private void cmbClient_DropDownOpened(object sender, EventArgs e)
        {
            LoadClients(tbxClientName.Text);

            
        }

    }
}
