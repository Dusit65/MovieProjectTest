using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MovieProjectTest
{
    public partial class FrmMovie : Form
    {
        public FrmMovie()
        {
            InitializeComponent();
        }
        //++++++++++++++++++++++++++Method And Variable++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //Create variable collect Movie and Director Image in data type ( byte[] ) for store in DB type image
        byte[] movieImg, movieDirImg;

        //Connection DB string
        private static string conStr = "Server=ComputerKung\\SQLEXPRESS;Database=movie_record_db;Trusted_connection=True";

        //showWarningMSG
        public static void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        //Method Get data from DB and show in DGV
        private void getMovieFromDBToDGV()
        {
            // ติดต่อ DB
            SqlConnection conn = new SqlConnection(conStr);
            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();

                // คำสั่ง SQL ที่ดึงข้อมูลจากฐานข้อมูล
                string strSql = "SELECT movieId, movieName, movieDetail, movieDateSale, movieTypeName FROM movie_tb " +
                                "INNER JOIN movie_type_tb ON movie_tb.movieTypeId = movie_type_tb.movieTypeId";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(strSql, conn);
                DataTable dataTable = new DataTable();

                // ดึงข้อมูลจากฐานข้อมูลและเติมลงใน DataTable
                dataAdapter.Fill(dataTable);

                // ล้างข้อมูลเก่าที่แสดงใน DataGridView
                dgvMovieShowAll.Rows.Clear();

                // กำหนด CultureInfo เป็นภาษาไทย
                var thaiCulture = new System.Globalization.CultureInfo("th-TH");

                // เติมข้อมูลลงใน DataGridView โดยใช้ข้อมูลจาก DataTable
                foreach (DataRow row in dataTable.Rows)
                {
                    // แปลงวันที่จาก DateTime เป็นวันที่ภาษาไทย
                    DateTime movieDateSale = Convert.ToDateTime(row["movieDateSale"]);
                    string dateOnly = movieDateSale.ToString("d MMMM yyyy", thaiCulture); // แสดงวันที่เป็นภาษาไทย

                    // เพิ่มแถวใหม่ใน DataGridView และใส่ข้อมูลจาก DataTable ลงในแต่ละคอลัมน์
                    dgvMovieShowAll.Rows.Add(row["movieId"], row["movieName"], row["movieDetail"], dateOnly, row["movieTypeName"]);
                    dgvMovieShowAll.ClearSelection();
                    dgvMovieShowAll.EnableHeadersVisualStyles = false;
                }
            }
            catch (Exception ex)
            {
                showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        
        //Method create new movieId
        private string generateNewMovieId()
        {
            string newMovieId = "mv001"; // ค่าตั้งต้น
            string lastMovieId = "";

            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();

                string strSql = "SELECT TOP 1 movieId FROM movie_tb ORDER BY movieId DESC";
                using (SqlCommand cmd = new SqlCommand(strSql, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        lastMovieId = result.ToString();

                        // ตัด "mv" ออกแล้วแปลงเป็นตัวเลข
                        int numberPart = int.Parse(lastMovieId.Substring(2));
                        numberPart++; // เพิ่มค่าทีละ 1
                        newMovieId = "mv" + numberPart.ToString("D3"); // ให้คงรูปแบบ mvNNN
                    }
                }
            }
            return newMovieId;
        }
        
        private bool IsNewMovie(string movieId)
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();
                string strSql = "SELECT COUNT(*) FROM movie_tb WHERE movieId = @movieId";
                using (SqlCommand sqlCommand = new SqlCommand(strSql, conn))
                {
                    sqlCommand.Parameters.AddWithValue("@movieId", movieId);
                    int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    return count == 0; // ถ้า 0 แสดงว่าเป็นหนังใหม่
                }
            }
        }
        //LoadDataIntoComboBox
        private void LoadDataIntoComboBox()
        {
            // การเชื่อมต่อฐานข้อมูล
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    string strSql = "SELECT movieTypeName FROM movie_type_tb"; // เลือกคอลัมน์ที่ต้องการดึงมาแสดงใน ComboBox

                    // สร้าง command
                    using (SqlCommand sqlCommand = new SqlCommand(strSql, conn))
                    {
                        // สั่งให้ command ทำงาน (Select)
                        SqlDataReader reader = sqlCommand.ExecuteReader();

                        // ล้างข้อมูลใน ComboBox ก่อน
                        cbbMovieType.Items.Clear();

                        // อ่านข้อมูลจากฐานข้อมูลและเพิ่มลงใน ComboBox
                        while (reader.Read())
                        {
                            // เพิ่มอีเมล์ลงใน ComboBox
                            cbbMovieType.Items.Add(reader["movieTypeName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
            }

        }
        //Method Clear/Cancel
        private void CancelClearFrm()
        {
            rdMovieId.Checked = true;
            btAdd.Enabled = true;
            btEdit.Enabled = false;
            btDel.Enabled = false;
            btSaveAddEdit.Enabled = false;
            groupBox2.Enabled = false;
            lbMovieId.Text = "";
            tbMovieSearch.Clear();
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            tbMovieDVDTotal.Clear();
            tbMovieDVDPrice.Clear();
            lsMovieShow.Items.Clear();
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0;
            dtpMovieDateSale.Value = DateTime.Now;
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;
        }
        //SearchByMovieID
        private void SearchByMovieID(string movieId)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT movieId, movieName FROM movie_tb WHERE movieId = @movieId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@movieId", movieId);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ListViewItem item = new ListViewItem("1"); // ลำดับที่ 1 สำหรับการค้นหาด้วยรหัส
                        item.SubItems.Add(reader["movieName"].ToString());
                        item.Tag = reader["movieId"].ToString(); // เก็บ movieId ไว้ใน Tag
                        lsMovieShow.Items.Add(item);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        //SearchByMovieName
        private void SearchByMovieName(string movieName)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT movieId, movieName FROM movie_tb WHERE movieName LIKE @movieName";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@movieName", "%" + movieName + "%"); // ใช้ LIKE เพื่อค้นหาบางส่วนของชื่อ

                    SqlDataReader reader = command.ExecuteReader();
                    int count = 1; // เริ่มนับลำดับที่ 1
                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(count.ToString()); // แสดงลำดับ
                        item.SubItems.Add(reader["movieName"].ToString());
                        item.Tag = reader["movieId"].ToString(); // เก็บ movieId ไว้ใน Tag
                        lsMovieShow.Items.Add(item);
                        count++; // เพิ่มลำดับ
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }



        //+++++++++++++++++++++++++++++++++End of Method And Variable +++++++++++++++++++++++++++++++++++++++++++++

        //FrmMovie_load======================================
        private void FrmMovie_Load(object sender, EventArgs e)
        {
            CancelClearFrm();
            getMovieFromDBToDGV();
            LoadDataIntoComboBox();
        }

        //btSelectImgMv_Click======================================
        private void btSelectImgMv_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //show selected image in PictureBox
                pcbMovieImg.Image = Image.FromFile(ofd.FileName);

                //Convert selected image to byte[] store in movieImg
                //Create variable store "type of file"
                string extFile = Path.GetExtension(ofd.FileName);
                //Convert to byte[]
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    movieImg = ms.ToArray();
                }
            }
        }

        //btSelectImgDir_Click=======================================
        private void btSelectImgDir_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //show selected image in PictureBox
                pcbDirMovie.Image = Image.FromFile(ofd.FileName);

                //Convert selected image to byte[] store in movieImg
                //Create variable store "type of file"
                string extFile = Path.GetExtension(ofd.FileName);
                //Convert to byte[]
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    movieDirImg = ms.ToArray();
                }
            }
        }
        
        //btAdd_CLick======================================
        private void btAdd_Click(object sender, EventArgs e)
        {
            // btAdd Disable and btSaveAddEdit Enable
            btAdd.Enabled = false;
            btSaveAddEdit.Enabled = true;

            groupBox2.Enabled = true;
            lbMovieId.Text = "";
            tbMovieSearch.Clear();
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            tbMovieDVDTotal.Clear();
            tbMovieDVDPrice.Clear();
            lbMovieId.Text = generateNewMovieId();
        }

        //btDel_Click======================================
        private void btDel_Click(object sender, EventArgs e)
        {
            if (lsMovieShow.SelectedItems.Count > 0)
            {
                // ดึง movieId จาก Tag ของ ListViewItem ที่เลือก
                string movieId = lsMovieShow.SelectedItems[0].Tag.ToString();

                DialogResult result = MessageBox.Show("คุณต้องการลบข้อมูลภาพยนตร์นี้ใช่หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {

                    using (SqlConnection connection = new SqlConnection(conStr))
                    {
                        try
                        {
                            connection.Open();
                            string query = "DELETE FROM movie_tb WHERE movieId = @movieId";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@movieId", movieId);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("ลบข้อมูลภาพยนตร์สำเร็จ");

                                FrmMovie_Load(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลที่ต้องการลบ");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("กรุณาเลือกภาพยนตร์ที่ต้องการลบ");
            }
        }
        //btEdit_Click=======================================
        private void btEdit_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;
            btSaveAddEdit.Enabled = true;
            btAdd.Enabled = false;
            btEdit.Enabled = false;
            btDel.Enabled = false;
        }
        //btSaveAddEdit_Click=======================================
        private void btSaveAddEdit_Click(object sender, EventArgs e)
        {
            ////Validate 
            if (tbMovieName.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนชื่อภาพยนต์ด้วย");
            }
            else if (tbMovieDetail.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนรายละเอียดภาพยนต์ด้วย");
            }
            else if (nudMovieHour.Value <= 0)
            {
                showWarningMSG("ชั่วโมงต้องมากกว่า 0");
            }
            else if (tbMovieDVDTotal.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนจำนวน DVD ด้วย");
            }
            else if (tbMovieDVDPrice.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนราคา DVD ด้วย");
            }
            else if (movieImg == null)
            {
                showWarningMSG("เลือกรูปตัวอย่างภาพยนต์ด้วย");
            }
            else if (movieDirImg == null)
            {
                showWarningMSG("เลือกรูปผู้กำกับภาพยนต์ด้วย");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    SqlTransaction sqlTransaction = conn.BeginTransaction();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = conn;
                    sqlCommand.Transaction = sqlTransaction;
                    try
                    {
                        string strSql;

                        // ตรวจสอบว่าเป็นข้อมูลใหม่หรือเก่า
                        if (IsNewMovie(lbMovieId.Text)) // ถ้าเป็นหนังใหม่ → INSERT
                        {
                            strSql = "INSERT INTO movie_tb (movieId, movieName, movieDetail, movieDateSale, movieLengthHour, movieLengthMinute, movieTypeId, movieDVDTotal, movieDVDPrice, movieImg, movieDirImg) " +
                                     "VALUES (@movieId, @movieName, @movieDetail, @movieDateSale, @movieLengthHour, @movieLengthMinute, @movieTypeId, @movieDVDTotal, @movieDVDPrice, @movieImg, @movieDirImg)";
                        }
                        else // ถ้ามีอยู่แล้ว → UPDATE
                        {
                            strSql = "UPDATE movie_tb SET movieName=@movieName, movieDetail=@movieDetail, movieDateSale=@movieDateSale, movieLengthHour=@movieLengthHour, " +
                                     "movieLengthMinute=@movieLengthMinute, movieTypeId=@movieTypeId, movieDVDTotal=@movieDVDTotal, movieDVDPrice=@movieDVDPrice, movieImg=@movieImg , movieDirImg=@movieDirImg " +
                                     "WHERE movieId=@movieId";
                        }

                        sqlCommand.CommandText = strSql;

                        // กำหนดค่า Parameter
                        sqlCommand.Parameters.AddWithValue("@movieId", lbMovieId.Text.Trim());
                        sqlCommand.Parameters.AddWithValue("@movieName", tbMovieName.Text.Trim());
                        sqlCommand.Parameters.AddWithValue("@movieDetail", tbMovieDetail.Text.Trim());
                        sqlCommand.Parameters.AddWithValue("@movieDateSale", dtpMovieDateSale.Value);
                        sqlCommand.Parameters.AddWithValue("@movieLengthHour", nudMovieHour.Value);
                        sqlCommand.Parameters.AddWithValue("@movieLengthMinute", nudMovieMinute.Value);
                        sqlCommand.Parameters.AddWithValue("@movieTypeId", cbbMovieType.SelectedIndex + 1); // ✅ บันทึก Index+1
                        sqlCommand.Parameters.AddWithValue("@movieDVDTotal", Convert.ToInt32(tbMovieDVDTotal.Text));
                        sqlCommand.Parameters.AddWithValue("@movieDVDPrice", Convert.ToDecimal(tbMovieDVDPrice.Text));
                        sqlCommand.Parameters.AddWithValue("@movieImg", movieImg);
                        sqlCommand.Parameters.AddWithValue("@movieDirImg", movieDirImg);

                        // สั่งให้ SQL ทำงาน
                        sqlCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();
                        showWarningMSG("บันทึกข้อมูลสำเร็จ!");
                        CancelClearFrm();
                        FrmMovie_Load(sender, e);
                    }
                    catch (Exception ex)
                    {
                        sqlTransaction.Rollback();
                        showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }
        
        //btCancel_Click=======================================
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ต้องการล้างข้อมูลหรือไม่", "ยืนยัน",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                CancelClearFrm();
            }
           
        }

        

        //btExit_Click======================================
        private void btExit_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ต้องการออกจากแอปหรือไม่", "ยืนยัน",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        //btMovieSearch_Click=======================================
        private void btMovieSearch_Click(object sender, EventArgs e)
        {
            string searchText = tbMovieSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("กรุณาป้อนคำค้นหา");
                return;
            }
            lsMovieShow.Items.Clear(); // ล้างรายการใน ListView ก่อนแสดงผลลัพธ์ใหม่
            if (rdMovieId.Checked)
            {
                SearchByMovieID(searchText);
                groupBox2.Enabled = false;
            }
            else if (rdMovieName.Checked)
            {
                SearchByMovieName(searchText);
                groupBox2.Enabled = false;

            }

            // ตรวจสอบว่ามีรายการใน lsMovieShow หรือไม่
            if (lsMovieShow.Items.Count == 0)
            {
                MessageBox.Show("ไม่พบข้อมูลที่ค้นหา");
            }



        }
        //rdMovieId_Click=======================================
        private void rdMovieId_Click(object sender, EventArgs e)
        {
            lsMovieShow.Items.Clear();
            tbMovieSearch.Clear();
        }
        //rdMovieName_Click======================================
        private void rdMovieName_Click(object sender, EventArgs e)
        {
            lsMovieShow.Items.Clear();
            tbMovieSearch.Clear();
        }
        //SelectedIndexChanged===================================================
        private void lsMovieShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsMovieShow.SelectedItems.Count > 0)
            {
                btAdd.Enabled = false;
                btEdit.Enabled = true;
                btDel.Enabled = true;
                btSaveAddEdit.Enabled = false;
                string movieId = lsMovieShow.SelectedItems[0].Tag.ToString(); // ดึง movieId จาก Tag

                // ดึงข้อมูลภาพยนตร์จากฐานข้อมูลและแสดงในฟอร์ม
                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    try
                    {
                        connection.Open();
                        string query = "SELECT * FROM movie_tb WHERE movieId = @movieId";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@movieId", movieId);
                        DataTable dt = new DataTable();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            lbMovieId.Text = movieId;
                            tbMovieName.Text = reader["movieName"].ToString();
                            tbMovieDetail.Text = reader["movieDetail"].ToString();
                            dtpMovieDateSale.Value = Convert.ToDateTime(reader["movieDateSale"]);
                            nudMovieHour.Value = Convert.ToInt32(reader["movieLengthHour"]);
                            nudMovieMinute.Value = Convert.ToInt32(reader["movieLengthMinute"]);
                            cbbMovieType.SelectedIndex = Convert.ToInt32(reader["movieTypeId"]) - 1; // แสดง Index-1
                            tbMovieDVDTotal.Text = reader["movieDVDTotal"].ToString();
                            tbMovieDVDPrice.Text = reader["movieDVDPrice"].ToString();
                            
                            // ดึงรูปภาพจากฐานข้อมูลและแสดงใน PictureBox
                                    byte[] imageBytes = reader["movieImg"] as byte[];
                            if (imageBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pcbMovieImg.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                    movieImg = imageBytes;
                                }
                            }

                            imageBytes = reader["movieDirImg"] as byte[];
                            if (imageBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pcbDirMovie.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                    movieDirImg = imageBytes;
                                }
                            }


                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }
        //+++++++++++++++++++++++++++++++KeyPress+++++++++++++++++++++++++++++++++++++++++++
        private void tbMovieDVDTotal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // ป้องกันการป้อนค่าที่ไม่ใช่ตัวเลข
            }
        }

        

        private void tbMovieDVDPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตให้กด Backspace
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // ตรวจสอบว่าป้อนตัวเลขได้หรือไม่
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // ตรวจสอบเงื่อนไขของจุดทศนิยม
            if (e.KeyChar == '.')
            {
                // ถ้า TextBox ว่างเปล่าอยู่ ห้ามให้ป้อนจุดก่อนตัวเลข
                if (tbMovieDVDPrice.Text.Length == 0)
                {
                    e.Handled = true;
                    return;
                }

                // ถ้ามีจุดอยู่แล้ว ห้ามให้ป้อนเพิ่ม
                if (tbMovieDVDPrice.Text.Contains("."))
                {
                    e.Handled = true;
                    return;
                }

                return;
            }

            // ถ้าไม่ใช่ตัวเลข หรือ จุดทศนิยม ให้ปฏิเสธการป้อน
            e.Handled = true;
        }
    
    }
}
