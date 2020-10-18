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
        private readonly CgeActivityEditorDriver _driver;
        private readonly CgeEditorEffector _editorEffector;
        private readonly Action _repaint;
        private readonly CgeBlendTreeEffector _blendTreeEffector;

        public CgeLayoutManipulateTrees(CgeLayoutCommon common, CgeActivityEditorDriver driver, CgeEditorEffector editorEffector, Action repaint, CgeBlendTreeEffector blendTreeEffector)
        {
            _common = common;
            _driver = driver;
            _editorEffector = editorEffector;
            _repaint = repaint;
            _blendTreeEffector = blendTreeEffector;
        }

        public void Layout(Rect position)
        {
            LayoutPuppetSpecifics();
            LayoutBlendTreeAssetCreator();
            LayoutBlendTreeViewer(position);
        }

        private void LayoutBlendTreeViewer(Rect position)
        {
            var mainTree = (BlendTree)_editorEffector.SpProperty("mainTree").objectReferenceValue;
            if (mainTree == null) return;

            var xMin = mainTree.children.Select(motion => motion.position.x).Min();
            var yMin = mainTree.children.Select(motion => motion.position.y).Min();
            var xMax = mainTree.children.Select(motion => motion.position.x).Max();
            var yMax = mainTree.children.Select(motion => motion.position.y).Max();
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
            foreach (var child in mainTree.children)
            {
                var centered = CalculateCentered(child.position, min, max, position, headerPadding, fixture, sliderThickness);
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

            EditorGUILayout.PropertyField(_editorEffector.SpProperty("mainTree"), new GUIContent("Blend tree asset"));
            EditorGUILayout.PropertyField(_editorEffector.SpTransitionDuration(), new GUIContent("Transition duration (s)"));
        }

        private void LayoutBlendTreeAssetCreator()
        {
            if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Puppet && _editorEffector.GetPuppet().mainTree != null) return;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Create a new blend tree", EditorStyles.boldLabel);
            _blendTreeEffector.CurrentTemplate = (PuppetTemplate) EditorGUILayout.EnumPopup("Template", _blendTreeEffector.CurrentTemplate);
            _blendTreeEffector.MiddleClip = (AnimationClip) EditorGUILayout.ObjectField(
                _blendTreeEffector.CurrentTemplate == PuppetTemplate.SingleAnalogWithHairTrigger
                    ? "Joystick center animation"
                    : "Animation at rest", _blendTreeEffector.MiddleClip, typeof(AnimationClip), false);
            EditorGUI.BeginDisabledGroup(_blendTreeEffector.CurrentTemplate == PuppetTemplate.SingleAnalogWithHairTrigger);
            _blendTreeEffector.CenterSafety = EditorGUILayout.Toggle("Fix joystick snapping", _blendTreeEffector.CenterSafety);
            _blendTreeEffector.Maximum = EditorGUILayout.Slider("Joystick maximum tilt", _blendTreeEffector.Maximum, 0.9f, 1.0f);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Create a new blend tree asset"))
            {
                var createdTree = CreateNewBlendTreeAsset();
                if (createdTree != null)
                {
                    if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Puppet)
                    {
                        _editorEffector.GetPuppet().mainTree = createdTree;
                    }
                }
            }
        }

        private BlendTree CreateNewBlendTreeAsset()
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
