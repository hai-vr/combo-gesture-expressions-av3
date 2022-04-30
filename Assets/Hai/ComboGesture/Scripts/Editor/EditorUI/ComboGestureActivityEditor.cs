using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureActivity))]
    [CanEditMultipleObjects]
    public class ComboGestureActivityEditor : UnityEditor.Editor
    {
        public SerializedProperty anim00;
        public SerializedProperty anim01;
        public SerializedProperty anim02;
        public SerializedProperty anim03;
        public SerializedProperty anim04;
        public SerializedProperty anim05;
        public SerializedProperty anim06;
        public SerializedProperty anim07;
        public SerializedProperty anim11;
        public SerializedProperty anim12;
        public SerializedProperty anim13;
        public SerializedProperty anim14;
        public SerializedProperty anim15;
        public SerializedProperty anim16;
        public SerializedProperty anim17;
        public SerializedProperty anim22;
        public SerializedProperty anim23;
        public SerializedProperty anim24;
        public SerializedProperty anim25;
        public SerializedProperty anim26;
        public SerializedProperty anim27;
        public SerializedProperty anim33;
        public SerializedProperty anim34;
        public SerializedProperty anim35;
        public SerializedProperty anim36;
        public SerializedProperty anim37;
        public SerializedProperty anim44;
        public SerializedProperty anim45;
        public SerializedProperty anim46;
        public SerializedProperty anim47;
        public SerializedProperty anim55;
        public SerializedProperty anim56;
        public SerializedProperty anim57;
        public SerializedProperty anim66;
        public SerializedProperty anim67;
        public SerializedProperty anim77;
        // ReSharper disable once InconsistentNaming
        public SerializedProperty anim11_L;
        // ReSharper disable once InconsistentNaming
        public SerializedProperty anim11_R;

        public SerializedProperty anim10;
        public SerializedProperty anim20;
        public SerializedProperty anim21;
        public SerializedProperty anim30;
        public SerializedProperty anim31;
        public SerializedProperty anim32;
        public SerializedProperty anim40;
        public SerializedProperty anim41;
        public SerializedProperty anim42;
        public SerializedProperty anim43;
        public SerializedProperty anim50;
        public SerializedProperty anim51;
        public SerializedProperty anim52;
        public SerializedProperty anim53;
        public SerializedProperty anim54;
        public SerializedProperty anim60;
        public SerializedProperty anim61;
        public SerializedProperty anim62;
        public SerializedProperty anim63;
        public SerializedProperty anim64;
        public SerializedProperty anim65;
        public SerializedProperty anim70;
        public SerializedProperty anim71;
        public SerializedProperty anim72;
        public SerializedProperty anim73;
        public SerializedProperty anim74;
        public SerializedProperty anim75;
        public SerializedProperty anim76;
        public SerializedProperty enablePermutations;
        public SerializedProperty oneHandMode;

        public SerializedProperty transitionDuration;

        public SerializedProperty previewAnimator;
        public SerializedProperty editorLegacyFoldout;

        public ReorderableList blinkingReorderableList;

        private void OnEnable()
        {
            transitionDuration = serializedObject.FindProperty(nameof(ComboGestureActivity.transitionDuration));
            anim00 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim00));
            anim01 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim01));
            anim02 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim02));
            anim03 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim03));
            anim04 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim04));
            anim05 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim05));
            anim06 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim06));
            anim07 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim07));
            anim11 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim11));
            anim12 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim12));
            anim13 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim13));
            anim14 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim14));
            anim15 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim15));
            anim16 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim16));
            anim17 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim17));
            anim22 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim22));
            anim23 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim23));
            anim24 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim24));
            anim25 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim25));
            anim26 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim26));
            anim27 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim27));
            anim33 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim33));
            anim34 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim34));
            anim35 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim35));
            anim36 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim36));
            anim37 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim37));
            anim44 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim44));
            anim45 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim45));
            anim46 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim46));
            anim47 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim47));
            anim55 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim55));
            anim56 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim56));
            anim57 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim57));
            anim66 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim66));
            anim67 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim67));
            anim77 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim77));
            anim11_L = serializedObject.FindProperty(nameof(ComboGestureActivity.anim11_L));
            anim11_R = serializedObject.FindProperty(nameof(ComboGestureActivity.anim11_R));
            previewAnimator = serializedObject.FindProperty(nameof(ComboGestureActivity.previewAnimator));
            editorLegacyFoldout = serializedObject.FindProperty(nameof(ComboGestureActivity.editorLegacyFoldout));
            anim10 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim10));
            anim20 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim20));
            anim21 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim21));
            anim30 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim30));
            anim31 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim31));
            anim32 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim32));
            anim40 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim40));
            anim41 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim41));
            anim42 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim42));
            anim43 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim43));
            anim50 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim50));
            anim51 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim51));
            anim52 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim52));
            anim53 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim53));
            anim54 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim54));
            anim60 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim60));
            anim61 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim61));
            anim62 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim62));
            anim63 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim63));
            anim64 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim64));
            anim65 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim65));
            anim70 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim70));
            anim71 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim71));
            anim72 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim72));
            anim73 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim73));
            anim74 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim74));
            anim75 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim75));
            anim76 = serializedObject.FindProperty(nameof(ComboGestureActivity.anim76));
            enablePermutations = serializedObject.FindProperty(nameof(ComboGestureActivity.enablePermutations));
            oneHandMode = serializedObject.FindProperty(nameof(ComboGestureActivity.oneHandMode));

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            blinkingReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty(nameof(ComboGestureActivity.blinking)),
                true, true, true, true
            );
            blinkingReorderableList.drawElementCallback = BlinkingListElement;
            blinkingReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Closed eyes Animations (to disable blinking)");

            _guideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");
        }

        private void BlinkingListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = blinkingReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        }

        private bool _foldoutAll = true;
        private bool _foldoutFist;
        private bool _foldoutHandopen;
        private bool _foldoutFingerpoint;
        private bool _foldoutVictory;
        private bool _foldoutRocknroll;
        private bool _foldoutHandpistol;
        private bool _foldoutThumbsup;
        private bool _foldoutPermutations;

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

            if (!serializedObject.isEditingMultipleObjects) {
                editorLegacyFoldout.boolValue = EditorGUILayout.Foldout(editorLegacyFoldout.boolValue, "Legacy editor");
            }
            else
            {
                GUILayout.Label("Legacy editor / Multi editing", EditorStyles.boldLabel);
            }

            if (serializedObject.isEditingMultipleObjects || editorLegacyFoldout.boolValue)
            {
                LegacyEditor();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LegacyEditor()
        {
            _foldoutAll = EditorGUILayout.Foldout(_foldoutAll, "Show all gestures");
            if (_foldoutAll)
            {
                EditorGUILayout.PropertyField(anim00, new GUIContent("No gesture"));
                WhenFoldoutAll(null);
            }

            _foldoutFist = EditorGUILayout.Foldout(_foldoutFist, "Show Fist gestures");
            if (_foldoutFist)
            {
                WhenFoldoutAll("Fist");
            }

            _foldoutHandopen = EditorGUILayout.Foldout(_foldoutHandopen, "Show Open gestures");
            if (_foldoutHandopen)
            {
                WhenFoldoutAll("Open");
            }

            _foldoutFingerpoint = EditorGUILayout.Foldout(_foldoutFingerpoint, "Show Point gestures");
            if (_foldoutFingerpoint)
            {
                WhenFoldoutAll("Point");
            }

            _foldoutVictory = EditorGUILayout.Foldout(_foldoutVictory, "Show Victory gestures");
            if (_foldoutVictory)
            {
                WhenFoldoutAll("Victory");
            }

            _foldoutRocknroll = EditorGUILayout.Foldout(_foldoutRocknroll, "Show RockNRoll gestures");
            if (_foldoutRocknroll)
            {
                WhenFoldoutAll("RockNRoll");
            }

            _foldoutHandpistol = EditorGUILayout.Foldout(_foldoutHandpistol, "Show Gun gestures");
            if (_foldoutHandpistol)
            {
                WhenFoldoutAll("Gun");
            }

            _foldoutThumbsup = EditorGUILayout.Foldout(_foldoutThumbsup, "Show ThumbsUp gestures");
            if (_foldoutThumbsup)
            {
                WhenFoldoutAll("ThumbsUp");
            }

            EditorGUILayout.PropertyField(oneHandMode, new GUIContent("One hand mode (overrides permutations)"));
            EditorGUILayout.PropertyField(enablePermutations, new GUIContent("Enable permutations"));
            _foldoutPermutations = EditorGUILayout.Foldout(_foldoutPermutations, "Show permutations");
            if (_foldoutPermutations)
            {
                WhenFoldoutPermutations();
            }

            EditorGUILayout.PropertyField(anim00, new GUIContent("No gesture"));

            EditorGUILayout.Separator();

            blinkingReorderableList.DoLayoutList();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(transitionDuration, new GUIContent("Transition duration (s)"));

            if (serializedObject.isEditingMultipleObjects) {
                EditorGUILayout.PropertyField(previewAnimator, new GUIContent("Preview animator"));
            }
        }

        private void WhenFoldoutAll(string filter)
        {
            CreatePropertyField(anim01, "Exactly one Fist", filter);
            CreatePropertyField(anim02, "Exactly one Open", filter);
            CreatePropertyField(anim03, "Exactly one Point", filter);
            CreatePropertyField(anim04, "Exactly one Victory", filter);
            CreatePropertyField(anim05, "Exactly one RockNRoll", filter);
            CreatePropertyField(anim06, "Exactly one Gun", filter);
            CreatePropertyField(anim07, "Exactly one ThumbsUp", filter);
            CreatePropertyField(anim11, "Fist on both hands", filter);
            CreatePropertyField(anim11_L, "  - Left Fist only", filter);
            CreatePropertyField(anim11_R, "  - Right Fist only", filter);
            CreatePropertyField(anim12, "Fist and Open", filter);
            CreatePropertyField(anim13, "Fist and Point", filter);
            CreatePropertyField(anim14, "Fist and Victory", filter);
            CreatePropertyField(anim15, "Fist and RockNRoll", filter);
            CreatePropertyField(anim16, "Fist and Gun", filter);
            CreatePropertyField(anim17, "Fist and ThumbsUp", filter);
            CreatePropertyField(anim22, "Open on both hands", filter);
            CreatePropertyField(anim23, "Open and Point", filter);
            CreatePropertyField(anim24, "Open and Victory", filter);
            CreatePropertyField(anim25, "Open and RockNRoll", filter);
            CreatePropertyField(anim26, "Open and Gun", filter);
            CreatePropertyField(anim27, "Open and ThumbsUp", filter);
            CreatePropertyField(anim33, "Point on both hands", filter);
            CreatePropertyField(anim34, "Point and Victory", filter);
            CreatePropertyField(anim35, "Point and RockNRoll", filter);
            CreatePropertyField(anim36, "Point and Gun", filter);
            CreatePropertyField(anim37, "Point and ThumbsUp", filter);
            CreatePropertyField(anim44, "Victory on both hands", filter);
            CreatePropertyField(anim45, "Victory and RockNRoll", filter);
            CreatePropertyField(anim46, "Victory and Gun", filter);
            CreatePropertyField(anim47, "Victory and ThumbsUp", filter);
            CreatePropertyField(anim55, "RockNRoll on both hands", filter);
            CreatePropertyField(anim56, "RockNRoll and Gun", filter);
            CreatePropertyField(anim57, "RockNRoll and ThumbsUp", filter);
            CreatePropertyField(anim66, "Gun on both hands", filter);
            CreatePropertyField(anim67, "Gun and ThumbsUp", filter);
            CreatePropertyField(anim77, "ThumbsUp on both hands", filter);
        }

        private void WhenFoldoutPermutations()
        {
            CreatePropertyField(anim10, "Permutation Fist > None", null);
            CreatePropertyField(anim20, "Permutation Open > None", null);
            CreatePropertyField(anim21, "Permutation Open > Fist", null);
            CreatePropertyField(anim30, "Permutation Point > None", null);
            CreatePropertyField(anim31, "Permutation Point > Fist", null);
            CreatePropertyField(anim32, "Permutation Point > Open", null);
            CreatePropertyField(anim40, "Permutation Victory > None", null);
            CreatePropertyField(anim41, "Permutation Victory > Fist", null);
            CreatePropertyField(anim42, "Permutation Victory > Open", null);
            CreatePropertyField(anim43, "Permutation Victory > Point", null);
            CreatePropertyField(anim50, "Permutation RockNRoll > None", null);
            CreatePropertyField(anim51, "Permutation RockNRoll > Fist", null);
            CreatePropertyField(anim52, "Permutation RockNRoll > Open", null);
            CreatePropertyField(anim53, "Permutation RockNRoll > Point", null);
            CreatePropertyField(anim54, "Permutation RockNRoll > Victory", null);
            CreatePropertyField(anim60, "Permutation Gun > None", null);
            CreatePropertyField(anim61, "Permutation Gun > Fist", null);
            CreatePropertyField(anim62, "Permutation Gun > Open", null);
            CreatePropertyField(anim63, "Permutation Gun > Point", null);
            CreatePropertyField(anim64, "Permutation Gun > Victory", null);
            CreatePropertyField(anim65, "Permutation Gun > RockNRoll", null);
            CreatePropertyField(anim70, "Permutation ThumbsUp > None", null);
            CreatePropertyField(anim71, "Permutation ThumbsUp > Fist", null);
            CreatePropertyField(anim72, "Permutation ThumbsUp > Open", null);
            CreatePropertyField(anim73, "Permutation ThumbsUp > Point", null);
            CreatePropertyField(anim74, "Permutation ThumbsUp > Victory", null);
            CreatePropertyField(anim75, "Permutation ThumbsUp > RockNRoll", null);
            CreatePropertyField(anim76, "Permutation ThumbsUp > Gun", null);
        }

        private static void CreatePropertyField(SerializedProperty property, string label, string filter)
        {
            var isNotFiltered = filter == null;
            if (isNotFiltered || label.Contains(filter))
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            }
        }
    }
}
