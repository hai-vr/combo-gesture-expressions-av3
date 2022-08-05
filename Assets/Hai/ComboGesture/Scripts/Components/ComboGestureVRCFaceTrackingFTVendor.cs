using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Hai.ComboGesture.Scripts.Components.CgeElement;

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

        private class CgeVRCFaceTrackingFTVendorMap
        {
            public CgeElementActuator[] EyesX() => Joystick(nameof(EyesX), Eye_Left_Left, Eye_Left_Right).Joystick(Eye_Right_Left, Eye_Right_Right).ToArray();
            public CgeElementActuator[] EyesY() => Joystick(nameof(EyesY), Eye_Left_Down, Eye_Left_Up).Joystick(Eye_Right_Down, Eye_Right_Up).ToArray();
            public CgeElementActuator[] LeftEyeLid() => P01(nameof(LeftEyeLid), Eye_Left_Down).ToArray();
            public CgeElementActuator[] RightEyeLid() => P01(nameof(RightEyeLid), Eye_Right_Down).ToArray();
            public CgeElementActuator[] CombinedEyeLid() => P01(nameof(CombinedEyeLid), Eye_Left_Down).Positive(Eye_Right_Down).ToArray();
            public CgeElementActuator[] EyesWiden() => P01(nameof(EyesWiden), Eye_Left_Wide).Positive(Eye_Right_Wide).ToArray();
            public CgeElementActuator[] EyesDilation() => Aperture(nameof(EyesDilation), Eye_Left_Dilation, Eye_Right_Dilation).ToArray();
            public CgeElementActuator[] EyesSqueeze() => P01(nameof(EyesSqueeze), Eye_Left_Squeeze).Positive(Eye_Right_Squeeze).ToArray();
            public CgeElementActuator[] LeftEyeX() => Joystick(nameof(LeftEyeX), Eye_Left_Left, Eye_Left_Right).ToArray();
            public CgeElementActuator[] LeftEyeY() => Joystick(nameof(LeftEyeY), Eye_Left_Down, Eye_Left_Up).ToArray();
            public CgeElementActuator[] RightEyeX() => Joystick(nameof(RightEyeX), Eye_Right_Left, Eye_Right_Right).ToArray();
            public CgeElementActuator[] RightEyeY() => Joystick(nameof(RightEyeY), Eye_Right_Down, Eye_Right_Up).ToArray();
            public CgeElementActuator[] LeftEyeWiden() => P01(nameof(LeftEyeWiden), Eye_Left_Wide).ToArray();
            public CgeElementActuator[] RightEyeWiden() => P01(nameof(RightEyeWiden), Eye_Right_Wide).ToArray();
            public CgeElementActuator[] LeftEyeSqueeze() => P01(nameof(LeftEyeSqueeze), Eye_Left_Squeeze).ToArray();
            public CgeElementActuator[] RightEyeSqueeze() => P01(nameof(RightEyeSqueeze), Eye_Right_Squeeze).ToArray();
            public CgeElementActuator[] LeftEyeLidExpanded() => P01(nameof(LeftEyeLidExpanded), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] RightEyeLidExpanded() => P01(nameof(RightEyeLidExpanded), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] CombinedEyeLidExpanded() => P01(nameof(CombinedEyeLidExpanded), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] LeftEyeLidExpandedSqueeze() => P01(nameof(LeftEyeLidExpandedSqueeze), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] RightEyeLidExpandedSqueeze() => P01(nameof(RightEyeLidExpandedSqueeze), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] CombinedEyeLidExpandedSqueeze() => P01(nameof(CombinedEyeLidExpandedSqueeze), NOT_IMPLEMENTED).ToArray();
            public CgeElementActuator[] JawRight() => P01(nameof(JawRight), Jaw_Right).ToArray();
            public CgeElementActuator[] JawLeft() => P01(nameof(JawLeft), Jaw_Left).ToArray();
            public CgeElementActuator[] JawForward() => P01(nameof(JawForward), Jaw_Forward).ToArray();
            public CgeElementActuator[] JawOpen() => P01(nameof(JawOpen), Jaw_Open).ToArray();
            public CgeElementActuator[] MouthApeShape() => P01(nameof(MouthApeShape), Mouth_Ape_Shape).ToArray();
            public CgeElementActuator[] MouthUpperRight() => P01(nameof(MouthUpperRight), Mouth_Upper_Right).ToArray();
            public CgeElementActuator[] MouthUpperLeft() => P01(nameof(MouthUpperLeft), Mouth_Upper_Left).ToArray();
            public CgeElementActuator[] MouthLowerRight() => P01(nameof(MouthLowerRight), Mouth_Lower_Right).ToArray();
            public CgeElementActuator[] MouthLowerLeft() => P01(nameof(MouthLowerLeft), Mouth_Lower_Left).ToArray();
            public CgeElementActuator[] MouthUpperOverturn() => P01(nameof(MouthUpperOverturn), Mouth_Upper_Overturn).ToArray();
            public CgeElementActuator[] MouthLowerOverturn() => P01(nameof(MouthLowerOverturn), Mouth_Lower_Overturn).ToArray();
            public CgeElementActuator[] MouthPout() => P01(nameof(MouthPout), Mouth_Pout).ToArray();
            public CgeElementActuator[] MouthSmileRight() => P01(nameof(MouthSmileRight), Mouth_Smile_Right).ToArray();
            public CgeElementActuator[] MouthSmileLeft() => P01(nameof(MouthSmileLeft), Mouth_Smile_Left).ToArray();
            public CgeElementActuator[] MouthSadRight() => P01(nameof(MouthSadRight), Mouth_Sad_Right).ToArray();
            public CgeElementActuator[] MouthSadLeft() => P01(nameof(MouthSadLeft), Mouth_Sad_Left).ToArray();
            public CgeElementActuator[] CheekPuffRight() => P01(nameof(CheekPuffRight), Cheek_Puff_Right).ToArray();
            public CgeElementActuator[] CheekPuffLeft() => P01(nameof(CheekPuffLeft), Cheek_Puff_Left).ToArray();
            public CgeElementActuator[] CheekSuck() => P01(nameof(CheekSuck), Cheek_Suck).ToArray();
            public CgeElementActuator[] MouthUpperUpRight() => P01(nameof(MouthUpperUpRight), Mouth_Upper_UpRight).ToArray();
            public CgeElementActuator[] MouthUpperUpLeft() => P01(nameof(MouthUpperUpLeft), Mouth_Upper_UpLeft).ToArray();
            public CgeElementActuator[] MouthLowerDownRight() => P01(nameof(MouthLowerDownRight), Mouth_Lower_DownRight).ToArray();
            public CgeElementActuator[] MouthLowerDownLeft() => P01(nameof(MouthLowerDownLeft), Mouth_Lower_DownLeft).ToArray();
            public CgeElementActuator[] MouthUpperInside() => P01(nameof(MouthUpperInside), Mouth_Upper_Inside).ToArray();
            public CgeElementActuator[] MouthLowerInside() => P01(nameof(MouthLowerInside), Mouth_Lower_Inside).ToArray();
            public CgeElementActuator[] MouthLowerOverlay() => P01(nameof(MouthLowerOverlay), Mouth_Lower_Overlay).ToArray();
            public CgeElementActuator[] TongueLongStep1() => P01(nameof(TongueLongStep1), Tongue_LongStep1).ToArray();
            public CgeElementActuator[] TongueLongStep2() => P01(nameof(TongueLongStep2), Tongue_LongStep2).ToArray();
            public CgeElementActuator[] TongueDown() => P01(nameof(TongueDown), Tongue_Down).ToArray();
            public CgeElementActuator[] TongueUp() => P01(nameof(TongueUp), Tongue_Up).ToArray();
            public CgeElementActuator[] TongueRight() => P01(nameof(TongueRight), Tongue_Right).ToArray();
            public CgeElementActuator[] TongueLeft() => P01(nameof(TongueLeft), Tongue_Left).ToArray();
            public CgeElementActuator[] TongueRoll() => P01(nameof(TongueRoll), Tongue_Roll).ToArray();
            public CgeElementActuator[] TongueUpLeftMorph() => P01(nameof(TongueUpLeftMorph), Tongue_UpLeft_Morph).ToArray();
            public CgeElementActuator[] TongueUpRightMorph() => P01(nameof(TongueUpRightMorph), Tongue_UpRight_Morph).ToArray();
            public CgeElementActuator[] TongueDownLeftMorph() => P01(nameof(TongueDownLeftMorph), Tongue_DownLeft_Morph).ToArray();
            public CgeElementActuator[] TongueDownRightMorph() => P01(nameof(TongueDownRightMorph), Tongue_DownRight_Morph).ToArray();
            public CgeElementActuator[] JawX() => P01(nameof(JawX), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpper() => P01(nameof(MouthUpper), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLower() => P01(nameof(MouthLower), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthX() => P01(nameof(MouthX), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileSadRight() => P01(nameof(SmileSadRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileSadLeft() => P01(nameof(SmileSadLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileSad() => P01(nameof(SmileSad), CgeElementTODO).ToArray();
            public CgeElementActuator[] TongueY() => P01(nameof(TongueY), CgeElementTODO).ToArray();
            public CgeElementActuator[] TongueX() => P01(nameof(TongueX), CgeElementTODO).ToArray();
            public CgeElementActuator[] TongueSteps() => P01(nameof(TongueSteps), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffSuckRight() => P01(nameof(PuffSuckRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffSuckLeft() => P01(nameof(PuffSuckLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffSuck() => P01(nameof(PuffSuck), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenApe() => P01(nameof(JawOpenApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenPuff() => P01(nameof(JawOpenPuff), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenPuffRight() => P01(nameof(JawOpenPuffRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenPuffLeft() => P01(nameof(JawOpenPuffLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenSuck() => P01(nameof(JawOpenSuck), CgeElementTODO).ToArray();
            public CgeElementActuator[] JawOpenForward() => P01(nameof(JawOpenForward), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpRightUpperInside() => P01(nameof(MouthUpperUpRightUpperInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpRightPuffRight() => P01(nameof(MouthUpperUpRightPuffRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpRightApe() => P01(nameof(MouthUpperUpRightApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpRightPout() => P01(nameof(MouthUpperUpRightPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpRightOverlay() => P01(nameof(MouthUpperUpRightOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpLeftUpperInside() => P01(nameof(MouthUpperUpLeftUpperInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpLeftPuffLeft() => P01(nameof(MouthUpperUpLeftPuffLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpLeftApe() => P01(nameof(MouthUpperUpLeftApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpLeftPout() => P01(nameof(MouthUpperUpLeftPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpLeftOverlay() => P01(nameof(MouthUpperUpLeftOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpUpperInside() => P01(nameof(MouthUpperUpUpperInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpInside() => P01(nameof(MouthUpperUpInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpPuff() => P01(nameof(MouthUpperUpPuff), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpPuffLeft() => P01(nameof(MouthUpperUpPuffLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpPuffRight() => P01(nameof(MouthUpperUpPuffRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpApe() => P01(nameof(MouthUpperUpApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpPout() => P01(nameof(MouthUpperUpPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthUpperUpOverlay() => P01(nameof(MouthUpperUpOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownRightLowerInside() => P01(nameof(MouthLowerDownRightLowerInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownRightPuffRight() => P01(nameof(MouthLowerDownRightPuffRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownRightApe() => P01(nameof(MouthLowerDownRightApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownRightPout() => P01(nameof(MouthLowerDownRightPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownRightOverlay() => P01(nameof(MouthLowerDownRightOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLeftLowerInside() => P01(nameof(MouthLowerDownLeftLowerInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLeftPuffLeft() => P01(nameof(MouthLowerDownLeftPuffLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLeftApe() => P01(nameof(MouthLowerDownLeftApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLeftPout() => P01(nameof(MouthLowerDownLeftPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLeftOverlay() => P01(nameof(MouthLowerDownLeftOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownLowerInside() => P01(nameof(MouthLowerDownLowerInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownInside() => P01(nameof(MouthLowerDownInside), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownPuff() => P01(nameof(MouthLowerDownPuff), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownPuffLeft() => P01(nameof(MouthLowerDownPuffLeft), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownPuffRight() => P01(nameof(MouthLowerDownPuffRight), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownApe() => P01(nameof(MouthLowerDownApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownPout() => P01(nameof(MouthLowerDownPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] MouthLowerDownOverlay() => P01(nameof(MouthLowerDownOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightUpperOverturn() => P01(nameof(SmileRightUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightLowerOverturn() => P01(nameof(SmileRightLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightOverturn() => P01(nameof(SmileRightOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightApe() => P01(nameof(SmileRightApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightOverlay() => P01(nameof(SmileRightOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileRightPout() => P01(nameof(SmileRightPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftUpperOverturn() => P01(nameof(SmileLeftUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftLowerOverturn() => P01(nameof(SmileLeftLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftOverturn() => P01(nameof(SmileLeftOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftApe() => P01(nameof(SmileLeftApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftOverlay() => P01(nameof(SmileLeftOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLeftPout() => P01(nameof(SmileLeftPout), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileUpperOverturn() => P01(nameof(SmileUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileLowerOverturn() => P01(nameof(SmileLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileApe() => P01(nameof(SmileApe), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmileOverlay() => P01(nameof(SmileOverlay), CgeElementTODO).ToArray();
            public CgeElementActuator[] SmilePout() => P01(nameof(SmilePout), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffRightUpperOverturn() => P01(nameof(PuffRightUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffRightLowerOverturn() => P01(nameof(PuffRightLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffRightOverturn() => P01(nameof(PuffRightOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffLeftUpperOverturn() => P01(nameof(PuffLeftUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffLeftLowerOverturn() => P01(nameof(PuffLeftLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffLeftOverturn() => P01(nameof(PuffLeftOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffUpperOverturn() => P01(nameof(PuffUpperOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffLowerOverturn() => P01(nameof(PuffLowerOverturn), CgeElementTODO).ToArray();
            public CgeElementActuator[] PuffOverturn() => P01(nameof(PuffOverturn), CgeElementTODO).ToArray();

            public Dictionary<string, CgeElementActuator[]> ToMap()
            {
                return GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(info => info.ReturnType == typeof(CgeElementActuator[]))
                    .ToDictionary(info => info.Name, info => (CgeElementActuator[])info.Invoke(this, Array.Empty<object>()));
            }

            private CgeInternalVRCFTContinuation P01(string parameter, CgeElement element) =>
                new CgeInternalVRCFTContinuation(parameter).Positive(element);
            private CgeInternalVRCFTContinuation Joystick(string parameter, CgeElement negativeLeftDown, CgeElement positiveUpRight) =>
                new CgeInternalVRCFTContinuation(parameter).Joystick(negativeLeftDown, positiveUpRight);
            private CgeInternalVRCFTContinuation Aperture(string parameter, CgeElement zero, CgeElement one) =>
                new CgeInternalVRCFTContinuation(parameter).Aperture(zero, one);
        }

        protected override Dictionary<string, CgeElementActuator[]> ExposeMap()
        {
            return new CgeVRCFaceTrackingFTVendorMap().ToMap();
        }
    }
}