using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node2D
{
    public bool Wall { get; set; }
    public bool Empty { get; set; }
    public bool Combined { get; set; }
    public GameObject Unit { get; set; }
    public int Value { get; set; }
    public int XPos { get; set; }
    public int YPos { get; set; }
    public Node2D Right { get; set; }
    public Node2D Left { get; set; }
    public Node2D Up { get; set; }
    public Node2D Down { get; set; }

    public int GetUnitIndex
    {
        get 
        {
            int i = 0;
            int value = this.Value;
            while (value != 2)
            {
                value /= 2;
                i++;
            }
            return i;
        }
    }

    public Node2D()
    {
        this.Empty = true;
    }
    public Node2D(int value)
    {
        this.Value = value;
        this.Empty = false;
    }
    public Node2D(bool value)
    {
        this.Wall = value;
        this.Empty = false;
    }
    public Node2D(int value, int xPos, int yPos, Node2D right, Node2D left, Node2D up, Node2D down)
    {
        this.Value = value;

        this.XPos = xPos;
        this.YPos = yPos;

        this.Right = right;
        this.Left = left;
        this.Up = up;
        this.Down = down;

        this.Wall = false;
        this.Empty = false;
    }
}
