using System;
using System.Collections.Generic;

namespace SolitaireSprint0
{
    public abstract class SolitaireGame
    {
        protected bool?[,] _cells = new bool?[0, 0];
        public BoardType Type { get; protected set; }
        public int Size { get; protected set; }
        public int Rows => _cells.GetLength(0);
        public int Cols => _cells.GetLength(1);
        public GameStatus Status { get; protected set; } = GameStatus.InProgress;

        public bool? GetCell(int r, int c) => _cells[r, c];

        public virtual void NewGame(BoardType type, int size)
        {
            if (size < 3) throw new ArgumentOutOfRangeException(nameof(size), "Size must be >= 3.");
            Type = type;
            Size = size;

            _cells = type switch
            {
                BoardType.English => BuildEnglish(size),
                BoardType.Diamond => BuildDiamond(size),
                BoardType.Hexagon => BuildHexagonRadius(size),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            var (cr, cc) = FindCenterPlayable();
            _cells[cr, cc] = false;
            RecomputeStatus();
        }

        public void Randomize()
        {
            Random rng = new Random();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c].HasValue)
                        _cells[r, c] = rng.Next(2) == 0;
            RecomputeStatus();
        }

        public abstract bool TryMove(Move? m = null);

        protected void RecomputeStatus()
        {
            if (PegCount() == 1)
            {
                Status = GameStatus.Won;
                return;
            }
            Status = GetAllValidMoves().Count > 0 ? GameStatus.InProgress : GameStatus.NoMovesLeft;
        }

        public int PegCount()
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c] == true) count++;
            return count;
        }

        public List<Move> GetAllValidMoves()
        {
            var moves = new List<Move>();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    if (_cells[r, c] != true) continue;
                    TryAdd(moves, r, c, r - 2, c);
                    TryAdd(moves, r, c, r + 2, c);
                    TryAdd(moves, r, c, r, c - 2);
                    TryAdd(moves, r, c, r, c + 2);
                }
            return moves;
        }

        private void TryAdd(List<Move> list, int fr, int fc, int tr, int tc)
        {
            if (!IsPlayable(tr, tc)) return;
            var mv = new Move(fr, fc, tr, tc);
            if (_cells[tr, tc] == false && IsPlayable(mv.MidRow, mv.MidCol) && _cells[mv.MidRow, mv.MidCol] == true)
                list.Add(mv);
        }

        protected bool IsPlayable(int r, int c)
        {
            if (r < 0 || c < 0 || r >= Rows || c >= Cols) return false;
            return _cells[r, c].HasValue;
        }

        public void SetupDemoStartWith5Pegs()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c].HasValue) _cells[r, c] = false;

            var (cr, cc) = FindCenterPlayable();
            _cells[cr, cc] = false;

            TryPlacePeg(cr - 2, cc); TryPlacePeg(cr - 1, cc);
            TryPlacePeg(cr, cc - 2); TryPlacePeg(cr, cc - 1);

            if (!TryPlacePeg(cr + 2, cc)) PlaceFirstAvailablePeg();
            EnsureExactlyNPegs(10);
            RecomputeStatus();
        }

        private bool TryPlacePeg(int r, int c)
        {
            if (!IsPlayable(r, c) || _cells[r, c] == true) return false;
            _cells[r, c] = true; return true;
        }

        private void PlaceFirstAvailablePeg()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c] == false) { _cells[r, c] = true; return; }
        }

        private void EnsureExactlyNPegs(int target)
        {
            int current = PegCount();
            while (current < target)
            {
                bool added = false;
                for (int r = 0; r < Rows && !added; r++)
                    for (int c = 0; c < Cols && !added; c++)
                        if (_cells[r, c] == false) { _cells[r, c] = true; current++; added = true; }
                if (!added) break;
            }
            while (current > target)
            {
                bool removed = false;
                for (int r = Rows - 1; r >= 0 && !removed; r--)
                    for (int c = Cols - 1; c >= 0 && !removed; c--)
                        if (_cells[r, c] == true) { _cells[r, c] = false; current--; removed = true; }
                if (!removed) break;
            }
        }

        private (int r, int c) FindCenterPlayable()
        {
            int cr = Rows / 2, cc = Cols / 2;
            if (_cells[cr, cc].HasValue) return (cr, cc);
            for (int dist = 1; dist < Math.Max(Rows, Cols); dist++)
                for (int r = cr - dist; r <= cr + dist; r++)
                    for (int c = cc - dist; c <= cc + dist; c++)
                        if (r >= 0 && c >= 0 && r < Rows && c < Cols && _cells[r, c].HasValue) return (r, c);
            return (0, 0);
        }

        private static bool?[,] BuildEnglish(int n)
        {
            var cells = new bool?[n, n];
            int band = n / 3, start = band, end = n - band - 1;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    bool inCross = (r >= start && r <= end) || (c >= start && c <= end);
                    bool inCorner = (r < start || r > end) && (c < start || c > end);
                    cells[r, c] = (inCross && !inCorner) ? true : null;
                }
            return cells;
        }

        private static bool?[,] BuildDiamond(int n)
        {
            var cells = new bool?[n, n];
            int center = n / 2;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int dist = Math.Abs(r - center) + Math.Abs(c - center);
                    cells[r, c] = dist <= center ? true : null;
                }
            return cells;
        }

        private static bool?[,] BuildHexagonRadius(int radius)
        {
            int n = 2 * radius - 1;
            var cells = new bool?[n, n];
            int center = n / 2;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int x = c - center, z = r - center, y = -x - z;
                    int dist = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
                    cells[r, c] = dist <= center ? true : null;
                }
            return cells;
        }
    }
}