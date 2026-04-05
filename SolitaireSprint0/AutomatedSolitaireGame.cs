namespace SolitaireSprint0
{
    public class AutomatedSolitaireGame : SolitaireGame
    {
        public override bool TryMove(Move? m = null)
        {
            if (Status != GameStatus.InProgress) return false;

            var moves = GetAllValidMoves();
            if (moves.Count == 0) return false;

            Move autoMove = moves[0];
            _cells[autoMove.FromRow, autoMove.FromCol] = false;
            _cells[autoMove.MidRow, autoMove.MidCol] = false;
            _cells[autoMove.ToRow, autoMove.ToCol] = true;

            RecomputeStatus();
            return true;
        }
    }
}