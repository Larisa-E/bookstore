using System.Windows.Forms;

namespace bookstore
{
    /// Simple prompt dialog for user input.
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form() { Width = 400, Height = 150, Text = caption };
            Label lbl = new Label() { Left = 20, Top = 20, Text = text, Width = 100 };
            TextBox txt = new TextBox() { Left = 120, Top = 20, Width = 200, Text = defaultValue };
            Button ok = new Button() { Text = "OK", Left = 250, Width = 70, Top = 60, DialogResult = DialogResult.OK };
            prompt.Controls.Add(lbl);
            prompt.Controls.Add(txt);
            prompt.Controls.Add(ok);
            prompt.AcceptButton = ok;
            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
    }
}