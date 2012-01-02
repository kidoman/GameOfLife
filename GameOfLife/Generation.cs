namespace GameOfLife {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Generation {
        // Just in case we feel curious about the defining parameters.
        public const int TooFewLimit = 2;
        public const int TooManyLimit = 3;
        public const int PerfectAmount = 3;
        public const char LiveCellChar = 'X';
        public const char DeadCellChar = '-';

        private readonly char _liveCell;
        private readonly char _deadCell;

        // Use a BST to speed up lookups/inserts/retrieval
        private readonly SortedDictionary<Coordinate, Cell> _coords;

        /// <summary>
        /// Initialize the grid based on string input and specified rows and columns.
        /// </summary>
        /// <param name="grid">String based grid seed.</param>
        /// <param name="rows">Total rows in the grid.</param>
        /// <param name="cols">Total columns in the grid.</param>
        /// <param name="liveCell">Character representing the live cell.</param>
        /// <param name="deadCell">Character representing the dead cell.</param>
        /// <param name="caseSensitive">If the grid is considered case sensitive.</param>
        public Generation(string grid, int rows, int cols, char liveCell = LiveCellChar, char deadCell = DeadCellChar, bool caseSensitive = false) {
            _liveCell = liveCell;
            _deadCell = deadCell;

            grid = grid.Trim();
            if (grid.Length != rows * cols)
                throw new ArgumentException("Too few cells.", "grid");

            _coords = new SortedDictionary<Coordinate, Cell>();
            for (int j = 0; j < rows; j++)
                for (int i = 0; i < cols; i++)
                    if (caseSensitive && grid[j * cols + i] == _liveCell ||
                        caseSensitive == false && grid[j * cols + i].ToString().ToUpper() == _liveCell.ToString().ToUpper())
                        _coords.Add(new Coordinate(i, j), new Cell(CellState.Alive));
        }

        /// <summary>
        /// Initialize the grid based on a series of coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates to initialize with.</param>
        public Generation(params Coordinate[] coordinates)
            : this(coordinates.Select(c => new KeyValuePair<Coordinate, Cell>(c, new Cell(CellState.Alive)))) {
        }

        private Generation(IEnumerable<KeyValuePair<Coordinate, Cell>> grid) {
            _liveCell = LiveCellChar;
            _deadCell = DeadCellChar;

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
                            let count = GetLiveNeighboursCount(c.Key)
                            where count >= TooFewLimit && count <= TooManyLimit
                            select c;

            // Easily calculate the 'dead' neighbouring cells (then we will check the freq. of the dead neighbouring cells and if freq. = 3, BOOM! LIFE!)
            var newBorn = _coords.SelectMany(c => GetDeadNeighbours(c.Key))
                                 .Distinct()
                                 .Where(c => GetLiveNeighboursCount(c) == PerfectAmount)
                                 .Select(c => new KeyValuePair<Coordinate, Cell>(c, new Cell(CellState.Alive)));

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

        private int GetLiveNeighboursCount(Coordinate coord) {
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
        public int GetLiveNeighboursCount(int x, int y) {
            return GetLiveNeighboursCount(new Coordinate(x, y));
        }

        // Accessor function (use with care!)
        private Cell GetCell(int x, int y) {
            return _coords.Single(c => c.Key.Equals(new Coordinate(x, y))).Value;
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

        /// <summary>
        /// Give a character representation of the current grid state.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString() {
            return ToString(0, 0);
        }

        /// <summary>
        /// Give a character representation of the current grid state.
        /// </summary>
        /// <param name="rows">How many rows to show.</param>
        /// <param name="cols">How many columns to show.</param>
        /// <returns>String representation.</returns>
        public string ToString(int rows, int cols) {
            if (TotalAlive < 1)
                return string.Empty;

            var sb = new StringBuilder();

            for (int j = Math.Min(0, _coords.Min(c => c.Key.Y)); j <= Math.Max(rows - 1, _coords.Max(c => c.Key.Y)); j++) {
                for (int i = Math.Min(0, _coords.Min(c => c.Key.X)); i <= Math.Max(cols - 1, _coords.Max(c => c.Key.X)); i++)
                    sb.Append(IsAlive(i, j) ? GetCell(i, j).ToString() : _deadCell.ToString());

                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
    }
}
