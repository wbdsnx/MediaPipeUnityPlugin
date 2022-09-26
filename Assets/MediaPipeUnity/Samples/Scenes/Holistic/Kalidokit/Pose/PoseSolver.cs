using System.Collections.Generic;
using UnityEngine;


public enum PositionIndex : int
{
  Hip,

  RHip,
  RKnee,
  RFoot,
  RToe,

  LHip,
  LKnee,
  LFoot,
  LToe,

  Spine,
  Chest,
  Neck,
  Head,
  Nose,
  REye,
  LEye,

  LShoulder,
  LElbow,
  LWrist,

  RShoulder,
  RElbow,
  RWrist,

  Count,
  None,
}

public static partial class EnumExtend
{
  public static int Int(this PositionIndex i)
  {
    return (int)i;
  }
}

public class JointPoint
{
  public Vector4 Now3D = new Vector4();
  public Transform Transform = null;
  public Quaternion InitRotation;
  public JointPoint Child = null;
  public Quaternion offset;
  public Vector3 forward;
  public Vector2 down;
  public float len;
}

public class PoseSolver
{
  private Animator animator;

  private Vector3 body_init_dir;

  private Dictionary<HumanBodyBones, Quaternion> unityT = new Dictionary<HumanBodyBones, Quaternion>(); //用来存储初始状态的值

  private float boneThreshold = 0.999f; //用来判定关节是否显示

  private JointPoint[] jointPoints = new JointPoint[22];

  public PoseSolver(Animator anim)
  {
    animator = anim;
    for (int i = 0; i < jointPoints.Length; i++)
    {
      jointPoints[i] = new JointPoint();
    }

    jointPoints[PositionIndex.Hip.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Hips);
    jointPoints[PositionIndex.RHip.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
    jointPoints[PositionIndex.RKnee.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
    jointPoints[PositionIndex.RFoot.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    jointPoints[PositionIndex.RToe.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightToes);
    jointPoints[PositionIndex.LHip.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
    jointPoints[PositionIndex.LKnee.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    jointPoints[PositionIndex.LFoot.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
    jointPoints[PositionIndex.LToe.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftToes);
    jointPoints[PositionIndex.Spine.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Spine);
    jointPoints[PositionIndex.Chest.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Chest);
    jointPoints[PositionIndex.Neck.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Neck);
    jointPoints[PositionIndex.Head.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Head);
    jointPoints[PositionIndex.Nose.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.Head);
    jointPoints[PositionIndex.REye.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightEye);
    jointPoints[PositionIndex.LEye.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftEye);

    jointPoints[PositionIndex.RShoulder.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
    jointPoints[PositionIndex.RElbow.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
    jointPoints[PositionIndex.RWrist.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.RightHand);
    jointPoints[PositionIndex.LShoulder.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
    jointPoints[PositionIndex.LElbow.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
    jointPoints[PositionIndex.LWrist.Int()].Transform = animator.GetBoneTransform(HumanBodyBones.LeftHand);


    // Child Settings
    // Right Arm
    jointPoints[PositionIndex.RShoulder.Int()].Child = jointPoints[PositionIndex.RElbow.Int()];
    jointPoints[PositionIndex.RElbow.Int()].Child = jointPoints[PositionIndex.RWrist.Int()];

    // Left Arm
    jointPoints[PositionIndex.LShoulder.Int()].Child = jointPoints[PositionIndex.LElbow.Int()];
    jointPoints[PositionIndex.LElbow.Int()].Child = jointPoints[PositionIndex.LWrist.Int()];

    // Fase

    // Right Leg
    jointPoints[PositionIndex.RHip.Int()].Child = jointPoints[PositionIndex.RKnee.Int()];
    jointPoints[PositionIndex.RKnee.Int()].Child = jointPoints[PositionIndex.RFoot.Int()];
    jointPoints[PositionIndex.RFoot.Int()].Child = jointPoints[PositionIndex.RToe.Int()];

    // Left Leg
    jointPoints[PositionIndex.LHip.Int()].Child = jointPoints[PositionIndex.LKnee.Int()];
    jointPoints[PositionIndex.LKnee.Int()].Child = jointPoints[PositionIndex.LFoot.Int()];
    jointPoints[PositionIndex.LFoot.Int()].Child = jointPoints[PositionIndex.LToe.Int()];

    // etc
    jointPoints[PositionIndex.Hip.Int()].Child = jointPoints[PositionIndex.Spine.Int()];
    jointPoints[PositionIndex.Spine.Int()].Child = jointPoints[PositionIndex.Chest.Int()];
    jointPoints[PositionIndex.Chest.Int()].Child = jointPoints[PositionIndex.Neck.Int()];
    jointPoints[PositionIndex.Neck.Int()].Child = jointPoints[PositionIndex.Head.Int()];


    /////////////////////////////////////////////////// 骨骼长度 ///////////////////////////////////////////////////
    float ratio =
      (jointPoints[PositionIndex.LElbow.Int()].Transform.position -
       jointPoints[PositionIndex.LShoulder.Int()].Transform.position).magnitude /
      jointPoints[PositionIndex.LElbow.Int()].Transform.localPosition.magnitude;

    for (int i = 0; i < jointPoints.Length; i++)
    {
      //InitRotation Settings
      if (jointPoints[i].Transform != null)
      {
        jointPoints[i].InitRotation = jointPoints[i].Transform.rotation;
      }

      if (jointPoints[i].Child != null && jointPoints[i].Child.Transform != null)
      {
        Debug.Log(i);
        jointPoints[i].offset =
          Quaternion.FromToRotation(jointPoints[i].Child.Transform.localPosition, Vector3.forward);
        jointPoints[i].len = jointPoints[i].Child.Transform.localPosition.magnitude * ratio;
      }
    }


    jointPoints[PositionIndex.Hip.Int()].offset =
      Quaternion.FromToRotation(jointPoints[PositionIndex.Hip.Int()].Transform.rotation * Vector3.forward,
        Vector3.forward);


    // 指示信息
    body_init_dir =
      Vector3.Cross(
        jointPoints[PositionIndex.RHip.Int()].Transform.position -
        jointPoints[PositionIndex.LHip.Int()].Transform.position, Vector3.up);
    Vector3 spine_root = jointPoints[PositionIndex.Chest.Int()].Transform.position -
                         jointPoints[PositionIndex.Spine.Int()].Transform.position;
    Vector3.OrthoNormalize(ref spine_root, ref body_init_dir);
    jointPoints[PositionIndex.Spine.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.Spine.Int()].Transform.rotation) * body_init_dir;
    Vector3 chest_spine = jointPoints[PositionIndex.Neck.Int()].Transform.position -
                          jointPoints[PositionIndex.Chest.Int()].Transform.position;
    Vector3.OrthoNormalize(ref chest_spine, ref body_init_dir);
    jointPoints[PositionIndex.Chest.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.Chest.Int()].Transform.rotation) * body_init_dir;
    Vector3 tmp1, tmp2, tmp3;
    // 左臂
    tmp1 = jointPoints[PositionIndex.LElbow.Int()].Transform.position -
           jointPoints[PositionIndex.LShoulder.Int()].Transform.position;
    tmp2 = jointPoints[PositionIndex.LWrist.Int()].Transform.position -
           jointPoints[PositionIndex.LElbow.Int()].Transform.position;
    tmp3 = Vector3.Cross(
      animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal).position -
      jointPoints[PositionIndex.LWrist.Int()].Transform.position,
      animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal).position -
      jointPoints[PositionIndex.LWrist.Int()].Transform.position);
    jointPoints[PositionIndex.LWrist.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.LWrist.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp1, ref tmp3);

    jointPoints[PositionIndex.LShoulder.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.LShoulder.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp2, ref tmp3);
    jointPoints[PositionIndex.LElbow.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.LElbow.Int()].Transform.rotation) * tmp3;
    //右臂
    tmp1 = jointPoints[PositionIndex.RElbow.Int()].Transform.position -
           jointPoints[PositionIndex.RShoulder.Int()].Transform.position;
    tmp2 = jointPoints[PositionIndex.RWrist.Int()].Transform.position -
           jointPoints[PositionIndex.RElbow.Int()].Transform.position;
    tmp3 = Vector3.Cross(
      animator.GetBoneTransform(HumanBodyBones.RightThumbProximal).position -
      jointPoints[PositionIndex.RWrist.Int()].Transform.position,
      animator.GetBoneTransform(HumanBodyBones.RightLittleProximal).position -
      jointPoints[PositionIndex.RWrist.Int()].Transform.position);
    jointPoints[PositionIndex.RWrist.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.RWrist.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp1, ref tmp3);

    jointPoints[PositionIndex.RShoulder.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.RShoulder.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp2, ref tmp3);
    jointPoints[PositionIndex.RElbow.Int()].down =
      Quaternion.Inverse(jointPoints[PositionIndex.RElbow.Int()].Transform.rotation) * tmp3;
    //左腿
    tmp1 = jointPoints[PositionIndex.LKnee.Int()].Transform.position -
           jointPoints[PositionIndex.LHip.Int()].Transform.position;
    tmp2 = jointPoints[PositionIndex.LFoot.Int()].Transform.position -
           jointPoints[PositionIndex.LKnee.Int()].Transform.position;
    tmp3 = jointPoints[PositionIndex.LToe.Int()].Transform.position -
           jointPoints[PositionIndex.LFoot.Int()].Transform.position;
    jointPoints[PositionIndex.LFoot.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.LFoot.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp1, ref tmp3);
    jointPoints[PositionIndex.LHip.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.LHip.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp2, ref tmp3);
    jointPoints[PositionIndex.LKnee.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.LKnee.Int()].Transform.rotation) * tmp3;
    //右腿
    tmp1 = jointPoints[PositionIndex.RKnee.Int()].Transform.position -
           jointPoints[PositionIndex.RHip.Int()].Transform.position;
    tmp2 = jointPoints[PositionIndex.RFoot.Int()].Transform.position -
           jointPoints[PositionIndex.RKnee.Int()].Transform.position;
    tmp3 = jointPoints[PositionIndex.RToe.Int()].Transform.position -
           jointPoints[PositionIndex.RFoot.Int()].Transform.position;
    jointPoints[PositionIndex.RFoot.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.RFoot.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp1, ref tmp3);
    jointPoints[PositionIndex.RHip.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.RHip.Int()].Transform.rotation) * tmp3;
    Vector3.OrthoNormalize(ref tmp2, ref tmp3);
    jointPoints[PositionIndex.RKnee.Int()].forward =
      Quaternion.Inverse(jointPoints[PositionIndex.RKnee.Int()].Transform.rotation) * tmp3;
  }

  Vector3 recal_pos(Vector3 a, Vector3 b, float len)
  {
    return a + (b - a).normalized * len;
  }

  Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
  {
    Vector3 d1 = a - b;
    Vector3 d2 = a - c;

    Vector3 dd = Vector3.Cross(d1, d2);
    dd.Normalize();
    return dd;
  }


  private const float TWO_PI = Mathf.PI * 2;
  private const float PI = Mathf.PI;

  float normalizeAngle(float radians)
  {
    var angle = radians % TWO_PI;
    angle = angle > PI ? angle - TWO_PI : angle < -PI ? TWO_PI + angle : angle;
    return angle / PI;
  }


  float find2DAngle(float cx, float cy, float ex, float ey)
  {
    var dy = ey - cy;
    var dx = ex - cx;
    var theta = Mathf.Atan2(dy, dx);
    return theta;
  }

  Vector3 rollPitchYaw(Vector3 a, Vector3 b)
  {
    return new Vector3(
      normalizeAngle(find2DAngle(a.z, a.y, b.z, b.y)),
      normalizeAngle(find2DAngle(a.z, a.x, b.z, b.x)),
      normalizeAngle(find2DAngle(a.x, a.y, b.x, b.y))
    );
  }


  public void Solve(Vector4[] landmarks)
  {
    jointPoints[PositionIndex.RHip.Int()].Now3D = landmarks[24];
    jointPoints[PositionIndex.LHip.Int()].Now3D = landmarks[23];
    jointPoints[PositionIndex.Hip.Int()].Now3D =
      (jointPoints[PositionIndex.LHip.Int()].Now3D + jointPoints[PositionIndex.LHip.Int()].Now3D) / 2f;
    jointPoints[PositionIndex.LKnee.Int()].Now3D = landmarks[25];
    jointPoints[PositionIndex.LFoot.Int()].Now3D = landmarks[27];
    jointPoints[PositionIndex.LToe.Int()].Now3D = landmarks[31];
    jointPoints[PositionIndex.RKnee.Int()].Now3D = landmarks[26];
    jointPoints[PositionIndex.RFoot.Int()].Now3D = landmarks[28];
    jointPoints[PositionIndex.RToe.Int()].Now3D = landmarks[32];

    jointPoints[PositionIndex.RShoulder.Int()].Now3D = landmarks[12];
    jointPoints[PositionIndex.LShoulder.Int()].Now3D = landmarks[11];
    jointPoints[PositionIndex.Neck.Int()].Now3D = (jointPoints[PositionIndex.LShoulder.Int()].Now3D +
                                                   jointPoints[PositionIndex.RShoulder.Int()].Now3D) / 2f;
    jointPoints[PositionIndex.Spine.Int()].Now3D =
      (jointPoints[PositionIndex.Neck.Int()].Now3D + jointPoints[PositionIndex.Hip.Int()].Now3D) / 2f;
    ;
    // Calculate head location
    var cEar = (jointPoints[PositionIndex.LEye.Int()].Now3D + jointPoints[PositionIndex.REye.Int()].Now3D) / 2f;
    var hv = cEar - jointPoints[PositionIndex.Neck.Int()].Now3D;
    Vector4 nhv = Vector3.Normalize(hv);
    var nv = jointPoints[PositionIndex.Nose.Int()].Now3D - jointPoints[PositionIndex.Neck.Int()].Now3D;
    jointPoints[PositionIndex.Head.Int()].Now3D =
      jointPoints[PositionIndex.Neck.Int()].Now3D + nhv * Vector3.Dot(nhv, nv);

    jointPoints[PositionIndex.REye.Int()].Now3D = landmarks[5];
    jointPoints[PositionIndex.LEye.Int()].Now3D = landmarks[2];
    jointPoints[PositionIndex.RElbow.Int()].Now3D = landmarks[14];
    jointPoints[PositionIndex.RWrist.Int()].Now3D = landmarks[16];
    jointPoints[PositionIndex.LElbow.Int()].Now3D = landmarks[13];
    jointPoints[PositionIndex.LWrist.Int()].Now3D = landmarks[15];

    /////////////////////////////////// head direction//////////////////////////////////
    if (jointPoints[PositionIndex.Neck.Int()].Now3D.w < 0.99f)
    {
      return;
    }

    /// 
    // Head Rotation
    var gaze = jointPoints[PositionIndex.Head.Int()].Now3D - jointPoints[PositionIndex.Nose.Int()].Now3D;
    var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Now3D, jointPoints[PositionIndex.REye.Int()].Now3D,
      jointPoints[PositionIndex.LEye.Int()].Now3D);
    var head = jointPoints[PositionIndex.Head.Int()];
    head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InitRotation;
    /////////////////////////////////// body direction//////////////////////////////////
    // update root   
    Vector3 body_dir =
      Vector3.Cross(jointPoints[PositionIndex.RHip.Int()].Now3D - jointPoints[PositionIndex.LHip.Int()].Now3D,
        Vector3.up);
    jointPoints[PositionIndex.Hip.Int()].Transform
      .LookAt(jointPoints[PositionIndex.Hip.Int()].Transform.position + body_dir, Vector3.up);
    Quaternion update_rot = Quaternion.FromToRotation(body_dir, body_init_dir);

    Vector3 chest_dir = jointPoints[PositionIndex.Hip.Int()].Now3D - jointPoints[PositionIndex.Spine.Int()].Now3D;
    Vector3 spine_dir = jointPoints[PositionIndex.Spine.Int()].Now3D - jointPoints[PositionIndex.Hip.Int()].Now3D;

    Vector3 shoulder_dir =
      Vector3.Cross(jointPoints[PositionIndex.RShoulder.Int()].Now3D - jointPoints[PositionIndex.LShoulder.Int()].Now3D,
        Vector3.up);

    /// update spine 
    // look at position       
    //Debug.Log(Vector3.Angle(spine.rotation * spine_forward, chest.position - spine.position));
    jointPoints[PositionIndex.Spine.Int()].Transform
      .LookAt(jointPoints[PositionIndex.Spine.Int()].Transform.position + spine_dir, Vector3.zero);
    jointPoints[PositionIndex.Spine.Int()].Transform.rotation =
      jointPoints[PositionIndex.Spine.Int()].Transform.rotation * jointPoints[PositionIndex.Spine.Int()].offset;
    //Debug.Log(Vector3.Angle(spine_dir, chest.position - spine.position));
    // twist
    Vector3 spine_forward_new = body_dir.normalized + shoulder_dir.normalized;
    Vector3.OrthoNormalize(ref (spine_dir), ref (spine_forward_new));
    Vector3 spine_forward_old = jointPoints[PositionIndex.Spine.Int()].Transform.rotation *
                                jointPoints[PositionIndex.Spine.Int()].forward;
    Quaternion spine_add_rot = Quaternion.FromToRotation(spine_forward_old, spine_forward_new);
    jointPoints[PositionIndex.Spine.Int()].Transform.rotation =
      spine_add_rot * jointPoints[PositionIndex.Spine.Int()].Transform.rotation;
    
    RigArm(jointPoints[PositionIndex.LShoulder.Int()], jointPoints[PositionIndex.LShoulder.Int()].Now3D.w < .99f);
    RigArm(jointPoints[PositionIndex.RShoulder.Int()], jointPoints[PositionIndex.RShoulder.Int()].Now3D.w < .99f);
    RigLeg(jointPoints[PositionIndex.RHip.Int()], jointPoints[PositionIndex.RHip.Int()].Now3D.w < .99f);
    RigLeg(jointPoints[PositionIndex.LHip.Int()], jointPoints[PositionIndex.LHip.Int()].Now3D.w < .99f);
  }

  public void RigArm(JointPoint jp, bool reset = false)
  {
    if (reset)
    {
      jp.Transform.rotation = jp.InitRotation;
      jp.Child.Transform.rotation = jp.Child.InitRotation;
      jp.Child.Child.Transform.rotation = jp.Child.Child.InitRotation;
      return;
    }

    Vector3 cur_shoulder = jp.Transform.position;
    Vector3 up_arm_dir = jp.Child.Now3D - jp.Now3D;
    Vector3 low_arm_dir = jp.Child.Child.Now3D - jp.Child.Now3D;
    Vector3 arm_norm = Vector3.Cross(up_arm_dir, low_arm_dir);
    Vector3
      hand_norm = arm_norm; // Vector3.Cross(bone_pos[9] -  jointPoints[PositionIndex.Spine.Int()].Now3D,  jointPoints[PositionIndex.Hip.Int()].Now3D -  jointPoints[PositionIndex.Spine.Int()].Now3D);
    Vector3 elbow_norm = hand_norm;
    Vector3.OrthoNormalize(ref low_arm_dir, ref elbow_norm);
    /// update right elbow 
    // look at position
    Vector3 new_relbow_pos = recal_pos(cur_shoulder, cur_shoulder + up_arm_dir, jp.len);
    jp.Transform.LookAt(new_relbow_pos, Vector3.zero);
    jp.Transform.rotation = jp.Transform.rotation * jp.offset;
    //twist
    Vector3 uparm_twist_ori = jp.Transform.rotation * jp.down;
    Quaternion r_uparm_twist_rot = Quaternion.FromToRotation(uparm_twist_ori, arm_norm);
    jp.Transform.rotation = r_uparm_twist_rot * jp.Transform.rotation;

    /// update right wrist 
    // look at position        
    Vector3 new_rhand_pos = recal_pos(new_relbow_pos, new_relbow_pos + low_arm_dir, jp.Child.len);
    jp.Child.Transform.LookAt(new_rhand_pos, Vector3.zero);
    jp.Child.Transform.rotation = jp.Child.Transform.rotation * jp.Child.offset;
    // twist 
    Vector3 r_lowarm_twist_ori = jp.Child.Transform.rotation * jp.Child.down;
    if (Vector3.Angle(arm_norm, hand_norm) > 180)
    {
      hand_norm = -hand_norm;
    }

    Quaternion r_lowarm_twist_rot = Quaternion.FromToRotation(r_lowarm_twist_ori, elbow_norm);
    jp.Child.Transform.rotation = r_lowarm_twist_rot * jp.Child.Transform.rotation;
  }

  public void RigLeg(JointPoint jp, bool reset = false)
  {
    if (reset)
    {
      jp.Transform.rotation = jp.InitRotation;
      jp.Child.Transform.rotation = jp.Child.InitRotation;
      jp.Child.Child.Transform.rotation = jp.Child.Child.InitRotation;
      jp.Child.Child.Child.Transform.rotation = jp.Child.Child.Child.InitRotation;
      return;
    }

    Vector3 up_leg_dir =
      jp.Child.Now3D - jp.Now3D;
    Vector3 low_leg_dir =
      jp.Child.Child.Now3D - jp.Child.Now3D;
    Vector3
      foot_dir = jp.Child.Child.Child.Now3D -
                 jp.Child.Child.Now3D; //bone_pos[19] - jointPoints[PositionIndex.LKnee.Int()].Now3D; //暂定人体方向是脚的方向

    Vector3 leg_norm = Vector3.Cross(up_leg_dir, low_leg_dir);
    Vector3 foot_norm = Vector3.Cross(low_leg_dir, foot_dir);
    Vector3 toe_norm = leg_norm;

    /// update left knee
    // look at position
    Vector3 cur_hip = jp.Transform.position;
    Vector3 new_knee_pos = recal_pos(cur_hip, cur_hip + up_leg_dir,
      jp.len);
    jp.Transform.LookAt(new_knee_pos, Vector3.zero);
    jp.Transform.rotation =
      jp.Transform.rotation * jp.offset;
    // twist
    Vector3 upleg_twist_ori = jp.Transform.rotation *
                              jp.forward;
    Vector3 upleg_twist_new = jp.Transform.rotation *
                              jp.forward;
    Vector3.OrthoNormalize(ref leg_norm, ref upleg_twist_new);
    if (Vector3.Angle(new Vector3((jp.Child.Child.Child.Now3D -
                                   jp.Child.Child.Now3D).x, 0, (jp.Child.Child.Child.Now3D -
                                                                jp.Child.Child.Now3D).z),
          new Vector3(upleg_twist_new.x, 0, upleg_twist_new.z)) > 90)
      upleg_twist_new = -upleg_twist_new;
    Quaternion l_upleg_twist_rot = Quaternion.FromToRotation(upleg_twist_ori, upleg_twist_new);
    jp.Transform.rotation =
      l_upleg_twist_rot * jp.Transform.rotation;

    ///update left foot
    // look at position
    Vector3 new_foot_pos = recal_pos(new_knee_pos, new_knee_pos + low_leg_dir,
      jp.Child.len);
    jp.Child.Transform.LookAt(new_foot_pos, Vector3.zero);
    jp.Child.Transform.rotation =
      jp.Child.Transform.rotation * jp.Child.offset;
    // twist
    Vector3 lowleg_twist_ori = jp.Child.Transform.rotation *
                               jp.Child.forward;
    Vector3 lowleg_twist_new = jp.Child.Transform.rotation *
                               jp.Child.forward;
    Vector3.OrthoNormalize(ref foot_norm, ref lowleg_twist_new);
    if (Vector3.Angle(new Vector3((jp.Child.Child.Child.Now3D -
                                   jp.Child.Child.Now3D).x, 0, (jp.Child.Child.Child.Now3D -
                                                                jp.Child.Child.Now3D).z),
          new Vector3(lowleg_twist_new.x, 0, lowleg_twist_new.z)) > 90)
      lowleg_twist_new = -lowleg_twist_new;
    Quaternion l_lowleg_twist_rot = Quaternion.FromToRotation(lowleg_twist_ori, lowleg_twist_new);
    jp.Child.Transform.rotation =
      l_lowleg_twist_rot * jp.Child.Transform.rotation;
  }
}
