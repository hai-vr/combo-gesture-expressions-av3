using System;
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
            _blendTreeEffector.MiddleClip = (AnimationClip) EditorGUILayout.ObjectField("Joystick center animation", _blendTreeEffector.MiddleClip, typeof(AnimationClip), false);
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
