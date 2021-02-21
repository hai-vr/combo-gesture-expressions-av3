using System;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutManipulateTrees
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeBlendTreeEffector _blendTreeEffector;

        public CgeLayoutManipulateTrees(CgeLayoutCommon common, CgeEditorEffector editorEffector, CgeBlendTreeEffector blendTreeEffector)
        {
            _common = common;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
        }

        public void Layout(Rect position)
        {
            LayoutPuppetSpecifics();
            LayoutBlendTreeAssetCreator();
            LayoutBlendTreeViewer(position, false);
        }

        public void LayoutAssetCreator(Rect position)
        {
            LayoutBlendTreeAssetCreator();
        }

        public void LayoutTreeViewer(Rect position)
        {
            LayoutBlendTreeViewer(position, true);
        }

        private void LayoutBlendTreeViewer(Rect position, bool showAsset)
        {
            BlendTree treeBeingEdited;
            if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Puppet)
            {
                var value = _editorEffector.SpProperty("mainTree").objectReferenceValue;
                if (value != null && value is AnimationClip)
                {
                    _editorEffector.SpProperty("mainTree").objectReferenceValue = null;
                    treeBeingEdited = null;
                }
                else
                {
                    treeBeingEdited = (BlendTree)value;
                }
            }
            else
            {
                treeBeingEdited = _blendTreeEffector.BlendTreeBeingEdited;
            }

            if (showAsset)
            {
                // EditorGUI.BeginDisabledGroup(true);
                _blendTreeEffector.BlendTreeBeingEdited = (BlendTree) EditorGUILayout.ObjectField(treeBeingEdited, typeof(BlendTree), false);
                // EditorGUI.EndDisabledGroup();
            }

            if (treeBeingEdited == null) return;

            var is2D = treeBeingEdited.blendType == BlendTreeType.Simple1D || treeBeingEdited.blendType == BlendTreeType.Direct;
            var xMin = treeBeingEdited.children.Select(motion => is2D ? motion.threshold : motion.position.x).Min();
            var yMin = treeBeingEdited.children.Select(motion => is2D ? 0 : motion.position.y).Min();
            var xMax = treeBeingEdited.children.Select(motion => is2D ? motion.threshold : motion.position.x).Max();
            var yMax = treeBeingEdited.children.Select(motion => is2D ? 1 : motion.position.y).Max();
            var min = new Vector2(xMin, yMin);
            var max = new Vector2(xMax, yMax);

            var headerPadding = 5 * CgeLayoutCommon.SingleLineHeight;
            var fixture = 5 * CgeLayoutCommon.SingleLineHeight;
            var sliderThickness = CgeLayoutCommon.SingleLineHeight * 2;
            _blendTreeEffector.DescaleLevel = GUI.HorizontalSlider(
                new Rect(100, fixture - sliderThickness, position.width - 200, sliderThickness),
                _blendTreeEffector.DescaleLevel, 0.01f, 1f
            );
            _blendTreeEffector.HorizontalFocalPoint = GUI.HorizontalSlider(
                new Rect(sliderThickness, fixture, position.width - sliderThickness, sliderThickness),
                _blendTreeEffector.HorizontalFocalPoint, 0, 1f
            );
            _blendTreeEffector.VerticalFocalPoint = GUI.VerticalSlider(
                new Rect(0, fixture + sliderThickness, sliderThickness, position.height - fixture - headerPadding - sliderThickness),
                _blendTreeEffector.VerticalFocalPoint, 0, 1f
            );

            GUILayout.BeginArea(new Rect(
                sliderThickness,
                fixture + sliderThickness,
                position.width - sliderThickness,
                position.height - fixture - sliderThickness - CgeLayoutCommon.SingleLineHeight * 4));
            foreach (var child in treeBeingEdited.children)
            {
                var posVec = is2D
                    ? new Vector2(child.threshold, 0.5f)
                    : child.position;
                var centered = CalculateCentered(posVec, min, max, position, headerPadding, fixture, sliderThickness);
                GUILayout.BeginArea(centered);
                GUILayout.BeginArea(new Rect(0, 0, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
                _common.DrawPreviewOrRefreshButton(child.motion);
                GUILayout.EndArea();
                GUILayout.EndArea();
            }
            GUILayout.EndArea();
        }

        private Rect CalculateCentered(Vector2 childPosition, Vector2 min, Vector2 max, Rect position, float headerPadding, float fixture, float sliderThickness)
        {
            var inv = 1 / _blendTreeEffector.DescaleLevel;
            var scaleHoz = (-_blendTreeEffector.HorizontalFocalPoint + 0.5f) * (1 - _blendTreeEffector.DescaleLevel) * inv * 2;
            var scaleVert = (-_blendTreeEffector.VerticalFocalPoint + 0.5f) * (1 - _blendTreeEffector.DescaleLevel) * inv * 2;
            var xScalar = Mathf.Lerp(
                -inv + 1f + scaleHoz,
                inv + scaleHoz,
                Mathf.InverseLerp(min.x, max.x, childPosition.x));
            var yScalar = Mathf.Lerp(
                -inv + 1f + scaleVert,
                inv + scaleVert,
                Mathf.InverseLerp(max.y, min.y, childPosition.y));

            return new Rect(
                xScalar * (position.width - CgeLayoutCommon.GuiSquareWidth),
                yScalar * (position.height - CgeLayoutCommon.GuiSquareHeight - headerPadding - fixture - sliderThickness),
                CgeLayoutCommon.GuiSquareWidth,
                CgeLayoutCommon.GuiSquareHeight);
        }

        private void LayoutPuppetSpecifics()
        {
            if (_editorEffector.GetCurrentlyEditing() != CurrentlyEditing.Puppet) return;

            EditorGUILayout.PropertyField(_editorEffector.SpProperty("mainTree"), new GUIContent(CgeLocale.CGEE_Blend_tree_asset));
            EditorGUILayout.PropertyField(_editorEffector.SpTransitionDuration(), new GUIContent(CgeLocale.CGEE_Transition_duration));
        }

        private void LayoutBlendTreeAssetCreator()
        {
            if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Puppet && _editorEffector.GetPuppet().mainTree != null) return;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(CgeLocale.CGEE_Create_a_new_blend_tree, EditorStyles.boldLabel);
            _blendTreeEffector.CurrentTemplate = (PuppetTemplate) EditorGUILayout.EnumPopup("Template", _blendTreeEffector.CurrentTemplate);
            switch (_blendTreeEffector.CurrentTemplate)
            {
                case PuppetTemplate.FourDirections:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainFourDirections, MessageType.Info);
                    break;
                case PuppetTemplate.EightDirections:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainEightDirections, MessageType.Info);
                    break;
                case PuppetTemplate.SixDirectionsPointingForward:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainSixDirectionsPointingForward, MessageType.Info);
                    break;
                case PuppetTemplate.SixDirectionsPointingSideways:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainSixDirectionsPointingSideways, MessageType.Info);
                    break;
                case PuppetTemplate.SingleAnalogFistWithHairTrigger:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainSingleAnalogFistWithHairTrigger, MessageType.Info);
                    break;
                case PuppetTemplate.SingleAnalogFistAndTwoDirections:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainSingleAnalogFistAndTwoDirections, MessageType.Info);
                    break;
                case PuppetTemplate.DualAnalogFist:
                    EditorGUILayout.HelpBox(CgeLocale.CGEE_ExplainDualAnalogFist, MessageType.Info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var isFistRelated = _blendTreeEffector.CurrentTemplate == PuppetTemplate.SingleAnalogFistWithHairTrigger
                                || _blendTreeEffector.CurrentTemplate == PuppetTemplate.SingleAnalogFistAndTwoDirections
                                || _blendTreeEffector.CurrentTemplate == PuppetTemplate.DualAnalogFist;
            _blendTreeEffector.MiddleClip = (AnimationClip) EditorGUILayout.ObjectField(
                isFistRelated ? "Animation at rest" : "Joystick center animation", _blendTreeEffector.MiddleClip, typeof(AnimationClip), false);
            if (!isFistRelated) {
                _blendTreeEffector.CenterSafety = EditorGUILayout.Toggle("Fix joystick snapping", _blendTreeEffector.CenterSafety);
                _blendTreeEffector.Maximum = EditorGUILayout.Slider("Joystick maximum tilt", _blendTreeEffector.Maximum, 0.1f, 1.0f);
            }

            if (GUILayout.Button("Create a new blend tree asset"))
            {
                var createdTree = MaybeCreateNewBlendTreeAsset();
                if (createdTree != null)
                {
                    if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Puppet)
                    {
                        _editorEffector.GetPuppet().mainTree = createdTree;
                    }
                    else
                    {
                        _blendTreeEffector.BlendTreeBeingEdited = createdTree;
                        _editorEffector.SwitchAdditionalEditorTo(AdditionalEditorsMode.ViewBlendTrees);
                    }
                }
            }
        }

        private BlendTree MaybeCreateNewBlendTreeAsset()
        {
            var savePath = EditorUtility.SaveFilePanel("Create...", Application.dataPath, "", "asset");
            if (savePath == null || savePath.Trim() == "") return null;
            if (!savePath.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog("Invalid save path", "Save path must be in the project's /Assets path.", "OK");
                return null;
            }

            var assetPath = "Assets" + savePath.Substring(Application.dataPath.Length);
            var blendTreeToSave = _blendTreeEffector.CreateBlendTreeAsset();
            AssetDatabase.CreateAsset(blendTreeToSave, assetPath);
            EditorGUIUtility.PingObject(blendTreeToSave);

            return blendTreeToSave;
        }
    }
}
