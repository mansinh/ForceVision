using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MapDisplay : MonoBehaviour {
    public float width, height, cellSize;

    public int selectedCellIndex { get; private set; }
    public int Speed => speed;
    public int Accuracy => accuracy;

    [SerializeField] private bool reset = false, save = false;
    [SerializeField] private Map map;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Cell[] cells;
    [SerializeField] private Canvas actionUI;

    private int accuracy = 4, speed = 4;
    private BoxCollider boxCollider;
    private Vector3 origin;
 

    // Start is called before the first frame update
    void Start() {
        origin.x = width / 2 - 0.5f;
        origin.y = height / 2 - 0.5f;
    }

    // Update is called once per frame
    void Update() {
        if (reset) {
            Reset();
            reset = false;
        }
        if (save) {
            SaveMap();
            save = false;
        }
    }
    public void Reset() {
        ClearMap();
        LoadMap();
    }
    public bool IsCellSelected() {
        return IsIndexInRange(selectedCellIndex);
    }
    public void OnToggleMovement(bool isShowing) {
        isShowingMovement = isShowing;
        if (isShowing) {
            if (IsCellSelected())
                Movement.ShowAllMovementCost(speed, selectedCellIndex, cells);
        } else {
            Movement.HideAllMovement(cells);
        }
    }
    public void OnToggleLineOfSight(bool isShowing) {
        isShowingLineOfSight = isShowing;
        if (isShowing) {
            if (IsCellSelected())
                LineOfSight.ShowAllLinesOfSight((int)(accuracy*transform.localScale.x), selectedCellIndex, (int)width, cells);
        } else {
            LineOfSight.HideAllLinesOfSight(selectedCellIndex, cells);
        }
    }
    /*
    public void OnPlaceBlocker(bool blocked) {
        if (IsIndexInRange(selectedCellIndex)) {
            cells[selectedCellIndex].IsBlocked = blocked;
        }
    }*/
    public void ClearOccupied() {
        if (cells == null) return;
        for (int i = 0; i < cells.Length; i++) {
            if (cells[i]) cells[i].IsOccupied = false;
        }
    }
    public void SetAccuracy(string accuracyString) {
        int accuracy;
        if (int.TryParse(accuracyString, out accuracy)) {
            SetAccuracy(accuracy);
        }
    }
    public void SetAccuracy(int accuracy) {
        this.accuracy = accuracy;
        if (IsCellSelected() && isShowingLineOfSight) {
            LineOfSight.ShowAllLinesOfSight((int)(accuracy*transform.localScale.x), selectedCellIndex, (int)width, cells);
        }
    }
    public void SetSpeed(string speedString) {
        int speed;
        if (int.TryParse(speedString, out speed)) {
            SetSpeed(speed);
        }
    }
    public void SetSpeed(int speed) {
        this.speed = speed;
        if (IsCellSelected() && isShowingMovement) {
            Movement.ShowAllMovementCost(speed, selectedCellIndex, cells);
        }
    }
    public Cell[] GetCells() {
        return cells;
    }
    public Cell GetCell(Vector3 position) {
        return GetCell(PositionToIndex(position));
    }
    public Cell GetCell(int i) {
        if (i >= cells.Length || i < 0) return null;
        return cells[i];
    }

   
    private void CreateMap() {
        origin.x = width / 2 - 0.5f;
        origin.y = height / 2 - 0.5f;

        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(width, height, 1);
        cells = new Cell[(int)(width * height)];

        for (int i = 0; i < cells.Length; i++) {
            GameObject cell = Instantiate(cellPrefab.gameObject, transform);
            cell.transform.localPosition = IndexToPosition(i);
            cells[i] = cell.GetComponent<Cell>();
            cells[i].index = i;
            cells[i].coordinate = IndexToCoordinate(i);
        }
        for (int i = 0; i < cells.Length; i++) {
            cells[i].SetNeighbours(GetNeighbours(i));
        }
    }
    private void SaveMap() {
        if (!map) return;
        if (cells == null) return;
        map.width = width;
        map.height = height;
        map.cellSize = cellSize;
        map.isActiveCell = new bool[(int)(width * height)];
        map.isWalledNorth = new bool[(int)(width * height)];
        map.isWalledEast = new bool[(int)(width * height)];
        map.isWalledSouth = new bool[(int)(width * height)];
        map.isWalledWest = new bool[(int)(width * height)];
        map.isBlocked = new bool[(int)(width * height)];
        for (int i = 0; i < cells.Length; i++) {
            map.isActiveCell[i] = cells[i].gameObject.activeSelf;
            map.isWalledNorth[i] = cells[i].IsWalledNorth;
            map.isWalledEast[i] = cells[i].IsWalledEast;
            map.isWalledSouth[i] = cells[i].IsWalledSouth;
            map.isWalledWest[i] = cells[i].IsWalledWest;
            map.isBlocked[i] = cells[i].IsBlocked;
        }
        Debug.Log("Map" + map.name + "Saved");
    }

    private void LoadMap() {
        width = map.width;
        height = map.height;
        cellSize = map.cellSize;

        CreateMap();

        for (int i = 0; i < cells.Length; i++) {
            if (i >= map.isActiveCell.Length) break;
            cells[i].gameObject.SetActive(map.isActiveCell[i]);
            if (map.isWalledNorth.Length == cells.Length)
                cells[i].IsWalledNorth = map.isWalledNorth[i];
            if (map.isWalledEast.Length == cells.Length)
                cells[i].IsWalledEast = map.isWalledEast[i];
            if (map.isWalledSouth.Length == cells.Length)
                cells[i].IsWalledSouth = map.isWalledSouth[i];
            if (map.isWalledWest.Length == cells.Length)
                cells[i].IsWalledWest = map.isWalledWest[i];
            cells[i].IsBlocked = map.isBlocked[i];
        }
        Debug.Log("Map" + map.name + "Loaded");
    }
    bool isShowingMovement, isShowingLineOfSight;

    private void ClearMap() {
        if (cells == null) return;
        for (int i = 0; i < cells.Length; i++) {
            if (cells[i]) DestroyImmediate(cells[i].gameObject);
        }
    }


    private static bool ScreenToWorldPosition(Vector3 screenPosition, out Vector3 worldPosition) {
        int layerMask = 1 << 8;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        worldPosition = hit.point;
        return hasHit;
    }

    public void SelectCellFromScreenPosition(Vector3 screenPosition) {
        DeselectAllCells();
        Vector3 worldPosition;
        bool hasHit = ScreenToWorldPosition(screenPosition, out worldPosition);
        if (hasHit) {
            int index = PositionToIndex(worldPosition);
            if (OnCellSelected(index)) {
                selectedCellIndex = index;
            } else {
                selectedCellIndex = -1;
            }
        }
    }
    public void DeselectCell() {
        if (!IsCellSelected()) return;
        cells[selectedCellIndex].OnDeselect();
        selectedCellIndex = -1;
        DeselectAllCells();
    }
    private bool OnCellSelected(int index) {
        if (index < 0) return false;
        if (!cells[index].isActiveAndEnabled) return false;

        selectedCellIndex = index;
        if (cells[index].OnClicked()) {
            if (isShowingLineOfSight)
                LineOfSight.ShowAllLinesOfSight((int)(accuracy*transform.localScale.x), selectedCellIndex, (int)width, cells);
            if (isShowingMovement)
                Movement.ShowAllMovementCost(speed, selectedCellIndex, cells);
            return true;
        }
        return false;
    }
   
    public void DeselectAllCells() {
        foreach (Cell cell in cells) {
            if (cell.isActiveAndEnabled) {
                cell.OnDeselect();
            }
        }
    }
    public Cell GetCellFromScreenPosition(Vector3 screenPosition) {
        Vector3 worldPosition;
        bool hasHit = ScreenToWorldPosition(screenPosition, out worldPosition);
        if (hasHit) {
            int index = PositionToIndex(worldPosition);
            if (IsIndexInRange(index)) {
                return cells[index];
            }
        }
        return null;
    }
    private Vector3 IndexToPosition(int i) {
        return IndexToCoordinate(i) * cellSize - origin;
    }
    private int PositionToIndex(Vector3 position) {
        return CoordinateToIndex(PositionToCoordinates(position));
    }
    private Vector3 PositionToCoordinates(Vector3 position) {
        position = transform.InverseTransformPoint(position);
        int x = Mathf.RoundToInt(position.x / cellSize + origin.x);
        int y = Mathf.RoundToInt(position.y / cellSize + origin.y);
        return new Vector3(x, y, 0);
    }
    private int CoordinateToIndex(Vector3 coord) {
        return CoordinateToIndex((int)coord.x, (int)coord.y);
    }
    private int CoordinateToIndex(int x, int y) {
        if (!IsCoordinateInRange(x, y)) return -1;
        return (int)(y * width + x);
    }
    private Vector3 IndexToCoordinate(int i) {
        int x = (int)(i % width);
        int y = (int)(i / width);
        return new Vector3(x, y, 0);
    }
    private bool IsIndexInRange(int i) {
        return i >= 0 && i < width * height;
    }
    private bool IsCoordinateInRange(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public List<Cell> GetNeighbours(int index) {
        List<Cell> neighbours = new List<Cell>();
        neighbours.Add(GetNorth(index));
        neighbours.Add(GetEast(index));
        neighbours.Add(GetSouth(index));
        neighbours.Add(GetWest(index));
        neighbours.Add(GetNorthEast(index));
        neighbours.Add(GetSouthEast(index));
        neighbours.Add(GetSouthWest(index));
        neighbours.Add(GetNorthWest(index));
        return neighbours;
    }

    private Cell GetNorth(int index) { return GetCell(index + (int)width); }
    private Cell GetEast(int index) { return GetCell(index + 1); }
    private Cell GetSouth(int index) { return GetCell(index - (int)width); }
    private Cell GetWest(int index) { return GetCell(index - 1); }
    private Cell GetNorthEast(int index) { return GetCell(index + (int)width + 1); }
    private Cell GetSouthEast(int index) { return GetCell(index - (int)width + 1); }
    private Cell GetNorthWest(int index) { return GetCell(index + (int)width - 1); }
    private Cell GetSouthWest(int index) { return GetCell(index - (int)width - 1); }
}
