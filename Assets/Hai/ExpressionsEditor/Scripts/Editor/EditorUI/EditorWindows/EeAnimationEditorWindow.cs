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
                EditorGUILayout.TextField("No animation selected.", GUILayout.ExpandWidth(true));
            }
            else
            {
                _currentClipAssetRename = EditorGUILayout.TextField(_currentClipAssetRename, GUILayout.ExpandWidth(true));
            }
            EditorGUI.BeginDisabledGroup(!_accessCommands.HasActiveClip() || _currentClipAssetRename == currentClip.name || File.Exists(NewPath(currentClip)));
            if (GUILayout.Button("Rename", GUILayout.Width(70)))
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(currentClip), NewPath(currentClip));
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            UiPreviewSetup();
            UiPreviewCamera();
            if (!_accessCommands.HasActiveClip())
            {
                return;
            }

            if (_accessCommands.AllPreviewSetups().Count > 0)
            {
                if (ColoredBackground(_accessCommands.IsMaintaining(), Color.green, () => GUILayout.Button("Preview animation in Scene")))
                {
                    _previewCommands.ToggleMaintainPreview();
                }

                EditorGUI.BeginDisabledGroup(!_accessCommands.IsMaintaining());
                var scenePreviewMode = (EeAnimationEditorScenePreviewMode)EditorGUILayout.EnumPopup("Scene previews", _accessCommands.GetScenePreviewMode());
                if (scenePreviewMode != _accessCommands.GetScenePreviewMode())
                {
                    _previewCommands.SetForcePreviewGeneration(scenePreviewMode);
                }
                EditorGUI.EndDisabledGroup();

                var dummy = _accessCommands.DummyNullable();
                EditorGUI.BeginDisabledGroup(!dummy.HasValue);
                if (GUILayout.Button("Select animator to edit Animation"))
                {
                    Selection.SetActiveObjectWithContext(dummy.Value.Dummy.gameObject, null);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Generate previews"))
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
                if (GUILayout.Button("Delete", GUILayout.Width(StandardWidth / 5)))
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
                GUILayout.Label($"{stats.SmrMimicBlendshapes.Count} reset blendshapes are not shown above.", EditorStyles.boldLabel);
                _foldoutMassEdit = EditorGUILayout.Foldout(_foldoutMassEdit, "Show/Delete reset blendshapes");
                if (_foldoutMassEdit)
                {
                    if (ColoredBackground(true, Color.red, () => GUILayout.Button("Delete 0-values")))
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
                GUILayout.Label("This animation has additional properties not shown here:");
                LookupLabel(stats, EeNonEditableLookup.Transform, "transform-related");
                LookupLabel(stats, EeNonEditableLookup.Animator, "finger posing, other muscles, or Animator-related");
                LookupLabel(stats, EeNonEditableLookup.MaterialSwap, "material swaps");
                LookupLabel(stats, EeNonEditableLookup.Shader, "shader properties");
                LookupLabel(stats, EeNonEditableLookup.GameObjectToggle, "game object toggles");
                LookupLabel(stats, EeNonEditableLookup.Other, "other properties not cited above");
            }

            if (_isCgeInstalled)
            {
                if (stats.Quirk != EeQuirk.EmptyIssue && stats.Quirk != EeQuirk.FirstFrameIssue && stats.EffectiveFrameDuration > 1)
                {
                    GUILayout.Label($"This animation lasts {stats.EffectiveFrameDuration} keyframes.", EditorStyles.boldLabel);
                }
            }
            else
            {
                if (stats.Quirk != EeQuirk.EmptyIssue && stats.EffectiveFrameDuration > 1)
                {
                    GUILayout.Label($"This animation lasts {stats.EffectiveFrameDuration} keyframes.", EditorStyles.boldLabel);
                }
                switch (stats.Quirk)
                {
                    case EeQuirk.EmptyIssue:
                        EditorGUILayout.HelpBox($"This animation is empty. This will cause the animation to last {stats.EffectiveFrameDuration} keyframes.", MessageType.Warning);
                        break;
                    case EeQuirk.FirstFrameIssue:
                        EditorGUILayout.HelpBox($"All of the keyframes are located on frame 0. This will cause the animation to last {stats.EffectiveFrameDuration} keyframes.", MessageType.Warning);
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
                EditorGUILayout.HelpBox("This animation contains blendshapes that are animated differently over time.\nThey cannot be edited in Expressions Editor. Preview may not be accurate.", MessageType.Info);
            }

            var clipName = _accessCommands.ActiveClip().name;
            if (clipName.Contains("Autogenerated") || clipName.Contains("DO_NOT_EDIT"))
            {
                EditorGUILayout.HelpBox("This animation is likely auto-generated and may be overwritten at any time. You should probably not edit it.", MessageType.Error);
            }

            #if VRC_SDK_VRCSDK3
            if (editableProperties.Any(info => info.Property.ToLowerInvariant().StartsWith("blendshape.vrc.v_")))
            {
                EditorGUILayout.HelpBox("This animation animates blendshapes that begin with vrc.v_, usually designed for Visemes.\nIf any animation contains those, the mouth may not animate properly.", MessageType.Warning);
            }
            if (editableProperties.Any(info => info.Property.ToLowerInvariant().Equals("blendshape.blink")))
            {
                EditorGUILayout.HelpBox("This animation animates a blendshape called Blink, usually selected to make the avatar blink automatically.\nIf any animation contains it, the avatar may not blink properly.\nMany avatar bases have blendshapes to close each eye separately; they should be used instead.", MessageType.Warning);
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
            var newSelectedPreviewSetup = EditorGUILayout.Popup("Preview dummy", _selectedPreviewSetup, previewSetups);
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
                if (GUILayout.Button("Automatically setup preview!", GUILayout.Height(50), GUILayout.Width(300)))
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

            var newSelectedCamera = EditorGUILayout.Popup("Camera", _selectedPreviewCamera, cameras);
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
