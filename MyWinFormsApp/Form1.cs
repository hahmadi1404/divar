namespace MyWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Hello, Windows Forms!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
