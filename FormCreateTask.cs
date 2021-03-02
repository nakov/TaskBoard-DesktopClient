using System.Windows.Forms;

namespace TaskBoard_DesktopClient
{
    public partial class FormCreateTask : Form
    {
        public string Title { get => this.textBoxTitle.Text; }
        public string Description { get => this.textBoxDescription.Text; }

        public FormCreateTask()
        {
            InitializeComponent();
        }

        private void FormConnect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
