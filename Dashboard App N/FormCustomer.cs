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
    public partial class FormCustomer : Form
    {
        private string conString = "Data Source=DESKTOP-I7C7D5M\\ALFIANSQLSERVER;Initial Catalog=BikeStores;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand cmd;
        private DataTable dt;
        private string sql;

        public FormCustomer()
        {
            InitializeComponent();
        }

        private void FormCustomer_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(conString);
            getListCustomerTable();
        }

        public void getListCustomerTable()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "CONCAT(first_name, ' ', last_name) AS customer_name, phone, email, CONCAT(street, ', ', city, ', ', state, ' - ', zip_code) AS address " +
                        "FROM " +
                            "sales.customers";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dataGridViewCustomer.DataSource = null;
                dataGridViewCustomer.DataSource = dt;
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
