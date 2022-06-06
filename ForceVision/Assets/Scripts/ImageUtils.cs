
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

namespace ForceVision
{
    public class ImageUtils
    {
        public static void GetGreyScaledMat(Mat image, Mat greyScaledImage)
        {
            int channel = image.channels();
            switch (channel)
            {
                case 1: image.copyTo(greyScaledImage); break;
                case 3: Imgproc.cvtColor(image, greyScaledImage, Imgproc.COLOR_RGB2GRAY); break;
                case 4: Imgproc.cvtColor(image, greyScaledImage, Imgproc.COLOR_RGBA2GRAY); break;
            }
        }
        public static Mat GetCannyFromTex(Texture2D texture, double lowerThreshold, double upperThreshold)
        {
            Mat imageMat = GetMatFromTexture(texture);
            imageMat = GetCannyFromMat(imageMat, lowerThreshold, upperThreshold);
            return imageMat;
        }
        public static Mat GetCannyFromMat(Mat imageMat, double lowerThreshold, double upperThreshold)
        {
            Mat cannyMat = new Mat();
            GetGreyScaledMat(imageMat, cannyMat);
            Imgproc.medianBlur(cannyMat, cannyMat, 100);
            Imgproc.Canny(cannyMat, cannyMat, lowerThreshold, upperThreshold);
            return cannyMat;
        }
        public static Texture2D GetCannyTexFromTex(Texture2D texture, double lowerThreshold, double upperThreshold)
        {
            Mat cannyMat = GetCannyFromTex(texture, lowerThreshold, upperThreshold);
            Texture2D cannyTexture = new Texture2D(cannyMat.width(), cannyMat.height(), TextureFormat.RGBA32, false);
            OpenCVForUnity.UnityUtils.Utils.matToTexture2D(cannyMat, cannyTexture);
            return cannyTexture;
        }
        public static Mat GetMatFromTexture(Texture2D texture)
        {
            
            Mat imageMat= new Mat(texture.height, texture.width, CvType.CV_8UC3);
            Utils.texture2DToMat(texture, imageMat);
            return imageMat;
        }
    }
}
