using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
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
        public SideDecider(CurveKey key, float sampleValue, bool choice)
        {
            Key = key;
            SampleValue = sampleValue;
            Choice = choice;
        }

        public CurveKey Key { get; }
        public float SampleValue { get; }
        public bool Choice { get; set; }
    }

    public struct IntersectionDecider
    {
        public IntersectionDecider(CurveKey key, float sampleLeftValue, float sampleRightValue, IntersectionChoice choice)
        {
            Key = key;
            SampleLeftValue = sampleLeftValue;
            SampleRightValue = sampleRightValue;
            Choice = choice;
        }

        public CurveKey Key { get; }
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

        private readonly EeRenderingSample _leftPreview;
        private readonly EeRenderingSample _rightPreview;
        private EeRenderingSample _combinedPreview;
        private readonly Action _onClipRenderedFn;
        private readonly CgeEditorEffector _editorEffector;
        private readonly EeRenderingCommands _previewController;
        private CgeDecider _cgeDecider;

        public CgeActivityEditorCombiner(AnimationClip leftAnim, AnimationClip rightAnim, Action onClipRenderedFn, CgeEditorEffector editorEffector, EeRenderingCommands previewController)
        {
            _leftPreview = new EeRenderingSample(leftAnim, CgeMemoryQuery.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight), OnClipRendered);
            _rightPreview = new EeRenderingSample(rightAnim, CgeMemoryQuery.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight), OnClipRendered);
            _combinedPreview = new EeRenderingSample(new AnimationClip(), CgeMemoryQuery.NewPreviewTexture2D(CombinerPreviewCenterWidth, CombinerPreviewCenterHeight), OnClipRendered);
            _onClipRenderedFn = onClipRenderedFn;
            _editorEffector = editorEffector;
            _previewController = previewController;
        }

        public void Prepare()
        {
            var leftCurves = FilterAnimationClip(_leftPreview.Clip);
            var rightCurves = FilterAnimationClip(_rightPreview.Clip);

            _cgeDecider = CreateDeciders(leftCurves, rightCurves);

            _combinedPreview = new EeRenderingSample(GenerateCombinedClip(), _combinedPreview.RenderTexture, OnClipRendered);

            CreatePreviews();
        }

        private AnimationClip GenerateCombinedClip()
        {
            var generatedClip = new AnimationClip();

            var leftClipSettings = AnimationUtility.GetAnimationClipSettings(_leftPreview.Clip);
            AnimationUtility.SetAnimationClipSettings(generatedClip, leftClipSettings);

            var leftSide = AllActiveOf(_cgeDecider.Left)
                .Concat(AllIntersectOf(IntersectionChoice.UseLeft));

            var rightSide = AllActiveOf(_cgeDecider.Right)
                .Concat(AllIntersectOf(IntersectionChoice.UseRight));

            MutateClipUsing(_leftPreview.Clip, generatedClip, leftSide);
            MutateClipUsing(_rightPreview.Clip, generatedClip, rightSide);

            return generatedClip;
        }

        private void MutateClipUsing(AnimationClip source, AnimationClip destination, IEnumerable<CurveKey> curvesToKeep)
        {
            AnimationUtility.GetCurveBindings(source)
                .Where(binding => curvesToKeep.Contains(CurveKey.FromBinding(binding)))
                .ToList()
                .ForEach(binding =>
                {
                    var curve = AnimationUtility.GetEditorCurve(source, binding);
                    AnimationUtility.SetEditorCurve(destination, binding, curve);
                });
        }

        private List<CurveKey> AllIntersectOf(IntersectionChoice intersectionChoice)
        {
            return _cgeDecider.Intersection
                .Where(decider => decider.Choice == intersectionChoice)
                .Select(decider => decider.Key)
                .ToList();
        }

        private List<CurveKey> AllActiveOf(List<SideDecider> sideDeciders)
        {
            return sideDeciders
                .Where(decider => decider.Choice)
                .Select(decider => decider.Key)
                .ToList();
        }

        private static CgeDecider CreateDeciders(HashSet<SampledCurveKey> leftCurves, HashSet<SampledCurveKey> rightCurves)
        {
            var leftUniques = new HashSet<CurveKey>(leftCurves.Select(key => key.CurveKey).ToList());
            var rightUniques = new HashSet<CurveKey>(rightCurves.Select(key => key.CurveKey).ToList());

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

        public void UpdateSide(Side side, CurveKey keyToUpdate, float sampleValue, bool newChoice)
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

        private HashSet<SampledCurveKey> FilterAnimationClip(AnimationClip clip)
        {
            return new HashSet<SampledCurveKey>(AnimationUtility.GetCurveBindings(clip)
                .Select(binding => new SampledCurveKey(CurveKey.FromBinding(binding), AnimationUtility.GetEditorCurve(clip, binding).keys[0].value))
                .Where(sampledCurveKey => !sampledCurveKey.CurveKey.IsMuscleCurve())
                .Where(sampledCurveKey => sampledCurveKey.CurveKey.Path != "_ignored")
                .ToList());
        }

        private void CreatePreviews()
        {
            if (!_editorEffector.IsPreviewSetupValid()) return;

            var animationsPreviews = new[] {_leftPreview, _rightPreview, _combinedPreview}.ToList();

            _previewController.GenerateSpecific(animationsPreviews, _editorEffector.PreviewSetup());
        }

        private void RegenerateCombinedPreview()
        {
            if (!_editorEffector.IsPreviewSetupValid()) return;

            _combinedPreview = new EeRenderingSample(GenerateCombinedClip(), _combinedPreview.RenderTexture, OnClipRendered);

            var animationsPreviews = new[] {_combinedPreview}.ToList();

            _previewController.GenerateSpecific(animationsPreviews, _editorEffector.PreviewSetup());
        }

        private void OnClipRendered(EeRenderingSample obj)
        {
            _onClipRenderedFn.Invoke();
        }

        public Texture LeftTexture()
        {
            return _leftPreview.RenderTexture;
        }

        public Texture RightTexture()
        {
            return _rightPreview.RenderTexture;
        }

        public Texture CombinedTexture()
        {
            return _combinedPreview.RenderTexture;
        }

        public AnimationClip SaveTo(string candidateFileName)
        {
            var copyOfCombinedAnimation = Object.Instantiate(_combinedPreview.Clip);

            var originalAssetPath = AssetDatabase.GetAssetPath(_leftPreview.Clip);
            var folder = originalAssetPath.Replace(Path.GetFileName(originalAssetPath), "");

            var finalFilename = candidateFileName + "__" + DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HHmmss") + ".anim";

            var finalPath = folder + finalFilename;
            AssetDatabase.CreateAsset(copyOfCombinedAnimation, finalPath);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(finalPath);
        }
    }
}
