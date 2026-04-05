using Xunit;
using SolitaireSprint0;

namespace SolitaireSprint0.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ManualMove_DecreasesPegCount()
        {
            var g = new ManualSolitaireGame();
            g.NewGame(BoardType.English, 7);
            g.SetupDemoStartWith5Pegs();

            var moves = g.GetAllValidMoves();
            Assert.True(moves.Count > 0);

            int startCount = g.PegCount();
            bool success = g.TryMove(moves[0]);

            Assert.True(success);
            Assert.Equal(startCount - 1, g.PegCount());
        }

        [Fact]
        public void AutomatedMove_FindsItsOwnMove()
        {
            var g = new AutomatedSolitaireGame();
            g.NewGame(BoardType.Hexagon, 7);
            g.SetupDemoStartWith5Pegs();

            int startCount = g.PegCount();
            bool success = g.TryMove();

            Assert.True(success);
            Assert.Equal(startCount - 1, g.PegCount());
        }

        [Fact]
        public void Randomize_MaintainsValidStatus()
        {
            var g = new ManualSolitaireGame();
            g.NewGame(BoardType.English, 7);

            g.Randomize();

            Assert.True(g.Status == GameStatus.InProgress ||
                        g.Status == GameStatus.NoMovesLeft ||
                        g.Status == GameStatus.Won);
        }
    }
}