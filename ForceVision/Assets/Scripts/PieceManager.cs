using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour {
    public List<GamePiece> activeGamePieces = new List<GamePiece>();
    [SerializeField] KeyCode placeRandomlyKey = KeyCode.Space;
    [SerializeField] GamePiece[] gamePiecePrefabs;
    [SerializeField] int[] gamePieceCounts;
    [SerializeField] MapDisplay map;

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
        pieceDensity = (float)availableGamePieces.Count / (map.width*map.height);

        activeGamePieces.Clear();

        for (int i = 0; i < map.GetCells().Length; i++) {
            if (availableGamePieces.Count == 0)
                break;
            Cell cell = map.GetCell(i);
            if (cell == null)
                break;
            if (cell.gameObject.activeSelf && !cell.IsBlocked) {

                if (UnityEngine.Random.value < pieceDensity) {
                    PlacePiece(cell.transform.position);
                }
            }
        }
    }
    private void PlacePiece(Vector3 position) {
        GamePiece gamePiece = availableGamePieces[0];
        gamePiece.transform.position = position;;
        gamePiece.transform.localScale = 15*map.transform.localScale;
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
