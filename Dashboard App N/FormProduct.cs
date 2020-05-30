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
    public partial class FormProduct : Form
    {
        private string conString = "Data Source=DESKTOP-I7C7D5M\\ALFIANSQLSERVER;Initial Catalog=BikeStores;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand cmd;
        private DataTable dt;
        private string sql;

        public FormProduct()
        {
            InitializeComponent();
        }

        private void FormProduct_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(conString);
            getProductTable();
        }

        public void getProductTable()
        {
            try
            {
                con.Open();
                sql =   "SELECT " +
                            "product_name, brand_name, category_name, model_year, list_price " +
                        "FROM " +
                            "production.products p " +
                            "INNER JOIN production.brands b " +
                            "ON p.brand_id = b.brand_id " +
                            "INNER JOIN production.categories c " +
                            "ON p.category_id = c.category_id " +
                        "ORDER BY " +
                            "product_name ASC";

                cmd = new SqlCommand(sql, con);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dataGridViewProduct.DataSource = null;
                dataGridViewProduct.DataSource = dt;
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
