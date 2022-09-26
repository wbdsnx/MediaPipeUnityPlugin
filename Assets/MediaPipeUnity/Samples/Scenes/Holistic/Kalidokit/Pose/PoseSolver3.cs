using System;
using Unity.Mathematics;
using UnityEngine;

namespace Kalidokit
{
    public class PoseSolver3
    {
        public Animator animator;

        public PoseSolver3(Animator anim)
        {
            animator = anim;
        }

        private RotStruct chestRotation = RotStruct.identity;
        private RotStruct hipsRotation = RotStruct.identity;
        private RotStruct hipsPosition = RotStruct.identity;



        private RotStruct rUpperArm = RotStruct.identity;
        private RotStruct rLowerArm = RotStruct.identity;
        private RotStruct lUpperArm = RotStruct.identity;
        private RotStruct lLowerArm = RotStruct.identity;

        public void Solve(Vector4[] landmarks)
        {
            Quaternion chestRotation = Quaternion.identity;
            Quaternion hipsRotation = Quaternion.identity;
            Vector3 hipsPosition = Vector3.zero;
            Quaternion rUpperArm = Quaternion.identity;
            Quaternion rLowerArm = Quaternion.identity;
            Quaternion lUpperArm = Quaternion.identity;
            Quaternion lLowerArm = Quaternion.identity;

            float handExtraPercentage = 0.2f;
            try
            {
                Vector3 rShoulder = Utils.ConvertPoint(landmarks[12]);
                Vector3 lShoulder = Utils.ConvertPoint(landmarks[11]);
                float bodyRotation = 1.0f;
                {
                    Vector3 vRigA = Vector3.left;
                    Vector3 vRigB = rShoulder - lShoulder;
                    Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
                    chestRotation = rot;
                    hipsRotation = rot;
                    hipsPosition = new Vector3(
                        -(rShoulder.x + lShoulder.x) * 0.5f * 2,
                        -(rShoulder.z + lShoulder.z) * 0.5f,
                        (rShoulder.y + lShoulder.y) * 0.5f + 1.0f
                    );
                    bodyRotation = Mathf.Abs(Mathf.Cos(rot.eulerAngles.y * 1.6f));
                }
                float hep = handExtraPercentage * bodyRotation;
                {
                    Vector3 rElbow = Utils.ConvertPoint(landmarks[14]);
                    Vector3 rHand = Utils.ConvertPoint(landmarks[16]);
                    // If we have hand data
                    Vector3 vRigA = Vector3.right;
                    Vector3 vRigB = rElbow - rShoulder;
                    Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
                    rUpperArm = rot;

                    Vector3 vRigC = rHand - rElbow;
                    rot = Quaternion.FromToRotation(vRigA, vRigC);
                    rLowerArm = rot;
                }

                {
                    Vector3 lElbow = Utils.ConvertPoint(landmarks[13]);
                    Vector3 lHand = Utils.ConvertPoint(landmarks[15]);
                    // If we have hand data

                    Vector3 vRigA = Vector3.left;
                    Vector3 vRigB = lElbow - lShoulder;
                    Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
                    lUpperArm = rot;

                    Vector3 vRigC = lHand - lElbow;
                    rot = Quaternion.FromToRotation(vRigA, vRigC);
                    lLowerArm = rot;
                }
            }
            catch
            {
                throw new Exception("Unknown Exception");
            }

            // TODO: Do all of these one update late
            // So that everything can be smoothly synched between each timestep of the AI :D
            this.chestRotation.Set(chestRotation, Utils.TimeNow);
            // this.hipsPosition.Set(hipsPosition, TimeNow);
            this.hipsRotation.Set(hipsRotation, Utils.TimeNow);
            this.rUpperArm.Set(rUpperArm, Utils.TimeNow);
            this.rLowerArm.Set(rLowerArm, Utils.TimeNow);
            this.lUpperArm.Set(lUpperArm, Utils.TimeNow);
            this.lLowerArm.Set(lLowerArm, Utils.TimeNow);
            
            // this.hipsRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Hips), Utils.TimeNow);
            // this.chestRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Chest), Utils.TimeNow);
            // Debug.Log(animator.GetBoneTransform(HumanBodyBones.Chest).rotation);
            this.rUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), Utils.TimeNow);
            this.rLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), Utils.TimeNow);
            this.lUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), Utils.TimeNow);
            this.lLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), Utils.TimeNow);
        }
    }
}