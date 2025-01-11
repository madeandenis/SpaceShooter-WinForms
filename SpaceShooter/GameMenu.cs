using SpaceShooter.GameConfig;
using SpaceShooter.Utils;

namespace SpaceShooter
{
    public partial class GameMenu : Form
    {
        GameDifficulty GameDifficulty { get; set; }
        private string[] difficulties;
        private int difficultyIndex;
        private Label difficultyLabel;

        private TableLayoutPanel menuPanel;
        private int buttonHeight;

        public GameMenu(GameDifficulty gameDifficulty)
        {
            GameDifficulty = gameDifficulty;
            difficulties = Enum.GetNames(typeof(Difficulty));
            difficultyIndex = Array.IndexOf(difficulties, gameDifficulty.Difficulty.ToString());  

            InitializeComponent();
            InitializeLayout();
        }

        private void GameMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void GameMenu_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            ShowInTaskbar = false;
            TopMost = true;

            if (Owner != null)
            {
                Width = Owner.Width / 6;
                Height = Owner.Height / 2;
                StartPosition = FormStartPosition.Manual;
                Location = new Point(
                    Owner.Width / 2 - Width / 2,
                    Owner.Height / 2 - Height / 2
                );
            }
        }

        private void ResumeGame(object sender, EventArgs e)
        {
            Close();
        }

        private void DecreaseDifficulty(object sender, EventArgs e)
        {
            if (difficultyIndex > 0)
            {
                difficultyIndex--;
            }
            else if (difficultyIndex == 0)
            {
                difficultyIndex = difficulties.Length - 1;
            }
            string diffString = difficulties[difficultyIndex];
            GameDifficulty.Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), diffString);
            difficultyLabel.Text = diffString;
        }

        private void IncreaseDifficulty(object sender, EventArgs e)
        {
            if (difficultyIndex < difficulties.Length - 1)
            {
                difficultyIndex++;
            }
            else if (difficultyIndex == difficulties.Length - 1)
            {
                difficultyIndex = 0;
            }
            string diffString = difficulties[difficultyIndex];
            GameDifficulty.Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), diffString);
            difficultyLabel.Text = diffString;
        }

        private void ShowLeaderboard(object sender, EventArgs e)
        {
            MessageBox.Show("Leaderboard feature coming soon!", "Leaderboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void QuitGame(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeLayout()
        {
            buttonHeight = Height / 7;

            menuPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                BackColor = Color.Black,
            };

            Controls.Add(menuPanel);

            // Resume Button
            Button resumeButton = CreateButton("Resume", ResumeGame);
            menuPanel.Controls.Add(resumeButton);

            // Difficulty Controls
            TableLayoutPanel difficultyPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Fill,
            };

            difficultyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); 
            difficultyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); 
            difficultyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));

            Button leftArrow = CreateButton("<", DecreaseDifficulty);
            difficultyPanel.Controls.Add(leftArrow);

            difficultyLabel = new Label
            {
                Text = difficulties[difficultyIndex],
                Font = new Font(AssetLoader.LoadFont("big_noodle_titling.ttf"), buttonHeight / 2),
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = buttonHeight,
            };
            difficultyPanel.Controls.Add(difficultyLabel);

            Button rightArrow = CreateButton(">", IncreaseDifficulty);
            difficultyPanel.Controls.Add(rightArrow);

            menuPanel.Controls.Add(difficultyPanel);

            // Leaderboard Button
            Button leaderboardButton = CreateButton("Leaderboard", ShowLeaderboard);
            menuPanel.Controls.Add(leaderboardButton);

            // Quit Button
            Button quitButton = CreateButton("Quit", QuitGame);
            menuPanel.Controls.Add(quitButton);

            int xPadding = 20;
            int yPadding = (menuPanel.Height - menuPanel.Controls.Count * buttonHeight) / 2;
            menuPanel.Padding= new Padding(xPadding, yPadding, xPadding, yPadding); 
        }


        private Button CreateButton(string text, EventHandler onClick)
        {
            FontFamily HiScoreFont = AssetLoader.LoadFont("big_noodle_titling.ttf");

            var button = new Button
            {
                Text = text,
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(HiScoreFont, buttonHeight / 2),
                Height = buttonHeight,
                Dock = DockStyle.Fill,
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += onClick;

            return button;
        }
    }
}
