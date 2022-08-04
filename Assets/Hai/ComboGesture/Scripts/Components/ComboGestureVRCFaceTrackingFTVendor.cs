using System.Collections.Generic;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureVRCFaceTrackingFTVendor : ComboGestureFTVendor
    {
        // VRCFaceTracking Vendor
        // https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters

        public CgeVendorGroup GROUP_Eye_Tracking_Parameters = CgeVendorGroup.Some;
        public bool EyesX; //	Gaze Direction X	Combined
        public bool EyesY; //	Gaze Direction Y	Combined
        public bool LeftEyeLid; //	Eyelid Open	Left
        public bool RightEyeLid; //	Eyelid Open	Right
        public bool CombinedEyeLid; //	Eyelid Open	Combined
        public bool EyesWiden; //	Eye Widen	Combined
        public bool EyesDilation; //	Pupil Dilation (0 is Constricted, 1 is Dilated)	Combined
        public bool EyesSqueeze; //	Eyelid Squeeze	Combined
        public bool LeftEyeX; //	Gaze Direction X	Left
        public bool LeftEyeY; //	Gaze Direction Y	Left
        public bool RightEyeX; //	Gaze Direction X	Right
        public bool RightEyeY; //	Gaze Direction Y	Right
        public bool LeftEyeWiden; //	Eye Widen	Left
        public bool RightEyeWiden; //	Eye Widen	Right
        public bool LeftEyeSqueeze; //	Eyelid Squeeze	Left
        public bool RightEyeSqueeze; //	Eyelid Squeeze	Right
        public bool LeftEyeLidExpanded; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Left
        public bool RightEyeLidExpanded; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Right
        public bool CombinedEyeLidExpanded; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Combined
        public bool LeftEyeLidExpandedSqueeze; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Left
        public bool RightEyeLidExpandedSqueeze; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Right
        public bool CombinedEyeLidExpandedSqueeze; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Combined

        public CgeVendorGroup GROUP_Lip_Tracking_Parameters = CgeVendorGroup.Some;
        public bool JawRight; //	Jaw translation right	0.0 - 1.0
        public bool JawLeft; //	Jaw translation left	0.0 - 1.0
        public bool JawForward; //	Jaw translation jutting out	0.0 - 1.0
        public bool JawOpen; //	Jaw open	0.0 - 1.0
        public bool MouthApeShape; //	Jaw open, lips sealed closed	0.0 - 1.0
        public bool MouthUpperRight; //	Upper lip translate right, and not showing teeth	0.0 - 1.0
        public bool MouthUpperLeft; //	Upper lip translate left, and not showing teeth	0.0 - 1.0
        public bool MouthLowerRight; //	Lower lip translate right	0.0 - 1.0
        public bool MouthLowerLeft; //	Lower lip translate left	0.0 - 1.0
        public bool MouthUpperOverturn; //	Pushing top lip out	0.0 - 1.0
        public bool MouthLowerOverturn; //	Pouting out lower lip	0.0 - 1.0
        public bool MouthPout; //	Both lips pouting forward	0.0 - 1.0
        public bool MouthSmileRight; //	Smile right1	0.0 - 1.0
        public bool MouthSmileLeft; //	Smile left1	0.0 - 1.0
        public bool MouthSadRight; //	Sad Right1	0.0 - 1.0
        public bool MouthSadLeft; //	Sad Left1	0.0 - 1.0
        public bool CheekPuffRight; //	Cheek puffed out, right	0.0 - 1.0
        public bool CheekPuffLeft; //	Cheek puffed out, left	0.0 - 1.0
        public bool CheekSuck; //	Both cheeks sucked in	0.0 - 1.0
        public bool MouthUpperUpRight; //	Upper right lip drawn up to show teeth	0.0 - 1.0
        public bool MouthUpperUpLeft; //	Upper left lip drawn up to show teeth	0.0 - 1.0
        public bool MouthLowerDownRight; //	Bottom right lip drawn down to show teeth	0.0 - 1.0
        public bool MouthLowerDownLeft; //	Bottom left lip drawn down to show teeth	0.0 - 1.0
        public bool MouthUpperInside; //	Upper lip bitten by lower teeth	0.0 - 1.0
        public bool MouthLowerInside; //	Bottom lip bitten by upper teeth	0.0 - 1.0
        public bool MouthLowerOverlay; //	Upper lip out and over lower	0.0 - 1.0
        public bool TongueLongStep1; //	Seems to be an intermediate out	0.0 - 1.0
        public bool TongueLongStep2; //	Seems to be an intermediate out	0.0 - 1.0
        public bool TongueDown; //	Tongue tip angled down	0.0 - 1.0
        public bool TongueUp; //	Tongue tip angled up	0.0 - 1.0
        public bool TongueRight; //	Tongue tip angled right	0.0 - 1.0
        public bool TongueLeft; //	Tongue tip angled left	0.0 - 1.0
        public bool TongueRoll; //	Both sides of tongue brought up into "v"	0.0 - 1.0
        public bool TongueUpLeftMorph; //	Seems to deform upper left of tongue out of mouth	0.0 - 1.0
        public bool TongueUpRightMorph; //	Seems to deform upper right of tongue out of mouth	0.0 - 1.0
        public bool TongueDownLeftMorph; //	Seems to deform lower left of tongue out of mouth	0.0 - 1.0
        public bool TongueDownRightMorph; //	Seems to deform lower right of tongue out of mouth	0.0 - 1.0
        
        public CgeVendorGroup GROUP_General_Combined_Lip_Parameters = CgeVendorGroup.Some;
        public bool JawX; //	Jaw translation fully left to fully right	-1.0 - 1.0
        public bool MouthUpper; //	MouthUpperLeft to MouthUpperRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLower; //	MouthLowerLeft to MouthLowerRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthX; //	MouthLeft (Upper/Lower) to MouthRight (Upper/Lower), with 0 being neutral	-1.0 - 1.0
        public bool SmileSadRight; //	MouthSadRight to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileSadLeft; //	MouthSadLeft to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileSad; //	MouthSad (Left/Right) to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool TongueY; //	TongueDown to TongueUp, with 0 being neutral	-1.0 - 1.0
        public bool TongueX; //	TongueLeft to TongueRight, with 0 being neutral	-1.0 - 1.0
        public bool TongueSteps; //	TongueLongStep1 to TongueLongStep2, with -1 being tongue fully in to 1 being fully out	-1.0 - 1.0
        public bool PuffSuckRight; //	CheekSuck to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffSuckLeft; //	CheekSuck to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffSuck; //	CheekSuck to CheekPuff (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Jaw_Open_Combined_Lip_Parameters = CgeVendorGroup.Some;
        public bool JawOpenApe; //	MouthApeShape to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuff; //	CheekPuff (Left/Right) to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuffRight; //	CheekPuffRight to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuffLeft; //	CheekPuffLeft to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenSuck; //	CheekSuck to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenForward; //	JawForward to JawOpen, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Right_Combined_Lip_Parameters = CgeVendorGroup.Some;
        public bool MouthUpperUpRightUpperInside; //	MouthUpperInside to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightPuffRight; //	CheekPuffRight to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightApe; //	MouthApeShape to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightPout; //	MouthPout to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightOverlay; //	MouthLowerOverlay Shape to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Left_Combined_Parameters = CgeVendorGroup.Some;
        public bool MouthUpperUpLeftUpperInside; //	MouthUpperInside to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftPuffLeft; //	CheekPuffLeft to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftApe; //	MouthApeShape to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftPout; //	MouthPout to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftOverlay; //	MouthLowerOverlay Shape to MouthUpperUpLeft (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Combined_Parameters = CgeVendorGroup.Some;
        public bool MouthUpperUpUpperInside; //	MouthUpperInside to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpInside; //	MouthInside (Upper/Lower) to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuff; //	CheekPuff (Left/Right) to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuffLeft; //	CheekPuffLeft to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuffRight; //	CheekPuffRight to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpApe; //	MouthApeShape to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPout; //	MouthPout to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpOverlay; //	MouthLowerOverlay Shape to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Right_Combined_Parameters = CgeVendorGroup.Some;
        public bool MouthLowerDownRightLowerInside; //	MouthLowerInside to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightPuffRight; //	CheekPuffRight to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightApe; //	MouthApeShape to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightPout; //	MouthPout to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightOverlay; //	MouthLowerOverlay Shape to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Left_Combined_Parameters = CgeVendorGroup.Some;
        public bool MouthLowerDownLeftLowerInside; //	MouthLowerInside to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftPuffLeft; //	CheekPuffLeft to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftApe; //	MouthApeShape to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftPout; //	MouthPout to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftOverlay; //	MouthLowerOverlay Shape to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Combined_Parameters = CgeVendorGroup.Some;
        public bool MouthLowerDownLowerInside; //	MouthLowerInside to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownInside; //	MouthInside (Upper/Lower) to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuff; //	CheekPuff (Left/Right) to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuffLeft; //	CheekPuffLeft to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuffRight; //	CheekPuffRight to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownApe; //	MouthApeShape to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPout; //	MouthPout to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownOverlay; //	MouthLowerOverlay Shape to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Right_Combined_Parameters = CgeVendorGroup.Some;
        public bool SmileRightUpperOverturn; //	MouthUpperOverturn to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightLowerOverturn; //	MouthLowerOverturn to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightOverturn; //	MouthOverturn (Upper/Lower) to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightApe; //	MouthApeShape to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightOverlay; //	MouthLowerOverlay to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightPout; //	MouthPout to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Left_Combined_Parameters = CgeVendorGroup.Some;
        public bool SmileLeftUpperOverturn; //	MouthUpperOverturn to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftLowerOverturn; //	MouthLowerOverturn to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftOverturn; //	MouthOverturn (Upper/Lower) to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftApe; //	MouthApeShape to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftOverlay; //	MouthLowerOverlay to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftPout; //	MouthPout to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Combined_Parameters = CgeVendorGroup.Some;
        public bool SmileUpperOverturn; //	MouthUpperOverturn to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileLowerOverturn; //	MouthLowerOverturn to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileApe; //	MouthApeShape to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileOverlay; //	MouthLowerOverlay to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmilePout; //	MouthLowerPout to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Right_Combined_Parameters = CgeVendorGroup.Some;
        public bool PuffRightUpperOverturn; //	MouthUpperOverturn to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffRightLowerOverturn; //	MouthLowerOverturn to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffRightOverturn; //	MouthOverturn (Upper/Lower) to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Left_Combined_Parameters = CgeVendorGroup.Some;
        public bool PuffLeftUpperOverturn; //	MouthUpperOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLeftLowerOverturn; //	MouthLowerOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLeftOverturn; //	MouthOverturn (Upper/Lower) to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Combined_Parameters = CgeVendorGroup.Some;
        public bool PuffUpperOverturn; //	MouthUpperOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLowerOverturn; //	MouthLowerOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffOverturn; //	MouthOverturn (Upper/Lower) to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        
        public override CgeElementActuator[] ToElementActuators()
        {
            var result = new List<CgeElementActuator>();
            if (EyesX) Joystick(result, nameof(EyesX), CgeElement.Eye_Left_Left, CgeElement.Eye_Left_Right).Joystick(CgeElement.Eye_Right_Left, CgeElement.Eye_Right_Right); // XXX
            if (EyesY) Joystick(result, nameof(EyesX), CgeElement.Eye_Left_Down, CgeElement.Eye_Left_Up).Joystick(CgeElement.Eye_Right_Down, CgeElement.Eye_Right_Up); // XXX
            if (LeftEyeLid) Positive(result, nameof(LeftEyeLid), CgeElement.Eye_Left_Down); // XXX
            if (RightEyeLid) Positive(result, nameof(RightEyeLid), CgeElement.Eye_Right_Down); // XXX
            if (CombinedEyeLid) Positive(result, nameof(CombinedEyeLid), CgeElement.Eye_Left_Down).Positive(CgeElement.Eye_Right_Down); // XXX
            if (EyesWiden) Positive(result, nameof(EyesWiden), CgeElement.Eye_Left_Wide).Positive(CgeElement.Eye_Right_Wide); // XXX
            if (EyesDilation) Aperture(result, nameof(EyesDilation), CgeElement.Eye_Left_Dilation, CgeElement.Eye_Right_Dilation); // XXX
            if (EyesSqueeze) Positive(result, nameof(EyesSqueeze), CgeElement.Eye_Left_Squeeze).Positive(CgeElement.Eye_Right_Squeeze); // XXX
            if (LeftEyeX) Joystick(result, nameof(LeftEyeX), CgeElement.Eye_Left_Left, CgeElement.Eye_Left_Right); // XXX
            if (LeftEyeY) Joystick(result, nameof(LeftEyeY), CgeElement.Eye_Left_Down, CgeElement.Eye_Left_Up); // XXX
            if (RightEyeX) Joystick(result, nameof(RightEyeX), CgeElement.Eye_Right_Left, CgeElement.Eye_Right_Right); // XXX
            if (RightEyeY) Joystick(result, nameof(RightEyeY), CgeElement.Eye_Right_Down, CgeElement.Eye_Right_Up); // XXX
            if (LeftEyeWiden) Positive(result, nameof(LeftEyeWiden), CgeElement.TODO); // XXX
            if (RightEyeWiden) Positive(result, nameof(RightEyeWiden), CgeElement.TODO); // XXX
            if (LeftEyeSqueeze) Positive(result, nameof(LeftEyeSqueeze), CgeElement.TODO); // XXX
            if (RightEyeSqueeze) Positive(result, nameof(RightEyeSqueeze), CgeElement.TODO); // XXX
            if (LeftEyeLidExpanded) Positive(result, nameof(LeftEyeLidExpanded), CgeElement.TODO); // XXX
            if (RightEyeLidExpanded) Positive(result, nameof(RightEyeLidExpanded), CgeElement.TODO); // XXX
            if (CombinedEyeLidExpanded) Positive(result, nameof(CombinedEyeLidExpanded), CgeElement.TODO); // XXX
            if (LeftEyeLidExpandedSqueeze) Positive(result, nameof(LeftEyeLidExpandedSqueeze), CgeElement.TODO); // XXX
            if (RightEyeLidExpandedSqueeze) Positive(result, nameof(RightEyeLidExpandedSqueeze), CgeElement.TODO); // XXX
            if (CombinedEyeLidExpandedSqueeze) Positive(result, nameof(CombinedEyeLidExpandedSqueeze), CgeElement.TODO); // XXX
            if (JawRight) Positive(result, nameof(JawRight), CgeElement.TODO); // XXX
            if (JawLeft) Positive(result, nameof(JawLeft), CgeElement.TODO); // XXX
            if (JawForward) Positive(result, nameof(JawForward), CgeElement.TODO); // XXX
            if (JawOpen) Positive(result, nameof(JawOpen), CgeElement.TODO); // XXX
            if (MouthApeShape) Positive(result, nameof(MouthApeShape), CgeElement.TODO); // XXX
            if (MouthUpperRight) Positive(result, nameof(MouthUpperRight), CgeElement.TODO); // XXX
            if (MouthUpperLeft) Positive(result, nameof(MouthUpperLeft), CgeElement.TODO); // XXX
            if (MouthLowerRight) Positive(result, nameof(MouthLowerRight), CgeElement.TODO); // XXX
            if (MouthLowerLeft) Positive(result, nameof(MouthLowerLeft), CgeElement.TODO); // XXX
            if (MouthUpperOverturn) Positive(result, nameof(MouthUpperOverturn), CgeElement.TODO); // XXX
            if (MouthLowerOverturn) Positive(result, nameof(MouthLowerOverturn), CgeElement.TODO); // XXX
            if (MouthPout) Positive(result, nameof(MouthPout), CgeElement.TODO); // XXX
            if (MouthSmileRight) Positive(result, nameof(MouthSmileRight), CgeElement.TODO); // XXX
            if (MouthSmileLeft) Positive(result, nameof(MouthSmileLeft), CgeElement.TODO); // XXX
            if (MouthSadRight) Positive(result, nameof(MouthSadRight), CgeElement.TODO); // XXX
            if (MouthSadLeft) Positive(result, nameof(MouthSadLeft), CgeElement.TODO); // XXX
            if (CheekPuffRight) Positive(result, nameof(CheekPuffRight), CgeElement.TODO); // XXX
            if (CheekPuffLeft) Positive(result, nameof(CheekPuffLeft), CgeElement.TODO); // XXX
            if (CheekSuck) Positive(result, nameof(CheekSuck), CgeElement.TODO); // XXX
            if (MouthUpperUpRight) Positive(result, nameof(MouthUpperUpRight), CgeElement.TODO); // XXX
            if (MouthUpperUpLeft) Positive(result, nameof(MouthUpperUpLeft), CgeElement.TODO); // XXX
            if (MouthLowerDownRight) Positive(result, nameof(MouthLowerDownRight), CgeElement.TODO); // XXX
            if (MouthLowerDownLeft) Positive(result, nameof(MouthLowerDownLeft), CgeElement.TODO); // XXX
            if (MouthUpperInside) Positive(result, nameof(MouthUpperInside), CgeElement.TODO); // XXX
            if (MouthLowerInside) Positive(result, nameof(MouthLowerInside), CgeElement.TODO); // XXX
            if (MouthLowerOverlay) Positive(result, nameof(MouthLowerOverlay), CgeElement.TODO); // XXX
            if (TongueLongStep1) Positive(result, nameof(TongueLongStep1), CgeElement.TODO); // XXX
            if (TongueLongStep2) Positive(result, nameof(TongueLongStep2), CgeElement.TODO); // XXX
            if (TongueDown) Positive(result, nameof(TongueDown), CgeElement.TODO); // XXX
            if (TongueUp) Positive(result, nameof(TongueUp), CgeElement.TODO); // XXX
            if (TongueRight) Positive(result, nameof(TongueRight), CgeElement.TODO); // XXX
            if (TongueLeft) Positive(result, nameof(TongueLeft), CgeElement.TODO); // XXX
            if (TongueRoll) Positive(result, nameof(TongueRoll), CgeElement.TODO); // XXX
            if (TongueUpLeftMorph) Positive(result, nameof(TongueUpLeftMorph), CgeElement.TODO); // XXX
            if (TongueUpRightMorph) Positive(result, nameof(TongueUpRightMorph), CgeElement.TODO); // XXX
            if (TongueDownLeftMorph) Positive(result, nameof(TongueDownLeftMorph), CgeElement.TODO); // XXX
            if (TongueDownRightMorph) Positive(result, nameof(TongueDownRightMorph), CgeElement.TODO); // XXX
            if (JawX) Positive(result, nameof(JawX), CgeElement.TODO); // XXX
            if (MouthUpper) Positive(result, nameof(MouthUpper), CgeElement.TODO); // XXX
            if (MouthLower) Positive(result, nameof(MouthLower), CgeElement.TODO); // XXX
            if (MouthX) Positive(result, nameof(MouthX), CgeElement.TODO); // XXX
            if (SmileSadRight) Positive(result, nameof(SmileSadRight), CgeElement.TODO); // XXX
            if (SmileSadLeft) Positive(result, nameof(SmileSadLeft), CgeElement.TODO); // XXX
            if (SmileSad) Positive(result, nameof(SmileSad), CgeElement.TODO); // XXX
            if (TongueY) Positive(result, nameof(TongueY), CgeElement.TODO); // XXX
            if (TongueX) Positive(result, nameof(TongueX), CgeElement.TODO); // XXX
            if (TongueSteps) Positive(result, nameof(TongueSteps), CgeElement.TODO); // XXX
            if (PuffSuckRight) Positive(result, nameof(PuffSuckRight), CgeElement.TODO); // XXX
            if (PuffSuckLeft) Positive(result, nameof(PuffSuckLeft), CgeElement.TODO); // XXX
            if (PuffSuck) Positive(result, nameof(PuffSuck), CgeElement.TODO); // XXX
            if (JawOpenApe) Positive(result, nameof(JawOpenApe), CgeElement.TODO); // XXX
            if (JawOpenPuff) Positive(result, nameof(JawOpenPuff), CgeElement.TODO); // XXX
            if (JawOpenPuffRight) Positive(result, nameof(JawOpenPuffRight), CgeElement.TODO); // XXX
            if (JawOpenPuffLeft) Positive(result, nameof(JawOpenPuffLeft), CgeElement.TODO); // XXX
            if (JawOpenSuck) Positive(result, nameof(JawOpenSuck), CgeElement.TODO); // XXX
            if (JawOpenForward) Positive(result, nameof(JawOpenForward), CgeElement.TODO); // XXX
            if (MouthUpperUpRightUpperInside) Positive(result, nameof(MouthUpperUpRightUpperInside), CgeElement.TODO); // XXX
            if (MouthUpperUpRightPuffRight) Positive(result, nameof(MouthUpperUpRightPuffRight), CgeElement.TODO); // XXX
            if (MouthUpperUpRightApe) Positive(result, nameof(MouthUpperUpRightApe), CgeElement.TODO); // XXX
            if (MouthUpperUpRightPout) Positive(result, nameof(MouthUpperUpRightPout), CgeElement.TODO); // XXX
            if (MouthUpperUpRightOverlay) Positive(result, nameof(MouthUpperUpRightOverlay), CgeElement.TODO); // XXX
            if (MouthUpperUpLeftUpperInside) Positive(result, nameof(MouthUpperUpLeftUpperInside), CgeElement.TODO); // XXX
            if (MouthUpperUpLeftPuffLeft) Positive(result, nameof(MouthUpperUpLeftPuffLeft), CgeElement.TODO); // XXX
            if (MouthUpperUpLeftApe) Positive(result, nameof(MouthUpperUpLeftApe), CgeElement.TODO); // XXX
            if (MouthUpperUpLeftPout) Positive(result, nameof(MouthUpperUpLeftPout), CgeElement.TODO); // XXX
            if (MouthUpperUpLeftOverlay) Positive(result, nameof(MouthUpperUpLeftOverlay), CgeElement.TODO); // XXX
            if (MouthUpperUpUpperInside) Positive(result, nameof(MouthUpperUpUpperInside), CgeElement.TODO); // XXX
            if (MouthUpperUpInside) Positive(result, nameof(MouthUpperUpInside), CgeElement.TODO); // XXX
            if (MouthUpperUpPuff) Positive(result, nameof(MouthUpperUpPuff), CgeElement.TODO); // XXX
            if (MouthUpperUpPuffLeft) Positive(result, nameof(MouthUpperUpPuffLeft), CgeElement.TODO); // XXX
            if (MouthUpperUpPuffRight) Positive(result, nameof(MouthUpperUpPuffRight), CgeElement.TODO); // XXX
            if (MouthUpperUpApe) Positive(result, nameof(MouthUpperUpApe), CgeElement.TODO); // XXX
            if (MouthUpperUpPout) Positive(result, nameof(MouthUpperUpPout), CgeElement.TODO); // XXX
            if (MouthUpperUpOverlay) Positive(result, nameof(MouthUpperUpOverlay), CgeElement.TODO); // XXX
            if (MouthLowerDownRightLowerInside) Positive(result, nameof(MouthLowerDownRightLowerInside), CgeElement.TODO); // XXX
            if (MouthLowerDownRightPuffRight) Positive(result, nameof(MouthLowerDownRightPuffRight), CgeElement.TODO); // XXX
            if (MouthLowerDownRightApe) Positive(result, nameof(MouthLowerDownRightApe), CgeElement.TODO); // XXX
            if (MouthLowerDownRightPout) Positive(result, nameof(MouthLowerDownRightPout), CgeElement.TODO); // XXX
            if (MouthLowerDownRightOverlay) Positive(result, nameof(MouthLowerDownRightOverlay), CgeElement.TODO); // XXX
            if (MouthLowerDownLeftLowerInside) Positive(result, nameof(MouthLowerDownLeftLowerInside), CgeElement.TODO); // XXX
            if (MouthLowerDownLeftPuffLeft) Positive(result, nameof(MouthLowerDownLeftPuffLeft), CgeElement.TODO); // XXX
            if (MouthLowerDownLeftApe) Positive(result, nameof(MouthLowerDownLeftApe), CgeElement.TODO); // XXX
            if (MouthLowerDownLeftPout) Positive(result, nameof(MouthLowerDownLeftPout), CgeElement.TODO); // XXX
            if (MouthLowerDownLeftOverlay) Positive(result, nameof(MouthLowerDownLeftOverlay), CgeElement.TODO); // XXX
            if (MouthLowerDownLowerInside) Positive(result, nameof(MouthLowerDownLowerInside), CgeElement.TODO); // XXX
            if (MouthLowerDownInside) Positive(result, nameof(MouthLowerDownInside), CgeElement.TODO); // XXX
            if (MouthLowerDownPuff) Positive(result, nameof(MouthLowerDownPuff), CgeElement.TODO); // XXX
            if (MouthLowerDownPuffLeft) Positive(result, nameof(MouthLowerDownPuffLeft), CgeElement.TODO); // XXX
            if (MouthLowerDownPuffRight) Positive(result, nameof(MouthLowerDownPuffRight), CgeElement.TODO); // XXX
            if (MouthLowerDownApe) Positive(result, nameof(MouthLowerDownApe), CgeElement.TODO); // XXX
            if (MouthLowerDownPout) Positive(result, nameof(MouthLowerDownPout), CgeElement.TODO); // XXX
            if (MouthLowerDownOverlay) Positive(result, nameof(MouthLowerDownOverlay), CgeElement.TODO); // XXX
            if (SmileRightUpperOverturn) Positive(result, nameof(SmileRightUpperOverturn), CgeElement.TODO); // XXX
            if (SmileRightLowerOverturn) Positive(result, nameof(SmileRightLowerOverturn), CgeElement.TODO); // XXX
            if (SmileRightOverturn) Positive(result, nameof(SmileRightOverturn), CgeElement.TODO); // XXX
            if (SmileRightApe) Positive(result, nameof(SmileRightApe), CgeElement.TODO); // XXX
            if (SmileRightOverlay) Positive(result, nameof(SmileRightOverlay), CgeElement.TODO); // XXX
            if (SmileRightPout) Positive(result, nameof(SmileRightPout), CgeElement.TODO); // XXX
            if (SmileLeftUpperOverturn) Positive(result, nameof(SmileLeftUpperOverturn), CgeElement.TODO); // XXX
            if (SmileLeftLowerOverturn) Positive(result, nameof(SmileLeftLowerOverturn), CgeElement.TODO); // XXX
            if (SmileLeftOverturn) Positive(result, nameof(SmileLeftOverturn), CgeElement.TODO); // XXX
            if (SmileLeftApe) Positive(result, nameof(SmileLeftApe), CgeElement.TODO); // XXX
            if (SmileLeftOverlay) Positive(result, nameof(SmileLeftOverlay), CgeElement.TODO); // XXX
            if (SmileLeftPout) Positive(result, nameof(SmileLeftPout), CgeElement.TODO); // XXX
            if (SmileUpperOverturn) Positive(result, nameof(SmileUpperOverturn), CgeElement.TODO); // XXX
            if (SmileLowerOverturn) Positive(result, nameof(SmileLowerOverturn), CgeElement.TODO); // XXX
            if (SmileApe) Positive(result, nameof(SmileApe), CgeElement.TODO); // XXX
            if (SmileOverlay) Positive(result, nameof(SmileOverlay), CgeElement.TODO); // XXX
            if (SmilePout) Positive(result, nameof(SmilePout), CgeElement.TODO); // XXX
            if (PuffRightUpperOverturn) Positive(result, nameof(PuffRightUpperOverturn), CgeElement.TODO); // XXX
            if (PuffRightLowerOverturn) Positive(result, nameof(PuffRightLowerOverturn), CgeElement.TODO); // XXX
            if (PuffRightOverturn) Positive(result, nameof(PuffRightOverturn), CgeElement.TODO); // XXX
            if (PuffLeftUpperOverturn) Positive(result, nameof(PuffLeftUpperOverturn), CgeElement.TODO); // XXX
            if (PuffLeftLowerOverturn) Positive(result, nameof(PuffLeftLowerOverturn), CgeElement.TODO); // XXX
            if (PuffLeftOverturn) Positive(result, nameof(PuffLeftOverturn), CgeElement.TODO); // XXX
            if (PuffUpperOverturn) Positive(result, nameof(PuffUpperOverturn), CgeElement.TODO); // XXX
            if (PuffLowerOverturn) Positive(result, nameof(PuffLowerOverturn), CgeElement.TODO); // XXX
            if (PuffOverturn) Positive(result, nameof(PuffOverturn), CgeElement.TODO); // XXX

            return result.ToArray();
        }

        private class InternalVRCFTContinuation
        {
            private readonly List<CgeElementActuator> _result;
            private readonly string _parameter;

            public InternalVRCFTContinuation(List<CgeElementActuator> result, string parameter)
            {
                _result = result;
                _parameter = parameter;
            }
            
            internal InternalVRCFTContinuation Positive(params CgeElement element)
            {
                _result.Add(new CgeElementActuator
                {
                    element = element,
                    actuator = new CgeActuator
                    {
                        parameter = _parameter,
                        neutral = 0f,
                        actuated = 1f
                    }
                });
                return this;
            }

            internal InternalVRCFTContinuation Joystick(CgeElement negativeLeftDown, CgeElement positiveUpRight)
            {
                _result.Add(new CgeElementActuator
                {
                    element = negativeLeftDown,
                    actuator = new CgeActuator
                    {
                        parameter = _parameter,
                        neutral = 0f,
                        actuated = -1f
                    }
                });
                _result.Add(new CgeElementActuator
                {
                    element = positiveUpRight,
                    actuator = new CgeActuator
                    {
                        parameter = _parameter,
                        neutral = 0f,
                        actuated = 1f
                    }
                });
                return this;
            }

            public InternalVRCFTContinuation PositiveJoystick(CgeElement zero, CgeElement one)
            {
                _result.Add(new CgeElementActuator
                {
                    element = zero,
                    actuator = new CgeActuator
                    {
                        parameter = _parameter,
                        neutral = 0.5f,
                        actuated = 0f
                    }
                });
                _result.Add(new CgeElementActuator
                {
                    element = one,
                    actuator = new CgeActuator
                    {
                        parameter = _parameter,
                        neutral = 0.5f,
                        actuated = 1f
                    }
                });
                return this;
            }
        }

        private InternalVRCFTContinuation Positive(List<CgeElementActuator> result, string parameter, CgeElement element)
        {
            return new InternalVRCFTContinuation(result, parameter)
                .Positive(element);
        }

        private InternalVRCFTContinuation Aperture(List<CgeElementActuator> result, string parameter, CgeElement zero, CgeElement one)
        {
            return new InternalVRCFTContinuation(result, parameter)
                .PositiveJoystick(zero, one);
        }

        private InternalVRCFTContinuation Joystick(List<CgeElementActuator> result, string parameter, CgeElement negativeLeftDown, CgeElement positiveUpRight)
        {
            return new InternalVRCFTContinuation(result, parameter)
                .Joystick(negativeLeftDown, positiveUpRight);
        }
    }
}