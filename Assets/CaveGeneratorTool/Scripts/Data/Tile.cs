using System;

[Serializable]
public struct CaveTile : IEquatable<CaveTile>
{
    public int X;
    public int Y;
    public int Value;

    public CaveTile(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
    }

    public bool Equals(CaveTile other)
    {
        return (X == other.X && Y == other.Y);
    }
}
