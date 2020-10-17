using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp_ADO_NET_HW_4
{
    public partial class Form1 : Form
    {
        private SqlCommandBuilder commandBuilder;
        private readonly DataSet dataSet = new DataSet();
        private SqlDataAdapter adapter;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                adapter = new SqlDataAdapter("SELECT * FROM Herd", ConfigurationManager.ConnectionStrings["Cowshed"].ConnectionString);
                adapter.Fill(dataSet);

                using (SqlCommand delete = new SqlCommand())
                {
                    delete.CommandText = "DELETE FROM Herd WHERE ID = @p1";
                    delete.Parameters.Add("@p1", SqlDbType.Int);
                    delete.Parameters["@p1"].SourceColumn = "Id";
                    delete.Connection = adapter.SelectCommand.Connection;
                    commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.DeleteCommand = delete;
                }

                if (dataSet.Tables.Count != 0)
                {
                    dataGridView1.DataSource = dataSet.Tables[0];
                    dataGridView1.Columns[0].Visible = false;
                    dataGridView1.Columns[1].HeaderText = "Корова";
                    dataGridView1.Columns[2].HeaderText = "Надои";
                }
                else throw new Exception("У DataSet отсутствуют таблицы");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        // Сохранить
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataSet.GetChanges() != null)
                {
                    dataSet.AcceptChanges();
                    dataGridView2.DataSource = dataSet.Tables[0];
                    groupBox1.Visible = true;
                    dataGridView2.Columns[0].Visible = false;
                    dataGridView2.Columns[1].HeaderText = "Корова";
                    dataGridView2.Columns[2].HeaderText = "Надои";
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        // Отменить
        private void button2_Click(object sender, EventArgs e) => dataSet.RejectChanges();

        // Подтверждение изменений
        private void button3_Click(object sender, EventArgs e)
        {
            adapter.Update(dataSet, dataSet.Tables[0].TableName);
            groupBox1.Visible = false;
        }

        // Отмена в окне с Подтверждением изменений
        private void button4_Click(object sender, EventArgs e)
        {
            dataSet.RejectChanges();
            dataGridView1.Update();
            groupBox1.Visible = false;
        }
    }
}