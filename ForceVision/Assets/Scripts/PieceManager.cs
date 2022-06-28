using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour {
    public List<GamePiece> activeGamePieces = new List<GamePiece>();
    [SerializeField] KeyCode placeRandomlyKey = KeyCode.Space;
    [SerializeField] GamePiece[] gamePiecePrefabs;
    [SerializeField] int[] gamePieceCounts;
    [SerializeField] Map map;

    private List<GamePiece> gamePieces = new List<GamePiece>();
    private List<GamePiece> availableGamePieces = new List<GamePiece>();
    private float pieceDensity;

    void Awake() {
        CreateGamePieces();
        PlaceRandomly();
    }
    private void CreateGamePieces() {
        for (int i = 0; i < gamePiecePrefabs.Length; i++) {
            for (int j = 0; j < gamePieceCounts[i]; j++) {
                gamePieces.Add( Instantiate(gamePiecePrefabs[i], transform));
            }
        }
    }
    private void PlaceRandomly() {
        foreach (GamePiece gamePiece in gamePieces) {
            gamePiece.gameObject.SetActive(false);
            availableGamePieces.Add(gamePiece);
        }

        ShuffleList(availableGamePieces);
        pieceDensity = (float)availableGamePieces.Count / map.GetOccupiedCellCount();

        activeGamePieces.Clear();

        for (int i = 0; i < map.isActiveCell.Length; i++) {
            if (availableGamePieces.Count == 0)
                break;

            if (map.isActiveCell[i] && !map.isBlocked[i]) {

                if (UnityEngine.Random.value < pieceDensity) {
                    PlacePiece(map.GetCellLocalPosition(i) / map.width - new Vector3(0.5f - 1 / map.width / 2, 0, 0.5f - 1 / map.width / 2));
                }
            }
        }
    }
    private void PlacePiece(Vector3 position) {
        GamePiece gamePiece = availableGamePieces[0];
        gamePiece.transform.localPosition = position;;
        gamePiece.transform.localScale = Vector3.one;
        Vector3 eulerAngles = gamePiece.transform.localEulerAngles;
        eulerAngles.y = (int)(UnityEngine.Random.value * 4) * 90;
        gamePiece.transform.localEulerAngles = eulerAngles;
        gamePiece.gameObject.SetActive(true);
        activeGamePieces.Add(gamePiece);
        availableGamePieces.RemoveAt(0);
    }
    private void ShuffleList(List<GamePiece> list) {
        for (int i = 0; i < list.Count; i++) {
            int index = (int)(UnityEngine.Random.value * list.Count);
            GamePiece gamePiece = list[index];
            list.RemoveAt(index);
            list.Insert((int)(UnityEngine.Random.value * list.Count), gamePiece);
        }
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(placeRandomlyKey)) {
            PlaceRandomly();
        }
    }
}
