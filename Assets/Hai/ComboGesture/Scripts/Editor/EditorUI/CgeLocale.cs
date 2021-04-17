
// ReSharper disable InconsistentNaming

using System.Linq;
using System.Reflection;
using System.Text;
using Hai.ExpressionsEditor.Scripts.Editor.Internal;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public class CgeLocale
    {
        private static string CGE_Documentation_URL => LocalizeOrElse("CGE_Documentation_URL", CgeLocaleDefaults.CGE_Documentation_URL);
        private static string CGE_PermutationsDocumentation_URL => LocalizeOrElse("CGE_PermutationsDocumentation_URL", CgeLocaleDefaults.CGE_PermutationsDocumentation_URL);
        private static string CGE_IntegratorDocumentation_URL => LocalizeOrElse("CGE_IntegratorDocumentation_URL", CgeLocaleDefaults.CGE_IntegratorDocumentation_URL);
        // 1.4
        internal static string CGEE_Open_editor => LocalizeOrElse("CGEE_Open_editor", CgeLocaleDefaults.CGEE_Open_editor);
        internal static string CGEE_Additional_editors => LocalizeOrElse("CGEE_Additional_editors", CgeLocaleDefaults.CGEE_Additional_editors);
        internal static string CGEE_All_combos => LocalizeOrElse("CGEE_All_combos", CgeLocaleDefaults.CGEE_All_combos);
        internal static string CGEE_Analog_Fist => LocalizeOrElse("CGEE_Analog_Fist", CgeLocaleDefaults.CGEE_Analog_Fist);
        internal static string CGEE_Combine_expressions => LocalizeOrElse("CGEE_Combine_expressions", CgeLocaleDefaults.CGEE_Combine_expressions);
        internal static string CGEE_Combos => LocalizeOrElse("CGEE_Combos", CgeLocaleDefaults.CGEE_Combos);
        internal static string CGEE_Complete_view => LocalizeOrElse("CGEE_Complete_view", CgeLocaleDefaults.CGEE_Complete_view);
        internal static string CGEE_Create_blend_trees => LocalizeOrElse("CGEE_Create_blend_trees", CgeLocaleDefaults.CGEE_Create_blend_trees);
        internal static string CGEE_Edit_lipsync_settings => LocalizeOrElse("CGEE_Edit_lipsync_settings", CgeLocaleDefaults.CGEE_Edit_lipsync_settings);
        internal static string CGEE_Make_lipsync_movements_subtle => LocalizeOrElse("CGEE_Make_lipsync_movements_subtle", CgeLocaleDefaults.CGEE_Make_lipsync_movements_subtle);
        internal static string CGEE_Manipulate_trees => LocalizeOrElse("CGEE_Manipulate_trees", CgeLocaleDefaults.CGEE_Manipulate_trees);
        internal static string CGEE_Other_options => LocalizeOrElse("CGEE_Other_options", CgeLocaleDefaults.CGEE_Other_options);
        internal static string CGEE_Permutations => LocalizeOrElse("CGEE_Permutations", CgeLocaleDefaults.CGEE_Permutations);
        internal static string CGEE_Prevent_eyes_blinking => LocalizeOrElse("CGEE_Prevent_eyes_blinking", CgeLocaleDefaults.CGEE_Prevent_eyes_blinking);
        internal static string CGEE_Select_wide_open_mouth => LocalizeOrElse("CGEE_Select_wide_open_mouth", CgeLocaleDefaults.CGEE_Select_wide_open_mouth);
        internal static string CGEE_Set_face_expressions => LocalizeOrElse("CGEE_Set_face_expressions", CgeLocaleDefaults.CGEE_Set_face_expressions);
        internal static string CGEE_Simplified_view => LocalizeOrElse("CGEE_Simplified_view", CgeLocaleDefaults.CGEE_Simplified_view);
        internal static string CGEE_Singles => LocalizeOrElse("CGEE_Singles", CgeLocaleDefaults.CGEE_Singles);
        internal static string CGEE_Tutorials => LocalizeOrElse("CGEE_Tutorials", CgeLocaleDefaults.CGEE_Tutorials);
        internal static string CGEE_View_blend_trees => LocalizeOrElse("CGEE_View_blend_trees", CgeLocaleDefaults.CGEE_View_blend_trees);
        internal static string CGEE_Open_Documentation_and_tutorials => LocalizeOrElse("CGEE_Open_Documentation_and_tutorials", CgeLocaleDefaults.CGEE_Open_Documentation_and_tutorials);
        internal static string CGEE_PermutationsIntro => LocalizeOrElse("CGEE_PermutationsIntro", CgeLocaleDefaults.CGEE_PermutationsIntro);
        internal static string CGEE_ConfirmUsePermutations => LocalizeOrElse("CGEE_ConfirmUsePermutations", CgeLocaleDefaults.CGEE_ConfirmUsePermutations);
        internal static string CGEE_Enable_permutations_for_this_Activity => LocalizeOrElse("CGEE_Enable_permutations_for_this_Activity", CgeLocaleDefaults.CGEE_Enable_permutations_for_this_Activity);
        internal static string CGEE_PermutationsFootnote => LocalizeOrElse("CGEE_PermutationsFootnote", CgeLocaleDefaults.CGEE_PermutationsFootnote);
        internal static string CGEE_GeneratePreview => LocalizeOrElse("CGEE_GeneratePreview", CgeLocaleDefaults.CGEE_GeneratePreview);
        internal static string CGEE_SetupPreview => LocalizeOrElse("CGEE_SetupPreview", CgeLocaleDefaults.CGEE_SetupPreview);
        internal static string CGEE_SelectFaceExpressionsWithBothEyesClosed => LocalizeOrElse("CGEE_SelectFaceExpressionsWithBothEyesClosed", CgeLocaleDefaults.CGEE_SelectFaceExpressionsWithBothEyesClosed);
        internal static string CGEE_Blinking => LocalizeOrElse("CGEE_Blinking", CgeLocaleDefaults.CGEE_Blinking);
        internal static string CGEE_Transition_duration_in_seconds => LocalizeOrElse("CGEE_Transition_duration_in_seconds", CgeLocaleDefaults.CGEE_Transition_duration_in_seconds);
        internal static string CGEE_IncompletePreviewSetup => LocalizeOrElse("CGEE_IncompletePreviewSetup", CgeLocaleDefaults.CGEE_IncompletePreviewSetup);
        internal static string CGEE_Automatically_setup_preview => LocalizeOrElse("CGEE_Automatically_setup_preview", CgeLocaleDefaults.CGEE_Automatically_setup_preview);
        internal static string CGEE_AutoSetupReused => LocalizeOrElse("CGEE_AutoSetupReused", CgeLocaleDefaults.CGEE_AutoSetupReused);
        internal static string CGEE_AutoSetupNoActiveAvatarDescriptor => LocalizeOrElse("CGEE_AutoSetupNoActiveAvatarDescriptor", CgeLocaleDefaults.CGEE_AutoSetupNoActiveAvatarDescriptor);
        internal static string CGEE_AutoSetupCreated => LocalizeOrElse("CGEE_AutoSetupCreated", CgeLocaleDefaults.CGEE_AutoSetupCreated);
        internal static string CGEE_Transition_duration => LocalizeOrElse("CGEE_Transition_duration", CgeLocaleDefaults.CGEE_Transition_duration);
        internal static string CGEE_Preview_setup => LocalizeOrElse("CGEE_Preview_setup", CgeLocaleDefaults.CGEE_Preview_setup);
        internal static string CGEE_List_of_arbitrary_animations => LocalizeOrElse("CGEE_List_of_arbitrary_animations", CgeLocaleDefaults.CGEE_List_of_arbitrary_animations);
        internal static string CGEE_Generate_missing_previews => LocalizeOrElse("CGEE_Generate_missing_previews", CgeLocaleDefaults.CGEE_Generate_missing_previews);
        internal static string CGEE_Regenerate_all_previews => LocalizeOrElse("CGEE_Regenerate_all_previews", CgeLocaleDefaults.CGEE_Regenerate_all_previews);
        internal static string CGEE_Stop_generating_previews => LocalizeOrElse("CGEE_Stop_generating_previews", CgeLocaleDefaults.CGEE_Stop_generating_previews);
        internal static string CGEE_ExplainFourDirections => LocalizeOrElse("CGEE_ExplainFourDirections", CgeLocaleDefaults.CGEE_ExplainFourDirections);
        internal static string CGEE_ExplainEightDirections => LocalizeOrElse("CGEE_ExplainEightDirections", CgeLocaleDefaults.CGEE_ExplainEightDirections);
        internal static string CGEE_ExplainSixDirectionsPointingForward => LocalizeOrElse("CGEE_ExplainSixDirectionsPointingForward", CgeLocaleDefaults.CGEE_ExplainSixDirectionsPointingForward);
        internal static string CGEE_ExplainSixDirectionsPointingSideways => LocalizeOrElse("CGEE_ExplainSixDirectionsPointingSideways", CgeLocaleDefaults.CGEE_ExplainSixDirectionsPointingSideways);
        internal static string CGEE_ExplainSingleAnalogFistWithHairTrigger => LocalizeOrElse("CGEE_ExplainSingleAnalogFistWithHairTrigger", CgeLocaleDefaults.CGEE_ExplainSingleAnalogFistWithHairTrigger);
        internal static string CGEE_ExplainSingleAnalogFistAndTwoDirections => LocalizeOrElse("CGEE_ExplainSingleAnalogFistAndTwoDirections", CgeLocaleDefaults.CGEE_ExplainSingleAnalogFistAndTwoDirections);
        internal static string CGEE_ExplainDualAnalogFist => LocalizeOrElse("CGEE_ExplainDualAnalogFist", CgeLocaleDefaults.CGEE_ExplainDualAnalogFist);
        internal static string CGEE_Create_a_new_blend_tree => LocalizeOrElse("CGEE_Create_a_new_blend_tree", CgeLocaleDefaults.CGEE_Create_a_new_blend_tree);
        internal static string CGEE_Blend_tree_asset => LocalizeOrElse("CGEE_Blend_tree_asset", CgeLocaleDefaults.CGEE_Blend_tree_asset);
        //
        internal static string CGEC_Documentation_and_tutorials => LocalizeOrElse("CGEC_Documentation_and_tutorials", CgeLocaleDefaults.CGEC_Documentation_and_tutorials);
        internal static string CGEC_BackupFX => LocalizeOrElse("CGEC_BackupFX", CgeLocaleDefaults.CGEC_BackupFX);
        internal static string CGEC_FX_Animator_Controller => LocalizeOrElse("CGEC_FX_Animator_Controller", CgeLocaleDefaults.CGEC_FX_Animator_Controller);
        internal static string CGEC_FX_Playable_Layer => LocalizeOrElse("CGEC_FX_Playable_Layer", CgeLocaleDefaults.CGEC_FX_Playable_Layer);
        internal static string CGEC_Gesture_Playable_Layer => LocalizeOrElse("CGEC_Gesture_Playable_Layer", CgeLocaleDefaults.CGEC_Gesture_Playable_Layer);
        internal static string CGEC_Parameter_Mode => LocalizeOrElse("CGEC_Parameter_Mode", CgeLocaleDefaults.CGEC_Parameter_Mode);
        internal static string CGEC_Parameter_Name => LocalizeOrElse("CGEC_Parameter_Name", CgeLocaleDefaults.CGEC_Parameter_Name);
        internal static string CGEC_Parameter_Value => LocalizeOrElse("CGEC_Parameter_Value", CgeLocaleDefaults.CGEC_Parameter_Value);
        internal static string CGEC_Mood_sets => LocalizeOrElse("CGEC_Mood_sets", CgeLocaleDefaults.CGEC_Mood_sets);
        internal static string CGEC_HelpExpressionParameterOptimize => LocalizeOrElse("CGEC_HelpExpressionParameterOptimize", CgeLocaleDefaults.CGEC_HelpExpressionParameterOptimize);
        internal static string CGEC_WarnValuesOverlap => LocalizeOrElse("CGEC_WarnValuesOverlap", CgeLocaleDefaults.CGEC_WarnValuesOverlap);
        internal static string CGEC_WarnNamesOverlap => LocalizeOrElse("CGEC_WarnNamesOverlap", CgeLocaleDefaults.CGEC_WarnNamesOverlap);
        internal static string CGEC_WarnNoBlendTree => LocalizeOrElse("CGEC_WarnNoBlendTree", CgeLocaleDefaults.CGEC_WarnNoBlendTree);
        internal static string CGEC_WarnNoActivity => LocalizeOrElse("CGEC_WarnNoActivity", CgeLocaleDefaults.CGEC_WarnNoActivity);
        internal static string CGEC_HelpWhenAllParameterNamesDefined => LocalizeOrElse("CGEC_HelpWhenAllParameterNamesDefined", CgeLocaleDefaults.CGEC_HelpWhenAllParameterNamesDefined);
        internal static string CGEC_HintDefaultMood => LocalizeOrElse("CGEC_HintDefaultMood", CgeLocaleDefaults.CGEC_HintDefaultMood);
        internal static string CGEC_GestureWeight_correction => LocalizeOrElse("CGEC_GestureWeight_correction", CgeLocaleDefaults.CGEC_GestureWeight_correction);
        internal static string CGEC_Avatar_descriptor => LocalizeOrElse("CGEC_Avatar_descriptor", CgeLocaleDefaults.CGEC_Avatar_descriptor);
        internal static string CGEC_Lipsync_correction => LocalizeOrElse("CGEC_Lipsync_correction", CgeLocaleDefaults.CGEC_Lipsync_correction);
        internal static string CGEC_Found_lipsync_blendshapes => LocalizeOrElse("CGEC_Found_lipsync_blendshapes", CgeLocaleDefaults.CGEC_Found_lipsync_blendshapes);
        internal static string CGEC_No_lipsync_blendshapes_found => LocalizeOrElse("CGEC_No_lipsync_blendshapes_found", CgeLocaleDefaults.CGEC_No_lipsync_blendshapes_found);
        internal static string CGEC_Support_for_other_transforms => LocalizeOrElse("CGEC_Support_for_other_transforms", CgeLocaleDefaults.CGEC_Support_for_other_transforms);
        internal static string CGEC_Gesture_playable_layer_support => LocalizeOrElse("CGEC_Gesture_playable_layer_support", CgeLocaleDefaults.CGEC_Gesture_playable_layer_support);
        internal static string CGEC_BackupGesture => LocalizeOrElse("CGEC_BackupGesture", CgeLocaleDefaults.CGEC_BackupGesture);
        internal static string CGEC_Gesture_Animator_Controller => LocalizeOrElse("CGEC_Gesture_Animator_Controller", CgeLocaleDefaults.CGEC_Gesture_Animator_Controller);
        internal static string CGEC_MusclesUnsupported => LocalizeOrElse("CGEC_MusclesUnsupported", CgeLocaleDefaults.CGEC_MusclesUnsupported);
        internal static string CGEC_Synchronization => LocalizeOrElse("CGEC_Synchronization", CgeLocaleDefaults.CGEC_Synchronization);
        internal static string CGEC_Synchronize_Animator_FX_and_Gesture_layers => LocalizeOrElse("CGEC_Synchronize_Animator_FX_and_Gesture_layers", CgeLocaleDefaults.CGEC_Synchronize_Animator_FX_and_Gesture_layers);
        internal static string CGEC_Synchronize_Animator_FX_layers => LocalizeOrElse("CGEC_Synchronize_Animator_FX_layers", CgeLocaleDefaults.CGEC_Synchronize_Animator_FX_layers);
        internal static string CGEC_SynchronizationConditionsV1 => LocalizeOrElse("CGEC_SynchronizationConditionsV1", CgeLocaleDefaults.CGEC_SynchronizationConditionsV1);
        internal static string CGEC_Asset_generation => LocalizeOrElse("CGEC_Asset_generation", CgeLocaleDefaults.CGEC_Asset_generation);
        internal static string CGEC_Asset_container => LocalizeOrElse("CGEC_Asset_container", CgeLocaleDefaults.CGEC_Asset_container);
        internal static string CGEC_FX_Playable_Mode => LocalizeOrElse("CGEC_FX_Playable_Mode", CgeLocaleDefaults.CGEC_FX_Playable_Mode);
        internal static string CGEC_WarnCautiousWriteDefaultsChosenOff => LocalizeOrElse("CGEC_WarnCautiousWriteDefaultsChosenOff", CgeLocaleDefaults.CGEC_WarnCautiousWriteDefaultsChosenOff);
        internal static string CGEC_WarnWriteDefaultsChosenOff => LocalizeOrElse("CGEC_WarnWriteDefaultsChosenOff", CgeLocaleDefaults.CGEC_WarnWriteDefaultsChosenOff);
        internal static string CGEC_AndMoreOnly15FirstResults => LocalizeOrElse("CGEC_AndMoreOnly15FirstResults", CgeLocaleDefaults.CGEC_AndMoreOnly15FirstResults);
        internal static string CGEC_WarnWriteDefaultsOnStatesFound => LocalizeOrElse("CGEC_WarnWriteDefaultsOnStatesFound", CgeLocaleDefaults.CGEC_WarnWriteDefaultsOnStatesFound);
        internal static string CGEC_Gesture_Playable_Mode => LocalizeOrElse("CGEC_Gesture_Playable_Mode", CgeLocaleDefaults.CGEC_Gesture_Playable_Mode);
        internal static string CGEC_Other_tweaks => LocalizeOrElse("CGEC_Other_tweaks", CgeLocaleDefaults.CGEC_Other_tweaks);
        internal static string CGEC_Analog_fist_blinking_threshold => LocalizeOrElse("CGEC_Analog_fist_blinking_threshold", CgeLocaleDefaults.CGEC_Analog_fist_blinking_threshold);
        internal static string CGEC_AnalogFist_Popup => LocalizeOrElse("CGEC_AnalogFist_Popup", CgeLocaleDefaults.CGEC_AnalogFist_Popup);
        internal static string CGEC_Advanced => LocalizeOrElse("CGEC_Advanced", CgeLocaleDefaults.CGEC_Advanced);
        internal static string CGEC_WarnNoActivityName => LocalizeOrElse("CGEC_WarnNoActivityName", CgeLocaleDefaults.CGEC_WarnNoActivityName);
        internal static string CGEC_Capture_Transforms_Mode => LocalizeOrElse("CGEC_Capture_Transforms_Mode", CgeLocaleDefaults.CGEC_Capture_Transforms_Mode);
        internal static string CGEC_MissingFxMask => LocalizeOrElse("CGEC_MissingFxMask", CgeLocaleDefaults.CGEC_MissingFxMask);
        internal static string CGEC_Add_missing_masks => LocalizeOrElse("CGEC_Add_missing_masks", CgeLocaleDefaults.CGEC_Add_missing_masks);
        internal static string CGEC_Remove_applied_masks => LocalizeOrElse("CGEC_Remove_applied_masks", CgeLocaleDefaults.CGEC_Remove_applied_masks);
        internal static string CGEC_Unbind_Asset_container => LocalizeOrElse("CGEC_Unbind_Asset_container", CgeLocaleDefaults.CGEC_Unbind_Asset_container);
        //
        internal static string CGEI_Documentation => LocalizeOrElse("CGEI_Documentation", CgeLocaleDefaults.CGEI_Documentation);
        internal static string CGEI_BackupAnimator => LocalizeOrElse("CGEI_BackupAnimator", CgeLocaleDefaults.CGEI_BackupAnimator);
        internal static string CGEI_Animator_Controller => LocalizeOrElse("CGEI_Animator_Controller", CgeLocaleDefaults.CGEI_Animator_Controller);
        internal static string CGEI_Info => LocalizeOrElse("CGEI_Info", CgeLocaleDefaults.CGEI_Info);
        internal static string CGEI_Synchronize_Animator_layers => LocalizeOrElse("CGEI_Synchronize_Animator_layers", CgeLocaleDefaults.CGEI_Synchronize_Animator_layers);
        // 1.5
        internal static string CGEE_EyesAreClosed => LocalizeOrElse("CGEE_EyesAreClosed", CgeLocaleDefaults.CGEE_EyesAreClosed);

        private static string LocalizeOrElse(string key, string defaultCultureLocalization)
        {
            return CgeLocalization.LocalizeOrElse(key, defaultCultureLocalization);
        }

        public static string DocumentationUrl()
        {
            var localizedUrl = CGE_Documentation_URL;
            return localizedUrl.StartsWith(CgeLocaleDefaults.OfficialDocumentationUrlAsPrefix) ? localizedUrl : CgeLocaleDefaults.CGE_Documentation_URL;
        }

        public static string PermutationsDocumentationUrl()
        {
            var localizedUrl = CGE_PermutationsDocumentation_URL;
            return localizedUrl.StartsWith(CgeLocaleDefaults.OfficialDocumentationUrlAsPrefix) ? localizedUrl : CgeLocaleDefaults.CGE_PermutationsDocumentation_URL;
        }

        public static string IntegratorDocumentationUrl()
        {
            var localizedUrl = CGE_IntegratorDocumentation_URL;
            return localizedUrl.StartsWith(CgeLocaleDefaults.OfficialDocumentationUrlAsPrefix) ? localizedUrl : CgeLocaleDefaults.CGE_IntegratorDocumentation_URL;
        }

        public static string CompileDefaultLocaleJson()
        {
            var fields = typeof(CgeLocaleDefaults).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var jsonObject = new JSONObject();
            foreach (var field in fields.Where(info => info.Name.StartsWith("CGE")))
            {
                jsonObject[field.Name] = new JSONString((string) field.GetValue(null));
            }

            var sb = new StringBuilder();
            jsonObject.WriteToStringBuilder(sb, 0, 0, JSONTextMode.Indent);
            return sb.ToString();
        }
    }
}
