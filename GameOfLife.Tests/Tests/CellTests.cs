using System;

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