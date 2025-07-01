using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;

namespace bookstore
{
    public partial class BookManagementForm : Form
    {
        private readonly DataGridView grid;

        public BookManagementForm()
        {
            // Set form style
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.WhiteSmoke;
            this.Text = "Manage Books";
            this.Width = 800;
            this.Height = 600;

            // Set up the DataGridView
            grid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 350,
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

            var btnAdd = new Button { Text = "Add Book", Width = 120, Height = 40, Left = 20, Top = 20 };
            btnAdd.Click += (s, e) => AddBook();

            var btnEdit = new Button { Text = "Edit Book", Width = 120, Height = 40, Left = 160, Top = 20 };
            btnEdit.Click += (s, e) => EditBook();

            var btnDelete = new Button { Text = "Delete Book", Width = 120, Height = 40, Left = 300, Top = 20 };
            btnDelete.Click += (s, e) => DeleteBook();

            var btnView = new Button { Text = "Show BookAuthors View", Width = 170, Height = 40, Left = 440, Top = 20 };
            btnView.Click += (s, e) => ShowBookAuthors();

            var btnGroup = new Button { Text = "Books per Author", Width = 170, Height = 40, Left = 630, Top = 20 };
            btnGroup.Click += (s, e) => ShowBooksPerAuthor();

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnView);
            buttonPanel.Controls.Add(btnGroup);

            Controls.Add(buttonPanel);

            // Load book data
            LoadBooks();
        }

        /// Loads all books from the database and displays them in the DataGridView
        private void LoadBooks()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT BookID, Title, Price, Stock FROM Books ORDER BY BookID DESC", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                grid.DataSource = table;
            }
        }

        /// Adds a new book using a dialog for input, with a ComboBox for Author selection
        private void AddBook()
        {
            var title = Prompt.ShowDialog("Title:", "Add Book");
            var priceStr = Prompt.ShowDialog("Price:", "Add Book");
            var stockStr = Prompt.ShowDialog("Stock:", "Add Book");

            // Show a ComboBox dialog for Author selection
            int authorId = ShowAuthorSelectionDialog();
            if (!string.IsNullOrWhiteSpace(title)
                && decimal.TryParse(priceStr, out decimal price)
                && int.TryParse(stockStr, out int stock)
                && authorId > 0)
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO Books (Title, AuthorID, Price, Stock) VALUES (@Title, @AuthorID, @Price, @Stock)", conn);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@AuthorID", authorId);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Stock", stock);
                    cmd.ExecuteNonQuery();
                }
                LoadBooks();
            }
        }

        /// Helper to show a ComboBox dialog for selecting an author
        private int ShowAuthorSelectionDialog()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT AuthorID, Name FROM Authors", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                using (var form = new Form())
                {
                    form.Text = "Select Author";
                    var combo = new ComboBox { DataSource = table, DisplayMember = "Name", ValueMember = "AuthorID", Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
                    var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom };
                    form.Controls.Add(combo);
                    form.Controls.Add(ok);
                    form.AcceptButton = ok;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Width = 300;
                    form.Height = 100;
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        return (int)combo.SelectedValue;
                    }
                }
            }
            return -1;
        }

        /// Edits the selected book
        private void EditBook()
        {
            if (grid.SelectedRows.Count > 0)
            {
                var row = grid.SelectedRows[0];
                var id = row.Cells["BookID"].Value;
                var title = Prompt.ShowDialog("Title:", "Edit Book", row.Cells["Title"].Value.ToString());
                var priceStr = Prompt.ShowDialog("Price:", "Edit Book", row.Cells["Price"].Value.ToString());
                var stockStr = Prompt.ShowDialog("Stock:", "Edit Book", row.Cells["Stock"].Value.ToString());
                if (!string.IsNullOrWhiteSpace(title) && decimal.TryParse(priceStr, out decimal price) && int.TryParse(stockStr, out int stock))
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand("UPDATE Books SET Title = @Title, Price = @Price, Stock = @Stock WHERE BookID = @BookID", conn);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@Stock", stock);
                        cmd.Parameters.AddWithValue("@BookID", id);
                        cmd.ExecuteNonQuery();
                    }
                    LoadBooks();
                }
            }
        }

        /// Deletes the selected book
        private void DeleteBook()
        {
            if (grid.SelectedRows.Count > 0)
            {
                var row = grid.SelectedRows[0];
                var id = row.Cells["BookID"].Value;
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM Books WHERE BookID = @BookID", conn);
                    cmd.Parameters.AddWithValue("@BookID", id);
                    cmd.ExecuteNonQuery();
                }
                LoadBooks();
            }
        }

        /// Shows the BookAuthors view in a new DataGridView
        private void ShowBookAuthors()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM BookAuthors", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                var viewGrid = new DataGridView { DataSource = table, Dock = DockStyle.Bottom, Height = 150 };
                Controls.Add(viewGrid);
            }
        }

        /// Shows a group function: number of books per author
        private void ShowBooksPerAuthor()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT AuthorID, COUNT(*) AS NumberOfBooks FROM Books GROUP BY AuthorID", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                var groupGrid = new DataGridView { DataSource = table, Dock = DockStyle.Bottom, Height = 150 };
                Controls.Add(groupGrid);
            }
        }
    }
}