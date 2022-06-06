using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgprocModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForceVision
{
    public class PieceDetectorHough : MonoBehaviour
    {
        [SerializeField] double cannyUpperThreshold = 200;
        [SerializeField] double cannyLowerThreshold = 50;

        [SerializeField] CVWebCamController webCam;
        [SerializeField] double downSample = 2;
        [SerializeField] double minDist = 10;
        [SerializeField] double intersections = 150;
        [SerializeField] int minRadius = 5;
        [SerializeField] int maxRadius = 60;

        List<MatOfPoint> contours = new List<MatOfPoint>();
      
        Mat cannyMat = new Mat();
        Mat hierarchy = new Mat();
        Mat circlesMat = new Mat();
        Mat blurred = new Mat();

        List<Vector3> circles = new List<Vector3>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            cannyMat = webCam.GetCannyMat();
            circlesMat = new Mat();
            Imgproc.HoughCircles(cannyMat, circlesMat,
                Imgproc.HOUGH_GRADIENT,
                downSample, minDist, cannyUpperThreshold, intersections, minRadius, maxRadius);

            int count = circlesMat.cols();
           
             circles = new List<Vector3>();
            if (count>0)
            {
                Debug.Log(count + " " + circlesMat.cols() + " " + circlesMat.channels());
                for (int i = 0; i < count; i ++)
                {
                    double[] circlesArray = circlesMat.get(0, i);
                    float x = (float)circlesArray[0];
                    float y = (float)circlesArray[1];
                    float r = (float)circlesArray[2];
                    Debug.Log("Circle hough: " + x + " " + y + " " + r + " num "+(i/3+1));
                    circles.Add(new Vector3(x, y, r));
                }
               
            }
            webCam.SetCircles(circles);
        }
    }
}
