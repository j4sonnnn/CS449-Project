using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;

namespace SolitaireSprint0
{
    public partial class MainWindow : Window
    {
        // Fixed: Added the null-forgiving operator to remove the CS8618 warning
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

            if (rbManual.IsChecked == true)
                _game = new ManualSolitaireGame();
            else
                _game = new AutomatedSolitaireGame();

            int gameSize = (type == BoardType.Hexagon) ? (uiSize + 1) / 2 : uiSize;

            _game.NewGame(type, gameSize);
            _game.SetupDemoStartWith5Pegs();

            RenderBoard();
            UpdateStatus("Game started. Pick a blue peg to move.");
        }

        private async void Autoplay_Click(object sender, RoutedEventArgs e)
        {
            if (_game is not AutomatedSolitaireGame)
            {
                MessageBox.Show("Please select 'Automated' mode and start a New Game first.");
                return;
            }

            while (_game.Status == GameStatus.InProgress)
            {
                _game.TryMove();
                RenderBoard();
                UpdateStatus("AI is thinking...");
                await Task.Delay(400);
            }

            UpdateStatus("Automated Game Over.");
            ShowGameOverNotification();
        }

        private void Randomize_Click(object sender, RoutedEventArgs e)
        {
            _game.Randomize();
            _selected = null;
            RenderBoard();
            UpdateStatus("Board Randomized!");

            if (_game.Status != GameStatus.InProgress)
            {
                ShowGameOverNotification();
            }
        }

        private void RenderBoard()
        {
            boardGrid.Children.Clear();
            boardGrid.Rows = _game.Rows;
            boardGrid.Columns = _game.Cols;

            for (int r = 0; r < _game.Rows; r++)
                for (int c = 0; c < _game.Cols; c++)
                {
                    bool? cell = _game.GetCell(r, c);

                    if (!cell.HasValue)
                    {
                        boardGrid.Children.Add(new Border { Background = Brushes.Transparent });
                        continue;
                    }

                    var btn = new Button
                    {
                        Tag = (r, c),
                        Margin = new Thickness(2),
                        Padding = new Thickness(0),
                        FontSize = 14,
                        Background = cell.Value ? Brushes.DodgerBlue : Brushes.LightGray,
                        Content = cell.Value ? "●" : ""
                    };

                    btn.Click += Cell_Click;
                    boardGrid.Children.Add(btn);
                }
        }

        private Button? GetButtonAt(int r, int c)
        {
            foreach (var child in boardGrid.Children)
            {
                if (child is Button b && b.Tag is ValueTuple<int, int> t)
                {
                    if (t.Item1 == r && t.Item2 == c) return b;
                }
            }
            return null;
        }

        private void ClearHighlights()
        {
            foreach (var child in boardGrid.Children)
            {
                if (child is Button b && b.Tag is ValueTuple<int, int> t)
                {
                    var (r, c) = t;
                    bool? cell = _game.GetCell(r, c);
                    if (cell.HasValue)
                        b.Background = cell.Value ? Brushes.DodgerBlue : Brushes.LightGray;
                }
            }
        }

        private void HighlightValidDestinations(int sr, int sc)
        {
            int[,] dirs = { { -2, 0 }, { 2, 0 }, { 0, -2 }, { 0, 2 } };

            for (int i = 0; i < dirs.GetLength(0); i++)
            {
                int tr = sr + dirs[i, 0];
                int tc = sc + dirs[i, 1];

                if (tr < 0 || tc < 0 || tr >= _game.Rows || tc >= _game.Cols) continue;

                var mv = new Move(sr, sc, tr, tc);
                bool? dest = _game.GetCell(tr, tc);
                bool? mid = _game.GetCell(mv.MidRow, mv.MidCol);

                if (dest == false && mid == true)
                {
                    var destBtn = GetButtonAt(tr, tc);
                    if (destBtn != null) destBtn.Background = Brushes.LightGreen;
                }
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (_game is AutomatedSolitaireGame) return;

            if (sender is not Button btn) return;
            var (r, c) = ((int r, int c))btn.Tag;

            bool? cell = _game.GetCell(r, c);
            if (!cell.HasValue) return;

            if (_selected is null)
            {
                if (cell.Value != true)
                {
                    UpdateStatus("Pick a peg (blue circle) to move.");
                    return;
                }

                _selected = (r, c);
                ClearHighlights();
                btn.Background = Brushes.LightBlue;
                HighlightValidDestinations(r, c);

                UpdateStatus("Selected a peg. Click a GREEN hole to jump.");
                return;
            }

            var (sr, sc) = _selected.Value;
            _selected = null;

            bool moved = _game.TryMove(new Move(sr, sc, r, c));

            ClearHighlights();
            RenderBoard();

            UpdateStatus(moved ? "Move completed!" : "Invalid move. Select a peg, then click a GREEN hole.");

            if (moved && _game.Status != GameStatus.InProgress)
            {
                ShowGameOverNotification();
            }
        }

        private void UpdateStatus(string msg)
        {
            txtStatus.Text = $"Pegs: {_game.PegCount()} | Status: {_game.Status}\n{msg}";
        }

        private void ShowGameOverNotification()
        {
            if (_game.Status == GameStatus.Won)
            {
                MessageBox.Show(
                    "Congratulations! You won the game with only 1 peg left!",
                    "Game Over: Winner!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (_game.Status == GameStatus.NoMovesLeft)
            {
                MessageBox.Show(
                    $"Game Over! No more valid moves available.\n\nYou left {_game.PegCount()} pegs on the board.",
                    "Game Over: No Moves Left",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}