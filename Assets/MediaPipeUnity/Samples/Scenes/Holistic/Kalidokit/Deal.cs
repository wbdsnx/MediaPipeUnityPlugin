using UnityEngine;

namespace Kalidokit
{
    public class Deal
    {
        private Animator _animator;


        public Deal(Animator animator)
        {
            _animator = animator;
        }

        public void rigPosition(HumanBodyBones name, Vector3 position, float dampener = 1f, float lerpMount = 1f)
        {
            var part = _animator.GetBoneTransform(name);
            if (!part) return;
            part.position = Vector3.Lerp(part.position,position * dampener,lerpMount);
        }
        
        
        public void rigRotation(HumanBodyBones name, Vector3 rotation, float dampener = 1f, float lerpMount = 1f)
        {
            var part = _animator.GetBoneTransform(name);
            if (!part) return;
            var euler = Quaternion.Euler(rotation * (Mathf.Rad2Deg * dampener));
            part.localRotation = Quaternion.Slerp(part.localRotation, euler, lerpMount);    
        }
    }
}