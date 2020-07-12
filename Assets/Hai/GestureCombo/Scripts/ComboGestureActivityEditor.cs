#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hai.GestureCombo.Scripts
{
    [CustomEditor(typeof(ComboGestureActivity))]
    [CanEditMultipleObjects]
    public class ComboGestureActivityEditor : Editor
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
    
        public SerializedProperty transitionDuration;
    
        public ReorderableList blinkingReorderableList;

        private void OnEnable()
        {
            transitionDuration = serializedObject.FindProperty("transitionDuration");
            anim00 = serializedObject.FindProperty("anim00");
            anim01 = serializedObject.FindProperty("anim01");
            anim02 = serializedObject.FindProperty("anim02");
            anim03 = serializedObject.FindProperty("anim03");
            anim04 = serializedObject.FindProperty("anim04");
            anim05 = serializedObject.FindProperty("anim05");
            anim06 = serializedObject.FindProperty("anim06");
            anim07 = serializedObject.FindProperty("anim07");
            anim11 = serializedObject.FindProperty("anim11");
            anim12 = serializedObject.FindProperty("anim12");
            anim13 = serializedObject.FindProperty("anim13");
            anim14 = serializedObject.FindProperty("anim14");
            anim15 = serializedObject.FindProperty("anim15");
            anim16 = serializedObject.FindProperty("anim16");
            anim17 = serializedObject.FindProperty("anim17");
            anim22 = serializedObject.FindProperty("anim22");
            anim23 = serializedObject.FindProperty("anim23");
            anim24 = serializedObject.FindProperty("anim24");
            anim25 = serializedObject.FindProperty("anim25");
            anim26 = serializedObject.FindProperty("anim26");
            anim27 = serializedObject.FindProperty("anim27");
            anim33 = serializedObject.FindProperty("anim33");
            anim34 = serializedObject.FindProperty("anim34");
            anim35 = serializedObject.FindProperty("anim35");
            anim36 = serializedObject.FindProperty("anim36");
            anim37 = serializedObject.FindProperty("anim37");
            anim44 = serializedObject.FindProperty("anim44");
            anim45 = serializedObject.FindProperty("anim45");
            anim46 = serializedObject.FindProperty("anim46");
            anim47 = serializedObject.FindProperty("anim47");
            anim55 = serializedObject.FindProperty("anim55");
            anim56 = serializedObject.FindProperty("anim56");
            anim57 = serializedObject.FindProperty("anim57");
            anim66 = serializedObject.FindProperty("anim66");
            anim67 = serializedObject.FindProperty("anim67");
            anim77 = serializedObject.FindProperty("anim77");
        
            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            blinkingReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("blinking"),
                true, true, true, true
            );
            blinkingReorderableList.drawElementCallback = BlinkingListElement;
            blinkingReorderableList.drawHeaderCallback = BlinkingListHeader;
        }
        private void BlinkingListElement(Rect rect, int index, bool isActive, bool isFocused)
        {        
            SerializedProperty element = blinkingReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
                element,
                GUIContent.none
            );
        }

        private void BlinkingListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Closed eyes Animations (to disable blinking)");
        }

        private bool _foldoutAll = true;
        private bool _foldoutFist;
        private bool _foldoutHandopen;
        private bool _foldoutFingerpoint;
        private bool _foldoutVictory;
        private bool _foldoutRocknroll;
        private bool _foldoutHandpistol;
        private bool _foldoutThumbsup;
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _foldoutAll = EditorGUILayout.Foldout(_foldoutAll, "Show all gestures");
            if (_foldoutAll)
            {
                EditorGUILayout.PropertyField(anim00, new GUIContent("No gesture"));
                WhenFoldoutAll(null);
            }
        
            _foldoutFist = EditorGUILayout.Foldout(_foldoutFist, "Show FIST gestures");
            if (_foldoutFist)
            {
                WhenFoldoutAll("FIST");
            }
        
            _foldoutHandopen = EditorGUILayout.Foldout(_foldoutHandopen, "Show OPEN gestures");
            if (_foldoutHandopen)
            {
                WhenFoldoutAll("OPEN");
            }
        
            _foldoutFingerpoint = EditorGUILayout.Foldout(_foldoutFingerpoint, "Show POINT gestures");
            if (_foldoutFingerpoint)
            {
                WhenFoldoutAll("POINT");
            }
        
            _foldoutVictory = EditorGUILayout.Foldout(_foldoutVictory, "Show PEACE gestures");
            if (_foldoutVictory)
            {
                WhenFoldoutAll("PEACE");
            }
        
            _foldoutRocknroll = EditorGUILayout.Foldout(_foldoutRocknroll, "Show ROCKNROLL gestures");
            if (_foldoutRocknroll)
            {
                WhenFoldoutAll("ROCKNROLL");
            }
        
            _foldoutHandpistol = EditorGUILayout.Foldout(_foldoutHandpistol, "Show GUN gestures");
            if (_foldoutHandpistol)
            {
                WhenFoldoutAll("GUN");
            }
        
            _foldoutThumbsup = EditorGUILayout.Foldout(_foldoutThumbsup, "Show THUMBSUP gestures");
            if (_foldoutThumbsup)
            {
                WhenFoldoutAll("THUMBSUP");
            }
        
            EditorGUILayout.PropertyField(anim00, new GUIContent("No gesture"));
        
            EditorGUILayout.Separator();
        
            blinkingReorderableList.DoLayoutList();
        
            EditorGUILayout.Separator();
        
            EditorGUILayout.PropertyField(transitionDuration, new GUIContent("Transition duration (s)"));

            serializedObject.ApplyModifiedProperties();
        }

        private void WhenFoldoutAll(string filter)
        {
            CreatePropertyField(anim01, "Exactly one FIST", filter);
            CreatePropertyField(anim02, "Exactly one OPEN", filter);
            CreatePropertyField(anim03, "Exactly one POINT", filter);
            CreatePropertyField(anim04, "Exactly one PEACE", filter);
            CreatePropertyField(anim05, "Exactly one ROCKNROLL", filter);
            CreatePropertyField(anim06, "Exactly one GUN", filter);
            CreatePropertyField(anim07, "Exactly one THUMBSUP", filter);
            CreatePropertyField(anim11, "FIST on both hands", filter);
            CreatePropertyField(anim12, "FIST and OPEN", filter);
            CreatePropertyField(anim13, "FIST and POINT", filter);
            CreatePropertyField(anim14, "FIST and PEACE", filter);
            CreatePropertyField(anim15, "FIST and ROCKNROLL", filter);
            CreatePropertyField(anim16, "FIST and GUN", filter);
            CreatePropertyField(anim17, "FIST and THUMBSUP", filter);
            CreatePropertyField(anim22, "OPEN on both hands", filter);
            CreatePropertyField(anim23, "OPEN and POINT", filter);
            CreatePropertyField(anim24, "OPEN and PEACE", filter);
            CreatePropertyField(anim25, "OPEN and ROCKNROLL", filter);
            CreatePropertyField(anim26, "OPEN and GUN", filter);
            CreatePropertyField(anim27, "OPEN and THUMBSUP", filter);
            CreatePropertyField(anim33, "POINT on both hands", filter);
            CreatePropertyField(anim34, "POINT and PEACE", filter);
            CreatePropertyField(anim35, "POINT and ROCKNROLL", filter);
            CreatePropertyField(anim36, "POINT and GUN", filter);
            CreatePropertyField(anim37, "POINT and THUMBSUP", filter);
            CreatePropertyField(anim44, "PEACE on both hands", filter);
            CreatePropertyField(anim45, "PEACE and ROCKNROLL", filter);
            CreatePropertyField(anim46, "PEACE and GUN", filter);
            CreatePropertyField(anim47, "PEACE and THUMBSUP", filter);
            CreatePropertyField(anim55, "ROCKNROLL on both hands", filter);
            CreatePropertyField(anim56, "ROCKNROLL and GUN", filter);
            CreatePropertyField(anim57, "ROCKNROLL and THUMBSUP", filter);
            CreatePropertyField(anim66, "GUN on both hands", filter);
            CreatePropertyField(anim67, "GUN and THUMBSUP", filter);
            CreatePropertyField(anim77, "THUMBSUP on both hands", filter);
        }

        private void CreatePropertyField(SerializedProperty property, string label, string filter)
        {
            var isNotFiltered = filter == null;
            if (isNotFiltered || label.Contains(filter))
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            }
        }
    }
}
#endif