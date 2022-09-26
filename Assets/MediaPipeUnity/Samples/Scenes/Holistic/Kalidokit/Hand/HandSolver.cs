using UnityEngine;

namespace Kalidokit
{
    public class HandSolver
    {
        public Animator animator;

        public HandSolver(Animator anim)
        {
            animator = anim;
        }

        private Vector3 handPositionOffset = Vector3.zero;
        private Vector3 handPositionScale = Vector3.one;
        public bool fixHand = true;

        private RotStruct rHand = RotStruct.identity;
        private RotStruct rIndexPip = RotStruct.identity;
        private RotStruct rIndexDip = RotStruct.identity;
        private RotStruct rIndexTip = RotStruct.identity;
        private RotStruct rMiddlePip = RotStruct.identity;
        private RotStruct rMiddleDip = RotStruct.identity;
        private RotStruct rMiddleTip = RotStruct.identity;
        private RotStruct rRingPip = RotStruct.identity;
        private RotStruct rRingDip = RotStruct.identity;
        private RotStruct rRingTip = RotStruct.identity;
        private RotStruct rPinkyPip = RotStruct.identity;
        private RotStruct rPinkyDip = RotStruct.identity;
        private RotStruct rPinkyTip = RotStruct.identity;
        private RotStruct rThumbPip = RotStruct.identity;
        private RotStruct rThumbDip = RotStruct.identity;
        private RotStruct rThumbTip = RotStruct.identity;

        private RotStruct lHand = RotStruct.identity;
        private RotStruct lIndexPip = RotStruct.identity;
        private RotStruct lIndexDip = RotStruct.identity;
        private RotStruct lIndexTip = RotStruct.identity;
        private RotStruct lMiddlePip = RotStruct.identity;
        private RotStruct lMiddleDip = RotStruct.identity;
        private RotStruct lMiddleTip = RotStruct.identity;
        private RotStruct lRingPip = RotStruct.identity;
        private RotStruct lRingDip = RotStruct.identity;
        private RotStruct lRingTip = RotStruct.identity;
        private RotStruct lPinkyPip = RotStruct.identity;
        private RotStruct lPinkyDip = RotStruct.identity;
        private RotStruct lPinkyTip = RotStruct.identity;
        private RotStruct lThumbPip = RotStruct.identity;
        private RotStruct lThumbDip = RotStruct.identity;
        private RotStruct lThumbTip = RotStruct.identity;

        /// <summary>
        /// Solve
        /// </summary>
        /// <param name="landmarks"></param>
        /// <param name="side">
        ///	true is right,otherwise left
        /// </param>
        public void Solve(Vector4[] landmarks, bool side)
        {
            Quaternion preHand = Quaternion.identity;
            Quaternion hand = Quaternion.identity;
            Quaternion indexPip = Quaternion.identity;
            Quaternion indexDip = Quaternion.identity;
            Quaternion indexTip = Quaternion.identity;
            Quaternion middlePip = Quaternion.identity;
            Quaternion middleDip = Quaternion.identity;
            Quaternion middleTip = Quaternion.identity;
            Quaternion ringPip = Quaternion.identity;
            Quaternion ringDip = Quaternion.identity;
            Quaternion ringTip = Quaternion.identity;
            Quaternion pinkyPip = Quaternion.identity;
            Quaternion pinkyDip = Quaternion.identity;
            Quaternion pinkyTip = Quaternion.identity;
            Quaternion thumbPip = Quaternion.identity;
            Quaternion thumbDip = Quaternion.identity;
            Quaternion thumbTip = Quaternion.identity;

            {
                Vector3 handUpDir;
                Vector3 handForwardDir;
                {
                    Vector3 palm = Utils.ConvertPoint(landmarks[0]);
                    Vector3 indexFinger = Utils.ConvertPoint(landmarks[5]);
                    Vector3 middleFinger = Utils.ConvertPoint(landmarks[9]);
                    Vector3 ringFinger = Utils.ConvertPoint(landmarks[13]);
                    Vector3 pinkyFinger = Utils.ConvertPoint(landmarks[17]);

                    // Figure out their position on the eye socket plane
                    handUpDir = new Vector3(
                        (middleFinger.x - palm.x),
                        (middleFinger.y - palm.y),
                        (middleFinger.z - palm.z)
                    );

                    Plane plane = new Plane(palm, indexFinger, pinkyFinger);
                    handForwardDir = plane.normal;

                    Quaternion rotTest = Quaternion.Inverse(Quaternion.LookRotation(handForwardDir, handUpDir));
                    HandPoints.ComputeFinger2(rotTest, !side ? 1 : -1, 3,
                        indexFinger,
                        Utils.ConvertPoint(landmarks[6]),
                        Utils.ConvertPoint(landmarks[7]),
                        Utils.ConvertPoint(landmarks[8]),
                        out indexPip, out indexDip, out indexTip);
                    HandPoints.ComputeFinger2(rotTest, !side ? 1 : -1, 2,
                        middleFinger,
                        Utils.ConvertPoint(landmarks[10]),
                        Utils.ConvertPoint(landmarks[11]),
                        Utils.ConvertPoint(landmarks[12]),
                        out middlePip, out middleDip, out middleTip);
                    HandPoints.ComputeFinger2(rotTest, !side ? 1 : -1, 1,
                        ringFinger,
                        Utils.ConvertPoint(landmarks[14]),
                        Utils.ConvertPoint(landmarks[15]),
                        Utils.ConvertPoint(landmarks[16]),
                        out ringPip, out ringDip, out ringTip);
                    HandPoints.ComputeFinger2(rotTest, !side ? 1 : -1, 0,
                        pinkyFinger,
                        Utils.ConvertPoint(landmarks[18]),
                        Utils.ConvertPoint(landmarks[19]),
                        Utils.ConvertPoint(landmarks[20]),
                        out pinkyPip, out pinkyDip, out pinkyTip);
                    HandPoints.ComputeThumb(rotTest, !side ? 1 : -1, 4,
                        Utils.ConvertPoint(landmarks[0]),
                        Utils.ConvertPoint(landmarks[2]),
                        Utils.ConvertPoint(landmarks[3]),
                        Utils.ConvertPoint(landmarks[4]),
                        out thumbPip, out thumbDip, out thumbTip);
                }


                Quaternion test = Quaternion.Euler(0, 90, -90);
                if (side)
                {
                    preHand = Quaternion.LookRotation(-handForwardDir, -handUpDir);
                }
                else
                {
                    preHand = Quaternion.LookRotation(handForwardDir, handUpDir);
                }

               
                Quaternion rot = preHand * test;
                hand = rot;
            }

            float time = Utils.TimeNow;
            if (side)
            {
                rHand.Set(hand, time);
                rIndexPip.Set(indexPip, time);
                rIndexDip.Set(indexDip, time);
                rIndexTip.Set(indexTip, time);
                rMiddlePip.Set(middlePip, time);
                rMiddleDip.Set(middleDip, time);
                rMiddleTip.Set(middleTip, time);
                rRingPip.Set(ringPip, time);
                rRingDip.Set(ringDip, time);
                rRingTip.Set(ringTip, time);
                rPinkyPip.Set(pinkyPip, time);
                rPinkyDip.Set(pinkyDip, time);
                rPinkyTip.Set(pinkyTip, time);
                rThumbPip.Set(thumbPip, time);
                rThumbDip.Set(thumbDip, time);
                rThumbTip.Set(thumbTip, time);
            }
            else
            {
                lHand.Set(hand, time);
                lIndexPip.Set(indexPip, time);
                lIndexDip.Set(indexDip, time);
                lIndexTip.Set(indexTip, time);
                lMiddlePip.Set(middlePip, time);
                lMiddleDip.Set(middleDip, time);
                lMiddleTip.Set(middleTip, time);
                lRingPip.Set(ringPip, time);
                lRingDip.Set(ringDip, time);
                lRingTip.Set(ringTip, time);
                lPinkyPip.Set(pinkyPip, time);
                lPinkyDip.Set(pinkyDip, time);
                lPinkyTip.Set(pinkyTip, time);
                lThumbPip.Set(thumbPip, time);
                lThumbDip.Set(thumbDip, time);
                lThumbTip.Set(thumbTip, time);
            }

            if (side)
            {
                rHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightHand), time);
                rIndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightIndexProximal), time);
                rIndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), time);
                rIndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightIndexDistal), time);
                rMiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), time);
                rMiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), time);
                rMiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), time);
                rRingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightRingProximal), time);
                rRingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate), time);
                rRingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightRingDistal), time);
                rPinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightLittleProximal), time);
                rPinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), time);
                rPinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightLittleDistal), time);
                rThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightThumbProximal), time);
                rThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate), time);
                rThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightThumbDistal), time);
            }
            else
            {
                lHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftHand), time);
                lIndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal), time);
                lIndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate), time);
                lIndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal), time);
                lMiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), time);
                lMiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), time);
                lMiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), time);
                lRingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftRingProximal), time);
                lRingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate), time);
                lRingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftRingDistal), time);
                lPinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), time);
                lPinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), time);
                lPinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal), time);
                lThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal), time);
                lThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate), time);
                lThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal), time);
            }
        }
    }
}