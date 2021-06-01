using System;
using System.Collections.Generic;
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
using MySql.Data.MySqlClient;
using System.Data;

namespace uTasks
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        private bool isAdmin = false;
        public Window2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)//login try
        {
            DB db = new DB();
            DataTable table = new DataTable();

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand cmd = new MySqlCommand("SELECT user_id, login, is_admin FROM users WHERE `login` = @ul " +
                "AND `password` = @up", db.GetConnection());
            cmd.Parameters.Add("@ul", MySqlDbType.VarChar).Value = LoginBox.Text;
            cmd.Parameters.Add("@up", MySqlDbType.VarChar).Value = PasswordBox.Password;

            adapter.SelectCommand = cmd;
            adapter.Fill(table);

            if (table.Rows.Count>0){
                MessageBox.Show("Успішна авторизація!"); 
                if (Convert.ToString(table.Rows[0][2]) == "True")
                    isAdmin = true;
                Window1 w2 = new Window1(isAdmin, Convert.ToInt32(table.Rows[0][0]), Convert.ToString(table.Rows[0][1]));
                w2.Show();
                Close();
            }
            else{
                MessageBox.Show("Не правильний логі або пароль. Спробуйте ще раз!");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)//open regestration
        {
            Regestration w2 = new Regestration();
            w2.Show();
            Close();
        }
    }
}
