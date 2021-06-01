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
    public class Row
    {
        public Row(string str, int p)
        {
            _name = str;
            _price = p;
        }

        private string name;
        public string _name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }

        }

        private int price;
        public int _price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
            }
        }
    }
    /// <summary>
    /// Логика взаимодействия для PartsList.xaml
    /// </summary>
    public partial class PartsList : Window
    {
        private DataTable ItemsList = new DataTable();
        private int selectedRow;
        private Window1 w;
        User u;
        public PartsList(int selectedRow, Window1 w, User u)
        {
            this.selectedRow = selectedRow;
            this.w = w;
            this.u = u;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateList();
        }

        private void updateList()
        {
            try
            {
                DB db = new DB();
                db.openConnection();
                MySqlCommand cmd = new MySqlCommand("SELECT name, part_id, price FROM parts", db.GetConnection());
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                ItemsList.Clear();
                adp.Fill(ItemsList);
                db.closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            ObservableCollection<Row> collection = new ObservableCollection<Row>();
            this.List.ItemsSource = collection;
            for (int i = 0; i < ItemsList.Rows.Count; i++)
            {
                Row r = new Row(Convert.ToString(ItemsList.Rows[i][0]), Convert.ToInt32(ItemsList.Rows[i][2]));
                collection.Add(r);
            }
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            //удаление старого листа
            DB db = new DB();
            db.openConnection();
            MySqlCommand cmd = new MySqlCommand("DELETE FROM `parts_list` WHERE `parts_list`.`order_id` = @c", db.GetConnection());
            cmd.Parameters.Add("@c", MySqlDbType.Int16).Value = selectedRow;
            cmd.ExecuteNonQuery();

            //добавление отдельных записей для нового листа
            int selectedIndex = 0;
            for (int i = 0; i < List.SelectedItems.Count; i++)
            {
                for (int j = 0; j < ItemsList.Rows.Count; j++)
                {
                    Row r = (Row)List.SelectedItems[i];
                    if (r._name == Convert.ToString(ItemsList.Rows[j][0]))
                    {
                        selectedIndex = Convert.ToInt16(ItemsList.Rows[j][1]);
                        cmd = new MySqlCommand("INSERT INTO `parts_list` (`part_list_id`, `part_id`, `order_id`)" +
                            " VALUES (NULL, @part_id, @order_id)", db.GetConnection());
                        cmd.Parameters.Add("@part_id", MySqlDbType.Int16).Value = selectedIndex;
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
                MySqlCommand cmd = new MySqlCommand("INSERT INTO parts (part_id, name, price) VALUES (NULL, @name, @price)", db.GetConnection());
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = newOneTxtName.Text;
                cmd.Parameters.Add("@price", MySqlDbType.Int16).Value = Convert.ToInt32(newOneTxtPrice.Text);
                cmd.ExecuteNonQuery();
                updateList();
                db.closeConnection();
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if(List.SelectedItems.Count<1)
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
                    for (int j = 0; j < ItemsList.Rows.Count; j++)
                    {
                        Row r = (Row)List.SelectedItems[i];
                        if (r._name == Convert.ToString(ItemsList.Rows[j][0]))
                        {
                            selectedIndex = Convert.ToInt16(ItemsList.Rows[j][1]); 

                            MySqlCommand cmd = new MySqlCommand("DELETE FROM parts_list WHERE part_id = @id", db.GetConnection());
                            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = selectedIndex;
                            cmd.ExecuteNonQuery();

                            cmd = new MySqlCommand("DELETE FROM parts WHERE part_id = @id", db.GetConnection());
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
