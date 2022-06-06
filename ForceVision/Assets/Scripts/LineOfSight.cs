using UnityEngine;
public static class LineOfSight
{
    public static void HideAllLinesOfSight(int index, Cell[] cells)
    {
        for(int i = 0; i < cells.Length; i++)
        {
            if (i == index) continue;
            Cell c = cells[i];
            if(c.isActiveAndEnabled)
                c.HideInLineOfSight();
        }
    }
    public static void ShowAllLinesOfSight(int accuracy, int index, int width, Cell[] cells)
    {
        if (!cells[index].isActiveAndEnabled) return;
        HideAllLinesOfSight(index,cells);

        Vector3 origin = cells[index].transform.position;
        ShowLineOfSightFromPoint(accuracy, origin, cells, width);
        ShowLineOfSightFromPoint(accuracy, origin + new Vector3(0.5f,0.5f,0), cells, width);
        ShowLineOfSightFromPoint(accuracy, origin + new Vector3(-0.5f, 0.5f, 0),  cells, width);
        ShowLineOfSightFromPoint(accuracy, origin + new Vector3(-0.5f, -0.5f, 0),  cells, width);
        ShowLineOfSightFromPoint(accuracy, origin + new Vector3(0.5f, -0.5f, 0),  cells, width);
    }
    private static void ShowLineOfSightFromPoint(int accuracy, Vector3 origin, Cell[] cells, int width)
    {
        for (int i = 0; i < width; i++)
        {
            Vector3 destination = cells[i].transform.position;
            RayCast(accuracy, origin, destination);
            destination = cells[cells.Length - i - 1].transform.position;
            RayCast(accuracy, origin, destination);
        }
        for (int i = 0; i < cells.Length; i += width)
        {
            Vector3 destination = cells[i].transform.position;
            RayCast(accuracy, origin, destination);
            destination = cells[i + width - 1].transform.position;
            RayCast(accuracy, origin, destination);
        }

    }
    private static void RayCast(int accuracy, Vector3 origin, Vector3 destination) 
    {
        int wallLayerMask = 1 << 7;
        RaycastHit hit;
        Vector3 direction = destination - origin;

        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, wallLayerMask))
        {
            HighlightCells(accuracy, origin, hit.point);
            Debug.DrawLine(origin, hit.point, Color.white, 10);
        }
    }
    private static void HighlightCells(int accuracy, Vector3 origin, Vector3 hitPoint)
    {
        int cellLayerMask = 1 << 6;
        float distance = Mathf.Min(Vector3.Distance(origin, hitPoint)-0.5f, accuracy);
        RaycastHit[] hits = Physics.RaycastAll(origin, hitPoint - origin, distance, cellLayerMask);
        Debug.Log(hits.Length);
        foreach (RaycastHit h in hits)
        {
            Cell cellHit = h.collider.gameObject.GetComponent<Cell>();
            if (cellHit != null)
            {
                cellHit.OnInLineOfSight();
            }
        }
    }
}
