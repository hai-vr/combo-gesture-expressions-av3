using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public enum Side
    {
        Left, Right
    }

    public class CgeDecider
    {
        public List<SideDecider> Left;
        public List<SideDecider> Right;
        public List<IntersectionDecider> Intersection;
    }

    public enum IntersectionChoice
    {
        UseLeft, UseRight, UseNone
    }

    public struct SideDecider
    {
        public SideDecider(CgeCurveKey key, float sampleValue, bool choice)
        {
            Key = key;
            SampleValue = sampleValue;
            Choice = choice;
        }

        public CgeCurveKey Key { get; }
        public float SampleValue { get; }
        public bool Choice { get; set; }
    }

    public struct IntersectionDecider
    {
        public IntersectionDecider(CgeCurveKey key, float sampleLeftValue, float sampleRightValue, IntersectionChoice choice)
        {
            Key = key;
            SampleLeftValue = sampleLeftValue;
            SampleRightValue = sampleRightValue;
            Choice = choice;
        }

        public CgeCurveKey Key { get; }
        public float SampleLeftValue { get; }
        public float SampleRightValue { get; }
        public IntersectionChoice Choice { get; set; }
    }

    public class CgeActivityEditorCombiner
    {
        public const int CombinerPreviewWidth = 240;
        public const int CombinerPreviewHeight = 160;
        public const float CombinerPreviewCenterScale = 2f;
        public const int CombinerPreviewCenterWidth = (int) (CombinerPreviewWidth * CombinerPreviewCenterScale);
        public const int CombinerPreviewCenterHeight = (int) (CombinerPreviewHeight * CombinerPreviewCenterScale);

        private Texture2D _leftPreview;
        private Texture2D _rightPreview;
        private Texture2D _combinedPreview;
        private readonly AnimationClip _leftAnim;
        private readonly AnimationClip _rightAnim;
        private readonly Action _repaintCallback;
        private readonly CgeEditorHandler _editorHandler;
        private readonly EeRenderingCommands _renderingCommands;
        private CgeDecider _cgeDecider;
        private AnimationClip _combinedAnim;

        public CgeActivityEditorCombiner(AnimationClip leftAnim, AnimationClip rightAnim, Action repaintCallback, CgeEditorHandler editorHandler, EeRenderingCommands renderingCommands)
        {
            _leftPreview = EeRenderingCommands.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight);
            _rightPreview = EeRenderingCommands.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight);
            _combinedPreview = EeRenderingCommands.NewPreviewTexture2D(CombinerPreviewCenterWidth, CombinerPreviewCenterHeight);
            _leftAnim = leftAnim;
            _rightAnim = rightAnim;
            _combinedAnim = new AnimationClip();
            _repaintCallback = repaintCallback;
            _editorHandler = editorHandler;
            _renderingCommands = renderingCommands;
        }

        public void Prepare()
        {
            var leftCurves = FilterAnimationClip(_leftAnim);
            var rightCurves = FilterAnimationClip(_rightAnim);

            _cgeDecider = CreateDeciders(leftCurves, rightCurves);

            CreatePreviews();
        }

        private void RegenerateCombinedClip()
        {
            var generatedClip = GenerateCombinedClipInternal();

            _combinedAnim.ClearCurves();
            foreach (var editorCurveBinding in AnimationUtility.GetCurveBindings(generatedClip))
            {
                AnimationUtility.SetEditorCurve(_combinedAnim, editorCurveBinding, AnimationUtility.GetEditorCurve(generatedClip, editorCurveBinding));
            }
            foreach (var editorCurveBinding in AnimationUtility.GetObjectReferenceCurveBindings(generatedClip))
            {
                AnimationUtility.SetObjectReferenceCurve(_combinedAnim, editorCurveBinding, AnimationUtility.GetObjectReferenceCurve(generatedClip, editorCurveBinding));
            }

            var settings = AnimationUtility.GetAnimationClipSettings(generatedClip);
            AnimationUtility.SetAnimationClipSettings(_combinedAnim, settings);
        }

        private AnimationClip GenerateCombinedClipInternal()
        {
            var generatedClip = new AnimationClip();

            var leftClipSettings = AnimationUtility.GetAnimationClipSettings(_leftAnim);
            AnimationUtility.SetAnimationClipSettings(generatedClip, leftClipSettings);

            var leftSide = AllActiveOf(_cgeDecider.Left)
                .Concat(AllIntersectOf(IntersectionChoice.UseLeft));

            var rightSide = AllActiveOf(_cgeDecider.Right)
                .Concat(AllIntersectOf(IntersectionChoice.UseRight));

            MutateClipUsing(_leftAnim, generatedClip, leftSide);
            MutateClipUsing(_rightAnim, generatedClip, rightSide);

            return generatedClip;
        }

        private void MutateClipUsing(AnimationClip source, AnimationClip destination, IEnumerable<CgeCurveKey> curvesToKeep)
        {
            AnimationUtility.GetCurveBindings(source)
                .Where(binding => curvesToKeep.Contains(CgeCurveKey.FromBinding(binding)))
                .ToList()
                .ForEach(binding =>
                {
                    var curve = AnimationUtility.GetEditorCurve(source, binding);
                    AnimationUtility.SetEditorCurve(destination, binding, curve);
                });
        }

        private List<CgeCurveKey> AllIntersectOf(IntersectionChoice intersectionChoice)
        {
            return _cgeDecider.Intersection
                .Where(decider => decider.Choice == intersectionChoice)
                .Select(decider => decider.Key)
                .ToList();
        }

        private List<CgeCurveKey> AllActiveOf(List<SideDecider> sideDeciders)
        {
            return sideDeciders
                .Where(decider => decider.Choice)
                .Select(decider => decider.Key)
                .ToList();
        }

        private static CgeDecider CreateDeciders(HashSet<CgeSampledCurveKey> leftCurves, HashSet<CgeSampledCurveKey> rightCurves)
        {
            var leftUniques = new HashSet<CgeCurveKey>(leftCurves.Select(key => key.CurveKey).ToList());
            var rightUniques = new HashSet<CgeCurveKey>(rightCurves.Select(key => key.CurveKey).ToList());

            var leftDecidersUnsorted = leftCurves
                .Where(key => !rightUniques.Contains(key.CurveKey))
                .Select(key => new SideDecider(key.CurveKey, key.SampleValue, true))
                .ToList();
            var leftDeciders = leftDecidersUnsorted
                .Where(decider => decider.SampleValue != 0)
                .Concat(leftDecidersUnsorted
                    .Where(decider => decider.SampleValue == 0))
                .ToList();

            var rightDecidersUnsorted = rightCurves
                .Where(key => !leftUniques.Contains(key.CurveKey))
                .Select(key => new SideDecider(key.CurveKey, key.SampleValue, true))
                .ToList();
            var rightDeciders = rightDecidersUnsorted
                .Where(decider => decider.SampleValue != 0)
                .Concat(rightDecidersUnsorted
                    .Where(decider => decider.SampleValue == 0))
                .ToList();

            var intersectionDecidersUnsorted = leftCurves
                .Where(key => rightUniques.Contains(key.CurveKey))
                .Select(key =>
                {
                    var leftValue = key.SampleValue;
                    var rightValue = rightCurves.First(curveKey => curveKey.CurveKey == key.CurveKey).SampleValue;
                    return new IntersectionDecider(
                        key.CurveKey,
                        leftValue,
                        rightValue,
                        leftValue >= rightValue ? IntersectionChoice.UseLeft : IntersectionChoice.UseRight);
                })
                .ToList();

            var intersectionDeciders = intersectionDecidersUnsorted
                .Where(decider => decider.SampleLeftValue != decider.SampleRightValue
                    && decider.SampleLeftValue != 0 && decider.SampleRightValue != 0)
                .Concat(intersectionDecidersUnsorted
                    .Where(decider => decider.SampleLeftValue != decider.SampleRightValue
                    && (decider.SampleLeftValue == 0 || decider.SampleRightValue == 0)))
                .Concat(intersectionDecidersUnsorted
                    .Where(decider => decider.SampleLeftValue == decider.SampleRightValue && decider.SampleLeftValue != 0))
                .Concat(intersectionDecidersUnsorted
                    .Where(decider => decider.SampleLeftValue == decider.SampleRightValue && decider.SampleLeftValue == 0))
                .ToList();

            return new CgeDecider {Left = leftDeciders, Right = rightDeciders, Intersection = intersectionDeciders};
        }

        public CgeDecider GetDecider()
        {
            return _cgeDecider;
        }

        public void UpdateSide(Side side, CgeCurveKey keyToUpdate, float sampleValue, bool newChoice)
        {
            var sideDeciders = PickSide(side);
            var index = sideDeciders.FindIndex(decider => decider.Key == keyToUpdate);
            sideDeciders[index] = new SideDecider(keyToUpdate, sampleValue, newChoice);

            RegenerateCombinedPreview();
        }

        public void UpdateIntersection(IntersectionDecider intersectionDecider, IntersectionChoice newChoice)
        {
            var index = _cgeDecider.Intersection.FindIndex(decider => decider.Key == intersectionDecider.Key);
            _cgeDecider.Intersection[index] = new IntersectionDecider(intersectionDecider.Key, intersectionDecider.SampleLeftValue, intersectionDecider.SampleRightValue, newChoice);

            RegenerateCombinedPreview();
        }

        private List<SideDecider> PickSide(Side side)
        {
            switch (side)
            {
                case Side.Left:
                    return _cgeDecider.Left;
                case Side.Right:
                    return _cgeDecider.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        private HashSet<CgeSampledCurveKey> FilterAnimationClip(AnimationClip clip)
        {
            return new HashSet<CgeSampledCurveKey>(AnimationUtility.GetCurveBindings(clip)
                .Select(binding => new CgeSampledCurveKey(CgeCurveKey.FromBinding(binding), AnimationUtility.GetEditorCurve(clip, binding).keys[0].value))
                .Where(sampledCurveKey => !sampledCurveKey.CurveKey.IsMuscleCurve())
                .Where(sampledCurveKey => sampledCurveKey.CurveKey.Path != "_ignored")
                .ToList());
        }

        private void CreatePreviews()
        {
            if (!_editorHandler.IsPreviewSetupValid()) return;

            _leftPreview = _renderingCommands.RequireRender(_leftAnim, _repaintCallback).Normal;
            _rightPreview = _renderingCommands.RequireRender(_rightAnim, _repaintCallback).Normal;

            RegenerateCombinedClip();
            _combinedPreview = _renderingCommands.RequireRender(_combinedAnim, _repaintCallback, true).Normal;
            _renderingCommands.InvalidateSome(_repaintCallback, _leftAnim, _rightAnim, _combinedAnim);
        }

        private void RegenerateCombinedPreview()
        {
            if (!_editorHandler.IsPreviewSetupValid()) return;

            RegenerateCombinedClip();
            _combinedPreview = _renderingCommands.RequireRender(_combinedAnim, _repaintCallback, true).Normal;
            _renderingCommands.InvalidateSome(_repaintCallback, _combinedAnim);
        }

        public Texture LeftTexture()
        {
            return _leftPreview;
        }

        public Texture RightTexture()
        {
            return _rightPreview;
        }

        public Texture CombinedTexture()
        {
            return _combinedPreview;
        }

        public AnimationClip SaveTo(string candidateFileName)
        {
            var copyOfCombinedAnimation = Object.Instantiate(_combinedAnim);

            var originalAssetPath = AssetDatabase.GetAssetPath(_leftAnim);
            var folder = originalAssetPath.Replace(Path.GetFileName(originalAssetPath), "");

            var finalFilename = candidateFileName + "__" + DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HHmmss") + ".anim";

            var finalPath = folder + finalFilename;
            AssetDatabase.CreateAsset(copyOfCombinedAnimation, finalPath);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(finalPath);
        }
    }
}
