using Unity.Mathematics;
using UnityEngine;
using System;
namespace Kalidokit
{
    public static class Utils
    {
        public static readonly long StartTicks = DateTime.Now.Ticks;
        public static float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);
        public static float NormalizeAngle(float angle)
        {
            float newAngle = ((angle % 360.0f) + 360.0f) % 360.0f;
            return newAngle > 180.0f ? (newAngle - 360.0f) : newAngle;
        }

        public static Vector3 ConvertPoint(Vector4 landmark)
        {
            return new Vector3(landmark.x, landmark.y, landmark.z);
        }
        
        public static float GetTriangleArea(Vector3 A, Vector3 B, Vector3 C) {
            Vector3 AB = new Vector3(B.x - A.x, B.y - A.y, B.z - A.z);
            Vector3 AC = new Vector3(C.x - B.x, C.y - A.y, C.z - A.z);

            float P1 = (AB.y * AC.z - AB.z * AC.y);
            float P2 = (AB.z * AC.x - AB.x * AC.z);
            float P3 = (AB.x * AC.y - AB.y * AC.x);
            return 0.5f * Mathf.Sqrt(P1 * P1 + P2 * P2 + P3 * P3);
        }
        public static float Lerp(float a, float b, float t) => (b - a) * t + a;
    }
}