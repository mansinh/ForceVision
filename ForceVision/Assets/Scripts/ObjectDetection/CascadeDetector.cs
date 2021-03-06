using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForceVision {
    public class CascadeDetector : MonoBehaviour {
        public OpenCVForUnity.CoreModule.Rect[] detectedRects;
        public Image[] boundingBoxes;

        [SerializeField] RenderTexture renderTexture;
        [SerializeField] string classifierPath;

        [SerializeField] int minNeighbours = 8;
        [SerializeField] double scaleFactor = 1.05;
        [SerializeField] KeyCode detectKey = KeyCode.LeftAlt;
        [SerializeField] bool reloadClassifier = false;
        private CascadeClassifier classifier;
        void Start() {
            classifier = new CascadeClassifier();
            Debug.Log(classifier.load(classifierPath));
        }
        public void Detect() {
            
            Texture2D tex = RenderTextureToTexture2D(renderTexture);
            Mat mat = new Mat(renderTexture.height, renderTexture.width, CvType.CV_8UC4);
            Utils.texture2DToMat(tex, mat);
            MatOfRect rectMats = new MatOfRect();
            classifier.detectMultiScale(mat, rectMats, scaleFactor, minNeighbours, 0, new Size(24, 24), new Size(100, 100));
            detectedRects = rectMats.toArray();
            Debug.Log(detectedRects.Length);
            foreach (Image boundingBox in boundingBoxes) {
                boundingBox.gameObject.SetActive(false);
            }
            for (int i = 0; i < detectedRects.Length; i++) {
                OpenCVForUnity.CoreModule.Rect rect = detectedRects[i];
                if (boundingBoxes.Length > i) {
                    boundingBoxes[i].rectTransform.anchoredPosition = new Vector2(rect.x, -rect.y);
                    boundingBoxes[i].rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                    boundingBoxes[i].gameObject.SetActive(true);
                }
            }
        }
        private void Update() {
            if (Input.GetKeyUp(detectKey)) {
                Detect();
            }
            if (reloadClassifier) {
                reloadClassifier = false;
                classifier.load(classifierPath);
            }
        }

        Texture2D RenderTextureToTexture2D(RenderTexture rt) {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rt;
            tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
