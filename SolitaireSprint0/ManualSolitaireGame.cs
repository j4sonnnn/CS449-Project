namespace SolitaireSprint0
{
    public class ManualSolitaireGame : SolitaireGame
    {
        public override bool TryMove(Move? m)
        {
            if (m == null || Status != GameStatus.InProgress) return false;

            var validMoves = GetAllValidMoves();
            if (!validMoves.Contains(m.Value)) return false;

            Move move = m.Value;
            _cells[move.FromRow, move.FromCol] = false;
            _cells[move.MidRow, move.MidCol] = false;
            _cells[move.ToRow, move.ToCol] = true;

            RecomputeStatus();
            return true;
        }
    }
}