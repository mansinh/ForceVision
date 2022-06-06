using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TrainingDataGenerator : MonoBehaviour {
    [SerializeField] float deltaTime;

    [SerializeField] int azimuthSteps;

    [SerializeField] float minAltitude;
    [SerializeField] float maxAltitude;
    [SerializeField] int altitudeSteps;

    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] int distanceSteps;

    [SerializeField] float minLightIntensity;
    [SerializeField] float maxLightIntensity;
    [SerializeField] int lightIntensitySteps;

    [SerializeField] GamePiece[] gamePiecePrefabs;
    [SerializeField] int[] gamePieceCounts;
    [SerializeField] Map map;

    [SerializeField] GamePiece.PieceType targetType;

    StreamWriter writer;
    string annotationPath = "C:/Users/manhk/ForceVision/Assets/Database/opencv/build/x64/vc15/bin";
    string positiveImagesPath = "C:/Users/manhk/ForceVision/Assets/Database/opencv/build/x64/vc15/bin/Positive";
    string negativeImagesPath = "C:/Users/manhk/ForceVision/Assets/Database/opencv/build/x64/vc15/bin/Negative";
    private GamePiece[] gamePieces;
    private List<GamePiece> availableGamePieces = new List<GamePiece>();
    private List<GamePiece> activeGamePieces = new List<GamePiece>();
   
    private Light lightSource;
    private bool isGenerating = false;
    private float pieceDensity;
    private int imageCount = 0;

    private void Start() {
        lightSource = FindObjectOfType<Light>();
        CreateGamePieces();
        PlaceGamePieces();
    }

    private void CreateGamePieces() {
        for (int i = 0; i < gamePiecePrefabs.Length; i++) {
            for (int j = 0; j < gamePieceCounts[i]; j++) {
                Instantiate(gamePiecePrefabs[i], transform);
            }
        }
        gamePieces = FindObjectsOfType<GamePiece>();
    }
    private void PlaceGamePieces() {
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

            if (map.isActiveCell[i]) {
                if (UnityEngine.Random.value < pieceDensity) {
                    GamePiece gamePiece = availableGamePieces[0];
                    gamePiece.transform.localPosition = map.GetCellLocalPosition(i) / map.width - new Vector3(0.5f - 1 / map.width / 2, 0, 0.5f - 1 / map.width / 2);
                    gamePiece.transform.localScale = Vector3.one;
                    Vector3 eulerAngles = gamePiece.transform.localEulerAngles;
                    eulerAngles.y = (int)(UnityEngine.Random.value * 4) * 90;
                    gamePiece.transform.localEulerAngles = eulerAngles;
                    gamePiece.gameObject.SetActive(true);
                    activeGamePieces.Add(gamePiece);
                    availableGamePieces.RemoveAt(0);
                }
            }
        }
    }
    private void ShuffleList(List<GamePiece> list) {
        for (int i = 0; i < list.Count; i++) {
            int index = (int)(UnityEngine.Random.value * list.Count);
            GamePiece gamePiece = list[index];
            list.RemoveAt(index);
            list.Insert((int)(UnityEngine.Random.value * list.Count), gamePiece);
        }
    }
    bool isPositive;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.RightControl) && !isGenerating) {
            isPositive = true;
            Generate();
        }
        if (Input.GetKeyDown(KeyCode.RightAlt) && !isGenerating) {
            isPositive = false;
            Generate();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isGenerating) {
            PlaceGamePieces();
        }
    }

   

    private void Generate() {
        isGenerating = true;

        if (!Directory.Exists(annotationPath)) {
            Directory.CreateDirectory(annotationPath);
        }

        if (isPositive) {
            foreach (GamePiece gamePiece in activeGamePieces) {
                gamePiece.gameObject.SetActive(true);
            }
            writer = new StreamWriter(annotationPath + "/" + targetType.ToString() + "_Positive.txt", true);
 
        } else {
            foreach (GamePiece gamePiece in activeGamePieces) {
                if (gamePiece.type == targetType) {
                    gamePiece.gameObject.SetActive(false);
                }
            }
            writer = new StreamWriter(annotationPath + "/"+targetType.ToString() + "_Negative.txt", true);
        }
 
        StartCoroutine(PanGen());
    }

    private void PositiveData(string fileName) {
        if (AnnotatePositive(fileName)) {
            CapturePositive(fileName);
        }
    }
    private void NegativeData(string fileName) {
        AnnotateNegative(fileName);
        CaptureNegative(fileName);
    }
    private void CapturePositive(string fileName) {
        imageCount++;
        Texture2D capture = Capture();
        SaveImage(capture, fileName, positiveImagesPath);
        Destroy(capture);
    }
    private void CaptureNegative(string fileName) {
        imageCount++;
        Texture2D capture = Capture();
        SaveImage(capture, fileName, negativeImagesPath);
        Destroy(capture);
    }

    private Texture2D Capture() {
        RenderTexture current = RenderTexture.active;
        RenderTexture target = Camera.main.targetTexture;
        RenderTexture.active = target;
        Camera.main.Render();

        Texture2D capture = new Texture2D(target.width, target.height);
        capture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        capture.Apply();
        RenderTexture.active = current;
        return capture;
    }

    private void AnnotateNegative(string fileName) {
        writer.WriteLine($"Negative/{fileName}.jpg");
    }

    private bool AnnotatePositive(string fileName) {
        string line = "";
        int pieceCount = 0;
        int[] boundingRect = new int[4];
        foreach (GamePiece gamePiece in activeGamePieces) {
            if (gamePiece.CalculateBounds(out boundingRect)) {
                if (gamePiece.type == targetType) {
                   
                    pieceCount++;
                    // record bounding rect   
                    line += " " + boundingRect[0]
                          + " " + boundingRect[1]
                          + " " + boundingRect[2]
                          + " " + boundingRect[3]
                          + " ";
                }
            }
        }
        if (pieceCount == 0)
            return false;

        line = $"Positive/{fileName}.jpg" + "  " + pieceCount + " " + line;

        writer.WriteLine(line);
        return true;
    }

    private string SaveImage(Texture2D image, string fileName, string imagesPath) {
        if (!Directory.Exists(imagesPath)) {
            Directory.CreateDirectory(imagesPath);
        }
        string fullPath = $"{imagesPath}/{fileName}.jpg";
        File.WriteAllBytes(fullPath, image.EncodeToJPG());
        return fullPath;
    }

    private IEnumerator PanGen() {
        imageCount = 0;

        float dR = (maxDistance - minDistance) / distanceSteps;
        float dAz = 2 * Mathf.PI / azimuthSteps;
        float dAl = (maxAltitude - minAltitude) / altitudeSteps;
        float dLi = (maxLightIntensity - minLightIntensity) / lightIntensitySteps;
        for (int n = 0; n < 4; n++) {
            for (int l = 0; l <= lightIntensitySteps; l++) {
                for (int k = 0; k <= distanceSteps; k++) {
                    for (int j = 0; j <= altitudeSteps; j++) {
                        for (int i = 0; i < azimuthSteps; i++) {
                            float az = dAz * i;
                            float al = (dAl * j + minAltitude) * Mathf.Deg2Rad;

                            float r = dR * k + minDistance;
                            float li = dLi * l + minLightIntensity;

                            float x = r * Mathf.Cos(az) * Mathf.Sin(al);
                            float z = r * Mathf.Sin(az) * Mathf.Sin(al);
                            float y = r * Mathf.Cos(al);

                            Camera.main.transform.position = new Vector3(x, y, z);
                            Camera.main.transform.LookAt(transform.position, Vector3.up);

                            lightSource.intensity = li;
                            string fileName = targetType.ToString() + "_" +
                                "" + imageCount + "_" + (int)(az * Mathf.Rad2Deg) + "az_" + (int)(al * Mathf.Rad2Deg) + "al_" + r + "m_" + (int)(li * 100) + "i_"+n+"n";

                            if (isPositive) {
                                PositiveData(fileName);
                            } else {
                                NegativeData(fileName);
                            }

                            yield return new WaitForSeconds(deltaTime);
                        }
                    }
                }
            }
            if (isPositive) {
                PlaceGamePieces();
            } else {
                break;
            }
        }

        writer.Close();
        AssetDatabase.Refresh();
        isGenerating = false;

    }
}
