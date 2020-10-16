using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp_ADO_NET_HW_2
{
    public partial class Form1 : Form
    {
        private string connectionString;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["Sales"].ConnectionString;
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    DataTable dt = cnn.GetSchema("Tables");
                    foreach (DataRow row in dt.Rows)
                    {
                        comboBox1.Items.Add((string)row[2]);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    await cnn.OpenAsync();

                    using (SqlCommand select = new SqlCommand())
                    {
                        select.Connection = cnn;
                        string tableName = comboBox1.SelectedItem.ToString();

                        if (tableName == "Sellers" || tableName == "Buyers")
                        {
                            select.CommandText = "SELECT * FROM " + tableName;
                            ReadData(select, tableName);
                        }
                        else if (tableName == "Sales")
                        {
                            select.CommandText = 
                                "Select b.FirstName, b.SecondName, sl.FirstName, sl.SecondName, s.DealAmount, s.DateOfDeal " +
                                "from Sales s " +
                                "left join Buyers b on b.Id = s.BuyerId " +
                                "left join Sellers sl on sl.Id = s.SellerId";
                            ReadData(select, tableName);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void CreateDGV(string tableName)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (tableName == "Buyers" || tableName == "Sellers")
            {
                dataGridView1.ColumnCount = 2;
                dataGridView1.Columns[0].HeaderText = "Имя";
                dataGridView1.Columns[1].HeaderText = "Фамилия";
            }
            else if (tableName == "Sales")
            {
                dataGridView1.ColumnCount = 4;
                dataGridView1.Columns[0].HeaderText = "Покупатель";
                dataGridView1.Columns[1].HeaderText = "Продавец";
                dataGridView1.Columns[2].HeaderText = "Сумма сделки";
                dataGridView1.Columns[3].HeaderText = "Дата сделки";
            }
            else throw new Exception("Таблица с идентификатором: " + tableName + ", не предусмотрена к созданию.");
        }

        private async void ReadData(SqlCommand cmd, string tableName)
        {
            CreateDGV(tableName);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if(tableName == "Buyers" || tableName == "Sellers") dataGridView1.Rows.Add(reader.GetString(1), reader.GetString(2));

                    if (tableName == "Sales")
                    { 
                        dataGridView1.Rows.Add((string)reader[0] + " " + (string)reader[1], (string)reader[2] + " " + (string)reader[3], reader[4],
                            ((DateTime)reader[5]).ToShortDateString()); 
                    }
                }
            }
        }
    }
}