using Xunit;

namespace SolitaireSprint0.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void NewGame_CreatesBoard_WithSomePegs()
        {
            var g = new SolitaireSprint0.SolitaireGame();
            g.NewGame(SolitaireSprint0.BoardType.English, 7);

            Assert.True(g.PegCount() > 0);
            Assert.False(g.GameOver);
        }

        [Fact]
        public void InvalidMove_DoesNotChangePegCount()
        {
            var g = new SolitaireSprint0.SolitaireGame();
            g.NewGame(SolitaireSprint0.BoardType.English, 7);

            int before = g.PegCount();
            // totally invalid (random coords likely not valid jump)
            bool moved = g.TryApplyMove(new SolitaireSprint0.Move(0, 0, 0, 1));

            Assert.False(moved);
            Assert.Equal(before, g.PegCount());
        }
    }
}