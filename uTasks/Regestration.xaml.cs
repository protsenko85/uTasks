using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace uTasks
{
    /// <summary>
    /// Логика взаимодействия для Regestration.xaml
    /// </summary>
    public partial class Regestration : Window
    {
        public Regestration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)//regestration try
        {
            if(PasswordBox.Password != PasswordBox2.Password)
            {
                MessageBox.Show("Паролі не співпадають. ");
            }
            else if(LoginBox.Text == "")
            {
                MessageBox.Show("Введіть ім'я.");
            }
            else if(PasswordBox.Password == "")
            {
                MessageBox.Show("Введіть пароль.");
            }
            else if (NameBox.Text == "")
            {
                MessageBox.Show("Введіть ім'я.");
            }
            else if (LNameBox.Text == "")
            {
                MessageBox.Show("Введіть фамілію.");
            }
            else if(isUserExist())
            {
                MessageBox.Show("Користувач з таким логіном вже існує. Спробуйте інший.");
            }
            else
            {
                DB db = new DB();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO `users` (`user_id`, `login`," +
                    " `password`, `is_admin`, `fname`, `lname`) VALUES (NULL, @ul, @up," +
                    " 0, @ufn, @uln)", db.GetConnection());
                cmd.Parameters.Add("@ul", MySqlDbType.VarChar).Value = LoginBox.Text;
                cmd.Parameters.Add("@up", MySqlDbType.VarChar).Value = PasswordBox.Password;
                cmd.Parameters.Add("@ufn", MySqlDbType.VarChar).Value = NameBox.Text;
                cmd.Parameters.Add("@uln", MySqlDbType.VarChar).Value = LNameBox.Text;

                db.openConnection();
                MessageBox.Show("Реєстрація пройшла успішно.");
                Window2 w = new Window2();
                w.Show();
                Close();
                db.closeConnection();
            }
        }

        public bool isUserExist()//check is user exist
        {
            DB db = new DB();
            DataTable table = new DataTable();

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @ul ", 
                db.GetConnection());
            cmd.Parameters.Add("@ul", MySqlDbType.VarChar).Value = LoginBox.Text;

            adapter.SelectCommand = cmd;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Такий логін вже існує.");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//back to login
        {
            Window2 w = new Window2();
            w.Show();
            Close();
        }
    }
}
