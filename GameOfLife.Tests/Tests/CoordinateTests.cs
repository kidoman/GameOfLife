using GameOfLife;
using Xunit;
using Xunit.Extensions;

public class CoordinateTests {
    [Fact]
    public void Default_Constructor_Should_Set_Coordinate_To_0x0() {
        var coord = new Coordinate();

        Assert.Equal(0, coord.X);
        Assert.Equal(0, coord.Y);
    }

    [Fact]
    public void Constructor_Should_Set_The_Values_Properly() {
        var coord = new Coordinate(1, 1);

        Assert.Equal(1, coord.X);
        Assert.Equal(1, coord.Y);
    }

    [Fact]
    public void HashCode_Matches_For_Equal_Coords() {
        var coord1 = new Coordinate(1, 4);
        var coord2 = new Coordinate(1, 4);

        Assert.Equal(coord1.GetHashCode(), coord2.GetHashCode());
    }

    [Fact]
    public void Equals_Returns_True_For_Matching_Coords() {
        var coord1 = new Coordinate(1, 4);
        var coord2 = new Coordinate(1, 4);

        Assert.True(coord1.Equals(coord2));
    }

    [Theory]
    [InlineData(-1, -1, 1, 1, -1)]
    [InlineData(-10, -10, -5, -5, -1)]
    [InlineData(-10, -5, -5, -10, -1)]
    [InlineData(-5, -10, -10, -5, 1)]
    [InlineData(5, 5, 10, 10, -1)]
    [InlineData(5, 10, 10, 5, -1)]
    [InlineData(10, 5, 5, 10, 1)]
    public void CompareTo_Returns_Correct_Comparison_For_All_Data(int x1, int y1, int x2, int y2, int result) {
        var coord1 = new Coordinate(x1, y1);
        var coord2 = new Coordinate(x2, y2);

        Assert.Equal(result, coord1.CompareTo(coord2));
    }
}