using Xunit;
using SolitaireSprint0;

namespace SolitaireSprint0.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void NewGame_CreatesBoard_WithPegs_AndInProgress()
        {
            var g = new SolitaireGame();
            g.NewGame(BoardType.English, 7);
            Assert.True(g.PegCount() > 0);
            Assert.Equal(GameStatus.InProgress, g.Status);
        }

        [Fact]
        public void DemoSetup_StartsWithExactly5Pegs()
        {
            var g = new SolitaireGame();
            g.NewGame(BoardType.English, 7);
            g.SetupDemoStartWith5Pegs();
            Assert.Equal(5, g.PegCount());
        }

        [Fact]
        public void InvalidMove_DoesNotChangePegCount()
        {
            var g = new SolitaireGame();
            g.NewGame(BoardType.English, 7);
            g.SetupDemoStartWith5Pegs();

            int before = g.PegCount();
            bool moved = g.TryApplyMove(new Move(0, 0, 0, 1)); // invalid
            Assert.False(moved);
            Assert.Equal(before, g.PegCount());
        }

        [Fact]
        public void ValidMove_DecreasesPegCountByOne()
        {
            var g = new SolitaireGame();
            g.NewGame(BoardType.English, 7);
            g.SetupDemoStartWith5Pegs();

            // Find one valid move automatically (always exists by design)
            var moves = g.GetAllValidMoves();
            Assert.True(moves.Count > 0);

            int before = g.PegCount();
            bool moved = g.TryApplyMove(moves[0]);
            Assert.True(moved);
            Assert.Equal(before - 1, g.PegCount());
        }

        [Fact]
        public void Status_IsValid_AfterMovesOrSetup()
        {
            var g = new SolitaireGame();
            g.NewGame(BoardType.Diamond, 7);
            g.SetupDemoStartWith5Pegs();

            Assert.True(g.Status == GameStatus.InProgress ||
                        g.Status == GameStatus.Won ||
                        g.Status == GameStatus.NoMovesLeft);
        }
    }
}