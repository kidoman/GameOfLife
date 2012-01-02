using System;
using System.Collections.Generic;
using System.Linq;
using GameOfLife;
using Xunit;
using Xunit.Extensions;

public class GenerationTests {
    public class NeighbourCount {
        [Theory]
        [InlineData("----X----", 0)]
        [InlineData("X---X----", 1)]
        [InlineData("---XXX---", 2)]
        [InlineData("-X--X--X-", 2)]
        [InlineData("XXX-X----", 3)]
        [InlineData("----X-XXX", 3)]
        [InlineData("X--XX-X--", 3)]
        [InlineData("--X-XX--X", 3)]
        [InlineData("XX-XX-X--", 4)]
        [InlineData("XX-XX-XX-", 5)]
        [InlineData("XX-XXXXX-", 6)]
        [InlineData("XXXXX-XXX", 7)]
        [InlineData("XXXXXXXXX", 8)]
        public void Should_Calculate_Appropriate_Neighbour_Count_For_Middle_Cell_In_A_3x3_Grid(string grid, int expectedCount) {
            var generation = new Generation(grid, 3, 3);

            var count = generation.GetLiveNeighboursCount(1, 1);

            Assert.Equal(expectedCount, count);
        }
    }

    public class Rules {
        [Fact]
        public void Grid_Should_Be_Empty_If_Starts_Empty() {
            var generation = new Generation();

            Assert.Equal(0, generation.TotalAlive);
        }

        [Fact]
        public void Grid_Should_Have_Cells_If_Started_With_Cells() {
            var generation = new Generation(new Coordinate(1, 4), new Coordinate(2, 3));

            Assert.True(generation.IsAlive(1, 4));
            Assert.True(generation.IsAlive(2, 3));
        }

        [Fact]
        public void Tick_Should_Kill_Of_A_Lone_Cell() {
            var generation = new Generation(new Coordinate(1, 1));

            var next = generation.Tick();

            Assert.Equal(0, next.TotalAlive);
        }

        [Fact]
        public void Tick_Should_Kill_Of_A_Cell_With_Less_Than_2_Neighbours() {
            var generation = new Generation(new Coordinate(1, 1), new Coordinate(2, 2));

            var next = generation.Tick();

            Assert.Equal(0, next.TotalAlive);
        }

        [Fact]
        public void Tick_Should_Kill_Of_A_Cell_With_More_Than_3_Neighbours() {
            var generation = new Generation(new Coordinate(1, 1), new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(0, 2), new Coordinate(2, 0));

            var next = generation.Tick();

            Assert.Equal(4, next.TotalAlive);
            Assert.False(next.IsAlive(1, 1));
        }

        [Fact]
        public void Tick_Should_Create_A_Cell_With_Exactly_3_Neighbours_If_Already_Dead() {
            var generation = new Generation(new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(2, 1));

            var next = generation.Tick();

            Assert.Equal(3, next.TotalAlive);
            new List<Coordinate> { new Coordinate(1, 0), new Coordinate(1, 1), new Coordinate(1, 2) }.ForEach(c => Assert.True(next.IsAlive(c.X, c.Y)));
        }

        // Rules:
        // Any live cell with fewer than two live neighbours dies, as if by loneliness.
        // Any live cell with more than three live neighbours dies, as if by overcrowding.
        // Any live cell with two or three live neighbours lives, unchanged, to the next generation.
        // Any dead cell with exactly three live neighbours comes to life.
    }

    public class Misc {
        [Fact]
        public void StringConstructor_Should_Throw_Error_If_Grid_String_Has_Insufficient_Cells() {
            Assert.Throws(typeof(ArgumentException), () => new Generation(string.Empty, 1, 1));
        }

        [Fact]
        public void StringConstructor_Should_Ignore_Whitespace_At_Start_And_End() {
            var generation = new Generation("  -X-  ", 1, 3);

            Assert.Equal(1, generation.TotalAlive);
            Assert.True(generation.IsAlive(1, 0));
        }

        [Fact]
        public void StringConstructor_Should_Handle_Multiple_Rows() {
            var generation = new Generation("X--X--X--", 3, 3);

            Assert.Equal(3, generation.TotalAlive);
            Assert.True(generation.IsAlive(0, 0) && generation.IsAlive(0, 1) && generation.IsAlive(0, 2));
        }

        [Fact]
        public void StringConstructor_Should_Not_Be_Case_Sensitive_By_Default() {
            var generation = new Generation("x--x--x--", 3, 3);

            Assert.Equal(3, generation.TotalAlive);
        }

        [Fact]
        public void StringConstructor_Should_Be_Case_Sensitive_If_Overridden() {
            var generation = new Generation("x--x--x--", 3, 3, caseSensitive: true);

            Assert.Equal(0, generation.TotalAlive);
        }

        [Fact]
        public void ToString_Should_Return_Empty_String_If_Grid_Is_Empty() {
            var generation = new Generation();

            Assert.Equal(string.Empty, generation.ToString());
        }

        [Fact]
        public void ToString_Should_Return_Proper_String_With_Negative_Coords() {
            var generation = new Generation(new Coordinate(-1, -1));

            Assert.Equal(Generation.LiveCellChar.ToString(), generation.ToString());
        }

        [Fact]
        public void ToString_Should_Return_Proper_String_With_Two_Vert_Coords() {
            var generation = new Generation(new Coordinate(0, 0), new Coordinate(0, 1));

            Assert.Equal(Generation.LiveCellChar + Environment.NewLine + Generation.LiveCellChar, generation.ToString());
        }

        [Fact]
        public void ToString_Should_Return_Proper_String_With_Two_Horz_Coords() {
            var generation = new Generation(new Coordinate(0, 0), new Coordinate(1, 0));

            Assert.Equal(Generation.LiveCellChar.ToString() + Generation.LiveCellChar, generation.ToString());
        }

        [Fact]
        public void ToString_Should_Honour_Rows_Override() {
            var generation = new Generation(new Coordinate(0, 0));

            Assert.Equal(Generation.LiveCellChar + Environment.NewLine + Generation.DeadCellChar, generation.ToString(2, 0));
        }

        [Fact]
        public void ToString_Should_Honour_Cols_Override() {
            var generation = new Generation(new Coordinate(0, 0));

            Assert.Equal(Generation.LiveCellChar.ToString() + Generation.DeadCellChar, generation.ToString(0, 2));
        }
    }

    public class DiehardSeed {
        [Fact]
        public void Should_Result_In_A_Empty_Grid_With_Diehard_Seed_At_130th_Tick() {
            var grid = @"------X-XX-------X---XXX";

            var generation = new Generation(grid, 3, 8);

            // Run 129 ticks.
            for (int i = 0; i < 129; i++)
                generation = generation.Tick();

            Assert.NotEqual(0, generation.TotalAlive);

            // Run the 130th tick.
            generation = generation.Tick();

            Assert.Equal(0, generation.TotalAlive);
        }
    }

    public class ThoughtWorksExamples {
        [Theory]
        [InlineData(@"
XX
XX", @"
XX
XX")]
        [InlineData(@"
XX-
X-X
-X-", @"
XX-
X-X
-X-")]
        [InlineData(@"
-X-
-X-
-X-", @"
---
XXX")]
        [InlineData(@"
-XXX
XXX-", @"
--X-
X--X
X--X
-X--")]
        public void Should_Give_Proper_Output_For_Specific_Input(string inputGrid, string outputGrid) {
            inputGrid = inputGrid.Trim();
            outputGrid = outputGrid.Trim();

            var generation = new Generation(inputGrid.Replace(Environment.NewLine, ""), inputGrid.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Count(), inputGrid.IndexOf(Environment.NewLine));

            var next = generation.Tick();

            Assert.Equal(outputGrid, next.ToString());
        }
    }
}

