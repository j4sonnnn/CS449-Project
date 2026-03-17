using System;
using System.Collections.Generic;

namespace SolitaireSprint0
{
    // Game logic ONLY
    public sealed class SolitaireGame
    {
        // null = not part of board, true = peg, false = empty
        private bool?[,] _cells = new bool?[0, 0];

        public BoardType Type { get; private set; }
        public int Size { get; private set; }
        public int Rows => _cells.GetLength(0);
        public int Cols => _cells.GetLength(1);
        public GameStatus Status { get; private set; } = GameStatus.InProgress;

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
                BoardType.Hexagon => BuildHexagonRadius(size),  
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

        
            var (cr, cc) = FindCenterPlayable();
            _cells[cr, cc] = false;

            RecomputeStatus();
        }

     
        public void SetupDemoStartWith5Pegs()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c].HasValue)
                        _cells[r, c] = false;

            var (cr, cc) = FindCenterPlayable();
            _cells[cr, cc] = false; 

          
            TryPlacePeg(cr - 2, cc);
            TryPlacePeg(cr - 1, cc);

            TryPlacePeg(cr, cc - 2);
            TryPlacePeg(cr, cc - 1);

        
            if (!TryPlacePeg(cr + 2, cc))
            {
                
                PlaceFirstAvailablePeg();
            }

            
            EnsureExactlyNPegs(10);

            RecomputeStatus();
        }

        private bool TryPlacePeg(int r, int c)
        {
            if (r < 0 || c < 0 || r >= Rows || c >= Cols) return false;
            if (_cells[r, c].HasValue == false) return false;
            _cells[r, c] = true;
            return true;
        }

        private void PlaceFirstAvailablePeg()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c].HasValue)
                    {
                        _cells[r, c] = true;
                        return;
                    }
        }

        private void EnsureExactlyNPegs(int target)
        {
            if (target < 1) target = 1;

            // Count current pegs
            int current = PegCount();

            // If too few, add pegs anywhere playable
            while (current < target)
            {
                bool added = false;
                for (int r = 0; r < Rows && !added; r++)
                    for (int c = 0; c < Cols && !added; c++)
                    {
                        if (_cells[r, c].HasValue && _cells[r, c] == false)
                        {
                            _cells[r, c] = true;
                            current++;
                            added = true;
                        }
                    }
                if (!added) break; // no space
            }

            // If too many, remove pegs from bottom-right
            while (current > target)
            {
                bool removed = false;
                for (int r = Rows - 1; r >= 0 && !removed; r--)
                    for (int c = Cols - 1; c >= 0 && !removed; c--)
                    {
                        if (_cells[r, c] == true)
                        {
                            _cells[r, c] = false;
                            current--;
                            removed = true;
                        }
                    }
                if (!removed) break;
            }
        }

        // ----- existing game logic -----

        public int PegCount()
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_cells[r, c] == true) count++;
            return count;
        }

        public bool TryApplyMove(Move m)
        {
            if (Status != GameStatus.InProgress) return false;
            if (!IsPlayable(m.FromRow, m.FromCol) || !IsPlayable(m.ToRow, m.ToCol)) return false;

            if (_cells[m.FromRow, m.FromCol] != true) return false;
            if (_cells[m.ToRow, m.ToCol] != false) return false;

            int dr = m.ToRow - m.FromRow;
            int dc = m.ToCol - m.FromCol;

            bool orthJump = (Math.Abs(dr) == 2 && dc == 0) || (Math.Abs(dc) == 2 && dr == 0);
            if (!orthJump) return false;

            if (!IsPlayable(m.MidRow, m.MidCol)) return false;
            if (_cells[m.MidRow, m.MidCol] != true) return false;

            // apply
            _cells[m.FromRow, m.FromCol] = false;
            _cells[m.MidRow, m.MidCol] = false;
            _cells[m.ToRow, m.ToCol] = true;

            RecomputeStatus();
            return true;
        }

        public IReadOnlyList<Move> GetAllValidMoves()
        {
            var moves = new List<Move>();

            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    if (_cells[r, c] != true) continue;

                    TryAdd(r, c, r - 2, c);
                    TryAdd(r, c, r + 2, c);
                    TryAdd(r, c, r, c - 2);
                    TryAdd(r, c, r, c + 2);
                }

            return moves;

            void TryAdd(int fr, int fc, int tr, int tc)
            {
                if (!IsPlayable(tr, tc)) return;
                var mv = new Move(fr, fc, tr, tc);
                if (_cells[tr, tc] == false && IsPlayable(mv.MidRow, mv.MidCol) && _cells[mv.MidRow, mv.MidCol] == true)
                    moves.Add(mv);
            }
        }

        private void RecomputeStatus()
        {
            if (PegCount() == 1)
            {
                Status = GameStatus.Won;
                return;
            }

            Status = GetAllValidMoves().Count > 0 ? GameStatus.InProgress : GameStatus.NoMovesLeft;
        }

        private bool IsPlayable(int r, int c)
        {
            if (r < 0 || c < 0 || r >= Rows || c >= Cols) return false;
            return _cells[r, c].HasValue;
        }

        private (int r, int c) FindCenterPlayable()
        {
            int cr = Rows / 2;
            int cc = Cols / 2;
            if (_cells[cr, cc].HasValue) return (cr, cc);

            for (int dist = 1; dist < Math.Max(Rows, Cols); dist++)
                for (int r = cr - dist; r <= cr + dist; r++)
                    for (int c = cc - dist; c <= cc + dist; c++)
                        if (r >= 0 && c >= 0 && r < Rows && c < Cols && _cells[r, c].HasValue)
                            return (r, c);

            return (0, 0);
        }

        // English cross
        private static bool?[,] BuildEnglish(int n)
        {
            var cells = new bool?[n, n];

            int band = n / 3;
            int start = band;
            int end = n - band - 1;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    bool inCross = (r >= start && r <= end) || (c >= start && c <= end);
                    bool inCorner = (r < start || r > end) && (c < start || c > end);
                    cells[r, c] = (inCross && !inCorner) ? true : null;
                }

            return cells;
        }

        // Diamond mask
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

        // Hexagon mask on square grid (radius-based)
        private static bool?[,] BuildHexagonRadius(int radius)
        {
            int n = 2 * radius - 1;
            var cells = new bool?[n, n];
            int center = n / 2;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int dr = r - center;
                    int dc = c - center;

                    int x = dc;
                    int z = dr;
                    int y = -x - z;
                    int dist = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));

                    cells[r, c] = dist <= center ? true : null;
                }

            return cells;
        }
    }
}