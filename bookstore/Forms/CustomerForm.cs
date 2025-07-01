using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;

namespace bookstore
{
    public partial class CustomerForm : Form
    {
        private readonly DataGridView grid;

        public CustomerForm()
        {
            // Set form style
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.WhiteSmoke;
            this.Text = "Manage Customers";
            this.Width = 700;
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

            var btnAdd = new Button { Text = "Add Customer", Width = 140, Height = 40, Left = 20, Top = 20 };
            btnAdd.Click += (s, e) => AddCustomer();

            var btnEdit = new Button { Text = "Edit Customer", Width = 140, Height = 40, Left = 180, Top = 20 };
            btnEdit.Click += (s, e) => EditCustomer();

            var btnDelete = new Button { Text = "Delete Customer", Width = 140, Height = 40, Left = 340, Top = 20 };
            btnDelete.Click += (s, e) => DeleteCustomer();

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);

            Controls.Add(buttonPanel);

            // Load customer data
            LoadCustomers();
        }

        /// Loads all customers from the database and displays them in a DataGridView.
        private void LoadCustomers()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT CustomerID, Name, Email FROM Customers", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                grid.DataSource = table;
            }
        }

        /// Add a new customer using a dialog for input.
        private void AddCustomer()
        {
            var name = Prompt.ShowDialog("Name:", "Add Customer");
            var email = Prompt.ShowDialog("Email:", "Add Customer");
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email))
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)", conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.ExecuteNonQuery();
                }
                LoadCustomers();
            }
        }

        /// Edits the selected customer.
        private void EditCustomer()
        {
            if (grid.SelectedRows.Count > 0)
            {
                var row = grid.SelectedRows[0];
                var id = row.Cells["CustomerID"].Value;
                var name = Prompt.ShowDialog("Name:", "Edit Customer", row.Cells["Name"].Value.ToString());
                var email = Prompt.ShowDialog("Email:", "Edit Customer", row.Cells["Email"].Value.ToString());
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email))
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand("UPDATE Customers SET Name=@Name, Email=@Email WHERE CustomerID=@ID", conn);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.ExecuteNonQuery();
                    }
                    LoadCustomers();
                }
            }
        }

        /// Deletes the selected customer.
        private void DeleteCustomer()
        {
            if (grid.SelectedRows.Count > 0)
            {
                var row = grid.SelectedRows[0];
                var id = row.Cells["CustomerID"].Value;
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM Customers WHERE CustomerID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
                LoadCustomers();
            }
        }
    }
}