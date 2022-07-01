using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PieceManager))]
public class TrainingDataGenerator : MonoBehaviour {
    [SerializeField] KeyCode generatePositiveKey = KeyCode.RightAlt;
    [SerializeField] KeyCode generateNegativeKey = KeyCode.RightControl;

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
    [SerializeField] GamePiece.PieceType targetType;

    StreamWriter writer;
    string annotationPath = "Assets/Database/opencv/build/x64/vc15/bin";
    private Light lightSource;
    private bool isGenerating = false;
    private int imageCount = 0;
    private bool isPositive;
    private GamePiece[] pieces;
    //private PieceManager pieceManager;
   

    private void Start() {
        lightSource = FindObjectOfType<Light>();
        //pieceManager = GetComponent<PieceManager>();
    }

    private void Update() {
        if (Input.GetKeyDown(generatePositiveKey) && !isGenerating) {
            isPositive = true;
            StartImageGeneration();
        }
        if (Input.GetKeyDown(generateNegativeKey) && !isGenerating) {
            isPositive = false;
            StartImageGeneration();
        }
    }
    private void StartImageGeneration() {
        isGenerating = true;
        pieces = FindObjectsOfType<GamePiece>();
        if (!Directory.Exists(annotationPath)) {
            Directory.CreateDirectory(annotationPath);
        }

        if (isPositive) {
            foreach (GamePiece gamePiece in pieces) {
                if (gamePiece.type == targetType || targetType == GamePiece.PieceType.BASE) {
                    gamePiece.gameObject.SetActive(true);
                } else {
                    gamePiece.gameObject.SetActive(false);
                }
            }
            writer = new StreamWriter(annotationPath + "/" + targetType.ToString() + "_Positive.txt", true);

        } else {
            foreach (GamePiece gamePiece in pieces) {
                if (gamePiece.type == targetType || targetType == GamePiece.PieceType.BASE) {
                    gamePiece.gameObject.SetActive(false);
                } else {
                    gamePiece.gameObject.SetActive(true);
                }
            }
            writer = new StreamWriter(annotationPath + "/" + targetType.ToString() + "_Negative.txt", true);
        }

        StartCoroutine(Generate());
    }

    private void PositiveData(string fileName) {
        if (AnnotatePositive(fileName) || targetType == GamePiece.PieceType.BASE) {
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
        SaveImage(capture, fileName, annotationPath + "/" + targetType.ToString() +"_Positive");
        Destroy(capture);
    }
    private void CaptureNegative(string fileName) {
        imageCount++;
        Texture2D capture = Capture();
        SaveImage(capture, fileName, annotationPath + "/" + targetType.ToString() + "_Negative");
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
        writer.WriteLine($"{targetType.ToString()}_Negative/{fileName}.jpg");
    }

    private bool AnnotatePositive(string fileName) {
        string line = "";
        int pieceCount = 0;
        int[] boundingRect = new int[4];
        foreach (GamePiece gamePiece in pieces) {
            if (gamePiece.CalculateBounds(out boundingRect)) {
                if (gamePiece.type == targetType || targetType == GamePiece.PieceType.BASE) {

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

        line = $"{targetType.ToString()}_Positive/{fileName}.jpg" + "  " + pieceCount + " " + line;

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

    private IEnumerator Generate() {
        imageCount = 0;

        float dR = (maxDistance - minDistance) / distanceSteps;
        float dAz = 2 * Mathf.PI / azimuthSteps;
        float dAl = (maxAltitude - minAltitude) / altitudeSteps;
        float dLi = (maxLightIntensity - minLightIntensity) / lightIntensitySteps;

        for (int l = 0; l <= lightIntensitySteps; l++) {
            for (int k = 0; k <= distanceSteps; k++) {
                for (int j = 0; j <= altitudeSteps; j++) {
                    for (int i = 0; i < azimuthSteps; i++) {
                        float az = dAz * i;
                        float al = (dAl * j + minAltitude) * Mathf.Deg2Rad;

                        float r = dR * k + minDistance;
                        float li = maxLightIntensity - dLi * l;

                        float x = r * Mathf.Cos(az) * Mathf.Sin(al);
                        float z = r * Mathf.Sin(az) * Mathf.Sin(al);
                        float y = r * Mathf.Cos(al);

                        Camera.main.transform.position = new Vector3(x, y, z);
                        Camera.main.transform.LookAt(transform.position, Vector3.up);

                        lightSource.intensity = li;
                        string fileName = targetType.ToString() + "_" + imageCount;

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
        writer.Close();
        //AssetDatabase.Refresh();
        isGenerating = false;
    }
}
