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
using System.Collections.ObjectModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
//using System;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;
using static uTasks.Window1;
//using System.Text;


namespace uTasks
{
    /// <summary>
    /// Логика взаимодействия для Statistics.xaml
    /// </summary>
    public partial class Statistics : Window
    {
        private DataTable dt;
        private DateTime startDate;
        private DateTime endDate;
        private DataTable usersStat;
        private int unDoneCount, proceeds, periodCount;
        private User u;
        public Statistics(DataTable dt, User u)
        {
            this.u = u;
            InitializeComponent();
            this.dt = dt;//копіюємо таблицю з основної форми в цей клас

            startDate = DateTime.Now;
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            endDate = DateTime.Now;
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);

            startDatePick.SelectedDate = startDate;
            endDatePick.SelectedDate = endDate;

            fillusersStatistics();
            fillStatistics();
        }

        private void updateDates()
        {
            startDate = (DateTime)startDatePick.SelectedDate;
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            endDate = (DateTime)endDatePick.SelectedDate;
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
        }

        private void fillStatistics()//встановлюємо значення статистики
        {
            L1.Content = "Загальна кількість замовлень: " + dt.Rows.Count;
            unDoneCount = 0; proceeds = 0; periodCount = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToDateTime(dt.Rows[i][5])>startDate && Convert.ToDateTime(dt.Rows[i][6]) < endDate)
                {
                    proceeds += Convert.ToInt16(dt.Rows[i][8]);
                    periodCount++;
                    if (Convert.ToBoolean(dt.Rows[i][9]) == true)
                    {
                        unDoneCount++;
                    }
                }
            }
            L2.Content = "Кількість замовлень за вказаний період: " + periodCount;
            L3.Content = "З них не виконано: " + unDoneCount;
            L4.Content = "Виручка за вказаний період: " + proceeds + " грн";
        }

        private void fillusersStatistics()
        {
            DataTable buf;
            DB db = new DB();
            db.openConnection();

            MySqlCommand cmd = new MySqlCommand("SELECT user_id, login FROM users", db.GetConnection());
            MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
            usersStat = new DataTable();
            adp.Fill(usersStat);
            usersStat.Columns.Add("ordersCount", typeof(int));
            usersStat.Columns.Add("undoneOrdersCount", typeof(int));
            for (int i = 0; i < usersStat.Rows.Count; i++)
            {
                //додаємо запис про загальну кількість прийнятих замовлень
                buf = new DataTable();
                cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE orders.receipt_time > @rt" +
                    " AND orders.return_time < @srt AND orders.user_id = @uid", db.GetConnection());
                cmd.Parameters.Add("@uid", MySqlDbType.Int16).Value = usersStat.Rows[i][0];
                cmd.Parameters.Add("@rt", MySqlDbType.DateTime).Value = startDate;
                cmd.Parameters.Add("@srt", MySqlDbType.DateTime).Value = endDate;
                adp = new MySqlDataAdapter(cmd);
                adp.Fill(buf);
                usersStat.Rows[i][2] = buf.Rows[0][0];

                int z = Convert.ToInt32(buf.Rows[0][0]);
                //додаємо запис про кількість виконаних замовлень

                buf = new DataTable();
                cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE orders.receipt_time > @rt AND orders.return_time < @srt" +
                    " AND orders.user_id = @uid AND (orders.status = '+' OR orders.status = 'виконано' OR orders.status = 'Виконано')", db.GetConnection());
                cmd.Parameters.Add("@uid", MySqlDbType.Int16).Value = usersStat.Rows[i][0];
                cmd.Parameters.Add("@rt", MySqlDbType.DateTime).Value = startDate;
                cmd.Parameters.Add("@srt", MySqlDbType.DateTime).Value = endDate;
                adp = new MySqlDataAdapter(cmd);
                adp.Fill(buf);
                usersStat.Rows[i][3] = buf.Rows[0][0];

                int z2 = Convert.ToInt32(buf.Rows[0][0]);
            }

            List.ItemsSource = new DataView(usersStat);
            db.closeConnection();
        }

        private void startDataChanged(object sender, RoutedEventArgs e)
        {
            endDatePick.SelectedDate = endDate;
            updateDates();
            fillStatistics();
            fillusersStatistics();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Window1 w = new Window1(u.isAdmin, u.id, u.login);
            w.Show();
            this.Close();
        }

        private void endDataChanged(object sender, RoutedEventArgs e)
        {
            startDatePick.SelectedDate = startDate;
            updateDates();
            fillStatistics();
            fillusersStatistics();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DB db = new DB();
            DataTable buf = new DataTable();
            MySqlCommand cmd = new MySqlCommand("SELECT orders.order_id, customers.name, customers.phone_number, users.login, " +
                "orders.deskr, orders.receipt_time, orders.return_time, orders.status FROM orders, customers, users " +
                "WHERE orders.receipt_time > @rt AND orders.return_time < @srt AND orders.customer_id = customers.customer_id " +
                "AND orders.user_id = users.user_id", db.GetConnection());
            cmd.Parameters.Add("@rt", MySqlDbType.DateTime).Value = startDate;
            cmd.Parameters.Add("@srt", MySqlDbType.DateTime).Value = endDate;
            MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
            adp.Fill(buf);

            var document = new iTextSharp.text.Document();
            using (var writer = PdfWriter.GetInstance(document, new FileStream("C:/Users/user/Desktop/result.pdf", FileMode.Create)))
            {
                document.Open();

                BaseFont baseFont = BaseFont.CreateFont("C:/Windows/Fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);
                var helvetica = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12);
                var helveticaBase = helvetica.GetCalculatedBaseFont(false);
                writer.DirectContent.BeginText();
                writer.DirectContent.SetFontAndSize(baseFont, 14f);

                document.Add(new iTextSharp.text.Paragraph("Інформація за період з " + startDate.Day + "." + startDate.Month + "." + startDate.Year + " по " +
                    + endDate.Day + "." + endDate.Month + "." + endDate.Year, font));
                document.Add(new iTextSharp.text.Paragraph("\nЗагальна кількість замовлень: " + dt.Rows.Count, font));
                document.Add(new iTextSharp.text.Paragraph("\nКількість замовлень за вказаний період: " + periodCount, font));
                document.Add(new iTextSharp.text.Paragraph("\nЗ них не виконано: " + unDoneCount, font));
                document.Add(new iTextSharp.text.Paragraph("\nВиручка за вказаний період: " + proceeds + " грн", font));
                writer.DirectContent.EndText();

                document.Add(new iTextSharp.text.Paragraph("\n"));

                ////////////////////////////
                //Создаем объект таблицы и передаем в нее число столбцов таблицы из нашего датасета
                PdfPTable table = new PdfPTable(buf.Columns.Count-1);

                //Добавим в таблицу общий заголовок
                PdfPCell cell = new PdfPCell(new Phrase("БД"));

                cell.Colspan = buf.Columns.Count-1;
                cell.HorizontalAlignment = 1;
                //Убираем границу первой ячейки, чтобы балы как заголовок
                cell.Border = 0;
                table.AddCell(cell);

                //Сначала добавляем заголовки таблицы
                for (int j = 0; j < buf.Columns.Count-1; j++)
                {
                    cell = new PdfPCell(new Phrase(new Phrase(buf.Columns[j].ColumnName, font)));
                    //Фоновый цвет (необязательно, просто сделаем по красивее)
                    cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                    table.AddCell(cell);
                }

                //Добавляем все остальные ячейки
                for (int j = 0; j < buf.Rows.Count; j++)
                {
                    for (int k = 0; k < buf.Columns.Count-1; k++)
                    {
                        table.AddCell(new Phrase(buf.Rows[j][k].ToString(), font));
                    }
                }
                //Добавляем таблицу в документ
                document.Add(table);

                MessageBox.Show("Документ збережено на робочий стіл");

                ///////////////////////////
                document.Close();
                writer.Close();
            }

        }
    }
}
