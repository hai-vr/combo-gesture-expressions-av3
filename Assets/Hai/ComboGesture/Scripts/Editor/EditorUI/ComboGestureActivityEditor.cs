using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureActivity))]
    [CanEditMultipleObjects]
    public class ComboGestureActivityEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            _guideIcon32 = ComboGestureIcons.Instance.Guide32;
        }

        private bool _foldoutHelp;
        private Texture _guideIcon32;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Switch language (English / 日本語)"))
            {
                CgeLocalization.CycleLocale();
            }

            if (CgeLocalization.IsEnglishLocaleActive())
            {
                EditorGUILayout.LabelField("");
            }
            else
            {
                EditorGUILayout.LabelField("一部の翻訳は正確ではありません。cge.jp.jsonを編集することができます。");
            }

            _foldoutHelp = EditorGUILayout.Foldout(_foldoutHelp, new GUIContent("Help", _guideIcon32));
            if (_foldoutHelp)
            {
                if (GUILayout.Button(new GUIContent("Open documentation and tutorials", _guideIcon32)))
                {
                    Application.OpenURL("https://hai-vr.github.io/combo-gesture-expressions-av3/");
                }
            }

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Open_editor), GUILayout.Height(40)))
            {
                CgeWindowHandler.Obtain().ShowActivity((ComboGestureActivity)serializedObject.targetObject);
            }

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Editor window is not available in multi-editing.", MessageType.Info);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }
    }

    public class ComboGestureIcons
    {
        public readonly Texture Guide32 = LoadGuid("a28bf0ae55e45dc46a1a61b1afffb574");
        public readonly Texture Guide16 = LoadGuid("90dc72f3fb374d94e821a6ad173b5275");
        public readonly Texture Help16 = LoadGuid("56b8fe77cebd7fd449c4e2101ecb4744");

        public Texture Gesture(HandPose pose)
        {
            return _gestures[(int) pose];
        }

        private static ComboGestureIcons _instance;

        private readonly Texture[] _gestures = {
            // LoadGuid("56b8fe77cebd7fd449c4e2101ecb4744"),
            // LoadGuid("eace99aeb4760cf409ba23cf85ebc517"),
            // LoadGuid("3117be647abd8a249a8d25674e8e8384"),
            // LoadGuid("822629dcf5ae40c409148064e9076500"),
            // LoadGuid("2da273c081c852c42aa1955cd044247a"),
            // LoadGuid("be8bc96ae1098274dbe36f414b718a8d"),
            // LoadGuid("a55ca97ca49c51542a56070a646888b5"),
            // LoadGuid("76994cec0c2f44f4caf9724dfa235ade")
        };

        public static ComboGestureIcons Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComboGestureIcons();
                }

                return _instance;
            }
        }

        private static Texture LoadGuid(string guid)
        {
            return AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
