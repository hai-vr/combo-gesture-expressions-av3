using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Editor.Internal;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.EditorUI.EditorWindows
{
    public class EeAnimationEditorWindow : EditorWindow
    {
        private const int StandardWidth = 300;
        private const int StandardHeight = 200;
        private const int TempBorder = 10;
        private const int HalfWidth = StandardWidth / 2;
        private const int HalfHeight = StandardHeight / 2;

        private string _currentClipAssetRename;
        private Vector2 _scrollPos;
        private bool _disabledUndo;
        private int _selectedPreviewSetup;
        private bool _foldoutMassEdit;
        private int _selectedPreviewCamera;

        private EeSelectionCommands _selectionCommands;
        private EeEditCommands _editCommands;
        private EePreviewCommands _previewCommands;
        private EeAccessCommands _accessCommands;
        private bool _isCgeInstalled;

        private void OnEnable()
        {
            _selectionCommands = Ee.Get().SelectionCommands;
            _editCommands = Ee.Get().EditCommands;
            _previewCommands = Ee.Get().PreviewCommands;
            _accessCommands = Ee.Get().AccessCommands;
            _isCgeInstalled = Ee.Get().IsCgeInstalled;

            titleContent = new GUIContent("EE Animation Editor");
            Undo.undoRedoPerformed += () =>
            {
                if (_disabledUndo) return;
                OnUndoRedoPerformed();
            };
        }

        private void OnDestroy()
        {
            _disabledUndo = true;
        }

        private void OnInspectorUpdate()
        {
            var active = Selection.activeObject;
            if (active == null) return;
            if (!(active is AnimationClip)) return;

            _selectionCommands.SelectCurrentClip((AnimationClip)active);
        }

        private void OnUndoRedoPerformed()
        {
            // if (_currentClip == null) return;

            // OnNewClipSelected(_currentClip);
        }

        public void OnNewClipSelected(AnimationClip active)
        {
            _currentClipAssetRename = active.name;

            Repaint();
        }

        private void OnGUI()
        {
            var currentClip = _accessCommands.ActiveClip();

            GUILayout.BeginHorizontal();
            GUILayout.Box(_accessCommands.ActivePreview(), GUILayout.Width(StandardWidth), GUILayout.Height(StandardHeight));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (!_accessCommands.HasActiveClip())
            {
                EditorGUILayout.TextField(EeLocale.EEA_No_animation_selected, GUILayout.ExpandWidth(true));
            }
            else
            {
                _currentClipAssetRename = EditorGUILayout.TextField(_currentClipAssetRename, GUILayout.ExpandWidth(true));
            }
            EditorGUI.BeginDisabledGroup(!_accessCommands.HasActiveClip() || _currentClipAssetRename == currentClip.name || File.Exists(NewPath(currentClip)));
            if (GUILayout.Button(EeLocale.EEA_Rename, GUILayout.Width(70)))
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(currentClip), NewPath(currentClip));
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            if (GUILayout.Button(EeLocale.CORE_Switch))
            {
                EeLocalization.CycleLocale();
            }
            EditorGUILayout.LabelField(EeLocalization.IsEnglishLocaleActive() ? "" : EeLocale.CORE_Inaccuracy);

            UiPreviewSetup();
            UiPreviewCamera();
            if (!_accessCommands.HasActiveClip())
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("(+)", GUILayout.Width(HalfWidth / 2), GUILayout.Height(HalfHeight)))
                {
                    EePropertyExplorerWindow.OpenEditor();
                }
                GUILayout.EndHorizontal();

                return;
            }

            if (_accessCommands.AllPreviewSetups().Count > 0)
            {
                if (ColoredBackground(_accessCommands.IsMaintaining(), Color.green, () => GUILayout.Button(EeLocale.EEA_Preview_animation_in_scene)))
                {
                    _previewCommands.ToggleMaintainPreview();
                }

                EditorGUI.BeginDisabledGroup(!_accessCommands.IsMaintaining());
                var scenePreviewMode = (EeAnimationEditorScenePreviewMode)EditorGUILayout.EnumPopup(EeLocale.EEA_Scene_previews, _accessCommands.GetScenePreviewMode());
                if (scenePreviewMode != _accessCommands.GetScenePreviewMode())
                {
                    _previewCommands.SetForcePreviewGeneration(scenePreviewMode);
                }
                EditorGUI.EndDisabledGroup();

                var dummy = _accessCommands.DummyNullable();
                EditorGUI.BeginDisabledGroup(!dummy.HasValue);
                if (GUILayout.Button(EeLocale.EEA_Select_animator_to_edit_animation))
                {
                    Selection.SetActiveObjectWithContext(dummy.Value.Dummy.gameObject, null);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(EeLocale.EEA_Generate_previews))
                {
                    OnNewSweepRequested();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - StandardHeight - EditorGUIUtility.singleLineHeight * 2));
            GUILayout.BeginHorizontal();

            var widthRun = HalfWidth + TempBorder;
            var editableProperties = _accessCommands.Editables();
            for (var index = 0; index < editableProperties.Count; index++)
            {
                var editableProperty = editableProperties[index];
                GUILayout.BeginVertical();
                GUILayout.Box(editableProperty.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                var blendshapePrefix = "blendShape.";
                GUILayout.Label(editableProperty.Property.StartsWith(blendshapePrefix) ? editableProperty.Property.Substring(blendshapePrefix.Length) : editableProperty.Property, GUILayout.Width(HalfWidth));

                if (editableProperty.IsVaryingOverTime)
                {
                    GUILayout.Label($"(between {editableProperty.VaryingMinValue.ToString(CultureInfo.InvariantCulture)} and {editableProperty.Value.ToString(CultureInfo.InvariantCulture)})", GUILayout.Width(HalfWidth));
                }
                else
                {
                    var newValue = EditorGUILayout.Slider(editableProperty.Value, 0, 100, GUILayout.Width(HalfWidth));
                    if (newValue != editableProperty.Value)
                    {
                        _editCommands.UpdateBlendshape(editableProperty.Path, editableProperty.Property, newValue);
                    }
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                GUILayout.Label(_accessCommands.GetBasedOnWhat(editableProperty.Property) != null ? "Based" : "", GUILayout.Width(40));
                if (GUILayout.Button(EeLocale.EEA_Delete, GUILayout.Width(StandardWidth / 5)))
                {
                    _editCommands.DeleteBlendshape(editableProperty.Path, editableProperty.Property);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                widthRun += HalfWidth + TempBorder;
                if (Screen.width < widthRun)
                {
                    widthRun = HalfWidth + TempBorder;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(HalfWidth / 2), GUILayout.Height(HalfHeight)))
            {
                EePropertyExplorerWindow.OpenEditor();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var stats = _accessCommands.NonEditableStats();
            if (stats.SmrMimicBlendshapes.Count > 0)
            {
                GUILayout.Label(string.Format(EeLocale.EEA_Reset_blendshapes_are_not_shown_above, stats.SmrMimicBlendshapes.Count), EditorStyles.boldLabel);
                _foldoutMassEdit = EditorGUILayout.Foldout(_foldoutMassEdit, EeLocale.EEA_Show_delete_reset_blendshapes);
                if (_foldoutMassEdit)
                {
                    if (ColoredBackground(true, Color.red, () => GUILayout.Button(EeLocale.EEA_Delete_zero_values)))
                    {
                        _editCommands.DeleteAllNeutralizedBlendshapes();
                    }

                    var smrToBlendshape = stats.SmrMimicBlendshapes
                        .ToLookup(binding => binding.path, binding => binding.propertyName.Substring("blendShape.".Length))
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
                    EditorGUILayout.HelpBox(string.Join("\n\n", smrToBlendshape.Select(pair => $"{pair.Key} ({pair.Value.Count}):\n{string.Join(", ", pair.Value)}").ToList()), MessageType.Info);
                }
            }
            if (stats.HasAnyOtherStats)
            {
                GUILayout.Label(EeLocale.EEA_This_animation_has_additional_properties_not_shown_here);
                LookupLabel(stats, EeNonEditableLookup.Transform, EeLocale.EEA_Transform_related);
                LookupLabel(stats, EeNonEditableLookup.Animator, EeLocale.EEA_Finger_posing_other_muscles_or_animator_related);
                LookupLabel(stats, EeNonEditableLookup.MaterialSwap, EeLocale.EEA_Material_swaps);
                LookupLabel(stats, EeNonEditableLookup.Shader, EeLocale.EEA_Shader_properties);
                LookupLabel(stats, EeNonEditableLookup.GameObjectToggle, EeLocale.EEA_Game_object_toggles);
                LookupLabel(stats, EeNonEditableLookup.Other, EeLocale.EEA_Other_properties_not_cited_above);
            }

            if (_isCgeInstalled)
            {
                if (stats.Quirk != EeQuirk.EmptyIssue && stats.Quirk != EeQuirk.FirstFrameIssue && stats.EffectiveFrameDuration > 1)
                {
                    GUILayout.Label(string.Format(EeLocale.EEA_This_animation_lasts_keyframes, stats.EffectiveFrameDuration), EditorStyles.boldLabel);
                }
            }
            else
            {
                if (stats.Quirk != EeQuirk.EmptyIssue && stats.EffectiveFrameDuration > 1)
                {
                    GUILayout.Label(string.Format(EeLocale.EEA_This_animation_lasts_keyframes, stats.EffectiveFrameDuration), EditorStyles.boldLabel);
                }
                switch (stats.Quirk)
                {
                    case EeQuirk.EmptyIssue:
                        EditorGUILayout.HelpBox(string.Format(EeLocale.EEA_This_animation_is_empty, stats.EffectiveFrameDuration), MessageType.Warning);
                        break;
                    case EeQuirk.FirstFrameIssue:

                        EditorGUILayout.HelpBox(string.Format(EeLocale.EEA_All_of_the_keyframes_are_frame_zero, stats.EffectiveFrameDuration), MessageType.Warning);
                        break;
                    case EeQuirk.OneFrame:
                    case EeQuirk.MoreThanOneFrame:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (editableProperties.Any(info => info.IsVaryingOverTime))
            {
                EditorGUILayout.HelpBox(EeLocale.EEA_BlendshapeVaryingOverTimeWarning, MessageType.Info);
            }

            var clipName = _accessCommands.ActiveClip().name;
            if (clipName.Contains("Autogenerated") || clipName.Contains("DO_NOT_EDIT"))
            {
                EditorGUILayout.HelpBox(EeLocale.EEA_AutogeneratedAnimationWarning, MessageType.Error);
            }

            #if VRC_SDK_VRCSDK3
            if (editableProperties.Any(info => info.Property.ToLowerInvariant().StartsWith("blendshape.vrc.v_")))
            {
                EditorGUILayout.HelpBox(EeLocale.EEA_VrcVisemeBlendshapeWarning, MessageType.Warning);
            }
            if (editableProperties.Any(info => info.Property.ToLowerInvariant().Equals("blendshape.blink")))
            {
                EditorGUILayout.HelpBox(EeLocale.EEA_BlinkBlendshapeWarning, MessageType.Warning);
            }
            #endif

            GUILayout.EndScrollView();
        }

        private static void LookupLabel(EeNonEditableStats stats, EeNonEditableLookup type, string description)
        {
            if (!stats.OtherPropertyToCountLookup.ContainsKey(type)) return;

            GUILayout.Label($"- {stats.OtherPropertyToCountLookup[type]} {description}");
        }

        private void OnNewSweepRequested()
        {
            _accessCommands.ManuallyPreviewAll();
        }

        private void UiPreviewSetup()
        {
            var available = _accessCommands.AllPreviewSetups().Select(o => o).Reverse().ToList();

            var previewSetups = available.Select(previewable => previewable.AsGameObject().name).ToArray();
            if (previewSetups.Length == 0) previewSetups = new[] { "None" };
            var newSelectedPreviewSetup = EditorGUILayout.Popup(EeLocale.EEA_PreviewDummy, _selectedPreviewSetup, previewSetups);
            if (newSelectedPreviewSetup != _selectedPreviewSetup)
            {
                _selectedPreviewSetup = newSelectedPreviewSetup;

                var previewables = available.ToList();
                if (newSelectedPreviewSetup > previewables.Count)
                {
                    _selectionCommands.ForgetDummy();
                }
                else
                {
                    _selectionCommands.SelectDummy(available[newSelectedPreviewSetup]);
                }
            }

            if (_accessCommands.AllPreviewSetups().Count == 0)
            {
                if (GUILayout.Button(EeLocale.EEA_Automatically_setup_preview, GUILayout.Height(50), GUILayout.Width(300)))
                {
                    new EePreviewSetupWizard().AutoSetup();
                }
            }
        }

        private void UiPreviewCamera()
        {
            if (_accessCommands.AllPreviewSetups().Count == 0) return;

            var dummy = _accessCommands.DummyNullable();

            var cameras = new [] { "" };
            if (dummy.HasValue)
            {
                cameras = dummy.Value.Cameras.Select(camera => camera.name).ToArray();
            }

            var newSelectedCamera = EditorGUILayout.Popup(EeLocale.EEA_Camera, _selectedPreviewCamera, cameras);
            if (newSelectedCamera != _selectedPreviewCamera)
            {
                _selectedPreviewCamera = newSelectedCamera;
                _selectionCommands.SelectCamera(_selectedPreviewCamera);
            }
        }

        private string NewPath(AnimationClip currentClip)
        {
            return $"{FolderOfClip(currentClip)}{_currentClipAssetRename}.anim";
        }

        private string FolderOfClip(AnimationClip currentClip)
        {
            var assetPath = AssetDatabase.GetAssetPath(currentClip);
            return assetPath.Replace(Path.GetFileName(assetPath), "");
        }

        public static EeAnimationEditorWindow Obtain()
        {
            var editor = GetWindow<EeAnimationEditorWindow>(false, null, false);
            return editor;
        }

        [MenuItem("Window/Haï/EE Animation Editor")]
        public static void OpenEditor()
        {
            Obtain().Show();
        }

        internal static T ColoredBackground<T>(bool isActive, Color background, Func<T> inside)
        {
            var col = GUI.color;
            try
            {
                if (isActive) GUI.color = background;
                return inside();
            }
            finally
            {
                GUI.color = col;
            }
        }
    }
}
