using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboGestureCompilerInternal
    {
        private const string EmptyClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_EmptyClip.anim";

        internal const string HaiGestureComboParamName = "_Hai_GestureComboValue";
        internal const string HaiGestureComboAreEyesClosed = "_Hai_GestureComboAreEyesClosed";
        internal const string HaiGestureComboDisableExpressionsParamName = "_Hai_GestureComboDisableExpressions";
        internal const string HaiGestureComboDisableBlinkingOverrideParamName = "_Hai_GestureComboDisableBlinkingOverride";

        public const string GestureLeftWeight = "GestureLeftWeight";
        public const string GestureRightWeight = "GestureRightWeight";
        public const string GestureLeft = "GestureLeft";
        public const string GestureRight = "GestureRight";
        
        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly AnimatorController _animatorController;
        private readonly AnimationClip _customEmptyClip;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly FeatureToggles _featuresToggles;
        private readonly ConflictPreventionMode _compilerConflictPreventionMode;
        private readonly string _datetimeForAssetPack;

        public ComboGestureCompilerInternal(string activityStageName,
            List<GestureComboStageMapper> comboLayers,
            RuntimeAnimatorController animatorController,
            AnimationClip customEmptyClip,
            float analogBlinkingUpperThreshold,
            FeatureToggles featuresToggles,
            ConflictPreventionMode compilerConflictPreventionMode)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _animatorController = (AnimatorController) animatorController;
            _customEmptyClip = customEmptyClip;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _featuresToggles = featuresToggles;
            _compilerConflictPreventionMode = compilerConflictPreventionMode;
            
            _datetimeForAssetPack = DateTime.UtcNow .ToString("yyyyMMddHHmmss");
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Starting", 0f);
            var emptyClip = GetOrCreateEmptyClip();

            CreateParameters();
            CreateOrReplaceController(emptyClip);
            CreateOrReplaceExpressionsView(emptyClip);
            CreateOrReplaceBlinkingOverrideView(emptyClip);

            ReapAnimator();

            var isAssetRefreshingRequired = _compilerConflictPreventionMode == ConflictPreventionMode.ZERO_VALUES_PER_ACTIVITY;
            if (isAssetRefreshingRequired)
            {
                AssetDatabase.Refresh();
            }
            EditorUtility.ClearProgressBar();
        }

        private void ReapAnimator()
        {
            if (AssetDatabase.GetAssetPath(_animatorController) == "")
            {
                return;
            }

            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_animatorController));
        
            var reachableMotions = ConcatStateMachines()
                .SelectMany(machine => machine.states)
                .Select(state => state.state.motion)
                .ToList<Object>();
            Reap(allSubAssets, typeof(BlendTree), reachableMotions, o => o.name.StartsWith("autoBT_"));

            if (false) {
                UnsafelyReapAssetsLeftoverByUnsafeDelete(allSubAssets);
            }
        }

        private void UnsafelyReapAssetsLeftoverByUnsafeDelete(Object[] allSubAssets)
        {
            var reachableStates = ConcatStateMachines()
                .SelectMany(machine => machine.states)
                .Select(state => state.state)
                .ToList<Object>();
            var reachableMonoBehaviors = ConcatStateMachines()
                .SelectMany(machine => machine.states)
                .SelectMany(state => state.state.behaviours)
                .ToList<Object>();
            var reachableTransitions = ConcatStateMachines()
                .SelectMany(machine => machine.states)
                .SelectMany(state => state.state.transitions)
                .ToList<Object>();

            Reap(allSubAssets, typeof(AnimatorState), reachableStates, o => true);
            Reap(allSubAssets, typeof(StateMachineBehaviour), reachableMonoBehaviors, o => true);
            Reap(allSubAssets, typeof(AnimatorStateTransition), reachableTransitions, o => true);
        }

        private IEnumerable<AnimatorStateMachine> ConcatStateMachines()
        {
            return _animatorController.layers.Select(layer => layer.stateMachine)
                .Concat(_animatorController.layers.SelectMany(layer => layer.stateMachine.stateMachines).Select(machine => machine.stateMachine));
        }

        private static void Reap(Object[] allAssets, Type type, List<Object> existingAssets, Predicate<Object> predicate)
        {
            foreach (var o in allAssets)
            {
                if (o != null && (o.GetType() == type || o.GetType().IsSubclassOf(type)) && !existingAssets.Contains(o) && predicate.Invoke(o))
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        private void CreateParameters()
        {
            CreateParamIfNotExists("GestureLeft", AnimatorControllerParameterType.Int);
            CreateParamIfNotExists("GestureRight", AnimatorControllerParameterType.Int);
            CreateParamIfNotExists("GestureLeftWeight", AnimatorControllerParameterType.Float);
            CreateParamIfNotExists("GestureRightWeight", AnimatorControllerParameterType.Float);
            CreateParamIfNotExists(HaiGestureComboParamName, AnimatorControllerParameterType.Int);
            if (Feature(FeatureToggles.ExposeDisableExpressions)) {
                CreateParamIfNotExists(HaiGestureComboDisableExpressionsParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeDisableBlinkingOverride)) {
                CreateParamIfNotExists(HaiGestureComboDisableBlinkingOverrideParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeAreEyesClosed)) { 
                CreateParamIfNotExists(HaiGestureComboAreEyesClosed, AnimatorControllerParameterType.Int);
            }
            
            if (_activityStageName != "")
            {
                CreateParamIfNotExists(_activityStageName, AnimatorControllerParameterType.Int);
            }
        }

        private AnimationClip GetOrCreateEmptyClip()
        {
            var emptyClip = _customEmptyClip;
            if (emptyClip == null)
            {
                emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EmptyClipPath);
            }
            if (emptyClip == null)
            {
                emptyClip = GenerateEmptyClipAsset();
            }

            return emptyClip;
        }

        private static AnimationClip GenerateEmptyClipAsset()
        {
            var emptyClip = new AnimationClip();
            var settings = AnimationUtility.GetAnimationClipSettings(emptyClip);
            settings.loopTime = false;
            Keyframe[] keyframes = {new Keyframe(0, 0), new Keyframe(1 / 60f, 0)};
            var curve = new AnimationCurve(keyframes);
            emptyClip.SetCurve("_ignored", typeof(GameObject), "m_IsActive", curve);

            if (!AssetDatabase.IsValidFolder("Assets/Hai"))
            {
                AssetDatabase.CreateFolder("Assets", "Hai");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Hai/ComboGesture"))
            {
                AssetDatabase.CreateFolder("Assets/Hai", "ComboGesture");
            }

            AssetDatabase.CreateAsset(emptyClip, EmptyClipPath);
            return emptyClip;
        }

        private void CreateParamIfNotExists(string paramName,
            AnimatorControllerParameterType type)
        {
            if (_animatorController.parameters.FirstOrDefault(param => param.name == paramName) == null)
            {
                _animatorController.AddParameter(paramName, type);
            }
        }

        private void CreateOrReplaceController(AnimationClip emptyClip)
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing combo controller layer", 0f);
            var machine = ReinitializeAnimatorLayer("Hai_GestureCtrl", 0f);
        
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating combo controller layer", 0f);
        
            for (var left = 0; left < 8; left++)
            {
                for (var right = left; right < 8; right++)
                {
                    var state = machine.AddState(left + "" + right, GridPosition(right, left));
                    state.writeDefaultValues = false;
                    state.motion = emptyClip;
                
                    var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
                    {
                        new VRC_AvatarParameterDriver.Parameter {name = HaiGestureComboParamName, value = left * 10 + right}
                    };

                    {
                        var normal = machine.AddAnyStateTransition(state);
                        SetupImmediateTransition(normal);
                        normal.AddCondition(AnimatorConditionMode.Equals, left, "GestureLeft");
                        normal.AddCondition(AnimatorConditionMode.Equals, right, "GestureRight");
                    }
                    if (left != right)
                    {
                        var reverse = machine.AddAnyStateTransition(state);
                        SetupImmediateTransition(reverse);
                        reverse.AddCondition(AnimatorConditionMode.Equals, right, "GestureLeft");
                        reverse.AddCondition(AnimatorConditionMode.Equals, left, "GestureRight");
                    }
                }
            }
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip)
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing expressions layer", 0f);
            var machine = ReinitializeAnimatorLayer("Hai_GestureExp", 1f);

            var defaultState = machine.AddState("Default", GridPosition(-1, -1));
            defaultState.motion = emptyClip;
            if (Feature(FeatureToggles.ExposeDisableExpressions)) {
                CreateTransitionWhenExpressionsAreDisabled(machine, defaultState);
            }
            if (_activityStageName != "") { 
                CreateTransitionWhenActivityIsOutOfBounds(machine, defaultState);
            }

            var activityManifests = CreateManifest(emptyClip);
            if (_compilerConflictPreventionMode == ConflictPreventionMode.ZERO_VALUES_PER_ACTIVITY)
            {
                activityManifests = NeutralizeManifestAnimations(activityManifests);
            }
            var combinator = new IntermediateCombinator(activityManifests);

            new GestureCExpressionCombiner(_animatorController, machine, combinator.IntermediateToTransition, _activityStageName)
                .Populate();
        }

        private List<ActivityManifest> NeutralizeManifestAnimations(List<ActivityManifest> activityManifests)
        {
            var allAnimatedCurveKeys = FindAllAnimatedProperties(activityManifests);
            var allAnimationClips = new HashSet<AnimationClip>(activityManifests
                .SelectMany(manifest => manifest.Manifest.AnimationClips())
                .ToList());

            var remapping = CreateAssetContainerWithNeutralizedAnimations(allAnimationClips, allAnimatedCurveKeys);
            return activityManifests.Select(manifest => RemapManifest(manifest, remapping)).ToList();
        }

        private static ActivityManifest RemapManifest(ActivityManifest manifest, Dictionary<AnimationClip, AnimationClip> remapping)
        {
            var originalManifest = manifest.Manifest;
            var remappedManifest = new RawGestureManifest(
                originalManifest.AnimationClips().Select(clip => remapping[clip]).ToList(),
                originalManifest.Blinking,
                originalManifest.TransitionDuration
            );
            return new ActivityManifest(manifest.StageValue, remappedManifest, manifest.LayerOrdinal);
        }

        private Dictionary<AnimationClip, AnimationClip> CreateAssetContainerWithNeutralizedAnimations(HashSet<AnimationClip> animationClips, HashSet<CurveKey> allAnimatedCurveKeys)
        {
            var remapping = new Dictionary<AnimationClip, AnimationClip>();
            var assetContainer = new AnimatorController();
            AssetDatabase.CreateAsset(assetContainer, "Assets/GeneratedCGE__" + _datetimeForAssetPack + ".asset");

            foreach (var animationClip in animationClips)
            {
                var neutralizedAnimation = CopyAndNeutralize(animationClip, allAnimatedCurveKeys);
                AssetDatabase.AddObjectToAsset(neutralizedAnimation, assetContainer);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(neutralizedAnimation));

                remapping.Add(animationClip, neutralizedAnimation);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(assetContainer));

            return remapping;
        }

        private AnimationClip CopyAndNeutralize(AnimationClip animationClipToBePreserved, HashSet<CurveKey> allAnimatedCurveKeys)
        {
            var copyOfAnimationClip = GameObject.Instantiate(animationClipToBePreserved);
            copyOfAnimationClip.name = "AUTOGENERATED_" + animationClipToBePreserved.name + "_DO_NOT_EDIT";
            
            var bindings = AnimationUtility.GetCurveBindings(copyOfAnimationClip);
            var thisAnimationPaths = bindings
                .Select(CurveKey.FromBinding)
                .ToList();
            
            foreach (var curveKey in allAnimatedCurveKeys)
            {
                if (!thisAnimationPaths.Contains(curveKey))
                {
                    Keyframe[] keyframes = {new Keyframe(0, 0), new Keyframe(1 / 60f, 0)};
                    var curve = new AnimationCurve(keyframes);
                    copyOfAnimationClip.SetCurve(curveKey.Path, curveKey.Type, curveKey.PropertyName, curve);
                }
            }

            return copyOfAnimationClip;
        }
        
        struct CurveKey
        {
            public static CurveKey FromBinding(EditorCurveBinding binding)
            {
                return new CurveKey(binding.path, binding.type, binding.propertyName);
            }

            public CurveKey(string path, Type type, string propertyName)
            {
                Path = path;
                Type = type;
                PropertyName = propertyName;
            }

            public string Path { get; }
            public Type Type { get; }
            public string PropertyName { get; }

            public bool Equals(CurveKey other)
            {
                return Path == other.Path && Equals(Type, other.Type) && PropertyName == other.PropertyName;
            }

            public override bool Equals(object obj)
            {
                return obj is CurveKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Path != null ? Path.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private static HashSet<CurveKey> FindAllAnimatedProperties(List<ActivityManifest> activityManifests)
        {
            var animatedPaths = new List<CurveKey>();
            foreach (var activityManifest in activityManifests)
            {
                foreach (var animationClip in activityManifest.Manifest.AnimationClips())
                {
                    var editorCurveBindings = AnimationUtility.GetCurveBindings(animationClip);
                    foreach (var editorCurveBinding in editorCurveBindings)
                    {
                        animatedPaths.Add(CurveKey.FromBinding(editorCurveBinding));
                    }
                }
            }

            return new HashSet<CurveKey>(animatedPaths);
        }

        private static void CreateTransitionWhenExpressionsAreDisabled(AnimatorStateMachine machine, AnimatorState defaultState)
        {
            var transition = machine.AddAnyStateTransition(defaultState);
            SetupDefaultTransition(transition);
            transition.AddCondition(AnimatorConditionMode.NotEqual, 0, HaiGestureComboDisableExpressionsParamName);
        }

        private void CreateOrReplaceBlinkingOverrideView(AnimationClip emptyClip)
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing eyes blinking override layer", 0f);
            var machine = ReinitializeAnimatorLayer("Hai_GestureBlinking", 0f);
            var suspend = CreateSuspendState(machine, emptyClip);

            var activityManifests = CreateManifest(emptyClip);
            var combinator = new IntermediateBlinkingCombinator(activityManifests);
            if (!combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewMotion(true)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(true, false)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(false, true)))
            {
                return;
            }

            var enableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Tracking, emptyClip);
            var disableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Animation, emptyClip);
            if (Feature(FeatureToggles.ExposeDisableBlinkingOverride))
            {
                CreateTransitionWhenBlinkingIsDisabled(enableBlinking, suspend);
                CreateTransitionWhenBlinkingIsDisabled(disableBlinking, suspend);
            }
            CreateTransitionWhenActivityIsOutOfBounds(enableBlinking, suspend);
            CreateTransitionWhenActivityIsOutOfBounds(disableBlinking, suspend);
            if (Feature(FeatureToggles.ExposeAreEyesClosed))
            {
                CreateInternalParameterDriverWhenEyesAreOpen(enableBlinking);
                CreateInternalParameterDriverWhenEyesAreClosed(disableBlinking);
            }
        
            foreach (var layer in _comboLayers)
            {
                var transition = suspend.AddTransition(enableBlinking);
                SetupDefaultTransition(transition);
                transition.AddCondition(AnimatorConditionMode.Equals, layer.stageValue, _activityStageName);
                if (Feature(FeatureToggles.ExposeDisableBlinkingOverride)) {
                    transition.AddCondition(AnimatorConditionMode.Equals, 0, HaiGestureComboDisableBlinkingOverrideParamName);
                }
            }
        
            new GestureCBlinkingCombiner(combinator.IntermediateToBlinking, _activityStageName, _analogBlinkingUpperThreshold)
                .Populate(enableBlinking, disableBlinking);
        }

        private static void CreateInternalParameterDriverWhenEyesAreOpen(AnimatorState enableBlinking)
        {
            var driver = enableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = HaiGestureComboAreEyesClosed, value = 0}
            };
        }

        private static void CreateInternalParameterDriverWhenEyesAreClosed(AnimatorState disableBlinking)
        {
            var driver = disableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = HaiGestureComboAreEyesClosed, value = 1}
            };
        }

        private static RawGestureManifest FromManifest(ComboGestureActivity activity, AnimationClip fallbackWhen00ClipIsNull)
        {
            if (activity == null)
            {
                return new RawGestureManifest(
                    Enumerable.Repeat(fallbackWhen00ClipIsNull, 36).ToList(),
                    new List<AnimationClip>(), 
                    0.1f);
            }
            
            var neutral = activity.anim00 ? activity.anim00 : fallbackWhen00ClipIsNull;
            return new RawGestureManifest(new[]
            {
                activity.anim00, activity.anim01, activity.anim02, activity.anim03, activity.anim04, activity.anim05, activity.anim06, activity.anim07,
                activity.anim11, activity.anim12, activity.anim13, activity.anim14, activity.anim15, activity.anim16, activity.anim17,
                activity.anim22, activity.anim23, activity.anim24, activity.anim25, activity.anim26, activity.anim27,
                activity.anim33, activity.anim34, activity.anim35, activity.anim36, activity.anim37,
                activity.anim44, activity.anim45, activity.anim46, activity.anim47,
                activity.anim55, activity.anim56, activity.anim57,
                activity.anim66, activity.anim67,
                activity.anim77
            }.Select(clip => clip ? clip : neutral).ToList(), activity.blinking, activity.transitionDuration);
        }

        private List<ActivityManifest> CreateManifest(AnimationClip emptyClip)
        {
            return _comboLayers
                .Select((mapper, layerOrdinal) => new ActivityManifest(mapper.stageValue, FromManifest(mapper.activity, emptyClip), layerOrdinal))
                .ToList();
        }

        private static void CreateTransitionWhenBlinkingIsDisabled(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            SetupDefaultBlinkingTransition(transition);
            transition.AddCondition(AnimatorConditionMode.NotEqual, 0, HaiGestureComboDisableBlinkingOverrideParamName);
        }

        private static AnimatorState CreateSuspendState(AnimatorStateMachine machine, AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState("SuspendBlinking", GridPosition(1, 1));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = false;
            return enableBlinking;
        }

        private static AnimatorState CreateBlinkingState(AnimatorStateMachine machine, VRC_AnimatorTrackingControl.TrackingType type,
            AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", GridPosition(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = false;
            var tracking = enableBlinking.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            tracking.trackingEyes = type;
            return enableBlinking;
        }

        private void CreateTransitionWhenActivityIsOutOfBounds(AnimatorStateMachine machine, AnimatorState defaultState)
        {
            var transition = machine.AddAnyStateTransition(defaultState);
            SetupDefaultTransition(transition);

            foreach (var layer in _comboLayers)
            {
                transition.AddCondition(AnimatorConditionMode.NotEqual, layer.stageValue, _activityStageName);
            }
        }
    
        private void CreateTransitionWhenActivityIsOutOfBounds(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            SetupDefaultTransition(transition);

            foreach (var layer in _comboLayers)
            {
                transition.AddCondition(AnimatorConditionMode.NotEqual, layer.stageValue, _activityStageName);
            }
        }

        private static void SetupImmediateTransition(AnimatorStateTransition transition)
        {
            SetupCommonTransition(transition);

            transition.duration = 0;
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

        private static void SetupDefaultTransition(AnimatorStateTransition transition)
        {
            SetupCommonTransition(transition);
        
            transition.duration = 0.1f; // There seems to be a quirk if the duration is 0 when using DisableExpressions, so use 0.1f instead
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = true; // This is relevant as normal transitions may not check activity nor disabled expressions
        }

        private static void SetupDefaultBlinkingTransition(AnimatorStateTransition transition)
        {
            SetupCommonTransition(transition);
        
            transition.duration = 0;
            transition.orderedInterruption = false; // Is the difference relevant?!
            transition.canTransitionToSelf = false;
        }

        private static void SetupCommonTransition(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.offset = 0;
            transition.interruptionSource = TransitionInterruptionSource.None;
        }

        private AnimatorStateMachine ReinitializeAnimatorLayer(string layerName, float weightWhenCreating)
        {
            var layer = TryGetLayer(layerName);
            if (layer == null)
            {
                AddLayerWithWeight(layerName, weightWhenCreating);
                layer = TryGetLayer(layerName);
            }

            var machine = layer.stateMachine;
            SafeEraseStates(machine);

            machine.anyStatePosition = GridPosition(0, 7);
            machine.entryPosition = GridPosition(0, -1);
            machine.exitPosition = GridPosition(7, -1);

            return machine;
        }

        private static void EraseStates(AnimatorStateMachine machine)
        {
            // Running this may cause issues since it may not remove blend trees..?
        
            var states = machine.states;
            ArrayUtility.Clear(ref states);
            machine.states = states;

            var stateMachines = machine.stateMachines;
            ArrayUtility.Clear(ref stateMachines);
            machine.stateMachines = stateMachines;
        }

        private static void SafeEraseStates(AnimatorStateMachine machine)
        {
            foreach (var state in machine.states)
            {
                machine.RemoveState(state.state);
            }
        }

        private AnimatorControllerLayer TryGetLayer(string layerName)
        {
            return _animatorController.layers.FirstOrDefault(it => it.name == layerName);
        }

        private void AddLayerWithWeight(string layerName, float weightWhenCreating)
        {
            // This function is a replication of AnimatorController::AddLayer(string) behavior, in order to change the weight.
            // For some reason I cannot find how to change the layer weight after it has been created.
        
            var newLayer = new AnimatorControllerLayer();
            newLayer.name = _animatorController.MakeUniqueLayerName(layerName);
            newLayer.stateMachine = new AnimatorStateMachine();
            newLayer.stateMachine.name = newLayer.name;
            newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            newLayer.defaultWeight = weightWhenCreating;
            if (AssetDatabase.GetAssetPath(_animatorController) != "")
            {
                AssetDatabase.AddObjectToAsset(newLayer.stateMachine,
                    AssetDatabase.GetAssetPath(_animatorController));
            }

            _animatorController.AddLayer(newLayer);
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }

        private static Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * 200 , y * 70, 0);
        }
    }

    class ActivityManifest
    {
        public int StageValue { get; }
        public RawGestureManifest Manifest { get; }
        public int LayerOrdinal { get; }

        public ActivityManifest(int stageValue, RawGestureManifest manifest, int layerOrdinal)
        {
            StageValue = stageValue;
            Manifest = manifest;
            LayerOrdinal = layerOrdinal;
        }
    }
}
