using System.Collections.Generic;
using UnityEngine;

namespace Kalidokit
{
    public class FaceSolver
    {
        public static FaceStruct Solve(Vector4[] landmarks, bool smoothBlink = true)
        {
            HeadStruct head = CalcHead(landmarks);
            EyeStruct eyes = CalcEyes(landmarks);
            if (smoothBlink)
            {
                eyes = EyeUtils.StabilizeBlink(eyes, head.y);
            }
            FaceStruct face = new FaceStruct
            {
                Head = head,
                Mouth = CalcMouth(landmarks),
                Eye = eyes,
                // Pupil = CalcPupils(landmarks),
                Brow = CalcBrow(landmarks),
            };
            
            return face;
        }
        private static HeadStruct CalcHead(Vector4[] landmarks)
        {
            EulerPlane plane = new EulerPlane(landmarks[21], landmarks[251], landmarks[397], landmarks[172]);
            Vector3[] vector = plane.GetVector();

            Vector3 rotation = VectorUtils.RollPitchYaw(vector[0], vector[1], vector[2]);
            Vector3 midPoint = Vector3.Lerp(vector[0], vector[1], 0.5f);
            float width = Vector3.Distance(vector[0], vector[1]),
                  height = Vector3.Distance(midPoint, vector[2]);
            rotation.x *= -1;
            rotation.z *= -1;

            return new HeadStruct
            {
                x = rotation.x * Mathf.PI,
                y = rotation.y * Mathf.PI,
                z = rotation.z * Mathf.PI,
                degrees = rotation * 180,
                width = width,
                height = height,
                position = Vector3.Lerp(midPoint, vector[2], 0.5f),
                normalized = rotation,
            };
        }
        private static MouthStruct CalcMouth(Vector4[] landmarks)
        {
            Vector3 eyeInnerCornerL = VectorUtils.GenerateVector3(landmarks[133]);
            Vector3 eyeInnerCornerR = VectorUtils.GenerateVector3(landmarks[362]);
            Vector3 eyeOuterCornerL = VectorUtils.GenerateVector3(landmarks[130]);
            Vector3 eyeOuterCornerR = VectorUtils.GenerateVector3(landmarks[263]);

            float eyeInnerDistance = Vector3.Distance(eyeInnerCornerL, eyeInnerCornerR);
            float eyeOuterDistance = Vector3.Distance(eyeOuterCornerL, eyeOuterCornerR);

            Vector3 upperInnerLip = VectorUtils.GenerateVector3(landmarks[13]);
            Vector3 lowerInnerLip = VectorUtils.GenerateVector3(landmarks[14]);
            Vector3 mouthCornerLeft = VectorUtils.GenerateVector3(landmarks[61]);
            Vector3 mouthCornerRight = VectorUtils.GenerateVector3(landmarks[291]);

            float mouthOpen = Vector3.Distance(upperInnerLip,lowerInnerLip);
            float mouthWidth = Vector3.Distance(mouthCornerLeft,mouthCornerRight);

            float ratioY = mouthOpen / eyeInnerDistance;
            float ratioX = mouthWidth / eyeOuterDistance;

            ratioY = Helper.Remap(ratioY, 0.15f, 0.7f);

            // normalize and scale mouth shape
            ratioX = Helper.Remap(ratioX, 0.45f, 0.9f);
            ratioX = (ratioX - 0.3f) * 2;

            float mouthX = ratioX;
            float mouthY = Helper.Remap(mouthOpen / eyeInnerDistance, 0.17f, 0.5f);

            float ratioI = Helper.Clamp(Helper.Remap(mouthX, 0, 1) * 2 * Helper.Remap(mouthY, 0.2f, 0.7f), 0, 1);
            float ratioA = mouthY * 0.4f + mouthY * (1 - ratioI) * 0.6f;
            float ratioU = mouthY * Helper.Remap(1 - ratioI, 0, 0.3f) * 0.1f;
            float ratioE = Helper.Remap(ratioU, 0.2f, 1) * (1 - ratioI) * 0.3f;
            float ratioO = (1 - ratioI) * Helper.Remap(mouthY, 0.3f, 1) * 0.4f;

            return new MouthStruct
            {
                x = ratioX,
                y = ratioY,
                shape = new MouthShape
                {
                    A = ratioA,
                    E = ratioE,
                    I = ratioI,
                    O = ratioO,
                    U = ratioU,
                }
            };
        }
        private static EyeStruct CalcEyes(Vector4[] landmarks, float high = 0.85f, float low = 0.55f)
        {
            //open [0,1]
            EyeOpenStruct leftEyeLid = EyeUtils.GetEyeOpen(landmarks, "left");
            EyeOpenStruct rightEyeLid = EyeUtils.GetEyeOpen(landmarks, "right");
            return new EyeStruct {
                l = leftEyeLid.norm,
                r = rightEyeLid.norm,
            };
        }
        private static PupilsStruct CalcPupils(Vector4[] landmarks)
        {
            if (landmarks.Length != 478)
            {
                return new PupilsStruct { x = 0, y = 0 };
            }
            else
            {
                //track pupils using left eye
                PupilPosStruct pupilL = EyeUtils.GetPupilPos(landmarks, "left");
                PupilPosStruct pupilR = EyeUtils.GetPupilPos(landmarks, "right");

                return new PupilsStruct
                {
                    x = (pupilL.x + pupilR.x) * 0.5f,
                    y = (pupilL.y + pupilR.y) * 0.5f,
                };
            }
        }
        private static float CalcBrow(Vector4[] landmarks)
        {
            if (landmarks.Length != 478)
            {
                return 0;
            }
            else
            {
                float leftBrow = EyeUtils.GetBrowRaise(landmarks, "left");
                float rightBrow = EyeUtils.GetBrowRaise(landmarks, "right");
                return (leftBrow + rightBrow) / 2f;
            }
        }
    }
}
