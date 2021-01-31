using DlibDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace headTracking
{
    class FacialLandmarks
    {
        FrontalFaceDetector fd = Dlib.GetFrontalFaceDetector();
        ShapePredictor sp = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat");

        public FacialLandmarks()
        {
        }

        private double calculateEAR(Point2d[] eyesPoints)
        {
            var horizontalLineLength = Utility.euclideanDistance(eyesPoints[0].X, eyesPoints[0].Y, eyesPoints[3].X, eyesPoints[3].Y);
            double[] centerTop = Utility.midpoint(eyesPoints[1], eyesPoints[2]);
            double[] centerBottom = Utility.midpoint(eyesPoints[5], eyesPoints[4]);
            var verticalLineLength = Utility.euclideanDistance(centerTop[0], centerTop[1], centerBottom[0], centerBottom[1]);
            double leftEAR = verticalLineLength / horizontalLineLength;

            horizontalLineLength = Utility.euclideanDistance(eyesPoints[6].X, eyesPoints[6].Y, eyesPoints[9].X, eyesPoints[9].Y);
            centerTop = Utility.midpoint(eyesPoints[7], eyesPoints[8]);
            centerBottom = Utility.midpoint(eyesPoints[11], eyesPoints[10]);
            verticalLineLength = Utility.euclideanDistance(centerTop[0], centerTop[1], centerBottom[0], centerBottom[1]);
            double rightEAR = verticalLineLength / horizontalLineLength;

            return ((leftEAR + rightEAR) / 2);
        }
        public double[] detectFaceLandmarks(Array2D<RgbPixel> frame)
        {

            var img = frame;
            double[] headParams = new double[3];
            var faces = fd.Operator(img);

            foreach (var face in faces)
            {

                var shape = sp.Detect(img, face);

                var eyesPoints =
                        (from i in new int[] { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47 }
                         let pt = shape.GetPart((uint)i)
                         select new OpenCvSharp.Point2d(pt.X, pt.Y)).ToArray();
                
                headParams[2] = calculateEAR(eyesPoints);

                var model = Utility.GetFaceModel();

                var landmarks = new MatOfPoint2d(1, 6,
                    (from i in new int[] { 30, 8, 36, 45, 48, 54 }
                     let pt = shape.GetPart((uint)i)
                     select new OpenCvSharp.Point2d(pt.X, pt.Y)).ToArray());

                var cameraMatrix = Utility.GetCameraMatrix((int)img.Rect.Width, (int)img.Rect.Height);

                var coeffs = new MatOfDouble(4, 1);
                coeffs.SetTo(0);

                Mat rotation = new MatOfDouble();
                Mat translation = new MatOfDouble();

                Cv2.SolvePnP(model, landmarks, cameraMatrix, coeffs, rotation, translation);

               /* var euler = Utility.GetEulerMatrix(rotation);

                var yaw = 180 * euler.At<double>(0, 2) / Math.PI;
                var pitch = 180 * euler.At<double>(0, 1) / Math.PI;
                var roll = 180 * euler.At<double>(0, 0) / Math.PI;

                pitch = Math.Sign(pitch) * 180 - pitch;
               */
                var poseModel = new MatOfPoint3d(1, 1, new Point3d(0, 0, 1000));
                var poseProjection = new MatOfPoint2d();
                Cv2.ProjectPoints(poseModel, rotation, translation, cameraMatrix, coeffs, poseProjection);
                var landmark = landmarks.At<Point2d>(0);
                var p = poseProjection.At<Point2d>(0);
                headParams[0] = (double)p.X;
                headParams[1] = (double)p.Y;

            }

            return headParams;
        }
    }
}
