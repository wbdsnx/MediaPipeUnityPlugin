
using System;
using UnityEngine;


namespace Kalidokit
{


    public class RotStruct
    {
        // public int TestInterpolation = 0;
        // public float InterpolationValue = .1f; // 0.4 is really good for the current frame interval. Make this adjustable
        public static int TestInterpolationStatic = 0;
        public static float TestInterpolationValue = .4f;
        public static RotStruct identity => new RotStruct(Quaternion.identity, 0);

        private float lastTime;
        private float currTime;
        private Quaternion curr;

        public RotStruct(Quaternion init, float time)
        {
            currTime = time;
            lastTime = time;
            curr = init;
        }

        public void Set(Quaternion value, float time)
        {
            lastTime = currTime;
            currTime = time;
            curr = value;
        }

        private Quaternion GetUpdatedRotation(Quaternion current, Quaternion curr, float time)
        {
            switch (TestInterpolationStatic)
            {
                default:
                {
                    return Quaternion.Lerp(current, curr, TestInterpolationValue);
                }
                case 1:
                {
                    float timeLength = currTime - lastTime;
                    float delta = (time - currTime) / timeLength;
                    return Quaternion.Lerp(current, curr, delta);
                }
                case 2:
                {
                    return curr;
                }
            }
        }

        public void UpdateRotation(Transform transform, float time)
        {
            if (time - 1 > currTime)
            {
                // If the part was lost we slowly put it back to it's original position
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, TestInterpolationValue);
            }
            else
            {
                transform.rotation = GetUpdatedRotation(transform.rotation, curr, time);
            }
        }

        public void UpdateLocalRotation(Transform transform, float time)
        {
            transform.localRotation = GetUpdatedRotation(transform.localRotation, curr, time);
        }
    }
}