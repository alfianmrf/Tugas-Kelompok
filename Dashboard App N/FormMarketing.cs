using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using LiveCharts;
using LiveCharts.Wpf;

namespace Dashboard_App_N
{
    public partial class FormMarketing : Form
    {
        private string conString = "Data Source=DESKTOP-I7C7D5M\\ALFIANSQLSERVER;Initial Catalog=BikeStores;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand cmd;
        private DataTable dt;
        private string sql;

        public FormMarketing()
        {
            InitializeComponent();
        }

        private void FormMarketing_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(conString);
            getListStaffTable();
        }

        public void getListStaffTable()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "s.first_name + ' ' + s.last_name staff_name, s.email, s.phone, store_name, m.first_name + ' ' + m.last_name manager_name " +
                        "FROM " +
                            "sales.staffs s " +
                            "INNER JOIN sales.stores t " +
                            "ON s.store_id = t.store_id " +
                            "LEFT JOIN sales.staffs m " +
                            "ON m.staff_id = s.manager_id";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dataGridViewStaff.DataSource = null;
                dataGridViewStaff.DataSource = dt;
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
