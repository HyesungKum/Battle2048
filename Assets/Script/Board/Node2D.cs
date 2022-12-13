using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Data
{
    public bool Wall { get; set; }
    public bool Combined { get; set; }

    public BasicUnit Unit { get; set; }
    public Data(bool wall, bool combine, BasicUnit unit)
    {
        this.Wall = wall;
        this.Combined = combine;
        this.Unit = unit;
    }
}

public class Node
{
    //=====================data=======================
    public Data nodeData = new(false, false, null);
    public int XPos { get; set; }
    public int YPos { get; set; }
    //==================connection====================
    public Node Right { get; set; }
    public Node Left { get; set; }
    public Node Up { get; set; }
    public Node Down { get; set; }
    //================================================

    public int GetUnitIndex()
    {
        if (this.nodeData.Unit != null)
        {
            return this.nodeData.Unit.index;
        }
        else
        {
            Debug.Log("node2d ## get index error: 접근가능한 유닛이 없습니다.");
            return 0;
        }
    }
    public void SetWall()
    {
        this.nodeData.Wall = true;
    }
    public GameObject GetUnit()
    {
        if (this.nodeData.Unit != null)
        {
            GameObject unit = this.nodeData.Unit.gameObject;
            this.nodeData.Unit = null;

            return unit;
        }
        else
        {
            Debug.Log("node2d ## get unit error : 접근가능한 유닛이 없습니다.");
            return null;
        }
    }

    public Node()
    { }
    public Node(bool value)
    {
        this.nodeData.Wall = value;
    }
    public Node(int xPos, int yPos, Node right, Node left, Node up, Node down)
    {
        this.XPos = xPos;
        this.YPos = yPos;

        this.Right = right;
        this.Left = left;
        this.Up = up;
        this.Down = down;

        this.nodeData.Wall = false;
    }
}

[Serializable]
public class LinkedNode2D
{
    public Node[,] nodes = null;
    public LinkedNode2D(int size)
    {
        nodes = new Node[size + 2, size + 2];
    }
}
