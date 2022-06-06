using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceVision
{
    public class ARUtils
    {
        public static Matrix4x4 ComputePose(Pattern pattern, MatOfPoint2f perspectiveTransformedPoints, Mat cameraMat)
        {
            Mat rotationVector = new Mat();
            Mat translationVector = new Mat();
            Mat rotationMat = new Mat(3, 3, CvType.CV_64FC1);
            MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);
            Matrix4x4 pose = new Matrix4x4();


            Calib3d.solvePnP(pattern.points3d, perspectiveTransformedPoints,
                cameraMat, distCoeffs,
                rotationVector, translationVector);

            rotationVector.convertTo(rotationVector, CvType.CV_32F);
            translationVector.convertTo(translationVector, CvType.CV_32F);

            Calib3d.Rodrigues(rotationVector, rotationMat);

            //Left-handed pose transform matrix
            //[rx(0,0),upx(0,1),fwdx(0,2),tx(0,0)]
            //[ry(1,0),upy(1,1),fwdy(1,2),ty(1,0)]
            //[rz(2,0),upz(2,1),fwdz(2,2),tz(2,0)]
            //[0      ,0      ,0      ,1      ]

            float rx = (float)rotationMat.get(0, 0)[0];
            float ry = (float)rotationMat.get(1, 0)[0];
            float rz = (float)rotationMat.get(2, 0)[0];
            float upX = (float)rotationMat.get(0, 1)[0];
            float upY = (float)rotationMat.get(1, 1)[0];
            float upZ = (float)rotationMat.get(2, 1)[0];
            float fwdX = (float)rotationMat.get(0, 2)[0];
            float fwdY = (float)rotationMat.get(1, 2)[0];
            float fwdZ = (float)rotationMat.get(2, 2)[0];

            float translateX = (float)translationVector.get(0, 0)[0];
            float translateY = (float)translationVector.get(1, 0)[0];
            float translateZ = (float)translationVector.get(2, 0)[0];

            pose.SetRow(0, new Vector4(rx, upX, fwdX, translateX));
            pose.SetRow(1, new Vector4(ry, upY, fwdY, translateY));
            pose.SetRow(2, new Vector4(rz, upZ, fwdZ, translateZ));
            pose.SetRow(3, new Vector4(0, 0, 0, 1));

            rotationVector.Dispose();
            rotationMat.Dispose();
            translationVector.Dispose();
            distCoeffs.Dispose();

            return pose;
        }
        static Matrix4x4 invertY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
        static Matrix4x4 invertZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

        public static ARPose ApplyTransform(Transform objectTransform, Transform cameraTransform, Matrix4x4 poseMat)
        {
            poseMat = invertY * poseMat * invertY;
            poseMat = poseMat * invertY * invertZ;
            poseMat = cameraTransform.localToWorldMatrix * poseMat;

            Vector3 translate = new Vector3(poseMat.m03, poseMat.m13, poseMat.m23);
            Quaternion rotation = Quaternion.LookRotation(
                new Vector3(poseMat.m02, poseMat.m12, poseMat.m22),//forward vector
                new Vector3(poseMat.m01, poseMat.m11, poseMat.m21));//up vector

           // Debug.Log("pose " + new Vector3(poseMat.m02, poseMat.m12, poseMat.m22) + " " +
                // new Vector3(poseMat.m01, poseMat.m11, poseMat.m21));

            Vector3 scale = new Vector3(poseMat.GetRow(0).magnitude,
                                        poseMat.GetRow(1).magnitude,
                                        poseMat.GetRow(2).magnitude);

            ARPose pose = new ARPose();
            pose.localPosition = translate;
            pose.localRotation = rotation;
            pose.localScale = scale;

            return pose;
        }
        public class ARPose
        {
            public Vector3 localPosition;
            public Vector3 localScale;
            public Quaternion localRotation;
        }
    }

}