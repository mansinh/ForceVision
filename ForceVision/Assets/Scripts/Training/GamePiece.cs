using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//

[ExecuteAlways]
public class GamePiece : MonoBehaviour
{
    public enum PieceType { 
        TROOPER,
        PROBE_DROID,
        AT_ST
    }
    public PieceType type;
    [SerializeField] private BoxCollider[] boundingBoxes;
    [SerializeField] private Image boundingBoxImage; 
    [SerializeField] private Text topLeft, size;
    public Vector3[] boundingScreenCorners;
    // Update is called once per frame
    void LateUpdate()
    {
        if (Application.isEditor)
        {
            int[] boundingRect = new int[4];
            CalculateBounds(out boundingRect);
        }     
    }

    public bool CalculateBounds(out int[] boundingRect) 
    {
        float left = Mathf.Infinity;
        float top = -Mathf.Infinity;
        float right = -Mathf.Infinity;
        float bottom = Mathf.Infinity;
        for (int i = 0; i < boundingBoxes.Length; i++)
        {
            BoxCollider boundingBox = boundingBoxes[i];
            Vector3[] screenCorners = GetScreenCorners(boundingBox);

            for (int j = 0; j < screenCorners.Length; j++)
            {
                if (screenCorners[j].x < left) left = screenCorners[j].x;
                if (screenCorners[j].y > top) top = screenCorners[j].y;

                if (screenCorners[j].x > right) right = screenCorners[j].x;
                if (screenCorners[j].y < bottom) bottom = screenCorners[j].y;
            }
        }
       
        Vector3 center = new Vector3(left + right, bottom + top, 0) / 2;
        float width = right - left;
        float height = top - bottom;
        boundingBoxImage.rectTransform.anchoredPosition = center;
        boundingBoxImage.rectTransform.sizeDelta = new Vector2(width, height);

        

        boundingRect = new int[] { (int)left, (int)(Screen.height - top), (int)width, (int)height };
        
        topLeft.rectTransform.anchoredPosition = new Vector2(left, top);
        topLeft.text = "["+boundingRect[LEFT] + ", " + boundingRect[TOP]+"]" ;

        size.rectTransform.anchoredPosition = new Vector2(right, bottom);
        size.text = boundingRect[WIDTH] + "x" + boundingRect[HEIGHT];

        return IsInCameraFrame(boundingRect);
    }

    public static int LEFT = 0, TOP = 1, WIDTH = 2, HEIGHT = 3;

    private bool IsInCameraFrame(int[] boundingRect) {
        if (boundingRect[0] < 0) return false;
        if (boundingRect[1] < 0) return false;
        if (boundingRect[0] + boundingRect[2] > Screen.width) return false;
        if (boundingRect[1] + boundingRect[3] > Screen.height) return false;
        if (boundingRect[0] > Screen.width) return false;
        if (boundingRect[1] > Screen.height) return false;
        return true;
    }

    Vector3[] GetCorners(BoxCollider boundingBox) {
        Vector3[] corners = new Vector3[8];
        float x = boundingBox.size.x/2;
        float y = boundingBox.size.y/2;
        float z = boundingBox.size.z/2;
        Vector3 position = boundingBox.transform.position;
        corners[0] = boundingBox.transform.TransformPoint(new Vector3(x, y, z));
        corners[1] = boundingBox.transform.TransformPoint(new Vector3(x, y, -z));
        corners[2] = boundingBox.transform.TransformPoint(new Vector3(x, -y, z));
        corners[3] = boundingBox.transform.TransformPoint(new Vector3(-x, y, z));
        corners[4] = boundingBox.transform.TransformPoint(-new Vector3(x, y, z));
        corners[5] = boundingBox.transform.TransformPoint(-new Vector3(x, y, -z));
        corners[6] = boundingBox.transform.TransformPoint(-new Vector3(x, -y, z));
        corners[7] = boundingBox.transform.TransformPoint(-new Vector3(-x, y, z));
  
        return corners;
    }
    Vector3[] GetScreenCorners(BoxCollider boundingBox)
    {
        return GetScreenCorners(GetCorners(boundingBox));
    }
    Vector3[] GetScreenCorners(Vector3[] worldCorners)
    {
        Vector3[] screenCorners = new Vector3[worldCorners.Length];
        for (int i = 0; i < worldCorners.Length; i++)
        {
            screenCorners[i] = Camera.main.WorldToScreenPoint(worldCorners[i], Camera.main.stereoActiveEye);
        }
        return screenCorners;
    }
}
