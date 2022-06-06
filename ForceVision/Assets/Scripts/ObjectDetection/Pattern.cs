using ForceVision;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgprocModule;
using System.Collections.Generic;
public class Pattern 
{
    public Size size;
    public Mat imageMat;
    public Mat keyPointsMat;
    public Mat greyScaledMat = new Mat();
    public MatOfKeyPoint keyPoints = new MatOfKeyPoint();
    public Mat descriptors = new Mat();
    public MatOfPoint2f points2d = new MatOfPoint2f();
    public MatOfPoint3f points3d = new MatOfPoint3f();
    public int maxKeyPointCount;

    public static int USE_ORB = 0;
    public static int USE_SIFT = 1;

    public void Create(Mat imageMat, int maxKeyPointCount, DetectorUtils.DETECTOR_TYPE detectorType)
    {
        this.imageMat = imageMat;
        this.maxKeyPointCount = maxKeyPointCount;
 
        size = new Size(imageMat.cols(), imageMat.rows());
        ImageUtils.GetGreyScaledMat(imageMat, greyScaledMat);
        CLAHE clahe = Imgproc.createCLAHE(2.0, new Size(8, 8));
        //clahe.apply(greyScaledMat, greyScaledMat);

        //Contours
        List<Point> points2dList = new List<Point>(4);
        List<Point3> points3dList = new List<Point3>(4);

        points2dList.Add(new Point(0, 0));
        points2dList.Add(new Point(size.width, 0));
        points2dList.Add(new Point(size.width, size.height));
        points2dList.Add(new Point(0, size.height));
        points2d.fromList(points2dList);

        points3dList.Add(new Point3(-0.5f, -0.5f, 0));
        points3dList.Add(new Point3(+0.5f, -0.5f, 0));
        points3dList.Add(new Point3(+0.5f, +0.5f, 0));
        points3dList.Add(new Point3(-0.5f, +0.5f, 0));
        points3d.fromList(points3dList);

        DetectorUtils.ExtractKeyPoints(imageMat, keyPoints, descriptors, maxKeyPointCount,detectorType);
        Features2d.drawKeypoints(imageMat, keyPoints, imageMat, Scalar.all(-1));
    }
}
