using ForceVision;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgprocModule;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace ForceVision
{
    public class MapDetector : MonoBehaviour
    {
        [SerializeField] List<Texture2D> mapTextures;
        [SerializeField] DetectorUtils.DETECTOR_TYPE detectorType = DetectorUtils.DETECTOR_TYPE.ORB;
        [SerializeField] int maxKeyPointCount = 100;
        [SerializeField] bool enableRatioTest = true;
        [SerializeField] CVWebCamController webCam;
        [SerializeField] Text statusText;
        [SerializeField] MeshRenderer targetMapRenderer;
        [SerializeField] ARMap map;

        List<DetectionProfile> detectionProfiles = new List<DetectionProfile>();
        DetectionProfile detectionProfile;
        int currentProfileIndex = 0;

        ARUtils.ARPose pose = new ARUtils.ARPose();
        bool mapFound = false;
        Mat imageMat;
        bool isStill = false;
        string statusString = "";

        void Start()
        {
            Input.gyro.enabled = true;
            foreach (Texture2D mapTexture in mapTextures)
            {
                detectionProfiles.Add(new DetectionProfile(mapTexture, maxKeyPointCount, detectorType));
            }
           
        }
        Thread detectorThread;
        bool isRunning = false;

        public void ToggleDetection()
        {
            TodoQueue.Clear();
            if (!isRunning)
            {
                map.gameObject.SetActive(false);
                if (detectorThread == null)
                {
                    detectorThread = new Thread(RunDetector);
                    detectorThread.Start();
                }
                isRunning = true;
            }
            else 
            {
                isRunning = false;
                statusString = "Map Detection Stopped";
                map.gameObject.SetActive(false);
            }
        }
        [SerializeField] int maxIterations = 1000;
        int iterations = 0;
        private void RunDetector()
        {
            Debug.Log("Thread Started");
            while (true)
            {
                if (isRunning)
                {
                    iterations++;
                    statusString = "Detecting Map...";
                    Detect();
                    if (mapFound == true || iterations > maxIterations)
                    {
                        isRunning = false;
                        iterations = 0;

                        if (mapFound)
                        {
                            statusString = "Map Detected ";
                            map.gameObject.SetActive(true);
                            SetMapType();
                        }
                        else 
                        {
                            statusString = "Map Detection Failed";
                            map.gameObject.SetActive(false);
                        }
                    }
                    Thread.Sleep(20);
                }
            }
        }

        Queue<Action> TodoQueue = new Queue<Action>();

        private void Update()
        {
                 
            if (TodoQueue.Count > 0)
            {
                TodoQueue.Dequeue().Invoke();
            }


            //Detect();
            //webCam.SetKeyPoints(detectedKeyPoints);
            if (mapFound)
            {
                GetDetectedPose();
                
            }
            MoveToTarget();

            statusText.text = statusString + "FPS " + (int)(1f / Time.unscaledDeltaTime);
        }

        Vector3 targetPosition;
        Vector3 targetScale;
        Quaternion targetRotation;

        void GetDetectedPose()
        {
            Matrix4x4 poseMat = detectionProfile.GetPoseMat(webCam.GetCameraMat());
            pose = ARUtils.ApplyTransform(map.transform, webCam.GetARCamera().transform, poseMat);
            targetPosition = pose.localPosition;
            targetScale = pose.localScale;
            targetRotation = pose.localRotation;
        }
        void SetMapType()
        {
            Texture2D mapTexture = detectionProfile.targetImage;
            map.SetTexture(mapTexture);
        }

        void MoveToTarget()
        {
            float distanceDelta = 10 * (map.transform.localPosition - targetPosition).magnitude * Time.deltaTime;
            map.transform.localPosition = Vector3.MoveTowards(map.transform.localPosition, targetPosition, distanceDelta);

            float scaleDelta = 10 * (map.transform.localScale - targetScale).magnitude * Time.deltaTime;
             map.transform.localScale = Vector3.MoveTowards(map.transform.localScale, pose.localScale, scaleDelta);

            float rotationDelta = 10 * Quaternion.Angle(map.transform.localRotation, targetRotation) * Time.deltaTime;
            map.transform.localRotation = Quaternion.RotateTowards(map.transform.localRotation, pose.localRotation, rotationDelta);
        }

        public void Detect()
        {
           
           
            detectionProfile = detectionProfiles[currentProfileIndex];
            mapFound = detectionProfile.Detect(webCam.GetGreyScaledMat());
          
            if (!mapFound) 
            {
                currentProfileIndex = (currentProfileIndex + 1) % detectionProfiles.Count;
            }

            Action UpdateARObject = () =>
            {
                targetMapRenderer.material.mainTexture = detectionProfile.mapKeyPointsTexture;
                webCam.SetKeyPoints(detectionProfile.GetDetectedKeyPoints());

            };
            TodoQueue.Enqueue(UpdateARObject);
        }
    }
}

class DetectionProfile
{
    public Texture2D targetImage;
    public Texture2D mapKeyPointsTexture;
    public int maxKeyPointCount;
    public DetectorUtils.DETECTOR_TYPE detectorType = DetectorUtils.DETECTOR_TYPE.BEBLID;

    private Pattern pattern = new Pattern();
    private MatOfKeyPoint detectedKeyPoints = new MatOfKeyPoint();
    private Mat detectedDescriptors = new Mat();
    private MatOfDMatch matches = new MatOfDMatch();
    private List<MatOfDMatch> knnMatches = new List<MatOfDMatch>();
    private DescriptorMatcher matcher;
    private Mat warpedImage = new Mat();
    private Mat roughPerspective = new Mat();
    private Mat refinedPerspective = new Mat();
    private Mat resultPerspective = new Mat();
    private MatOfPoint2f perspectiveTransformedPoints = new MatOfPoint2f();

    public DetectionProfile(Texture2D targetImage, int maxKeyPointCount, DetectorUtils.DETECTOR_TYPE detectorType)
    {
        this.maxKeyPointCount = maxKeyPointCount;
        this.detectorType = detectorType;
        this.targetImage = targetImage;

        matcher = DetectorUtils.CreateMatcher(detectorType);
        Mat imageMat = ImageUtils.GetMatFromTexture(targetImage);
        pattern.Create(imageMat, maxKeyPointCount, detectorType);
        DetectorUtils.Train(matcher, pattern);

        mapKeyPointsTexture = new Texture2D(pattern.imageMat.width(), pattern.imageMat.height(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(pattern.imageMat, mapKeyPointsTexture);
    }

    public bool Detect(Mat imageMat)
    {
        DetectorUtils.ExtractKeyPoints(imageMat, detectedKeyPoints, detectedDescriptors, maxKeyPointCount, detectorType);
        DetectorUtils.FindMatches(matcher, detectedDescriptors, matches);
        bool perspectiveFound = DetectorUtils.RefineWithPerspective(detectedKeyPoints, pattern.keyPoints, 3, matches, roughPerspective);

        if (perspectiveFound)
        {

            using (MatOfKeyPoint warpedKeyPoints = new MatOfKeyPoint())
            using (MatOfDMatch refinedMatches = new MatOfDMatch())
            {
                Imgproc.warpPerspective(
                                    imageMat,
                                    warpedImage,
                                    roughPerspective,
                                    pattern.size,
                                    Imgproc.WARP_INVERSE_MAP | Imgproc.INTER_CUBIC);

                DetectorUtils.ExtractKeyPoints(warpedImage, warpedKeyPoints, detectedDescriptors, maxKeyPointCount, detectorType);
                DetectorUtils.FindMatches(matcher, detectedDescriptors, refinedMatches);

                perspectiveFound = DetectorUtils.RefineWithPerspective(warpedKeyPoints, pattern.keyPoints, 3, refinedMatches, refinedPerspective);

                Core.gemm(roughPerspective, refinedPerspective, 1, new Mat(), 0, resultPerspective);
                Core.perspectiveTransform(pattern.points2d, perspectiveTransformedPoints, resultPerspective);
            }
        }
        return perspectiveFound;
    }
    public MatOfKeyPoint GetDetectedKeyPoints()
    {
        return detectedKeyPoints;
    }

    public Matrix4x4 GetPoseMat(Mat cameraMat) 
    { 
        return ARUtils.ComputePose(pattern, perspectiveTransformedPoints, cameraMat); ;
    }
  
}

