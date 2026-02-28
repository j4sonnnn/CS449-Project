using System;
using System.Collections.Generic;

namespace SolitaireSprint0
{
    public sealed class SolitaireGame
    {
        // Cells: null = not part of board, true = peg present, false = empty hole
        private bool?[,] _cells = new bool?[0, 0];

        public BoardType Type { get; private set; }
        public int Size { get; private set; }              // Meaning depends on board type
        public int Rows => _cells.GetLength(0);
        public int Cols => _cells.GetLength(1);

        public bool GameOver { get; private set; }
        public bool Won { get; private set; }

        public bool? GetCell(int r, int c) => _cells[r, c];

        public void NewGame(BoardType type, int size)
        {
            if (size < 3) throw new ArgumentOutOfRangeException(nameof(size), "Size must be >= 3.");

            Type = type;
            Size = size;

            _cells = type switch
            {
                BoardType.English => BuildEnglish(size),
                BoardType.Diamond => BuildDiamond(size),
                BoardType.Hexagon => BuildHexagon(size),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            // Default empty: center
            var (cr, cc) = FindCenterHole();
            _cells[cr, cc] = false;

            RecomputeGameOver();
        }

        public bool TryApplyMove(Move m)
        {
            if (GameOver) return false;
            if (!IsInside(m.FromRow, m.FromCol) || !IsInside(m.ToRow, m.ToCol)) return false;

            // must be peg -> empty
            if (_cells[m.FromRow, m.FromCol] != true) return false;
            if (_cells[m.ToRow, m.ToCol] != false) return false;

            // Only orthogonal jumps by 2
            int dr = m.ToRow - m.FromRow;
            int dc = m.ToCol - m.FromCol;
            bool isOrthJump = (Math.Abs(dr) == 2 && dc == 0) || (Math.Abs(dc) == 2 && dr == 0);
            if (!isOrthJump) return false;

            // middle must be peg
            if (!IsInside(m.MidRow, m.MidCol)) return false;
            if (_cells[m.MidRow, m.MidCol] != true) return false;

            // Apply
            _cells[m.FromRow, m.FromCol] = false;
            _cells[m.MidRow, m.MidCol] = false;
            _cells[m.ToRow, m.ToCol] = true;

            RecomputeGameOver();
            return true;
        }

        public IReadOnlyList<Move> GetAllValidMoves()
        {
            var moves = new List<Move>();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    if (_cells[r, c] != true) continue;

                    TryAddMove(moves, r, c, r - 2, c);
                    TryAddMove(moves, r, c, r + 2, c);
                    TryAddMove(moves, r, c, r, c - 2);
                    TryAddMove(moves, r, c, r, c + 2);
                }
            return moves;

            void TryAddMove(List<Move> list, int fr, int fc, int tr, int tc)
            {
                if (!IsInside(tr, tc)) return;
                var mv = new Move(fr, fc, tr, tc);
                if (_cells[fr, fc] == true && _cells[tr, tc] == false && _cells[mv.MidRow, mv.MidCol] == true)
                    list.Add(mv);
            }
        }

        public int PegCount()
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c] == true) count++;
            return count;
        }

        private void RecomputeGameOver()
        {
            int pegs = PegCount();
            Won = (pegs == 1);
            bool hasMoves = GetAllValidMoves().Count > 0;
            GameOver = Won || !hasMoves;
        }

        private bool IsInside(int r, int c)
        {
            if (r < 0 || c < 0 || r >= Rows || c >= Cols) return false;
            return _cells[r, c].HasValue; // part of board
        }

        private (int r, int c) FindCenterHole()
        {
            // pick the nearest valid cell to geometric center
            int cr = Rows / 2;
            int cc = Cols / 2;
            if (_cells[cr, cc].HasValue) return (cr, cc);

            // search outward
            for (int dist = 1; dist < Math.Max(Rows, Cols); dist++)
                for (int r = cr - dist; r <= cr + dist; r++)
                    for (int c = cc - dist; c <= cc + dist; c++)
                        if (r >= 0 && c >= 0 && r < Rows && c < Cols && _cells[r, c].HasValue)
                            return (r, c);

            return (0, 0);
        }

        // ---------- Board builders ----------

        // English cross: size is arm thickness (commonly 7 board => size=7)
        // We'll interpret size as full grid dimension (odd recommended). If even, still works.
        private static bool?[,] BuildEnglish(int size)
        {
            int n = size;
            var cells = new bool?[n, n];

            int third = n / 3;               // roughly
            int start = third;
            int end = n - third - 1;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    bool inCross = (r >= start && r <= end) || (c >= start && c <= end);
                    // corners removed: if both r and c are in corner bands
                    bool inCorner = (r < start || r > end) && (c < start || c > end);
                    if (inCross && !inCorner) cells[r, c] = true;
                    else cells[r, c] = null;
                }

            return cells;
        }

        // Diamond: size = width/height (odd recommended)
        private static bool?[,] BuildDiamond(int size)
        {
            int n = size;
            var cells = new bool?[n, n];
            int center = n / 2;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int dist = Math.Abs(r - center) + Math.Abs(c - center);
                    if (dist <= center) cells[r, c] = true;
                    else cells[r, c] = null;
                }

            return cells;
        }

        // Hexagon-ish on a square grid: size = radius (>=3 recommended)
        // We'll map into a (2*size-1) square with a hex mask.
        private static bool?[,] BuildHexagon(int size)
        {
            int radius = size;
            int n = 2 * radius - 1;
            var cells = new bool?[n, n];
            int center = n / 2;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    // cube coords distance on axial projected grid approximation:
                    int dr = r - center;
                    int dc = c - center;
                    int dist = Math.Max(Math.Abs(dr), Math.Abs(dc)); // simple mask
                    if (dist <= center) cells[r, c] = true;
                    else cells[r, c] = null;
                }

            return cells;
        }
    }
}