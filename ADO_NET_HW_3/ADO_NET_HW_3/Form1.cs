using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp_ADO_NET_HW_3_
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
            connectionString = ConfigurationManager.ConnectionStrings["Users"].ConnectionString;
            userDataTableAdapter.Fill(usersDataSet.UserData);
        }

        // Кнопка "Добавить пользователя"
        private void button1_Click(object sender, EventArgs e) 
        {
            groupBox2.Visible = false;
            groupBox1.Visible = true;
        }

        // Добавить         
        private void button4_Click_1(object sender, EventArgs e) => AddUserAsync();

        private async void AddUserAsync()
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    await cnn.OpenAsync();
                    using (SqlCommand insert = cnn.CreateCommand())
                    {
                        if (textBox1.Text.Length > 0 && textBox2.Text.Length > 0)
                        {
                            insert.CommandText = "INSERT INTO UserData(Login, HashPassword,  Address, Number, IsAdmin) " + "values(@login, @hashPass, @address, @number, @isAdmin)";
                            if (await IsUniqLoginAsync(textBox1.Text))
                            {
                                int isAdmin = -1;
                                if (radioButton1.Checked) isAdmin = 1;
                                else isAdmin = 0;

                                insert.Parameters.AddWithValue("@login", textBox1.Text);
                                insert.Parameters.AddWithValue("@hashPass", CreatePasswordHash(textBox2.Text));
                                insert.Parameters.AddWithValue("@address", textBox3.Text);
                                insert.Parameters.AddWithValue("@number", textBox4.Text);
                                insert.Parameters.AddWithValue("@isAdmin", isAdmin);
                                await insert.ExecuteNonQueryAsync();

                                userDataTableAdapter.Fill(usersDataSet.UserData);

                                MessageBox.Show("Добавление пользователя завершено успешно!", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                groupBox1.Visible = false;
                                ClearTextBoxesGB1();
                            }
                            else
                            {
                                MessageBox.Show("Пользователь с введенным Вами логином уже существет. Укажите другой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                textBox1.Clear();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            textBox1.Clear();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        // Проверка на уникальность логина
        private async Task<bool> IsUniqLoginAsync(string login)
        {
            // isFreeLogin - хранимая процедукра, проверяющая уникальность логина. Возвращает кол-во пользователей с переданным логином.
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                await cnn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("isFreeLogin", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserLogin", SqlDbType.NVarChar, 100).Value = login;

                    SqlParameter outputParam = new SqlParameter("@FreeOrNot", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(outputParam);
                    await cmd.ExecuteNonQueryAsync();

                    int tmp = (int)cmd.Parameters["@FreeOrNot"].Value;
                    return tmp == 0 ? true : false;
                }
            }
        }

        private void ClearTextBoxesGB1()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
        }

        // Хэширование пароля с использованием SHA384 и "соли"
        private static string CreatePasswordHash(string pwd) => CreatePasswordHash(pwd, CreateSalt());

        private static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = pwd + salt;
            string hashedPwd = GetHashString(saltAndPwd);
            var saltPosition = 5;
            hashedPwd = hashedPwd.Insert(saltPosition, salt);
            return hashedPwd;
        }

        // Проверка соответствия пароля и хэш-представления
        private static bool Validate(string password, string passwordHash)
        {
            var saltPosition = 5;
            var saltSize = 10;
            var salt = passwordHash.Substring(saltPosition, saltSize);
            var hashedPassword = CreatePasswordHash(password, salt);
            return hashedPassword == passwordHash;
        }

        // Создание "соли"
        private static string CreateSalt()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] buff = new byte[20];
                rng.GetBytes(buff);
                var saltSize = 10;
                string salt = Convert.ToBase64String(buff);
                if (salt.Length > saltSize)
                {
                    salt = salt.Substring(0, saltSize);
                    return salt.ToUpper();
                }

                var saltChar = '^';
                salt = salt.PadRight(saltSize, saltChar);
                return salt.ToUpper();
            }
        }

        private static string GetHashString(string password)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(password))
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private static byte[] GetHash(string password)
        {
            using (SHA384 sha = new SHA384CryptoServiceProvider())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Отмена при добавлении
        private void button5_Click(object sender, EventArgs e)
        {
            button8.Visible = false;
            groupBox1.Visible = false;
            ClearTextBoxesGB1();
        }

        // Кнопка "Удалить пользователя"
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            groupBox2.Visible = true;
        }

        // Удалить в "Удалить пользователя"
        private void button7_Click(object sender, EventArgs e) => DelUserAsync();

        // Удаление пользователя
        private async void DelUserAsync()
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    await cnn.OpenAsync();

                    using (SqlCommand delete = cnn.CreateCommand(), getHashPass = cnn.CreateCommand())
                    {
                        if(textBox5.Text.Length > 0 && textBox6.Text.Length > 0)
                        {
                            getHashPass.CommandText = "SELECT HashPassword FROM UserData WHERE Login LIKE @userLogin";
                            getHashPass.Parameters.AddWithValue("@userLogin", textBox5.Text);
                            string str = getHashPass.ExecuteScalar().ToString();

                            if (Validate(textBox6.Text, str))
                            {
                                delete.CommandText = "DELETE FROM UserData WHERE Login like @userLogin";
                                delete.Parameters.AddWithValue("@userLogin", textBox5.Text);
                                await delete.ExecuteNonQueryAsync();

                                userDataTableAdapter.Fill(usersDataSet.UserData);

                                MessageBox.Show("Удаление завершено успешно!", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                groupBox2.Visible = false;
                                ClearTextBoxesGB2();
                            }
                            else
                            {
                                MessageBox.Show("Пользователь с введенным Вами логином уже существет. Укажите другой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox6.Clear();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBox6.Clear();
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Пользователь с введеным Вами логином не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox5.Clear();
            }
        }

        // Отмена при удалении
        private void button6_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = false;
            ClearTextBoxesGB2();
        } 

        private void ClearTextBoxesGB2()
        {
            textBox5.Clear();
            textBox6.Clear();
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0 && dataGridView1.SelectedCells.Count == 3)
            {
                ClearTextBoxesGB1();
                textBox1.Text = dataGridView1.SelectedCells[0].Value.ToString();
                textBox3.Text = dataGridView1.SelectedCells[1].Value.ToString();
                textBox4.Text = dataGridView1.SelectedCells[2].Value.ToString();
                button8.Visible = true;
                groupBox1.Visible = true;
            }
            else
            {
                MessageBox.Show("Выделите все поля записи, которую хотите редактировать.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Редактировать
        private void button8_Click(object sender, EventArgs e) => UpdateUserDataAsync();

        // Редактирование данных пользователя
        private async void UpdateUserDataAsync()
        {
            try
            {
                if(textBox1.Text.Length > 0 && textBox2.Text.Length > 0)
                {
                    if (textBox1.Text == dataGridView1.SelectedCells[0].Value.ToString())
                    {
                        MakeUpdate();
                    }
                    else if ( await IsUniqLoginAsync(textBox1.Text))
                    {
                        MakeUpdate();
                    }
                    else
                    {
                        MessageBox.Show("Пользователь с введенным Вами логином уже существет. Укажите другой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Поле с Логином или Паролем не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private async void MakeUpdate()
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                await cnn.OpenAsync();

                using (SqlCommand update = new SqlCommand("UPDATE UserData SET HashPassword = @userPass, Address = @userAddress, Number = @userNumber, IsAdmin = @userIsAdmin WHERE Login like @login", cnn))
                {
                    update.Parameters.AddWithValue("@login", textBox1.Text);
                    update.Parameters.AddWithValue("@userPass", CreatePasswordHash(textBox2.Text));
                    update.Parameters.AddWithValue("@userAddress", textBox3.Text);
                    update.Parameters.AddWithValue("@userNumber", textBox4.Text);

                    int isAdmin = -1;
                    if (radioButton1.Checked) isAdmin = 1;
                    else isAdmin = 0;

                    update.Parameters.AddWithValue("@userIsAdmin", isAdmin);
                    await update.ExecuteNonQueryAsync();

                    userDataTableAdapter.Fill(usersDataSet.UserData);

                    MessageBox.Show("Редактирование пользователя завершено успешно!", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    groupBox1.Visible = false;
                    ClearTextBoxesGB1();
                    button8.Visible = false;
                }
            }
        }
    }
}