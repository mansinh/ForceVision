using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForceVision
{
    public class PieceDetectorController : MonoBehaviour
    {
        [SerializeField] CVWebCamController webCam;
        [SerializeField] Texture2D[] trainingImages;
        [SerializeField] MeshRenderer targetKeyPointsRenderer;
        [SerializeField] DetectorUtils.DETECTOR_TYPE detectorType = DetectorUtils.DETECTOR_TYPE.ORB;
        [SerializeField] int maxKeyPointCount = 100;
        [SerializeField] bool enableRatioTest = true;
        [SerializeField] bool enablePerspectiveRefinement = true;
        [SerializeField] int perspectiveThreshold = 3;
        [SerializeField] Text status;
        [SerializeField] bool useCanny;
        [SerializeField] int lowerThreshold, upperThreshold;

        private MatOfKeyPoint detectedKeyPoints = new MatOfKeyPoint();
        private Mat detectedDescriptors = new Mat();


        int showTargeIndex = 0;
        private List<Model> models = new List<Model>();

        private void Start()
        {
            foreach (Texture2D trainingImage in trainingImages)
            {
               Mat imageMat = ImageUtils.GetMatFromTexture(trainingImage);
                if (useCanny)
                {
                    imageMat = ImageUtils.GetCannyFromTex(trainingImage, lowerThreshold, upperThreshold);
                }
              
                models.Add(new Model(detectorType, imageMat, maxKeyPointCount, enableRatioTest));
            }
            //InvokeRepeating("SwitchTargetImage", 1, 3);  
        }
        private void SwitchTargetImage()
        {
            targetKeyPointsRenderer.material.mainTexture = models[showTargeIndex].targetKeypointsImage;
            webCam.SetKeyPoints(detectedKeyPoints);
            showTargeIndex++;
            if (showTargeIndex >= models.Count)
            {
                showTargeIndex = 0;
            }
        }
        private void Update()
        {
            if (useCanny)
            {
                ExtractKeyPoints(webCam.GetCannyMat(), detectedKeyPoints, detectedDescriptors);
            }
            else 
            {
                ExtractKeyPoints(webCam.GetGreyScaledMat(), detectedKeyPoints, detectedDescriptors);
            }
            int detectedCount = 0;
            foreach (Model model in models)
            {
                if (Detect(model)) 
                {
                    detectedCount++;
                    targetKeyPointsRenderer.material.mainTexture = model.targetKeypointsImage;
                    webCam.SetKeyPoints(detectedKeyPoints);
                }  
            }
            status.text = detectedCount + " found";
        }
        public bool Detect(Model model)
        {
            Mat warpedImage = new Mat();
            Mat roughPerspective = new Mat();
            Mat refinedPerspective = new Mat();
            Mat resultPerspective = new Mat();
            MatOfPoint2f points2d = new MatOfPoint2f();
            Mat warpedDescriptors = new Mat();
            MatOfDMatch matches = new MatOfDMatch(); 

            model.FindMatches(detectedDescriptors, matches);
            Pattern pattern = model.GetPattern();

            bool perspectiveFound = RefineWithPerspective(detectedKeyPoints, pattern.keyPoints, perspectiveThreshold, matches, roughPerspective);

            if (perspectiveFound)
            {
                if (enablePerspectiveRefinement)
                {
                    Imgproc.warpPerspective(
                        webCam.GetGreyScaledMat(),
                        warpedImage,
                        roughPerspective,
                        pattern.size,
                        Imgproc.WARP_INVERSE_MAP | Imgproc.INTER_CUBIC);

                    using (MatOfKeyPoint warpedKeyPoints = new MatOfKeyPoint())
                    using (MatOfDMatch refinedMatches = new MatOfDMatch())
                    {

                        ExtractKeyPoints(warpedImage, warpedKeyPoints, warpedDescriptors);
                        model.FindMatches(warpedDescriptors,refinedMatches);

                        perspectiveFound = RefineWithPerspective(warpedKeyPoints, pattern.keyPoints, perspectiveThreshold, refinedMatches, refinedPerspective);
                    }
                    Core.gemm(roughPerspective, refinedPerspective, 1, new Mat(), 0, resultPerspective);

                }

                else
                {
                    resultPerspective = roughPerspective;
                    Core.perspectiveTransform(pattern.points2d, points2d, roughPerspective);
                }
            }
            return perspectiveFound;
        }
  
        private void ExtractKeyPoints(Mat imageMat, MatOfKeyPoint keyPoints, Mat descriptors)
        {
            switch (detectorType)
            {
                case DetectorUtils.DETECTOR_TYPE.ORB: DetectorUtils.ExtractPatternOrb(imageMat, keyPoints, descriptors, maxKeyPointCount); break;
                case DetectorUtils.DETECTOR_TYPE.SIFT: DetectorUtils.ExtractPatternSift(imageMat, keyPoints, descriptors); break;
            }
        }
        private static bool RefineWithPerspective(MatOfKeyPoint detectedKeyPoints, MatOfKeyPoint featuresKeyPoints, int threshold, MatOfDMatch matches, Mat perspective)
        {
            int minMatches = 8;
            List<KeyPoint> detectedKeyPointsList = detectedKeyPoints.toList();
            List<KeyPoint> featuresKeyPointsList = featuresKeyPoints.toList();
            List<DMatch> matchesList = matches.toList();

            if (matchesList.Count < minMatches)
            {
                return false;
            }
            List<Point> sourcePointsList = new List<Point>(matchesList.Count);
            List<Point> destinationPointsList = new List<Point>(matchesList.Count);
            for (int i = 0; i < matchesList.Count; i++)
            {
                sourcePointsList.Add(featuresKeyPointsList[matchesList[i].trainIdx].pt);
                destinationPointsList.Add(detectedKeyPointsList[matchesList[i].queryIdx].pt);
            }
            MatOfPoint2f sourcePoints = new MatOfPoint2f();
            MatOfPoint2f destinationPoints = new MatOfPoint2f();
            MatOfByte mask = new MatOfByte(new byte[sourcePointsList.Count]);
            sourcePoints.fromList(sourcePointsList);
            destinationPoints.fromList(destinationPointsList);

            Calib3d.findHomography(sourcePoints,
                destinationPoints,
                Calib3d.FM_RANSAC,
                threshold,
                mask,
                2000,
                0.955).copyTo(perspective);

            if (perspective.rows() != 3 || perspective.cols() != 3) return false;

            List<byte> maskList = mask.toList();

            List<DMatch> inliers = new List<DMatch>();
            for (int i = 0; i < maskList.Count; i++)
            {
                if (maskList[i] == 1)
                    inliers.Add(matchesList[i]);
            }
            matches.fromList(inliers);
            return matchesList.Count > minMatches;


        }
    }
  
} 