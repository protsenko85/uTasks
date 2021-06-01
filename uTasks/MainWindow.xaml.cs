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
using System.Data;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Reflection;

namespace uTasks
{
    public partial class Window1 : Window
    {
        private DataTable ds;
        private User thisUser;

        public class User
        { 
            public User(bool isAdmin, int id, string login)
            {
                this.isAdmin = isAdmin;
                this.id = id;
                this.login = login;
            }
            public bool isAdmin { get; set; }
            public int id { get; set; }
            public string login { get; set; }
        }


        public Window1(bool isAdmin, int id, string login)
        {
            InitializeComponent();
            thisUser = new User(isAdmin, id, login);
            if (!thisUser.isAdmin)
            {
                BtnStat.Visibility = Visibility.Hidden;
            }
        }

        private void addBtn_click(object sender, RoutedEventArgs e)
        {
            if (addName.Text == "")
            {
                MessageBox.Show("Введіть ім'я замовника.");
            }
            else if (addPhone.Text == "")
            {
                MessageBox.Show("Введіть номер замовника.");
            }
            else if (Convert.ToString(DatePicker.SelectedDate) == "")
            {
                MessageBox.Show("Оберіть гарантовану дату видачі.");
            }
            else
            {
                DB db = new DB();
                db.openConnection();
                MySqlCommand cmd = new MySqlCommand("SELECT customer_id FROM customers WHERE name = @name", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = addName.Text;
                DataTable buf = new DataTable();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(buf);

                if (buf.Rows.Count < 1)
                {
                    cmd = new MySqlCommand("INSERT INTO customers (customer_id, name, phone_number) VALUES (NULL, @name, @pn)", db.GetConnection());
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = addName.Text;
                    cmd.Parameters.Add("@pn", MySqlDbType.VarChar).Value = addPhone.Text;
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Додано нового користувача!");

                    cmd = new MySqlCommand("SELECT customer_id FROM customers WHERE name = @name", db.GetConnection());
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = addName.Text;
                    adapter = new MySqlDataAdapter();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(buf);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Такий кристувач вже існує. Обновити його номер?", "Info", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            cmd = new MySqlCommand("UPDATE customers SET phone_number = @number WHERE customers.name = @name", db.GetConnection());
                            cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = addName.Text;
                            cmd.Parameters.Add("@number", MySqlDbType.VarChar).Value = addPhone.Text;
                            cmd.ExecuteNonQuery();
                            break;
                    }

                }

                cmd = new MySqlCommand("INSERT INTO `orders` (`order_id`, `deskr`, `status`, `receipt_time`, `return_time`, `user_id`, `customer_id`)" +
                    " VALUES(NULL, @deskr, 'Тільки прийнято', @receipt, @retrunTime, @userId, @customerId)", db.GetConnection());
                cmd.Parameters.Add("@deskr", MySqlDbType.VarChar).Value = addDetails.Text;
                cmd.Parameters.Add("@receipt", MySqlDbType.Date).Value = DateTime.Now;
                cmd.Parameters.Add("@retrunTime", MySqlDbType.Date).Value = DatePicker.SelectedDate;
                cmd.Parameters.Add("@userId", MySqlDbType.Int32).Value = thisUser.id;
                cmd.Parameters.Add("@customerId", MySqlDbType.Int32).Value = buf.Rows[0][0];
                cmd.ExecuteNonQuery();

                db.closeConnection();

                ds.Rows.Add(ds.Rows.Count + 1, addName.Text, addPhone.Text, thisUser.login, addDetails.Text, DateTime.Now, DatePicker.SelectedDate, "Тільки прийнято", 0, true);
            }
        }

        void cellEditEnding(object sender, DataGridCellEditEndingEventArgs e)//обновляємо БД після редагування таблиці
        {
            checkStatus();//перевіряємо, чи змінився статус
            DB db = new DB();
            db.openConnection();

            MySqlCommand cmd = new MySqlCommand("UPDATE orders SET " +
                " deskr = @ds, status = @st, receipt_time = @rc, return_time = @srt" +
                " WHERE orders.order_id = @c", db.GetConnection());

            cmd.Parameters.Add("@ds", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][4];
            cmd.Parameters.Add("@st", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][7];
            cmd.Parameters.Add("@rc", MySqlDbType.DateTime).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][5];
            cmd.Parameters.Add("@srt", MySqlDbType.DateTime).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][6];
            cmd.Parameters.Add("@c", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("Update customers SET phone_number = @pn WHERE name = @name", db.GetConnection());
            cmd.Parameters.Add("@pn", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][2];
            cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][1];
            cmd.ExecuteNonQuery();

            //перевіряємо, чи є такий користувач вже в бд
            DataTable buf = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            cmd = new MySqlCommand("SELECT * FROM `customers` WHERE `name` = @n", db.GetConnection());
            cmd.Parameters.Add("@n", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][1];
            adapter.SelectCommand = cmd;
            adapter.Fill(buf);
            if(buf.Rows.Count<1)//якщо немає - додаємо нового
            {
                cmd = new MySqlCommand("INSERT INTO customers (customer_id, name, phone_number) VALUES (NULL, @name, @number)", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][1];
                cmd.Parameters.Add("@number", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][2];
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand("SELECT customer_id FROM customers WHERE name = @name", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][1];
                adapter.SelectCommand = cmd;
                DataTable customerId = new DataTable();
                adapter.Fill(customerId);

                cmd = new MySqlCommand("UPDATE orders SET customer_id = @newCId WHERE order_id = @c", db.GetConnection());
                cmd.Parameters.Add("@c", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
                cmd.Parameters.Add("@newCId", MySqlDbType.Int32).Value = customerId.Rows[0][0];
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new MySqlCommand("SELECT customer_id FROM customers WHERE name = @name", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][1];
                adapter.SelectCommand = cmd;
                DataTable customerId = new DataTable();
                adapter.Fill(customerId);

                cmd = new MySqlCommand("UPDATE orders SET customer_id = @newCId WHERE order_id = @c", db.GetConnection());
                cmd.Parameters.Add("@c", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
                cmd.Parameters.Add("@newCId", MySqlDbType.Int32).Value = customerId.Rows[0][0];
                cmd.ExecuteNonQuery();
            }

            updateTable();
            db.closeConnection();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)//заповнюємо таблицю після завантаження
        {
            try
            {
                updateTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void updateTable()
        {
            //беремо дані з таблиці замовлень та вставляємо в таблицю
            DB db = new DB();
            db.openConnection();
            MySqlCommand cmd = new MySqlCommand("SELECT orders.order_id, customers.name, customers.phone_number, users.login, " +
                "orders.deskr, orders.receipt_time, orders.return_time, orders.status FROM orders, customers, users WHERE orders.customer_id = customers.customer_id " +
                "AND orders.user_id = users.user_id", db.GetConnection());
            MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
            ds = new DataTable();
            adp.Fill(ds);

            //знаходимо загальну вартість деталей
            ds.Columns.Add("price", typeof(int));
            cmd = new MySqlCommand("SELECT parts_list.order_id, parts.price FROM parts_list, parts WHERE parts_list.part_id = parts.part_id", db.GetConnection());
            adp = new MySqlDataAdapter(cmd);
            DataTable partsPriceBuf = new DataTable();
            adp.Fill(partsPriceBuf);
            db.closeConnection();

            int sum;
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                sum = 0;
                for (int j = 0; j < partsPriceBuf.Rows.Count; j++)
                {
                    if (Convert.ToInt16(partsPriceBuf.Rows[j][0]) == Convert.ToInt16(ds.Rows[i][0]))
                    {
                        sum += Convert.ToInt16(partsPriceBuf.Rows[j][1]);
                    }
                }
                ds.Rows[i][8] = sum;
            }

            //знаходимо загальну вартість послуг
            cmd = new MySqlCommand("SELECT services_list.order_id, services.price FROM services_list, services WHERE services_list.service_id = services.service_id",
                db.GetConnection());
            adp = new MySqlDataAdapter(cmd);
            DataTable servicesPriceBuf = new DataTable();
            adp.Fill(servicesPriceBuf);
            db.closeConnection();
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                sum = 0;
                for (int j = 0; j < servicesPriceBuf.Rows.Count; j++)
                {
                    if (Convert.ToInt16(servicesPriceBuf.Rows[j][0]) == Convert.ToInt16(ds.Rows[i][0]))
                    {
                        sum += Convert.ToInt16(servicesPriceBuf.Rows[j][1]);
                    }
                }
                ds.Rows[i][8] = Convert.ToInt16(ds.Rows[i][8]) + sum;
            }

            ds.Columns.Add("isActive", typeof(bool));
            checkStatus();

            //створюємо список деталей
            ds.Columns.Add("partsList", typeof(string));
            cmd = new MySqlCommand("SELECT parts_list.order_id, parts.name FROM parts_list, parts WHERE parts_list.part_id = parts.part_id", db.GetConnection());
            adp = new MySqlDataAdapter(cmd);
            DataTable partsList = new DataTable();
            adp.Fill(partsList);
            db.closeConnection();

            int z;
            for (int i = 0; i < partsList.Rows.Count; i++)
            {
                for (int j = 0; j < ds.Rows.Count; j++)
                {
                    if((int)partsList.Rows[i][0] == (int)ds.Rows[j][0])
                    {
                        ds.Rows[j][10] += Convert.ToString(partsList.Rows[i][1]) + ";\n";
                    }
                }
            }

            //створюємо список послуг
            ds.Columns.Add("servicesList", typeof(string));
            cmd = new MySqlCommand("SELECT services_list.order_id, services.name FROM services_list, services " +
                "WHERE services_list.service_id = services.service_id", db.GetConnection());
            adp = new MySqlDataAdapter(cmd);
            DataTable servicesList = new DataTable();
            adp.Fill(servicesList);

            for (int i = 0; i < servicesList.Rows.Count; i++)
            {
                for (int j = 0; j < ds.Rows.Count; j++)
                {
                    if ((int)servicesList.Rows[i][0] == (int)ds.Rows[j][0])
                    {
                        ds.Rows[j][11] += Convert.ToString(servicesList.Rows[i][1]) + ";\n";
                    }
                }
            }




            DataGrid.ItemsSource = new DataView(ds);
            db.closeConnection();
        }

        private void checkStatus()
        {
            string str;
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                str = Convert.ToString(ds.Rows[i][7]).ToLower();
                if (str=="виконано" || Convert.ToString(ds.Rows[i][7]) == "+")
                {
                    ds.Rows[i][9] = false;
                }
                else
                {
                    ds.Rows[i][9] = true;
                }
            }
        }

        private void PartsBtn_Click(object sender, RoutedEventArgs e)
        {
            PartsList w = new PartsList((int)ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0], this, thisUser);
            w.Show();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            DB db = new DB();
            db.openConnection();

            MySqlCommand cmd = new MySqlCommand("DELETE FROM parts_list WHERE order_id = @id", db.GetConnection());

            int i = (int)ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
            var b = DataGrid.SelectedItem;
            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("DELETE FROM services_list WHERE order_id = @id", db.GetConnection());
            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("DELETE FROM orders WHERE order_id = @id", db.GetConnection());
            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0];
            cmd.ExecuteNonQuery();

            db.closeConnection();

            ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)].Delete();
            ds.AcceptChanges();
        }

        private void BtnStat_Click(object sender, RoutedEventArgs e)
        {
            Statistics s = new Statistics(ds, thisUser);
            s.Show();
            this.Close();
        }
        private void ServicesBtn_Click(object sender, RoutedEventArgs e)
        {
            ServicesList ws = new ServicesList((int)ds.Rows[DataGrid.Items.IndexOf(DataGrid.SelectedItem)][0], this, thisUser);
            ws.Show();
        }
    }
}
