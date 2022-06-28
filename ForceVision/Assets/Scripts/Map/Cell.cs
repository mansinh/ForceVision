using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(TMP_Text))]
public class Cell : MonoBehaviour {
    public bool IsBlocked {
        get {
            isBlocked = blockerHighlight.activeSelf;
            return isBlocked;
        }
        set {
            isBlocked = value;
            UpdateBlock();
        }
    }
    public bool IsOccupied {
        get {
            isOccupied = occupiedHighlight.activeSelf;
            return isOccupied;
        }
        set {
            isOccupied = value;
            UpdateOccupied();
        }
    }

    [SerializeField] Sprite defaultSprite;
    [SerializeField] GameObject[] walls = new GameObject[4]; // [N, E, S, W]
    [SerializeField] GameObject blockerHighlight, selectedHighlight, movementHighlight, lineOfSightHighlight, occupiedHighlight;
    [SerializeField] bool isWalledNorth, isWalledEast, isWalledSouth, isWalledWest;
    [SerializeField] TMP_Text moveCostText;
    public bool IsWalledNorth { get { return walls[0].activeSelf; } set { walls[0].SetActive(value); } }
    public bool IsWalledEast { get { return walls[1].activeSelf; } set { walls[1].SetActive(value); } }
    public bool IsWalledSouth { get { return walls[2].activeSelf; } set { walls[2].SetActive(value); } }
    public bool IsWalledWest { get { return walls[3].activeSelf; } set { walls[3].SetActive(value); } }

    public float moveCost = 1000;
    public Vector3 coordinate;
    public int index;

    private bool selected = false;
    private bool isBlocked = false;
    private bool isOccupied = false;

    private BoxCollider boxCollider;
    private SpriteRenderer spriteRenderer;

    public const int N = 0, E = 1, S = 2, W = 3, NE = 4, SE = 5, SW = 6, NW = 7;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;
        boxCollider = GetComponent<BoxCollider>();
        CalculateCanMove();
    }

    [SerializeField] List<Cell> neighbours;
    [SerializeField] List<bool> canMoveToNeighbour;
    public void SetNeighbours(List<Cell> neighbours) {
        this.neighbours = neighbours;
        CalculateCanMove();
    }
    public List<Cell> GetNeighbours() {
        return neighbours;
    }
    public List<bool> GetCanMoveToNeighbour() {
        return canMoveToNeighbour;
    }
    public void CalculateCanMove() {
        canMoveToNeighbour = new List<bool>();
        canMoveToNeighbour.Add(CanMoveNorth());
        canMoveToNeighbour.Add(CanMoveEast());
        canMoveToNeighbour.Add(CanMoveSouth());
        canMoveToNeighbour.Add(CanMoveWest());
        canMoveToNeighbour.Add(CanMoveNorthEast());
        canMoveToNeighbour.Add(CanMoveSouthEast());
        canMoveToNeighbour.Add(CanMoveSouthWest());
        canMoveToNeighbour.Add(CanMoveNorthWest());
    }
    public bool CanMoveNorth() {
        return (neighbours[N] ? IsNeighbourAvailable(neighbours[N]) : false) && !IsWalledNorth;
    }
    public bool CanMoveEast() {
        return (neighbours[E] ? IsNeighbourAvailable(neighbours[E]) : false) && !IsWalledEast;
    }
    public bool CanMoveSouth() {
        return (neighbours[S] ? IsNeighbourAvailable(neighbours[S]) : false) && !IsWalledSouth;
    }
    public bool CanMoveWest() {
        return (neighbours[W] ? IsNeighbourAvailable(neighbours[W]) : false) && !IsWalledWest;
    }
    public bool CanMoveNorthEast() { 
        return (neighbours[NE]? IsNeighbourAvailable(neighbours[NE]):false) && 
            ((CanMoveNorth() && neighbours[N].CanMoveEast()) || (CanMoveEast() && neighbours[E].CanMoveNorth()));
    }
    public bool CanMoveSouthEast() {
        return (neighbours[SE] ? IsNeighbourAvailable(neighbours[SE]) : false) &&
            ((CanMoveSouth() && neighbours[S].CanMoveEast()) || (CanMoveEast() && neighbours[E].CanMoveSouth()));
    }
    public bool CanMoveSouthWest() {
        return (neighbours[SW] ? IsNeighbourAvailable(neighbours[SW]) : false) &&
            ((CanMoveSouth() && neighbours[S].CanMoveWest()) || (CanMoveWest() && neighbours[W].CanMoveSouth()));
    }
    public bool CanMoveNorthWest() {
        return (neighbours[NW] ? IsNeighbourAvailable(neighbours[NW]) : false) &&
            ((CanMoveNorth() && neighbours[N].CanMoveWest()) || (CanMoveWest() && neighbours[W].CanMoveNorth()));
    }
    private bool IsNeighbourAvailable(Cell neighbour) {
        return neighbour.isActiveAndEnabled && !neighbour.IsBlocked && !neighbour.isOccupied;
    }
    public void ShowMoveCost() {
        if (!isActiveAndEnabled) return;
        if (moveCostText == null) return;
        moveCostText.gameObject.SetActive(true);
        movementHighlight.SetActive(true);
    }

    public void SetMoveCost(float moveCost) {
        this.moveCost = moveCost;
        moveCostText.SetText("" + moveCost);
    }

    public void HideMoveCost() {
        if (!isActiveAndEnabled) return;
        if (moveCostText == null) return;

        moveCostText.gameObject.SetActive(false);
        movementHighlight.SetActive(false);
    }

    public bool OnClicked() {
        if (!selected) {
            OnSelect();
        } else {
            OnDeselect();
        }
        return selected;
    }

    public void OnSelect() {
        selectedHighlight.SetActive(true);
        selected = true;
    }
    public void OnDeselect() {
        HideInLineOfSight();
        HideMoveCost();
        selectedHighlight.SetActive(false);
        selected = false;
    }
    public void ShowInLineOfSight() {
        lineOfSightHighlight.SetActive(true);
    }
    public void HideInLineOfSight() {
        lineOfSightHighlight.SetActive(false);
        if (boxCollider)
            boxCollider.enabled = true;
    }
    public void OnInLineOfSight() {
        ShowInLineOfSight();
        if (boxCollider)
            boxCollider.enabled = false;
    }

    private void Update() {
        if (Application.isEditor) {
            if (isWalledNorth) { IsWalledNorth = !IsWalledNorth; isWalledNorth = false; }
            if (isWalledEast) { IsWalledEast = !IsWalledEast; isWalledEast = false; }
            if (isWalledSouth) { IsWalledSouth = !IsWalledSouth; isWalledSouth = false; }
            if (isWalledWest) { IsWalledWest = !IsWalledWest; isWalledWest = false; }

        }
    }
    private void UpdateBlock() {
        blockerHighlight.gameObject.SetActive(isBlocked);
    }
    private void UpdateOccupied() {
        occupiedHighlight.gameObject.SetActive(isOccupied);
    }
    private void ToggleAll() {
        Debug.Log("Toggle all walls");
        IsWalledNorth = !IsWalledNorth;
        IsWalledEast = !IsWalledEast;
        IsWalledSouth = !IsWalledSouth;
        IsWalledWest = !IsWalledWest;
    }
}
