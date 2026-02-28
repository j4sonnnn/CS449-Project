using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SolitaireSprint0
{
    public partial class MainWindow : Window
    {
        private readonly SolitaireGame _game = new();
        private (int r, int c)? _selected;

        public MainWindow()
        {
            InitializeComponent();

            cmbType.ItemsSource = new[] { BoardType.English, BoardType.Hexagon, BoardType.Diamond };
            cmbType.SelectedIndex = 0;

            // simple size options (you can change later)
            cmbSize.ItemsSource = new[] { 7, 9, 11 };
            cmbSize.SelectedIndex = 0;

            StartNewGame();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            _selected = null;

            var type = (BoardType)cmbType.SelectedItem!;
            int size = (int)cmbSize.SelectedItem!;

            _game.NewGame(type, size);
            RenderBoard();
            UpdateStatus();
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
                        // Not part of board
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

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            var (r, c) = ((int r, int c))btn.Tag;

            bool? cell = _game.GetCell(r, c);
            if (!cell.HasValue) return;

            if (_selected is null)
            {
                // select a peg only
                if (cell.Value == true)
                {
                    _selected = (r, c);
                    UpdateStatus($"Selected: ({r},{c})");
                }
                return;
            }

            // second click = try move to destination
            var (sr, sc) = _selected.Value;
            if (sr == r && sc == c)
            {
                _selected = null;
                UpdateStatus();
                return;
            }

            bool moved = _game.TryApplyMove(new Move(sr, sc, r, c));
            _selected = null;
            RenderBoard();
            UpdateStatus(moved ? "Move applied." : "Invalid move.");
        }

        private void UpdateStatus(string? extra = null)
        {
            string baseText = $"Pegs: {_game.PegCount()}";
            if (_game.GameOver)
                baseText += _game.Won ? " | YOU WIN" : " | GAME OVER";

            if (!string.IsNullOrWhiteSpace(extra))
                baseText += $" | {extra}";

            txtStatus.Text = baseText;
        }
    }
}