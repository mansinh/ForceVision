using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForceVision
{
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CVWebCamController:MonoBehaviour 
    {
        [SerializeField] bool showCanny = true, useCLAHE = true;
        [SerializeField] Camera ARCamera;
        public double upperThreshold = 200;
        public double lowerThreshold = 50;

        protected Mat greyScaledMat;
        protected Mat cannyMat;
        protected Mat cameraMat;
        protected MatOfDMatch matches;
        protected float imageScale = 1;
        protected MatOfKeyPoint keyPoints = new MatOfKeyPoint();
        protected List<MatOfPoint> contours = new List<MatOfPoint>();
        protected List<Vector3> circles = new List<Vector3>();
        protected MatOfRect boundingBoxes = new MatOfRect();


        private int width, height;

        public Mat GetGreyScaledMat() { return greyScaledMat; }
        public Mat GetCannyMat() { return cannyMat; }
        public Mat GetCameraMat() { return cameraMat; }
        public float GetImageScale() { return imageScale; }
        public Camera GetARCamera() { return ARCamera; }
        public void SetCannyMat(Mat cannyMat) { this.cannyMat = cannyMat; }
        public void SetKeyPoints(MatOfKeyPoint keyPoints) { this.keyPoints = keyPoints; }
        public void SetContours(List<MatOfPoint> contours) { this.contours = contours; }
        public void SetCircles(List<Vector3> circles) { this.circles = circles; }
        public void SetBoundingBoxes(MatOfRect boundingBoxes) { this.boundingBoxes = boundingBoxes; }
        public void SetMatches(MatOfDMatch matches)
        {
            this.matches = matches;
        }


        private WebCamTextureToMatHelper helper;
        private MeshRenderer webCamRenderer;
        private Texture2D webCamTexture;
      
        public void OnHelperInit()
        {
            width = helper.GetMat().width();
            height = helper.GetMat().height();
            InitRenderer();
            InitCamera();
        }
        private void Awake()
        {
            webCamRenderer = GetComponent<MeshRenderer>();
            helper = GetComponent<WebCamTextureToMatHelper>();
            helper.Initialize();
        }
        private void InitRenderer()
        {
            //helper.flipHorizontal = helper.GetWebCamDevice().isFrontFacing;
            webCamTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            webCamRenderer.material.mainTexture = webCamTexture;
            gameObject.transform.localScale = new Vector3(width, height, 1);

            int rows = helper.GetMat().rows();
            int cols = helper.GetMat().cols();
            greyScaledMat = new Mat(rows, cols, CvType.CV_8UC1);
            cannyMat = new Mat(rows, cols, CvType.CV_8UC1);
        }
        private void InitCamera()
        {
           
            Camera.main.orthographicSize =height / 2;

            double centerX = width / 2;
            double centerY = height / 2;

            //[width  , 0     ,0]
            //[0      ,width  ,0]
            //[centerX,centerY,1]

            cameraMat = Mat.zeros(3, 3, CvType.CV_64FC1);
            cameraMat.put(0, 0, width);
            cameraMat.put(0,2,centerX);
            cameraMat.put(1,1, width);
            cameraMat.put(1, 2,centerY);
            cameraMat.put(2, 2, 1);

            Size imageSize = new Size(width, height);
            double[] fieldOfViewX = new double[1];
            double[] fieldOfViewY = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0,0);
            double[] aspectRatio = new double[1];
           
            Calib3d.calibrationMatrixValues(
                cameraMat, imageSize,0,0,
                fieldOfViewX,fieldOfViewY,
                focalLength, principalPoint,
                aspectRatio);
            ARCamera.fieldOfView = (float)fieldOfViewY[0];
        }

  

        public void Update()
        {
            if (helper.IsPlaying() && helper.DidUpdateThisFrame())
            {
                Mat webCamMat = helper.GetMat();
               
                if (useCLAHE) 
                {
                    CLAHE clahe = Imgproc.createCLAHE(2.0, new Size(8, 8));
                    clahe.apply(greyScaledMat, greyScaledMat);
                }
                ImageUtils.GetGreyScaledMat(webCamMat,greyScaledMat);

                //Features2d.drawKeypoints(webCamMat, keyPoints, webCamMat, Scalar.all(-1));
                    
                
                /*
                OpenCVForUnity.CoreModule.Rect[] boundingRects = boundingBoxes.toArray();
                for (int i = 0; i < boundingRects.Length; i++)
                {
                    OpenCVForUnity.CoreModule.Rect rect = boundingRects[i];
                    Point point1 = new Point(rect.x, rect.y);
                    Point point2 = new Point(rect.width, rect.height) + point1;
                    Imgproc.rectangle(webCamMat, point1, point2, Scalar.all(255), 10);
                }*/

                OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(webCamMat, webCamTexture);
               
            }
        }
        public void OnHelperDisposed()
        {
            if (greyScaledMat != null) greyScaledMat.Dispose();
        }
        public void OnHelperError(WebCamTextureToMatHelper.ErrorCode error)
        {
            Debug.Log(error);
        }
    }
}
