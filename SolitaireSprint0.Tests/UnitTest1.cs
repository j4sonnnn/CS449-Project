using Xunit;
using SolitaireSprint0;

namespace SolitaireSprint0.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Add_2Plus3_Equals5()
        {
            Assert.Equal(5, MathHelper.Add(2, 3));
        }

        [Fact]
        public void Add_0Plus0_Equals0()
        {
            Assert.Equal(0, MathHelper.Add(0, 0));
        }
    }
}
