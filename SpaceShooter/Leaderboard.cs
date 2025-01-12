using SpaceShooter.DataAccess.Remote;
using SpaceShooter.Utils;

namespace SpaceShooter
{
    internal partial class Leaderboard : Form
    {
        private readonly RemoteScoreService _remoteScoreService;
        private readonly string _leaderboardKey;

        public Leaderboard()
        {
            _remoteScoreService = new RemoteScoreService();
            _leaderboardKey = "SpaceShooter";

            InitializeComponent();
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            Text = "Leaderboard";
            BackColor = Color.Black;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Width = 400;
            Height = 500;

            var leaderboardGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White, 
                GridColor = Color.FromArgb(60, 60, 60), 
                Font = new Font(AssetLoader.LoadFont("big_noodle_titling.ttf"), 20),
                ReadOnly = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,  
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect, 
                MultiSelect = false,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,

                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(50, 50, 50), 
                    ForeColor = Color.White,  
                    Font = new Font(AssetLoader.LoadFont("big_noodle_titling.ttf"), 22), 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                },

                RowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 40, 40), 
                    ForeColor = Color.White, 
                },

                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                },
            };
            leaderboardGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            leaderboardGrid.RowTemplate.Height = 40;
            leaderboardGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            leaderboardGrid.Columns.Add("Rank", "Rank");
            leaderboardGrid.Columns.Add("Username", "Username");
            leaderboardGrid.Columns.Add("Score", "Score");

            Controls.Add(leaderboardGrid);

            LoadLeaderboardData(leaderboardGrid);
        }

        private void LoadLeaderboardData(DataGridView leaderboardGrid)
        {
            try
            {
                var topScores = _remoteScoreService.GetTopScores(_leaderboardKey, 10);

                leaderboardGrid.Rows.Clear();

                for (int i = 0; i < topScores.Count; i++)
                {
                    var (playerName, score) = topScores[i];
                    leaderboardGrid.Rows.Add(i + 1, playerName, score);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading leaderboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
