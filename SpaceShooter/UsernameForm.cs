using SpaceShooter.Utils;

namespace SpaceShooter
{
    partial class UsernameForm : Form
    {
        private TextBox usernameTextBox;
        public string Username {  get; set; }

        public UsernameForm()
        {
            InitializeComponent();
            InitializeLayout();
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim(); 
            if (!string.IsNullOrEmpty(username))
            {
                Username = username;
                DialogResult = DialogResult.OK;  
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeLayout()
        {
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.ClientSize = new Size(400, 300);
            this.Padding = new Padding(40);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            FontFamily HiScoreFont = AssetLoader.LoadFont("big_noodle_titling.ttf");

            TableLayoutPanel layoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Top,
            };

            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            usernameTextBox = new TextBox
            {
                Font = new Font(HiScoreFont, 30),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
            };

            // Create a Submit Button
            Button submitButton = new Button
            {
                Text = "Go",
                Font = new Font(HiScoreFont, 30, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
            };
            submitButton.Click += SubmitButton_Click;

            layoutPanel.Controls.Add(usernameTextBox, 0, 0);
            layoutPanel.Controls.Add(submitButton, 1, 0);
            layoutPanel.Height = 60;

            this.Controls.Add(layoutPanel);

            Label usernameLabel = new Label
            {
                Text = "Enter Username",
                Font = new Font(HiScoreFont, 30),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100
            };
            this.Controls.Add(usernameLabel);
        }
    }
}
