using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

public class GenerationTests {
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

        var nextGeneration = generation.Tick();

        Assert.Equal(0, nextGeneration.Cells.Count());
    }
}

public class Generation {
    private readonly IList<Cell> _cells;
    public IList<Cell> Cells { get { return _cells; } }

    public Generation(params Cell[] cells) {
        _cells = new ReadOnlyCollection<Cell>(cells);
    }

    public Generation Tick() {
        return new Generation();
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