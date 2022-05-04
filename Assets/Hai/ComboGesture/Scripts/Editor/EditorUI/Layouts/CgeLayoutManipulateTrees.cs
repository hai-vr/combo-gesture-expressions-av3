using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutManipulateTrees
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorHandler _editorHandler;
        private readonly CgeBlendTreeHandler _blendTreeHandler;

        public CgeLayoutManipulateTrees(CgeLayoutCommon common, CgeEditorHandler editorHandler, CgeBlendTreeHandler blendTreeHandler)
        {
            _common = common;
            _editorHandler = editorHandler;
            _blendTreeHandler = blendTreeHandler;
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
            if (_editorHandler.GetCurrentlyEditing() == CurrentlyEditing.Puppet)
            {
                var value = _editorHandler.SpProperty("mainTree").objectReferenceValue;
                if (value != null && value is AnimationClip)
                {
                    _editorHandler.SpProperty("mainTree").objectReferenceValue = null;
                    treeBeingEdited = null;
                }
                else
                {
                    treeBeingEdited = (BlendTree)value;
                }
            }
            else
            {
                treeBeingEdited = _blendTreeHandler.BlendTreeBeingEdited;
            }

            if (showAsset)
            {
                // EditorGUI.BeginDisabledGroup(true);
                _blendTreeHandler.BlendTreeBeingEdited = (BlendTree) EditorGUILayout.ObjectField(treeBeingEdited, typeof(BlendTree), false);
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
            _blendTreeHandler.DescaleLevel = GUI.HorizontalSlider(
                new Rect(100, fixture - sliderThickness, position.width - 200, sliderThickness),
                _blendTreeHandler.DescaleLevel, 0.01f, 1f
            );
            _blendTreeHandler.HorizontalFocalPoint = GUI.HorizontalSlider(
                new Rect(sliderThickness, fixture, position.width - sliderThickness, sliderThickness),
                _blendTreeHandler.HorizontalFocalPoint, 0, 1f
            );
            _blendTreeHandler.VerticalFocalPoint = GUI.VerticalSlider(
                new Rect(0, fixture + sliderThickness, sliderThickness, position.height - fixture - headerPadding - sliderThickness),
                _blendTreeHandler.VerticalFocalPoint, 0, 1f
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
            var inv = 1 / _blendTreeHandler.DescaleLevel;
            var scaleHoz = (-_blendTreeHandler.HorizontalFocalPoint + 0.5f) * (1 - _blendTreeHandler.DescaleLevel) * inv * 2;
            var scaleVert = (-_blendTreeHandler.VerticalFocalPoint + 0.5f) * (1 - _blendTreeHandler.DescaleLevel) * inv * 2;
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
            if (_editorHandler.GetCurrentlyEditing() != CurrentlyEditing.Puppet) return;

            EditorGUILayout.PropertyField(_editorHandler.SpProperty("mainTree"), new GUIContent(CgeLocale.CGEE_Blend_tree_asset));
            EditorGUILayout.PropertyField(_editorHandler.SpTransitionDuration(), new GUIContent(CgeLocale.CGEE_Transition_duration));
        }

        private void LayoutBlendTreeAssetCreator()
        {
            if (_editorHandler.GetCurrentlyEditing() == CurrentlyEditing.Puppet && _editorHandler.GetPuppet().mainTree != null) return;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(CgeLocale.CGEE_Create_a_new_blend_tree, EditorStyles.boldLabel);
            _blendTreeHandler.CurrentTemplate = (PuppetTemplate) EditorGUILayout.EnumPopup("Template", _blendTreeHandler.CurrentTemplate);
            switch (_blendTreeHandler.CurrentTemplate)
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

            var isFistRelated = _blendTreeHandler.CurrentTemplate == PuppetTemplate.SingleAnalogFistWithHairTrigger
                                || _blendTreeHandler.CurrentTemplate == PuppetTemplate.SingleAnalogFistAndTwoDirections
                                || _blendTreeHandler.CurrentTemplate == PuppetTemplate.DualAnalogFist;
            _blendTreeHandler.MiddleClip = (AnimationClip) EditorGUILayout.ObjectField(
                isFistRelated ? CgeLocale.CGEE_TreeAnimationAtRest : CgeLocale.CGEE_TreeJoystickCenterAnimation, _blendTreeHandler.MiddleClip, typeof(AnimationClip), false);
            if (!isFistRelated) {
                _blendTreeHandler.CenterSafety = EditorGUILayout.Toggle(CgeLocale.CGEE_TreeFixJoystickSnapping, _blendTreeHandler.CenterSafety);
                _blendTreeHandler.Maximum = EditorGUILayout.Slider(CgeLocale.CGEE_TreeJoystickMaximumTilt, _blendTreeHandler.Maximum, 0.1f, 1.0f);
            }

            if (GUILayout.Button(CgeLocale.CGEE_TreeCreateAsset))
            {
                var createdTree = MaybeCreateNewBlendTreeAsset();
                if (createdTree != null)
                {
                    if (_editorHandler.GetCurrentlyEditing() == CurrentlyEditing.Puppet)
                    {
                        _editorHandler.GetPuppet().mainTree = createdTree;
                    }
                    else
                    {
                        _blendTreeHandler.BlendTreeBeingEdited = createdTree;
                        _editorHandler.SwitchAdditionalEditorTo(AdditionalEditorsMode.ViewBlendTrees);
                    }
                }
            }
        }

        private BlendTree MaybeCreateNewBlendTreeAsset()
        {
            var savePath = EditorUtility.SaveFilePanel(CgeLocale.CGEE_TreeFileCreate, Application.dataPath, "", "asset");
            if (savePath == null || savePath.Trim() == "") return null;
            if (!savePath.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(CgeLocale.CGEE_TreeFileInvalidSavePath, CgeLocale.CGEE_TreeFileInvalidSavePathMessage, "OK");
                return null;
            }

            var assetPath = "Assets" + savePath.Substring(Application.dataPath.Length);
            var blendTreeToSave = _blendTreeHandler.CreateBlendTreeAsset();
            AssetDatabase.CreateAsset(blendTreeToSave, assetPath);
            EditorGUIUtility.PingObject(blendTreeToSave);

            return blendTreeToSave;
        }
    }
}
