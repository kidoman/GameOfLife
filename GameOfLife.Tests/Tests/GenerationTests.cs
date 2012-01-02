using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

            var count = generation.GetNeighbourCount(1, 1);

            Assert.Equal(expectedCount, count);
        }
    }

    public class Rules {
        [Fact]
        public void Should_Be_Empty_If_Starts_Empty() {
            var generation = new Generation();

            Assert.Equal(0, generation.Cells.Count());
        }

        [Fact]
        public void Should_Have_Cells_If_Started_With_Cells() {
            var generation = new Generation(new Cell(1, 4), new Cell(2, 3));

            Assert.Equal(new Cell(1, 4), generation.Cells[0]);
            Assert.Equal(new Cell(2, 3), generation.Cells[1]);
        }

        [Fact]
        public void Should_Kill_Of_A_Lone_Cell() {
            var generation = new Generation(new Cell(1, 1));

            var next = generation.Tick();

            Assert.Equal(0, next.Cells.Count());
        }

        [Fact]
        public void Should_Kill_Of_A_Cell_With_Less_Than_2_Neighbours() {
            var generation = new Generation(new Cell(1, 1), new Cell(2, 2));

            var next = generation.Tick();

            Assert.Equal(0, next.Cells.Count());
        }

        [Fact]
        public void Should_Kill_Of_A_Cell_With_More_Than_3_Neighbours() {
            var generation = new Generation(new Cell(1, 1), new Cell(0, 0), new Cell(2, 2), new Cell(0, 2), new Cell(2, 0));

            var next = generation.Tick();

            Assert.Equal(4, next.Cells.Count());
            Assert.False(next.Cells.Contains(new Cell(1, 1)));
        }

        [Fact]
        public void Should_Create_A_Cell_With_Exactly_3_Neighbours_If_Already_Dead() {
            var generation = new Generation(new Cell(0, 1), new Cell(1, 1), new Cell(2, 1));

            var next = generation.Tick();

            Assert.Equal(3, next.Cells.Count());
            Assert.True(next.Cells.SequenceEqual(new[] { new Cell(1, 0), new Cell(1, 1), new Cell(1, 2) }));
        }

        // Rules:
        // Any live cell with fewer than two live neighbours dies, as if by loneliness.
        // Any live cell with more than three live neighbours dies, as if by overcrowding.
        // Any live cell with two or three live neighbours lives, unchanged, to the next generation.
        // Any dead cell with exactly three live neighbours comes to life.
    }

    public class Misc {

    }

    public class DiehardSeed {
        [Fact]
        public void Should_Result_In_A_Empty_Grid_With_Diehard_Seed() {
            var grid = @"------X-XX-------X---XXX";

            var generation = new Generation(grid, 3, 8);

            // Run 129 ticks.
            for (int i = 0; i < 129; i++)
                generation = generation.Tick();

            Assert.NotEqual(0, generation.Cells.Count());

            // Run the 130th tick.
            generation = generation.Tick();

            Assert.Equal(0, generation.Cells.Count());
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

public class Generation {
    public const char LiveCell = 'X';
    public const char DeadCell = '-';

    private readonly char _liveCell;
    private readonly char _deadCell;

    private readonly IList<Cell> _cells;
    public IList<Cell> Cells { get { return _cells; } }

    public Generation(string grid, int rows, int cols, char liveCell = LiveCell, char deadCell = DeadCell) {
        _liveCell = liveCell;
        _deadCell = deadCell;

        grid = grid.Trim();
        if (grid.Length != rows * cols)
            throw new ArgumentException("Too few cells", "grid");

        var cells = new List<Cell>();
        for (int j = 0; j < rows; j++)
            for (int i = 0; i < cols; i++)
                if (grid[j * cols + i] == _liveCell)
                    cells.Add(new Cell(i, j));

        _cells = new ReadOnlyCollection<Cell>(cells);
    }

    public Generation(params Cell[] cells) {
        _liveCell = LiveCell;
        _deadCell = DeadCell;

        _cells = new ReadOnlyCollection<Cell>(cells);
    }

    public Generation Tick() {
        var stayAlive = _cells.Where(c => GetNeighbourCount(c) >= 2 && GetNeighbourCount(c) <= 3);
        var allDeadNeighbours = _cells.SelectMany(c => GetNeighbourCells(c)).Where(c => !_cells.Any(ce => ce.Equals(c)));
        var newBorn = (from c in allDeadNeighbours
                       group c by c into g
                       select new { Cell = g.Key, Count = g.Count() }).Where(cc => cc.Count == 3).Select(cc => cc.Cell);

        return new Generation(stayAlive.Union(newBorn).OrderBy(c => c.X).ThenBy(c => c.Y).ToArray());
    }

    private int GetNeighbourCount(Cell cell) {
        return _cells.Count(c => Math.Abs(c.X - cell.X) <= 1 && Math.Abs(c.Y - cell.Y) <= 1 && !cell.Equals(c));
    }

    public int GetNeighbourCount(int x, int y) {
        return GetNeighbourCount(new Cell(x, y));
    }

    private static IEnumerable<Cell> GetNeighbourCells(Cell cell) {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (i == 0 && j == 0)
                    continue;
                else
                    yield return new Cell(cell.X + i, cell.Y + j);
    }

    public override string ToString() {
        var sb = new StringBuilder();

        for (int j = Math.Min(0, _cells.Min(c => c.Y)); j <= _cells.Max(c => c.Y); j++) {
            for (int i = Math.Min(0, _cells.Min(c => c.X)); i <= _cells.Max(c => c.X); i++)
                sb.Append(_cells.Contains(new Cell(i, j)) ? _liveCell : _deadCell);

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }
}

public struct Cell : IEquatable<Cell> {
    private readonly int _x, _y;

    public int X { get { return _x; } }
    public int Y { get { return _y; } }

    public Cell(int x, int y) {
        _x = x;
        _y = y;
    }

    public override bool Equals(object obj) {
        if (obj == null)
            return base.Equals(obj);

        if (obj is Cell)
            return Equals((Cell)obj);

        return false;
    }

    public override int GetHashCode() {
        return _x.GetHashCode() * 31 + _y.GetHashCode();
    }

    public override string ToString() {
        return string.Format("[{0},{1}]", _x, _y);
    }

    public bool Equals(Cell other) {
        return _x == other._x && _y == other._y;
    }
}