using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using System.Data.SqlClient;
using LiveCharts;
using LiveCharts.Wpf;

namespace Dashboard_App_N
{
    public partial class Form1 : Form
    {
        private string conString = "Data Source=DESKTOP-I7C7D5M\\ALFIANSQLSERVER;Initial Catalog=BikeStores;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand cmd;
        private DataTable dt;
        private string sql;

        //Fields
        private IconButton currentBtn; 
        private Panel leftBorderBtn;
        private Form currentChildForm; 

        //contructor
        public Form1()
        {
            InitializeComponent();
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7,60);
            panelMenu.Controls.Add(leftBorderBtn);
            //form 
            this.Text = string.Empty;
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            ActivateButton(btnDashboard, RGBColors.color1);
        }

        //Structs
        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(155, 89, 182);
            public static Color color2 = Color.FromArgb(26, 188, 156);
            public static Color color3 = Color.FromArgb(52, 152, 219);
            public static Color color4 = Color.FromArgb(230, 126, 34);
            public static Color color5 = Color.FromArgb(255, 77, 165);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(conString);
            getOrderTable();
            getOrderDiagram();
            dataOrder();
            getCategoryTable();
            getCategoryDiagram();
            getStaffTable();
            getStaffDiagram();
        }

        public void getOrderTable()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "DATEPART(YEAR, order_date) AS Tahun, " +
                            "DATEPART(MONTH, order_date) AS Bulan, " +
                            "COUNT(*) AS Total " +
                        "FROM " +
                            "sales.orders " +
                        "GROUP BY " +
                            "DATEPART(MONTH, order_date), " +
                            "DATEPART(YEAR, order_date) " +
                        "ORDER BY " +
                            "DATEPART(YEAR, order_date), " +
                            "DATEPART(MONTH, order_date)";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                con.Close();
            }
            catch(SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void getOrderDiagram()
        {
            cartesianChart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Bulan",
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" }
            });

            cartesianChart.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Total",
                LabelFormatter = value => value.ToString()
            });

            cartesianChart.LegendLocation  = LiveCharts.LegendLocation.Right;

            cartesianChart.Series.Clear();
            SeriesCollection series = new SeriesCollection();

            try
            {
                con.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                List<string> yearsList = new List<string>();
                
                while (myReader.Read())
                {
                    yearsList.Add(myReader["Tahun"].ToString());
                }

                var years = yearsList.Distinct();

                foreach(var year in years)
                {
                    List<double> orderList = new List<double>();
                    for(int month = 1; month <= 12; month++)
                    {
                        double order = 0;
                        DataRow[] result = dt.Select("Tahun = " + year + " AND Bulan = " + month);
                        foreach(DataRow row in result)
                        {
                            order = Double.Parse(row[2].ToString()); 
                        }
                        orderList.Add(order);
                    }
                    series.Add(new LineSeries()
                    {
                        Title = year.ToString(),
                        Values = new ChartValues<double>(orderList)
                    });
                    cartesianChart.Series = series;
                    con.Close();
                }
            }
            catch(SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void dataOrder()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "SUM(quantity * list_price * (1 - discount))Pendapatan, " +
                            "SUM(quantity)Terjual " +
                        "FROM " +
                            "BikeStores.sales.order_items";
                cmd = new SqlCommand(sql, con);
                using(SqlDataReader myReader = cmd.ExecuteReader())
                {
                    while (myReader.Read())
                    {
                        labelPendapatan.Text = "$ " + myReader["Pendapatan"].ToString();
                        labelTerjual.Text = myReader["Terjual"].ToString() + " Unit";
                    }
                }
                con.Close();
            }
            catch(SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void getCategoryTable()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "category_name, COUNT(*) product_count " +
                        "FROM " +
                            "BikeStores.production.products p " +
                            "INNER JOIN BikeStores.production.categories c " +
                            "ON c.category_id = p.category_id " +
                        "GROUP BY " +
                            "category_name";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void getCategoryDiagram()
        {
            Func<ChartPoint, string> labelpoint = chartpoint => string.Format("{0} ({1:P})", chartpoint.Y, chartpoint.Participation);
            SeriesCollection series = new SeriesCollection();
            DataRow[] result = dt.Select();
            foreach (DataRow row in result)
            {
                series.Add(new PieSeries()
                {
                    Title = row[0].ToString(),
                    Values = new ChartValues<int> { Int32.Parse(row[1].ToString()) },
                    DataLabels = true,
                    LabelPoint = labelpoint
                });
            }
            pieChartCategory.Series = series;
            pieChartCategory.LegendLocation = LegendLocation.Right;
        }

        public void getStaffTable()
        {
            try
            {
                con.Open();
                sql = "SELECT " +
                            "s.first_name + ' ' + s.last_name staff_name, COUNT(*) order_count " +
                        "FROM " +
                            "sales.staffs s " +
                            "INNER JOIN sales.orders o " +
                            "ON s.staff_id = o.staff_id " +
                        "GROUP BY " +
                            "s.first_name + ' ' + s.last_name";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void getStaffDiagram()
        {
            cartesianChartStaff.AxisX.Add(new Axis
            {
                Title = "Staff",
                Labels = new[] { "Penjualan Staff" }
            });

            cartesianChartStaff.AxisY.Add(new Axis
            {
                Title = "Penjualan",
                LabelFormatter = value => value.ToString()
            });

            cartesianChartStaff.LegendLocation = LegendLocation.Left;

            cartesianChartStaff.Series.Clear();
            SeriesCollection series = new SeriesCollection();

            try
            {
                con.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                List<string> yearsList = new List<string>();

                while (myReader.Read())
                {
                    yearsList.Add(myReader["staff_name"].ToString());
                }

                var staffName = yearsList.Distinct();

                foreach (var year in staffName)
                {
                    List<double> orderList = new List<double>();
                        
                    double order = 0;
                    DataRow[] result = dt.Select("staff_name = '" + year +"'");
                    foreach (DataRow row in result)
                    {
                        order = Double.Parse(row[1].ToString());
                    }
                    orderList.Add(order);
                    
                    series.Add(new ColumnSeries()
                    {
                        Title = year.ToString(),
                        Values = new ChartValues<double>(orderList)
                    });
                    cartesianChartStaff.Series = series;
                    con.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Methods
        private void ActivateButton(object senderBtn, Color color )
        {
            if(senderBtn!=null)
            {
                DisableButton();
                //button
                currentBtn = (IconButton)senderBtn;
                currentBtn.BackColor = Color.FromArgb(37, 36, 81);
                currentBtn.ForeColor = color;
                currentBtn.TextAlign = ContentAlignment.MiddleCenter;
                currentBtn.IconColor = color;
                currentBtn.TextImageRelation = TextImageRelation.TextBeforeImage;
                currentBtn.ImageAlign = ContentAlignment.MiddleRight;

                //left border button
                leftBorderBtn.BackColor = color;
                leftBorderBtn.Location = new Point(0,currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();

                //Icon Current Child  Form
                iconCurrentChildForm.IconChar = currentBtn.IconChar;
                iconCurrentChildForm.IconColor = color; 

            }
        }

        private void DisableButton()
        {
            if(currentBtn !=null)
            {
               currentBtn.BackColor = Color.FromArgb(6, 22, 58);
                currentBtn.ForeColor = Color.Gainsboro;
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;
                currentBtn.IconColor = Color.Gainsboro;
                currentBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
                currentBtn.ImageAlign = ContentAlignment.MiddleLeft;
            }
        }

        private void OpenChildForm(Form childForm)
        {
            if (iconCurrentChildForm != null)
            {
                currentChildForm = childForm;
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                panelDesktop.Controls.Add(childForm);
                panelDesktop.Tag = childForm;
                childForm.BringToFront();
                childForm.Show();
                lblTitleChildForm.Text = childForm.Text;
            }
        }
        private void iconButton1_Click(object sender, EventArgs e)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
                Reset();
                ActivateButton(sender, RGBColors.color1);
            }
        }

        private void iconButton5_Click(object sender, EventArgs e)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            ActivateButton(sender, RGBColors.color5);
            OpenChildForm(new FormMarketing());
        }

        private void iconButton4_Click(object sender, EventArgs e)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            ActivateButton(sender, RGBColors.color4);
            OpenChildForm(new FormCustomer());
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            ActivateButton(sender, RGBColors.color3);
            OpenChildForm(new FormProduct());
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            ActivateButton(sender, RGBColors.color2);
            OpenChildForm(new FormOrders());
        }

        private void Reset()
        {
            DisableButton();
            leftBorderBtn.Visible = false;
            iconCurrentChildForm.IconChar = currentBtn.IconChar;
            iconCurrentChildForm.IconColor = Color.MediumPurple;
            lblTitleChildForm.Text = "Dashboard"; 
        }

        private void iconPictureBox1_Click(object sender, EventArgs e)
        {

        }
        //drag form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void maximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        private void minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}
