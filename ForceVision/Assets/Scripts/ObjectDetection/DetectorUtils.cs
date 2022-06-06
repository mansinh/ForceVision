using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.Xfeatures2dModule;
using OpenCVForUnity.ObjdetectModule;
using System.Collections.Generic;
using OpenCVForUnity.Calib3dModule;

public class DetectorUtils
{
    public enum DETECTOR_TYPE
    {
        ORB,
        SIFT,
        BEBLID
    }
    static ORB orbDetector;
    static ORB orbExtractor;
    static SIFT siftDetector;
    static SIFT siftExtractor;
    static BEBLID beblidExtractor;

    public static void ExtractPatternOrb(Mat grayScaledImage, MatOfKeyPoint keyPoints, Mat descriptors, int maxFeatureCount)
    {
        if (orbDetector == null)
        {
            orbDetector = ORB.create();
            orbDetector.setMaxFeatures(maxFeatureCount);
        }
        if (orbExtractor == null)
        {
            orbExtractor = ORB.create();
            orbExtractor.setMaxFeatures(maxFeatureCount);
        }

        orbDetector.detect(grayScaledImage, keyPoints);
        if (keyPoints.total() == 0) return;

        orbExtractor.compute(grayScaledImage, keyPoints, descriptors);
        if (keyPoints.total() == 0) return;
    }
    public static void ExtractPatternBEBLID(Mat grayScaledImage, MatOfKeyPoint keyPoints, Mat descriptors, int maxFeatureCount)
    {
        if (orbDetector == null)
        {
            orbDetector = ORB.create();
            orbDetector.setMaxFeatures(maxFeatureCount);
        }

        if (beblidExtractor == null)
            beblidExtractor = BEBLID.create(0.75f);

        orbDetector.detect(grayScaledImage, keyPoints);
        if (keyPoints.total() == 0) return;

        beblidExtractor.compute(grayScaledImage, keyPoints, descriptors);
        if (keyPoints.total() == 0) return;
    }
    public static void ExtractPatternSift(Mat grayScaledImage, MatOfKeyPoint keyPoints, Mat descriptors)
    {
        if (siftDetector == null)
            siftDetector = SIFT.create();
        if (siftExtractor == null)
            siftExtractor = SIFT.create();

        siftDetector.detect(grayScaledImage, keyPoints);
        if (keyPoints.total() == 0) return;

        siftExtractor.compute(grayScaledImage, keyPoints, descriptors);
        if (keyPoints.total() == 0) return;
    }
    public static void ExtractHOG(Mat grayScaledImage, MatOfPoint locations, MatOfFloat descriptors)
    {
        Size cellSize = new Size(8, 8);
        Size blockSize = new Size(2 * cellSize.width, 2 * cellSize.height);
        Size blockStride = cellSize;

        int windowHeight = (int)((int)(grayScaledImage.size(0) / cellSize.height) * cellSize.height);
        int windowWidth = (int)((int)(grayScaledImage.size(1) / cellSize.width) * cellSize.width);
        Size windowSize = new Size(windowWidth, windowHeight);
        int binCount = 9;

        HOGDescriptor hog = new HOGDescriptor(windowSize, blockSize, blockStride, cellSize, binCount);

        Size padding = new Size(grayScaledImage.size(0) - windowWidth, grayScaledImage.size(1) - windowHeight) / 2;
        hog.compute(grayScaledImage, descriptors, blockStride, padding, locations);
    }
    public static DescriptorMatcher CreateMatcher(DETECTOR_TYPE detectorType)
    {
        switch (detectorType)
        {
            case DETECTOR_TYPE.BEBLID: return DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMING);
            case DETECTOR_TYPE.ORB: return DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMING);
            case DETECTOR_TYPE.SIFT: return DescriptorMatcher.create(DescriptorMatcher.FLANNBASED);
        }
        return null;
    }
    public static void ExtractKeyPoints(Mat imageMat, MatOfKeyPoint keyPoints, Mat descriptors, int maxKeyPointCount, DETECTOR_TYPE detectorType)
    {
        switch (detectorType)
        {
            case DETECTOR_TYPE.ORB: ExtractPatternOrb(imageMat, keyPoints, descriptors, maxKeyPointCount); break;
            case DETECTOR_TYPE.SIFT: ExtractPatternSift(imageMat, keyPoints, descriptors); break;
            case DETECTOR_TYPE.BEBLID: ExtractPatternBEBLID(imageMat, keyPoints, descriptors, maxKeyPointCount); break;
        }
    }
    public static void TrainMatcher(DescriptorMatcher matcher, Pattern pattern)
    {
        matcher.clear();
        List<Mat> descriptors = new List<Mat>();
        descriptors.Add(pattern.descriptors);
        matcher.add(descriptors);
        matcher.train();
    }
    public static bool RefineWithPerspective(MatOfKeyPoint detectedKeyPoints, MatOfKeyPoint featuresKeyPoints, int threshold, MatOfDMatch matches, Mat perspective)
    {
        int minMatches = 8;
        List<DMatch> matchesList = matches.toList();
        if (matchesList.Count < minMatches)
        {
            return false;
        }
        List<KeyPoint> detectedKeyPointsList = detectedKeyPoints.toList();
        List<KeyPoint> featuresKeyPointsList = featuresKeyPoints.toList();

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
            0.97).copyTo(perspective);

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
    public static void FindMatches(DescriptorMatcher matcher, Mat detectedDescriptors, MatOfDMatch matches)
    {
        List<DMatch> matchesList = new List<DMatch>();
        List<MatOfDMatch> knnMatches = new List<MatOfDMatch>();

        float minRatio = 0.75f;
        matcher.knnMatch(detectedDescriptors, knnMatches, 2);
        for (int i = 0; i < knnMatches.Count; i++)
        {
            List<DMatch> knnMatchesList = knnMatches[i].toList();
            DMatch bestMatch = knnMatchesList[0];
            DMatch nextBestMatch = knnMatchesList[1];

            if (bestMatch.distance < minRatio * nextBestMatch.distance)
            {
                matchesList.Add(bestMatch);
            }
        }
        matches.fromList(matchesList);
    }
    public static void Train(DescriptorMatcher matcher, Pattern pattern)
    {
        matcher.clear();
        List<Mat> descriptors = new List<Mat>();
      
        descriptors.Add(pattern.descriptors);
        matcher.add(descriptors);
        matcher.train();
    }
}

