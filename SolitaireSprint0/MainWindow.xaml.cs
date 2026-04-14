using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SolitaireSprint0
{
    public partial class MainWindow : Window
    {
        private SolitaireGame _game = null!;
        private (int r, int c)? _selected;

        public MainWindow()
        {
            InitializeComponent();
            cmbSize.ItemsSource = new[] { 7, 9 };
            cmbSize.SelectedIndex = 0;
            cmbType.ItemsSource = new[] { BoardType.English, BoardType.Hexagon, BoardType.Diamond };
            cmbType.SelectedIndex = 0;
            StartNewGame();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e) => StartNewGame();

        private void StartNewGame()
        {
            _selected = null;
            int uiSize = (int)cmbSize.SelectedItem!;
            var type = (BoardType)cmbType.SelectedItem!;

            if (rbManual.IsChecked == true) _game = new ManualSolitaireGame();
            else _game = new AutomatedSolitaireGame();

            int gameSize = (type == BoardType.Hexagon) ? (uiSize + 1) / 2 : uiSize;
            _game.NewGame(type, gameSize);
            _game.SetupDemoStartWith5Pegs();

            RenderBoard();
            UpdateStatus("Game started.");
        }

        // NEW SPRINT 4: REPLAY LOGIC
        private async void Replay_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                UpdateStatus("Replaying recorded game...");
                foreach (string state in lines)
                {
                    _game.LoadState(state);
                    RenderBoard();
                    await Task.Delay(500); // Visual step delay
                }
                MessageBox.Show("Replay Finished.");
            }
        }

        // NEW SPRINT 4: SAVE LOGIC
        private void SaveRecording()
        {
            if (chkRecord.IsChecked == true && _game.GameHistory.Count > 0)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text files (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllLines(saveFileDialog.FileName, _game.GameHistory);
                    MessageBox.Show("Game recorded successfully.");
                }
            }
        }

        private async void Autoplay_Click(object sender, RoutedEventArgs e)
        {
            if (_game is not AutomatedSolitaireGame) { MessageBox.Show("Select 'Automated' mode."); return; }
            while (_game.Status == GameStatus.InProgress)
            {
                _game.TryMove();
                if (chkRecord.IsChecked == true) _game.RecordCurrentState();
                RenderBoard();
                UpdateStatus("AI thinking...");
                await Task.Delay(400);
            }
            UpdateStatus("Auto Over.");
            SaveRecording();
            ShowGameOverNotification();
        }

        private void Randomize_Click(object sender, RoutedEventArgs e)
        {
            _game.Randomize();
            if (chkRecord.IsChecked == true) _game.RecordCurrentState();
            _selected = null;
            RenderBoard();
            UpdateStatus("Randomized!");
            if (_game.Status != GameStatus.InProgress) SaveRecording();
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (_game is AutomatedSolitaireGame) return;
            if (sender is not Button btn) return;
            var (r, c) = ((int r, int c))btn.Tag;

            if (_selected is null)
            {
                if (_game.GetCell(r, c) != true) return;
                _selected = (r, c);
                ClearHighlights();
                btn.Background = Brushes.LightBlue;
                HighlightValidDestinations(r, c);
                return;
            }

            var (sr, sc) = _selected.Value;
            _selected = null;
            bool moved = _game.TryMove(new Move(sr, sc, r, c));

            if (moved && chkRecord.IsChecked == true) _game.RecordCurrentState();

            ClearHighlights();
            RenderBoard();
            UpdateStatus(moved ? "Moved." : "Invalid.");

            if (moved && _game.Status != GameStatus.InProgress)
            {
                SaveRecording();
                ShowGameOverNotification();
            }
        }

        private void RenderBoard() { boardGrid.Children.Clear(); boardGrid.Rows = _game.Rows; boardGrid.Columns = _game.Cols; for (int r = 0; r < _game.Rows; r++) for (int c = 0; c < _game.Cols; c++) { bool? cell = _game.GetCell(r, c); if (!cell.HasValue) { boardGrid.Children.Add(new Border { Background = Brushes.Transparent }); continue; } var btn = new Button { Tag = (r, c), Margin = new Thickness(2), Background = cell.Value ? Brushes.DodgerBlue : Brushes.LightGray, Content = cell.Value ? "●" : "" }; btn.Click += Cell_Click; boardGrid.Children.Add(btn); } }
        private Button? GetButtonAt(int r, int c) { foreach (var child in boardGrid.Children) { if (child is Button b && b.Tag is ValueTuple<int, int> t) { if (t.Item1 == r && t.Item2 == c) return b; } } return null; }
        private void ClearHighlights() { foreach (var child in boardGrid.Children) { if (child is Button b && b.Tag is ValueTuple<int, int> t) { var (r, c) = t; bool? cell = _game.GetCell(r, c); if (cell.HasValue) b.Background = cell.Value ? Brushes.DodgerBlue : Brushes.LightGray; } } }
        private void HighlightValidDestinations(int sr, int sc) { int[,] dirs = { { -2, 0 }, { 2, 0 }, { 0, -2 }, { 0, 2 } }; for (int i = 0; i < dirs.GetLength(0); i++) { int tr = sr + dirs[i, 0]; int tc = sc + dirs[i, 1]; if (tr < 0 || tc < 0 || tr >= _game.Rows || tc >= _game.Cols) continue; var mv = new Move(sr, sc, tr, tc); bool? dest = _game.GetCell(tr, tc); bool? mid = _game.GetCell(mv.MidRow, mv.MidCol); if (dest == false && mid == true) { var destBtn = GetButtonAt(tr, tc); if (destBtn != null) destBtn.Background = Brushes.LightGreen; } } }
        private void UpdateStatus(string msg) { txtStatus.Text = $"Pegs: {_game.PegCount()}\n{msg}"; }
        private void ShowGameOverNotification() { MessageBox.Show("Game Over"); }
    }
}