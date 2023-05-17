using System;
using System.Windows;

namespace DreamTrip.Windows
{
    /// <summary>
    /// Представляет собой кастомное окно для вывода сообщений пользователю
    /// </summary>
    public partial class Message : Window
    {
        #region Variables
        bool Answer = false;
        bool isTwoAnswers;
        public event Action<bool> messageAnswer;
        #endregion

        #region Constructor
        public Message(string title, string text, bool temp_isTwoAnswers = false, bool isYesNo = false)
        {
            InitializeComponent();

            isTwoAnswers = temp_isTwoAnswers;
            if (title.Length > 18) MessageTitle.Text = title.Substring(0, 18);
            else MessageTitle.Text = title;
            MessageText.Text = text;
            if (temp_isTwoAnswers)
            {
                borderLeftButton.Visibility = Visibility.Visible;
                borderRightButton.Visibility = Visibility.Visible;
                if (isYesNo)
                {
                    btnRight.Content = "Да";
                    btnLeft.Content = "Нет";
                }
                else
                {
                    btnRight.Content = "Ок";
                    btnLeft.Content = "Отмена";
                }
            }
            else
            {
                borderCenterButton.Visibility = Visibility.Visible;
                btnCenter.Content = "Ок";
            }
        }
        #endregion

        #region ButtonsClick
        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            Answer = true;
            this.Close();
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            Answer = true;
            this.Close();
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            Answer = false;
            this.Close();
        }
        #endregion

        #region Functions
        public void Request()
        {
            messageAnswer(Answer);
        }
        #endregion

        #region Handlers
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isTwoAnswers) Request();
        }
        #endregion

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (isTwoAnswers) btnRight_Click(sender, e);
                else btnCenter_Click(sender, e);
            }
        }
    }


}
