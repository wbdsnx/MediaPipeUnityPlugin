using System.Collections.Generic;
using UnityEngine;

namespace Kalidokit
{
    public class EyeUtils
    {
        private static int[] LeftEyeIndex = { 130, 133, 160, 159, 158, 144, 145, 153 };
        private static int[] RightEyeIndex = { 263, 362, 387, 386, 385, 373, 374, 380 };
        private static int[] LeftBrowIndex = { 35, 244, 63, 105, 66, 229, 230, 231 };
        private static int[] RightBrowIndex = { 265, 464, 293, 334, 296, 449, 450, 451 };
        private static int[] LeftPupilIndex = { 468, 469, 470, 471, 472 };
        private static int[] RightPupilIndex = { 473, 474, 475, 476, 477 };

        public static EyeOpenStruct GetEyeOpen(Vector4[] poseLandmarks, string side = "left", float high = 0.85f,
            float low = 0.55f)
        {
#if UNITY_EDITOR
            float maxRatio = 0.475f;
#elif UNITY_ANDROID || UNITY_IOS
      float maxRatio = 0.215f;
#endif
            if (side.Equals("left"))
            {
                float eyeDistance = EyeLidRatio(
                    poseLandmarks[LeftEyeIndex[0]],
                    poseLandmarks[LeftEyeIndex[1]],
                    poseLandmarks[LeftEyeIndex[2]],
                    poseLandmarks[LeftEyeIndex[3]],
                    poseLandmarks[LeftEyeIndex[4]],
                    poseLandmarks[LeftEyeIndex[5]],
                    poseLandmarks[LeftEyeIndex[6]],
                    poseLandmarks[LeftEyeIndex[7]]
                );
                // human eye width to height ratio is roughly .3


                // compare ratio against max ratio
                float ratio = Helper.Clamp(eyeDistance / maxRatio, 0, 2);

                // remap eye open and close ratios to increase sensitivity
                float eyeOpenRatio = Helper.Remap(ratio, low, high);

                return new EyeOpenStruct
                {
                    // remapped ratio
                    norm = eyeOpenRatio,
                    // ummapped ratio
                    raw = ratio,
                };
            }
            else
            {
                float eyeDistance = EyeLidRatio(
                    poseLandmarks[RightEyeIndex[0]],
                    poseLandmarks[RightEyeIndex[1]],
                    poseLandmarks[RightEyeIndex[2]],
                    poseLandmarks[RightEyeIndex[3]],
                    poseLandmarks[RightEyeIndex[4]],
                    poseLandmarks[RightEyeIndex[5]],
                    poseLandmarks[RightEyeIndex[6]],
                    poseLandmarks[RightEyeIndex[7]]
                );
                // human eye width to height ratio is roughly .3

                // compare ratio against max ratio
                float ratio = Helper.Clamp(eyeDistance / maxRatio, 0, 2);
                // remap eye open and close ratios to increase sensitivity
                float eyeOpenRatio = Helper.Remap(ratio, low, high);
                return new EyeOpenStruct
                {
                    // remapped ratio
                    norm = eyeOpenRatio,
                    // ummapped ratio
                    raw = ratio,
                };
            }
        }

        public static float EyeLidRatio(Vector4 eyeOuterCorner, Vector4 eyeInnerCorner, Vector4 eyeOuterUpperLid,
            Vector4 eyeMidUpperLid, Vector4 eyeInnerUpperLid, Vector4 eyeOuterLowerLid, Vector4 eyeMidLowerLid,
            Vector4 eyeInnerLowerLid)
        {
            Vector2 eyeOuterCornerVector = VectorUtils.GenerateVector2(eyeOuterCorner),
                eyeInnerCornerVector = VectorUtils.GenerateVector2(eyeInnerCorner),
                eyeOuterUpperLidVector = VectorUtils.GenerateVector2(eyeOuterUpperLid),
                eyeMidUpperLidVector = VectorUtils.GenerateVector2(eyeMidUpperLid),
                eyeInnerUpperLidVector = VectorUtils.GenerateVector2(eyeInnerUpperLid),
                eyeOuterLowerLidVector = VectorUtils.GenerateVector2(eyeOuterLowerLid),
                eyeMidLowerLidVector = VectorUtils.GenerateVector2(eyeMidLowerLid),
                eyeInnerLowerLidVector = VectorUtils.GenerateVector2(eyeInnerLowerLid);
            float eyeWidth =
                Vector3.Distance(eyeOuterCornerVector,
                    eyeInnerCornerVector); //(eyeOuterCorner as Vector).distance(eyeInnerCorner as Vector, 2)
            float eyeOuterLidDistance =
                Vector3.Distance(eyeOuterUpperLidVector,
                    eyeOuterLowerLidVector); //(eyeOuterUpperLid as Vector).distance(eyeOuterLowerLid as Vector, 2)
            float eyeMidLidDistance =
                Vector3.Distance(eyeMidUpperLidVector,
                    eyeMidLowerLidVector); //(eyeMidUpperLid as Vector).distance(eyeMidLowerLid as Vector, 2)
            float eyeInnerLidDistance =
                Vector3.Distance(eyeInnerUpperLidVector,
                    eyeInnerLowerLidVector); //(eyeInnerUpperLid as Vector).distance(eyeInnerLowerLid as Vector, 2);
            float eyeLidAvg = (eyeOuterLidDistance + eyeMidLidDistance + eyeInnerLidDistance) / 3;
            float ratio = eyeLidAvg / eyeWidth;

            return ratio;
        }

        public static PupilPosStruct GetPupilPos(Vector4[] poseLandmarks, string side = "left")
        {
            if (side.Equals("left"))
            {
                Vector2 eyeOuterCorner = VectorUtils.GenerateVector2(poseLandmarks[LeftPupilIndex[0]]);
                Vector2 eyeInnerCorner = VectorUtils.GenerateVector2(poseLandmarks[LeftPupilIndex[1]]);
                float eyeWidth =
                    Vector3.Distance(eyeOuterCorner, eyeInnerCorner); //eyeOuterCorner.distance(eyeInnerCorner, 2);
                Vector2 midPoint =
                    Vector3.Lerp(eyeOuterCorner, eyeInnerCorner, 0.5f); //eyeOuterCorner.lerp(eyeInnerCorner, 0.5);
                Vector2 pupil = VectorUtils.GenerateVector2(poseLandmarks[LeftPupilIndex[0]]);
                float dx = midPoint.x - pupil.x;
                //eye center y is slightly above midpoint
                float dy = midPoint.y - eyeWidth * 0.075f - pupil.y;
                float ratioX = dx / (eyeWidth / 2);
                float ratioY = dy / (eyeWidth / 4);

                ratioX *= 4;
                ratioY *= 4;

                return new PupilPosStruct { x = ratioX, y = ratioY };
            }
            else
            {
                Vector2 eyeOuterCorner = VectorUtils.GenerateVector2(poseLandmarks[RightPupilIndex[0]]);
                Vector2 eyeInnerCorner = VectorUtils.GenerateVector2(poseLandmarks[RightPupilIndex[1]]);
                float eyeWidth =
                    Vector3.Distance(eyeOuterCorner, eyeInnerCorner); //eyeOuterCorner.distance(eyeInnerCorner, 2);
                Vector2 midPoint =
                    Vector3.Lerp(eyeOuterCorner, eyeInnerCorner, 0.5f); //eyeOuterCorner.lerp(eyeInnerCorner, 0.5);
                Vector2 pupil = VectorUtils.GenerateVector2(poseLandmarks[RightPupilIndex[0]]);
                float dx = midPoint.x - pupil.x;
                //eye center y is slightly above midpoint
                float dy = midPoint.y - eyeWidth * 0.075f - pupil.y;
                float ratioX = dx / (eyeWidth / 2);
                float ratioY = dy / (eyeWidth / 4);

                ratioX *= 4;
                ratioY *= 4;

                return new PupilPosStruct { x = ratioX, y = ratioY };
            }
        }

        public static EyeStruct StabilizeBlink(EyeStruct eye, float headY, bool enableWink = true, float maxRot = 0.5f)
        {
            eye.r = Helper.Clamp(eye.r, 0, 1);
            eye.l = Helper.Clamp(eye.l, 0, 1);

            float blinkDiff = Mathf.Abs(eye.l - eye.r);
            //theshold to which difference is considered a wink
            float blinkThresh = enableWink ? 0.8f : 1.2f;
            //detect when both eyes are closing
            bool isClosing = eye.l < 0.3f && eye.r < 0.3f;
            //detect when both eyes are opening
            bool isOpen = eye.l > 0.6f && eye.r > 0.6f;
            if (headY > maxRot)
            {
                return new EyeStruct
                    { l = eye.r, r = eye.r };
            }

            if (headY < -maxRot)
            {
                return new EyeStruct
                    { l = eye.l, r = eye.r };
            }

            return new EyeStruct
            {
                l =
                    blinkDiff >= blinkThresh && !isClosing && !isOpen
                        ? eye.l
                        : eye.r > eye.l
                            ? Utils.Lerp(eye.r, eye.l, 0.95f)
                            : Utils.Lerp(eye.r, eye.l, 0.05f),
                r =
                    blinkDiff >= blinkThresh && !isClosing && !isOpen
                        ? eye.r
                        : eye.r > eye.l
                            ? Utils.Lerp(eye.r, eye.l, 0.95f)
                            : Utils.Lerp(eye.r, eye.l, 0.05f),
            };
        }

        public static float GetBrowRaise(Vector4[] poseLandmarks, string side = "left")
        {
            if (side.Equals("left"))
            {
                float browDistance = EyeLidRatio(
                    poseLandmarks[LeftBrowIndex[0]],
                    poseLandmarks[LeftBrowIndex[1]],
                    poseLandmarks[LeftBrowIndex[2]],
                    poseLandmarks[LeftBrowIndex[3]],
                    poseLandmarks[LeftBrowIndex[4]],
                    poseLandmarks[LeftBrowIndex[5]],
                    poseLandmarks[LeftBrowIndex[6]],
                    poseLandmarks[LeftBrowIndex[7]]
                );

                float maxBrowRatio = 1.15f;
                float browHigh = 0.125f;
                float browLow = 0.07f;
                float browRatio = browDistance / maxBrowRatio - 1;
                float browRaiseRatio = (Helper.Clamp(browRatio, browLow, browHigh) - browLow) / (browHigh - browLow);
                return browRaiseRatio;
            }
            else
            {
                float browDistance = EyeLidRatio(
                    poseLandmarks[RightBrowIndex[0]],
                    poseLandmarks[RightBrowIndex[1]],
                    poseLandmarks[RightBrowIndex[2]],
                    poseLandmarks[RightBrowIndex[3]],
                    poseLandmarks[RightBrowIndex[4]],
                    poseLandmarks[RightBrowIndex[5]],
                    poseLandmarks[RightBrowIndex[6]],
                    poseLandmarks[RightBrowIndex[7]]
                );

                float maxBrowRatio = 1.15f;
                float browHigh = 0.125f;
                float browLow = 0.07f;
                float browRatio = browDistance / maxBrowRatio - 1;
                float browRaiseRatio = (Helper.Clamp(browRatio, browLow, browHigh) - browLow) / (browHigh - browLow);
                return browRaiseRatio;
            }
        }
    }
}