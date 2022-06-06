using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObjects/Map", order = 1)]
public class Map : ScriptableObject
{
    public float width, height, cellSize;
    public bool[] isActiveCell;
    public bool[] isWalledNorth;
    public bool[] isWalledEast;
    public bool[] isWalledWest;
    public bool[] isWalledSouth;

    public Vector3 GetCellLocalPosition(int index) 
    {
        float z = (int)(index % width);
        float x = (int)(index / width);

        return new Vector3(x,0,z);
    }
    public int GetOccupiedCellCount() {
        int count = 0;
        for (int i = 0; i < isActiveCell.Length; i++)
            if (isActiveCell[i])
                count++;
        return count;
    }
}
