using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForceVision
{
    public class Model
    {
        public DetectorUtils.DETECTOR_TYPE detectorType;
        public int maxKeyPointCount;
        public bool enableRatioTest;

        public Texture2D targetKeypointsImage;

        private Pattern pattern = new Pattern();
    
        private MatOfDMatch matches = new MatOfDMatch();
        private List<MatOfDMatch> knnMatches = new List<MatOfDMatch>();
        private DescriptorMatcher matcher;

     
        public Model(DetectorUtils.DETECTOR_TYPE detectorType, Mat trainingImage, int maxKeyPointCount, bool enableRatioTest)
        {
            
            this.detectorType = detectorType;
            this.maxKeyPointCount = maxKeyPointCount;
            this.enableRatioTest = enableRatioTest;


            switch (detectorType)
            {
                case DetectorUtils.DETECTOR_TYPE.ORB: matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT); break;
                case DetectorUtils.DETECTOR_TYPE.SIFT: matcher = DescriptorMatcher.create(DescriptorMatcher.FLANNBASED); break;
            }
            pattern.Create(trainingImage, maxKeyPointCount, detectorType);
            Train();

            targetKeypointsImage = new Texture2D(pattern.imageMat.width(), pattern.imageMat.height(), TextureFormat.RGBA32, false);
            OpenCVForUnity.UnityUtils.Utils.matToTexture2D(pattern.imageMat, targetKeypointsImage);
        }
      
        private void Train()
        {
            matcher.clear();
            List<Mat> descriptors = new List<Mat>();
            descriptors.Add(pattern.descriptors);
            matcher.add(descriptors);
            matcher.train();
        }

      
        public void FindMatches(Mat detectedDescriptors, MatOfDMatch matches)
        {
            List<DMatch> matchesList = new List<DMatch>();
            if (enableRatioTest)
            {
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
            else
            {
                matches.fromList(matchesList);
                matcher.match(detectedDescriptors, matches);
            }
          
        }

        public Pattern GetPattern()
        {
            return pattern;
        }
    }
}
