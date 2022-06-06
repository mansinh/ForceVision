using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Movement
{
    private static List<int> openCellIndices = new List<int>();
    public static void HideAllMovement(Cell[] cells)
    {
        foreach (Cell c in cells)
        {
            c.HideMoveCost();
        }
    }
    public static void ShowAllMovementCost(int speed, int index, Cell[] cells)
    {
        if (!cells[index].isActiveAndEnabled) return;

        foreach (Cell c in cells)
        {
            c.SetMoveCost(1000);
        }

        Cell cell = cells[index];
        cell.SetMoveCost(0);
        int i = index;
        openCellIndices.Add(i);

        while (openCellIndices.Count > 0)
        {
            i = PopFront();
            cell = cells[i];
            List<Cell> neighbours = cell.GetNeighbours();
            List<bool> canMoveToNeighbours = cell.GetCanMoveToNeighbour();
            for(int j = 0; j < neighbours.Count; j++)
            {
                Cell neighbour = neighbours[j];
                bool canMoveToNeighbour = canMoveToNeighbours[j];
                if(neighbour && canMoveToNeighbour)
                if (neighbour.moveCost > cell.moveCost + 1)
                {
                    ToBack(neighbour.index);
                    neighbour.SetMoveCost(cell.moveCost+1);
                }
            }
            if (cell.moveCost <= speed)
            {
                cell.ShowMoveCost();
            }
            else
            {
                cell.HideMoveCost();
            }
         
        }
    }
    private static int PopFront()
    {
        int front = openCellIndices[0];
        openCellIndices.RemoveAt(0);
        return front;
    }
    private static void ToBack(int index)
    {
        if (!openCellIndices.Contains(index))
        {
            openCellIndices.Remove(index);
        }
        openCellIndices.Add(index);
    }
}
