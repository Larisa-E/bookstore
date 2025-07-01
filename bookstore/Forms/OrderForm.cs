using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;

namespace bookstore
{
    public partial class OrderForm : Form
    {
        private readonly DataGridView grid;

        public OrderForm()
        {
            // Set form style
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.WhiteSmoke;
            this.Text = "Manage Orders";
            this.Width = 800;
            this.Height = 500;

            // Set up the DataGridView
            grid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.AliceBlue },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.LightSteelBlue },
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(grid);

            // Use a panel to group buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.Gainsboro
            };

            var btnAdd = new Button { Text = "Add Order", Width = 140, Height = 40, Left = 20, Top = 20 };
            btnAdd.Click += (s, e) => AddOrder();

            var btnDelete = new Button { Text = "Delete Order", Width = 140, Height = 40, Left = 180, Top = 20 };
            btnDelete.Click += (s, e) => DeleteOrder();

            var btnTotalSales = new Button { Text = "Total Sales", Width = 140, Height = 40, Left = 340, Top = 20 };
            btnTotalSales.Click += (s, e) => ShowTotalSales();

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnTotalSales);

            Controls.Add(buttonPanel);

            // Load order data
            LoadOrders();
        }

        /// Loads all orders from the database and displays them in a DataGridView.
        private void LoadOrders()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT OrderID, CustomerID, OrderDate, TotalAmount FROM Orders", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                grid.DataSource = table;
            }
        }

        /// Adds a new order for a selected customer.
        private void AddOrder()
        {
            var customerIdStr = Prompt.ShowDialog("Customer ID:", "Add Order");
            var bookIdStr = Prompt.ShowDialog("Book ID:", "Add Book to Order");
            var quantityStr = Prompt.ShowDialog("Quantity:", "Add Book to Order");

            if (int.TryParse(customerIdStr, out int customerId) &&
                int.TryParse(bookIdStr, out int bookId) &&
                int.TryParse(quantityStr, out int quantity))
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insert order
                            var cmd = new MySqlCommand("INSERT INTO Orders (CustomerID) VALUES (@CustomerID)", conn, tran);
                            cmd.Parameters.AddWithValue("@CustomerID", customerId);
                            cmd.ExecuteNonQuery();

                            // Get new OrderID
                            cmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn, tran);
                            int orderId = Convert.ToInt32(cmd.ExecuteScalar());

                            // Insert order detail
                            cmd = new MySqlCommand("INSERT INTO OrderDetails (OrderID, BookID, Quantity) VALUES (@OrderID, @BookID, @Quantity)", conn, tran);
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.Parameters.AddWithValue("@BookID", bookId);
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.ExecuteNonQuery();

                            // Update total amount
                            cmd = new MySqlCommand("UPDATE Orders SET TotalAmount = (SELECT SUM(od.Quantity * b.Price) FROM OrderDetails od JOIN Books b ON od.BookID = b.BookID WHERE od.OrderID = @OrderID) WHERE OrderID = @OrderID", conn, tran);
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Error adding order: " + ex.Message);
                        }
                    }
                }
                LoadOrders();
            }
        }

        /// Deletes the selected order.
        private void DeleteOrder()
        {
            if (grid.SelectedRows.Count > 0)
            {
                var row = grid.SelectedRows[0];
                var id = row.Cells["OrderID"].Value;
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM Orders WHERE OrderID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
                LoadOrders();
            }
        }

        /// Calls the GetTotalSales stored procedure and shows the result.
        private void ShowTotalSales()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("CALL GetTotalSales()", conn);
                var result = cmd.ExecuteScalar();
                MessageBox.Show($"Total Sales: {result}");
            }
        }
    }
}