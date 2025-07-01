using System;
using System.Windows.Forms;

namespace bookstore
{
    public partial class MainForm : Form
    {
        public MainForm()
        {

            var btnBooks = new Button { Text = "Manage Books", Left = 20, Top = 20, Width = 120 };
            btnBooks.Click += (s, e) => new BookManagementForm().ShowDialog();

            var btnCustomers = new Button { Text = "Manage Customers", Left = 20, Top = 60, Width = 120 };
            btnCustomers.Click += (s, e) => new CustomerForm().ShowDialog();

            var btnOrders = new Button { Text = "Manage Orders", Left = 20, Top = 100, Width = 120 };
            btnOrders.Click += (s, e) => new OrderForm().ShowDialog();

            Controls.Add(btnBooks);
            Controls.Add(btnCustomers);
            Controls.Add(btnOrders);

            Text = "Bookstore Management";
            Width = 700;
            Height = 500;
        }
    }
}