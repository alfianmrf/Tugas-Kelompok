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
    public partial class FormOrders : Form
    {
        private string conString = "Data Source=DESKTOP-I7C7D5M\\ALFIANSQLSERVER;Initial Catalog=BikeStores;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand cmd;
        private DataTable dt;
        private string sql;

        public FormOrders()
        {
            InitializeComponent();
        }

        private void FormOrders_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(conString);
            getListOrderTable();
        }

        public void getListOrderTable()
        {
            try
            {
                con.Open();
                sql = "SELECT " +
                            "CONCAT(first_name, ' ', last_name) AS customer_name, product_name, quantity, i.list_price, discount, quantity* i.list_price * (1 - discount) AS total " +
                        "FROM " +
                            "sales.customers c " +
                            "INNER JOIN sales.orders o " +
                            "ON c.customer_id = o.customer_id " +
                            "INNER JOIN sales.order_items i " +
                            "ON o.order_id = i.order_id " +
                            "INNER JOIN production.products p " +
                            "ON i.product_id = p.product_id";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dataGridViewOrder.DataSource = null;
                dataGridViewOrder.DataSource = dt;
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
