using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalidokit
{


    public class PoseDiffer
    {
        public static Vector3 x = new Vector3(1, 0, 0), y = new Vector3(0, 1, 0), z = new Vector3(0, 0, 1);

        /* 打印当前节点 */
        public static string Vector4toString(Vector4[] data)
        {
            string j = "[";
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0)
                {
                    j += ", ";
                }

                j += "{\"x\":" + data[i].x + ", \"y\":" + data[i].y + ", \"z\":" + data[i].z + ", \"w\":" + data[i].w +
                     "}";
            }

            j += "]";
            return j;
        }

        /* 计算当前两点形成的向量与x/y/z轴的夹角 */
        public static Vector3 VectorHorn(Vector3 tarPos, Vector3 srcPos)
        {
            Vector3 horn = Vector3.zero;
            Vector3 direction = tarPos - srcPos;
            horn.x = VectorHornSingle(direction, PoseDiffer.x);
            horn.y = VectorHornSingle(direction, PoseDiffer.y);
            horn.z = VectorHornSingle(direction, PoseDiffer.z);
            return horn;
        }

        /* 计算当前向量与某一个轴的夹角 */
        public static float VectorHornSingle(Vector3 v, Vector3 temp)
        {
            Quaternion srcQua = Quaternion.Euler(temp);
            Vector3 r = Quaternion.Inverse(srcQua) * v;
            float angle = Mathf.Atan2(r.x, r.z) * Mathf.Rad2Deg;
            return angle > 0 ? angle : angle + 360;
        }

        /* 计算两个夹角是否相似 */
        public static float Similarity(Vector3 tar, Vector3 src)
        {
            return (Circumference(tar.x, src.x) + Circumference(tar.y, src.y) + Circumference(tar.z, src.z)) / 3;
        }

        public static float Circumference(float horn1, float horn2)
        {
            float horn = Mathf.Abs(horn1 - horn2);
            if (horn > 270f)
            {
                horn = Mathf.Abs(360 - horn);
            }

            return horn;
        }
    }
}