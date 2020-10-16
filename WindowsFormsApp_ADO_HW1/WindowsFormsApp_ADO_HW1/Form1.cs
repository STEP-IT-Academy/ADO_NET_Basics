using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp_ADO_HW1
{
    public partial class Form1 : Form
    {
        private readonly string connectionString;

        public Form1()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["EducationInstitution"].ConnectionString;
            ShowData();
        }

        private bool ShowData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();

                    string firstCMD = "SELECT Surname, OOPMark, WinFormsMark, ADONETMark " +
                                      "FROM Students;";

                    string secondCMD = "SELECT Name, CuratorSurname " +
                                       "FROM Groups;";

                    string thirdCMD = "SELECT st.Surname, gr.Name, ROUND((SUM(st.OOPMark + st.WinFormsMark + st.ADONETMark) / 3.), 2) AverageMark " +
                                       "FROM Students st " +
                                       "LEFT JOIN Groups gr ON st.GroupId = gr.Id " +
                                       "GROUP BY st.Surname, gr.Name;";

                    cmd.CommandText = firstCMD + secondCMD + thirdCMD;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        byte j = 1;

                        do
                        {
                            while(reader.Read())
                            {
                                if (j == 1)
                                {
                                    dataGridView1.Rows.Add(reader["Surname"], reader["OOPMark"], reader["WinFormsMark"], reader["ADONETMark"]);
                                    continue;
                                }

                                if (j == 2)
                                {
                                    dataGridView2.Rows.Add(reader["Name"], reader["CuratorSurname"]);
                                    continue;
                                }

                                if (j == 3)
                                {
                                    dataGridView3.Rows.Add(reader["Surname"], reader["Name"], Math.Round(Convert.ToDouble(reader["AverageMark"]), 2));
                                    continue;
                                }
                            }

                            j++;
                        }
                        while (reader.NextResult());

                        j = 1;
                    }
                }

                return true;
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
                return false;
            }
        }

        // Add Group
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        string groupName = textBox1.Text;
                        string curatorName = textBox2.Text;

                        cmd.CommandText = "INSERT INTO Groups (Name, CuratorSurname)" + "values (@groupName, @curatorSurname)";
                        cmd.Parameters.AddWithValue("@groupName", groupName);
                        cmd.Parameters.AddWithValue("@curatorSurname", curatorName);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Upload was successful!");
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;
                button2_Click(this, null);
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        // Information about Gropus and Students
        private void button2_Click(object sender, EventArgs e)
        {
            ClearingAllDataGridViews();
            ShowData();
        }

        // Excellent & Looser students
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if(radioButton1.Checked)
                    {
                        using (SqlCommand cmd = new SqlCommand("getLooserStudentsCount", connection))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add("@GroupName", System.Data.SqlDbType.NVarChar).Value = textBox3.Text;

                            SqlParameter outputParam = new SqlParameter("@LooserStudentsCount", System.Data.SqlDbType.Int);
                            outputParam.Direction = System.Data.ParameterDirection.Output;
                            cmd.Parameters.Add(outputParam);
                            cmd.ExecuteNonQuery();
                            textBox4.Text = cmd.Parameters["@LooserStudentsCount"].Value.ToString();
                        }
                    }     
                    
                    if(radioButton2.Checked)
                    {
                        using (SqlCommand cmd = new SqlCommand("getExcellentStudentsCount", connection))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add("@GroupName", System.Data.SqlDbType.NVarChar).Value = textBox3.Text;

                            SqlParameter outputParam = new SqlParameter("@ExcellentStudentsCount", System.Data.SqlDbType.Int);
                            outputParam.Direction = System.Data.ParameterDirection.Output;
                            cmd.Parameters.Add(outputParam);
                            cmd.ExecuteNonQuery();
                            textBox4.Text = cmd.Parameters["@ExcellentStudentsCount"].Value.ToString();
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        } 

        private void ClearingAllDataGridViews()
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
        }
    }
}