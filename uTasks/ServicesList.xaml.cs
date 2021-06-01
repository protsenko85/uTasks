using MySql.Data.MySqlClient;
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
using System.Windows.Shapes;
using static uTasks.Window1;

namespace uTasks
{
    /// <summary>
    /// Логика взаимодействия для ServicesList.xaml
    /// </summary>
    public partial class ServicesList : Window
    {
        private DataTable ServicesList_ = new DataTable();
        private int selectedRow;
        private Window1 w;
        private User u;
        public ServicesList(int selectedRow, Window1 w, User u)
        {
            this.selectedRow = selectedRow;
            this.w = w;
            this.u = u;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                updateList(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void updateList()
        {
            ServicesList_.Clear();
            DB db = new DB();
            db.openConnection();
            MySqlCommand cmd = new MySqlCommand("SELECT name, service_id, price FROM services", db.GetConnection());
            MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
            adp.Fill(ServicesList_);
            db.closeConnection(); 
            ObservableCollection<Row> collection = new ObservableCollection<Row>();
            this.List.ItemsSource = collection;
            for (int i = 0; i < ServicesList_.Rows.Count; i++)
            {
                Row r = new Row(Convert.ToString(ServicesList_.Rows[i][0]), Convert.ToInt32(ServicesList_.Rows[i][2]));
                collection.Add(r);
            }
        }
        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            //удаление старого листа
            DB db = new DB();
            db.openConnection();
            MySqlCommand cmd = new MySqlCommand("DELETE FROM `services_list` WHERE `services_list`.`order_id` = @c", db.GetConnection());
            cmd.Parameters.Add("@c", MySqlDbType.Int16).Value = selectedRow;
            cmd.ExecuteNonQuery();

            //добавление отдельных записей для нового листа
            int selectedIndex = 0;
            for (int i = 0; i < List.SelectedItems.Count; i++)
            {
                for (int j = 0; j < ServicesList_.Rows.Count; j++)
                {
                    Row r = (Row)List.SelectedItems[i];
                    if (r._name == Convert.ToString(ServicesList_.Rows[j][0]))
                    {
                        selectedIndex = Convert.ToInt16(ServicesList_.Rows[j][1]);
                        cmd = new MySqlCommand("INSERT INTO `services_list` (`services_list_id`, `service_id`, `order_id`)" +
                            " VALUES (NULL, @service_id, @order_id)", db.GetConnection());
                        cmd.Parameters.Add("@service_id", MySqlDbType.Int16).Value = selectedIndex;
                        cmd.Parameters.Add("@order_id", MySqlDbType.Int16).Value = selectedRow;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            db.closeConnection();

            w.Close();
            w = new Window1(u.isAdmin, u.id, u.login);
            w.Show();

            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            w.Close();
            w = new Window1(u.isAdmin, u.id, u.login);
            w.Show();
            this.Close();
        }

        private void addOneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (newOneTxtName.Text == "" || newOneTxtPrice.Text == "")
            {
                MessageBox.Show("Заповніть поля для додавання нового елементу!");
            }
            else
            {
                DB db = new DB();
                db.openConnection();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO services (service_id, name, price) VALUES (NULL, @name, @price)", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = newOneTxtName.Text;
                cmd.Parameters.Add("@price", MySqlDbType.Int16).Value = Convert.ToInt32(newOneTxtPrice.Text);
                cmd.ExecuteNonQuery();
                updateList();
                db.closeConnection();
                newOneTxtName.Text = "";
                newOneTxtPrice.Text = "";
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItems.Count < 1)
            {
                MessageBox.Show("Оберіть елементи для видалення");
            }
            else
            {
                DB db = new DB();
                db.openConnection();

                int selectedIndex = 0;
                for (int i = 0; i < List.SelectedItems.Count; i++)
                {
                    for (int j = 0; j < ServicesList_.Rows.Count; j++)
                    {
                        Row r = (Row)List.SelectedItems[i];
                        if (r._name == Convert.ToString(ServicesList_.Rows[j][0]))
                        {
                            selectedIndex = Convert.ToInt16(ServicesList_.Rows[j][1]);

                            MySqlCommand cmd = new MySqlCommand("DELETE FROM services_list WHERE service_id = @id", db.GetConnection());
                            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = selectedIndex;
                            cmd.ExecuteNonQuery();

                            cmd = new MySqlCommand("DELETE FROM services WHERE service_id = @id", db.GetConnection());
                            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = selectedIndex;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                db.closeConnection();
            }
            updateList();
        }
    }
}
