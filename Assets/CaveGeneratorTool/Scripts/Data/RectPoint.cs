using UnityEngine;
using System;

[Serializable]
public struct RectPoint : IEquatable
{
    public int X;
    public int Y;

    public RectPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}
