using System;

public struct Coordinate : IEquatable<Coordinate>, IComparable<Coordinate> {
    private readonly int _x, _y;

    public int X { get { return _x; } }
    public int Y { get { return _y; } }

    public Coordinate(int x, int y) {
        _x = x;
        _y = y;
    }

    public override bool Equals(object obj) {
        if (obj == null)
            return base.Equals(obj);

        if (obj is Coordinate)
            return Equals((Coordinate)obj);

        return false;
    }

    public override int GetHashCode() {
        return _x.GetHashCode() * 31 + _y.GetHashCode();
    }

    public override string ToString() {
        return string.Format("[{0},{1}]", _x, _y);
    }

    public bool Equals(Coordinate other) {
        return _x == other._x && _y == other._y;
    }

    public int CompareTo(Coordinate other) {
        if (_x == other._x)
            return _y.CompareTo(other._y);
        else
            return _x.CompareTo(other._x);
    }
}

public struct Cell {
}