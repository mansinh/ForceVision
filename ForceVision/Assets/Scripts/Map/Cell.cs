using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(TMP_Text))]
public class Cell : MonoBehaviour
{
    [SerializeField] Sprite defaultSprite, selectedSprite;
    [SerializeField] Color defaultColor, movementColor, lineOfSightColor, enemyColor;
    [SerializeField] GameObject[] walls = new GameObject[4]; // [N, E, S, W]
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
    private BoxCollider boxCollider;
    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;
        boxCollider = GetComponent<BoxCollider>();
        CalculateCanMove();
    }

    [SerializeField] List<Cell> neighbours;
    [SerializeField] List<bool> canMoveToNeighbour;
    public void SetNeighbours(List<Cell> neighbours)
    {
        this.neighbours = neighbours;
        CalculateCanMove();
    }
    public List<Cell> GetNeighbours() 
    {
        return neighbours;
    }
    public List<bool> GetCanMoveToNeighbour()
    {
        return canMoveToNeighbour;
    }
    public void CalculateCanMove()
    {
        canMoveToNeighbour = new List<bool>();
        canMoveToNeighbour.Add((neighbours[0] ? neighbours[0].isActiveAndEnabled : false) && !IsWalledNorth);
        canMoveToNeighbour.Add((neighbours[1] ? neighbours[1].isActiveAndEnabled : false) && !IsWalledEast);
        canMoveToNeighbour.Add((neighbours[2] ? neighbours[2].isActiveAndEnabled : false) && !IsWalledSouth);
        canMoveToNeighbour.Add((neighbours[3] ? neighbours[3].isActiveAndEnabled : false) && !IsWalledWest);
        canMoveToNeighbour.Add((neighbours[4] ? neighbours[4].isActiveAndEnabled : false) && !(IsWalledNorth && IsWalledEast));
        canMoveToNeighbour.Add((neighbours[5] ? neighbours[5].isActiveAndEnabled : false) && !(IsWalledSouth && IsWalledEast));
        canMoveToNeighbour.Add((neighbours[6] ? neighbours[6].isActiveAndEnabled : false) && !(IsWalledSouth && IsWalledWest));
        canMoveToNeighbour.Add((neighbours[7] ? neighbours[7].isActiveAndEnabled : false) && !(IsWalledNorth && IsWalledWest));
    }
    public void ShowMoveCost()
    {
        if (!isActiveAndEnabled) return;
        if (moveCostText == null) return;
        moveCostText.gameObject.SetActive(true);
    }

    public void SetMoveCost(float moveCost)
    {
        this.moveCost = moveCost;
        moveCostText.SetText("" + moveCost);
    }

    public void HideMoveCost()
    {
        if (!isActiveAndEnabled) return;
        if (moveCostText == null) return;

        moveCostText.gameObject.SetActive(false);
    }

    public bool OnClicked()
    {
        Debug.Log("Cell clicked");
        if (!selected)
        {
            OnSelect();
        }
        else
        {
            OnDeselect();
        }
        return selected;
    }

    public void OnSelect()
    {
        spriteRenderer.sprite = selectedSprite;
        spriteRenderer.color = defaultColor;
        selected = true;
    }
    public void OnDeselect()
    {
        HideInLineOfSight();
        HideMoveCost();
        
        selected = false;
    }
    public void ShowInLineOfSight()
    {
        spriteRenderer.sprite = selectedSprite;
        spriteRenderer.color = lineOfSightColor;
    }
    public void HideInLineOfSight()
    {
        Debug.Log(index+" "+ spriteRenderer + " " + defaultSprite);
        spriteRenderer.sprite = defaultSprite;
        spriteRenderer.color = defaultColor;
        boxCollider.enabled = true;
    }
    public void OnInLineOfSight()
    {
        ShowInLineOfSight();
        boxCollider.enabled = false;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (isWalledNorth) { IsWalledNorth = !IsWalledNorth; isWalledNorth = false; }
            if (isWalledEast) { IsWalledEast = !IsWalledEast; isWalledEast = false; }
            if (isWalledSouth) { IsWalledSouth = !IsWalledSouth; isWalledSouth = false; }
            if (isWalledWest) { IsWalledWest = !IsWalledWest; isWalledWest = false; }

        }
    }
    private void ToggleAll()
    {
        Debug.Log("Toggle all walls");
        IsWalledNorth = !IsWalledNorth;
        IsWalledEast = !IsWalledEast;
        IsWalledSouth = !IsWalledSouth;
        IsWalledWest = !IsWalledWest;
    }
}
