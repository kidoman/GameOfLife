using System;
using System.Collections.Generic;
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

            Assert.Equal(0, generation.TotalAlive);
        }

        [Fact]
        public void Should_Have_Cells_If_Started_With_Cells() {
            var generation = new Generation(new Coordinate(1, 4), new Coordinate(2, 3));

            Assert.True(generation.IsAlive(1, 4));
            Assert.True(generation.IsAlive(2, 3));
        }

        [Fact]
        public void Should_Kill_Of_A_Lone_Cell() {
            var generation = new Generation(new Coordinate(1, 1));

            var next = generation.Tick();

            Assert.Equal(0, next.TotalAlive);
        }

        [Fact]
        public void Should_Kill_Of_A_Cell_With_Less_Than_2_Neighbours() {
            var generation = new Generation(new Coordinate(1, 1), new Coordinate(2, 2));

            var next = generation.Tick();

            Assert.Equal(0, next.TotalAlive);
        }

        [Fact]
        public void Should_Kill_Of_A_Cell_With_More_Than_3_Neighbours() {
            var generation = new Generation(new Coordinate(1, 1), new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(0, 2), new Coordinate(2, 0));

            var next = generation.Tick();

            Assert.Equal(4, next.TotalAlive);
            Assert.False(next.IsAlive(1, 1));
        }

        [Fact]
        public void Should_Create_A_Cell_With_Exactly_3_Neighbours_If_Already_Dead() {
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

public class Generation {
    // Just in case we feel curious about the defining parameters.
    public const int TooFewLimit = 2;
    public const int TooManyLimit = 3;
    public const int PerfectAmount = 3;

    /// <summary>
    /// Default live cell character.
    /// </summary>
    public const char LiveCell = 'X';
    /// <summary>
    /// Default dead cell character.
    /// </summary>
    public const char DeadCell = '-';

    private readonly char _liveCell;
    private readonly char _deadCell;

    // Use a BST to speed up lookups/inserts/retrieval
    private readonly SortedDictionary<Coordinate, Cell> _coords;

    public Generation(string grid, int rows, int cols, char liveCell = LiveCell, char deadCell = DeadCell) {
        _liveCell = liveCell;
        _deadCell = deadCell;

        grid = grid.Trim();
        if (grid.Length != rows * cols)
            throw new ArgumentException("Too few cells", "grid");

        _coords = new SortedDictionary<Coordinate, Cell>();
        for (int j = 0; j < rows; j++)
            for (int i = 0; i < cols; i++)
                if (grid[j * cols + i] == _liveCell)
                    _coords.Add(new Coordinate(i, j), new Cell());
    }

    public Generation(params Coordinate[] coordinates)
        : this(coordinates.Select(c => new KeyValuePair<Coordinate, Cell>(c, new Cell()))) {
    }

    private Generation(IEnumerable<KeyValuePair<Coordinate, Cell>> grid) {
        _liveCell = LiveCell;
        _deadCell = DeadCell;

        _coords = new SortedDictionary<Coordinate, Cell>();

        foreach (var cc in grid)
            _coords.Add(cc.Key, cc.Value);
    }

    /// <summary>
    /// Moves the generation one tick ahead (in a immutable fashion.)
    /// </summary>
    /// <returns>The next generation (Star Wars reference win!)</returns>
    public Generation Tick() {
        // A little LINQ to calculate the cells which should be allowed to stay alive.
        var stayAlive = from c in _coords
                        let count = GetNeighbourCount(c.Key)
                        where count >= TooFewLimit && count <= TooManyLimit
                        select c;

        // Easily calculate the 'dead' neighbouring cells (then we will check the freq. of the dead neighbouring cells and if freq. = 3, BOOM! LIFE!)
        var newBorn = _coords.SelectMany(c => GetDeadNeighbours(c.Key))
                             .Where(c => GetNeighbourCount(c) == PerfectAmount)
                             .Select(c => new KeyValuePair<Coordinate, Cell>(c, new Cell()));

        return new Generation(stayAlive.Union(newBorn));
    }

    private static IEnumerable<Coordinate> GetNeighbours(Coordinate coord) {
        // Implemented as an iterator so that we can maximise performance in a large grid.
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (i == 0 && j == 0)
                    continue;
                else
                    yield return new Coordinate(coord.X + i, coord.Y + j);
    }

    private int GetNeighbourCount(Coordinate coord) {
        return GetNeighbours(coord).Count(IsAlive);
    }

    private IEnumerable<Coordinate> GetDeadNeighbours(Coordinate coord) {
        return GetNeighbours(coord).Where(c => !IsAlive(c));
    }

    /// <summary>
    /// Calculates the total number of 'live' neighbour (immediate) cells.
    /// </summary>
    /// <param name="x">X coordinate in the grid.</param>
    /// <param name="y">Y coordinate in the grid.</param>
    /// <returns>Total number of 'live' neighbour (immediate) cells.</returns>
    public int GetNeighbourCount(int x, int y) {
        return GetNeighbourCount(new Coordinate(x, y));
    }

    private bool IsAlive(Coordinate coord) {
        return _coords.ContainsKey(coord);
    }

    /// <summary>
    /// Check if a particular cell is alive or not.
    /// </summary>
    /// <param name="x">X coordinate in the grid.</param>
    /// <param name="y">Y coordinate in the grid.</param>
    /// <returns>True if alive.</returns>
    public bool IsAlive(int x, int y) {
        return IsAlive(new Coordinate(x, y));
    }

    /// <summary>
    /// Get the total number of cells alive in this generation.
    /// </summary>
    public int TotalAlive {
        get {
            return _coords.Count();
        }
    }

    public override string ToString() {
        var sb = new StringBuilder();

        for (int j = Math.Min(0, _coords.Min(c => c.Key.Y)); j <= _coords.Max(c => c.Key.Y); j++) {
            for (int i = Math.Min(0, _coords.Min(c => c.Key.X)); i <= _coords.Max(c => c.Key.X); i++)
                sb.Append(IsAlive(i, j) ? _liveCell : _deadCell);

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }
}