using System;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutCreator
    {
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeRenderingCommands _renderingCommands;
        private readonly CgeCreatorEffector _creatorEffector;

        public CgeLayoutCreator(CgeEditorEffector editorEffector, CgeRenderingCommands renderingCommands, CgeCreatorEffector creatorEffector)
        {
            _editorEffector = editorEffector;
            _renderingCommands = renderingCommands;
            _creatorEffector = creatorEffector;
        }

        public void Layout(Action repaintCallback)
        {
            var previous = _creatorEffector.SkinnedMeshBeingFocused;
            var current = (SkinnedMeshRenderer) EditorGUILayout.ObjectField(previous, typeof(SkinnedMeshRenderer), true);
            if (previous != current)
            {
                SkinnedMeshChanged(current, repaintCallback);
            }

            GUILayout.Space(40);

            var allPreviews = _creatorEffector.Previews();
            var mod = 9;
            var element = 0;
            foreach (var preview in allPreviews)
            {
                GUILayout.Space(40);
                GUILayout.BeginArea(CgeLayoutCommon.RectAt(element % mod, element / mod));
                GUILayout.BeginArea(new Rect(0, 0, CgeLayoutCommon.GuiSquareWidth, CgeLayoutCommon.GuiSquareHeight));
                GUILayout.Box(preview.Preview.RenderTexture, GUIStyle.none, GUILayout.Width(CgeLayoutCommon.GuiSquareWidth), GUILayout.Height(CgeLayoutCommon.GuiSquareHeight - 20));
                GUILayout.EndArea();
                GUILayout.Space(CgeLayoutCommon.GuiSquareHeight - 35);
                GUILayout.Label(preview.Name + "");
                GUILayout.EndArea();
                element++;
            }
        }

        private void SkinnedMeshChanged(SkinnedMeshRenderer current, Action repaintCallback)
        {
            _creatorEffector.SkinnedMeshBeingFocused = current;
            if (current == null) return;

            _creatorEffector.Update(_editorEffector.PreviewSetup().avatarDescriptor.transform, CgeLayoutCommon.PictureWidth * 2, CgeLayoutCommon.PictureHeight * 2);
            _creatorEffector.GeneratePreviews(_renderingCommands, preview => repaintCallback());
        }
    }
}
