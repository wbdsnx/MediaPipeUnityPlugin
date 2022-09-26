// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using Kalidokit;
using UnityEngine;
using VRM;

namespace Mediapipe.Unity.Holistic
{
  public class HolisticTrackingSolution : ImageSourceSolution<HolisticTrackingGraph>
  {
    [SerializeField] private RectTransform _worldAnnotationArea;
    [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
    [SerializeField] private HolisticLandmarkListAnnotationController _holisticAnnotationController;
    [SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
    [SerializeField] private MaskAnnotationController _segmentationMaskAnnotationController;
    [SerializeField] private NormalizedRectAnnotationController _poseRoiAnnotationController;

    public HolisticTrackingGraph.ModelComplexity modelComplexity
    {
      get => graphRunner.modelComplexity;
      set => graphRunner.modelComplexity = value;
    }


    public bool smoothLandmarks
    {
      get => graphRunner.smoothLandmarks;
      set => graphRunner.smoothLandmarks = value;
    }

    public bool refineFaceLandmarks
    {
      get => graphRunner.refineFaceLandmarks;
      set => graphRunner.refineFaceLandmarks = value;
    }

    public bool enableSegmentation
    {
      get => graphRunner.enableSegmentation;
      set => graphRunner.enableSegmentation = value;
    }

    public bool smoothSegmentation
    {
      get => graphRunner.smoothSegmentation;
      set => graphRunner.smoothSegmentation = value;
    }

    public float minDetectionConfidence
    {
      get => graphRunner.minDetectionConfidence;
      set => graphRunner.minDetectionConfidence = value;
    }

    public float minTrackingConfidence
    {
      get => graphRunner.minTrackingConfidence;
      set => graphRunner.minTrackingConfidence = value;
    }

    protected override void SetupScreen(ImageSource imageSource)
    {
      base.SetupScreen(imageSource);
      _worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
    }

    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {
        graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
        graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
        graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
        graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
        graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
        graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
        graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
        graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
      }

      var imageSource = ImageSourceProvider.ImageSource;
      SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
      SetupAnnotationController(_holisticAnnotationController, imageSource);
      SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
      SetupAnnotationController(_segmentationMaskAnnotationController, imageSource);
      _segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
      SetupAnnotationController(_poseRoiAnnotationController, imageSource);
    }

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      graphRunner.AddTextureFrameToInputStream(textureFrame);
    }

    protected override IEnumerator WaitForNextValue()
    {
      Detection poseDetection = null;
      NormalizedLandmarkList faceLandmarks = null;
      NormalizedLandmarkList poseLandmarks = null;
      NormalizedLandmarkList leftHandLandmarks = null;
      NormalizedLandmarkList rightHandLandmarks = null;
      LandmarkList poseWorldLandmarks = null;
      ImageFrame segmentationMask = null;
      NormalizedRect poseRoi = null;

      if (runningMode == RunningMode.Sync)
      {
        var _ = graphRunner.TryGetNext(out poseDetection, out poseLandmarks, out faceLandmarks, out leftHandLandmarks,
          out rightHandLandmarks, out poseWorldLandmarks, out segmentationMask, out poseRoi, true);
      }
      else if (runningMode == RunningMode.NonBlockingSync)
      {
        yield return new WaitUntil(() =>
          graphRunner.TryGetNext(out poseDetection, out poseLandmarks, out faceLandmarks, out leftHandLandmarks,
            out rightHandLandmarks, out poseWorldLandmarks, out segmentationMask, out poseRoi, false));
      }

      _poseDetectionAnnotationController.DrawNow(poseDetection);
      _holisticAnnotationController.DrawNow(faceLandmarks, poseLandmarks, leftHandLandmarks, rightHandLandmarks);
      _poseWorldLandmarksAnnotationController.DrawNow(poseWorldLandmarks);
      _segmentationMaskAnnotationController.DrawNow(segmentationMask);
      _poseRoiAnnotationController.DrawNow(poseRoi);
    }

    private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs)
    {
      _poseDetectionAnnotationController.DrawLater(eventArgs.value);
    }

    private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      if (eventArgs.value != null)
      {
        FaceLandmarks = FaceLandmarkListToVector4Array(eventArgs.value);
      }

      _holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
    }

    private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
    }

    private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
    }


    private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
    {
      _holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);
    }


    private void Update()
    {
        pose.Solve(PoseLandmarks);
    }

    private Vector4[] PoseLandmarks = new Vector4[33];
    private Vector4[] FaceLandmarks = new Vector4[468];
    private PoseSolver pose;
    private Deal _deal;

    protected override IEnumerator Start()
    {
      anim = GameObject.Find("ybot").GetComponent<Animator>();
      pose = new PoseSolver(anim);
      _deal = new Deal(anim);
      return base.Start();
    }

    private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs)
    {
      if (eventArgs.value != null)
      {
        PoseLandmarks = PoseLandmarkListToVector4Array(eventArgs.value);
      }
      _poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
    }

    private Vector4[] FaceLandmarkListToVector4Array(NormalizedLandmarkList list)
    {
      Vector4[] result = new Vector4[list.Landmark.Count];
      for (int i = 0; i < list.Landmark.Count; i++)
      {
        result[i].x = list.Landmark[i].X;
        result[i].y = list.Landmark[i].Y;
        result[i].z = -list.Landmark[i].Z;
        result[i].w = list.Landmark[i].Visibility;
      }

      return result;
    }

    private Vector4[] PoseLandmarkListToVector4Array(LandmarkList list)
    {
      Vector4[] result = new Vector4[list.Landmark.Count];
      for (int i = 0; i < list.Landmark.Count; i++)
      {
        result[i].x = list.Landmark[i].X;
        result[i].y = -list.Landmark[i].Y;
        result[i].z = list.Landmark[i].Z;
        result[i].w = list.Landmark[i].Visibility;
      }

      return result;
    }

    private Animator anim;

    private void OnSegmentationMaskOutput(object stream, OutputEventArgs<ImageFrame> eventArgs)
    {
      _segmentationMaskAnnotationController.DrawLater(eventArgs.value);
    }

    private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs)
    {
      _poseRoiAnnotationController.DrawLater(eventArgs.value);
    }
  }
}
