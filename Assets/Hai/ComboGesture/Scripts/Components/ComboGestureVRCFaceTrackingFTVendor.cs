namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureVRCFaceTrackingFTVendor : ComboGestureFTVendor
    {
        // VRCFaceTracking Vendor
        // https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters

        public CgeVendorGroup GROUP_Eye_Tracking_Parameters = CgeVendorGroup.All;
        public bool EyesX = true; //	Gaze Direction X	Combined
        public bool EyesY = true; //	Gaze Direction Y	Combined
        public bool LeftEyeLid = true; //	Eyelid Open	Left
        public bool RightEyeLid = true; //	Eyelid Open	Right
        public bool CombinedEyeLid = true; //	Eyelid Open	Combined
        public bool EyesWiden = true; //	Eye Widen	Combined
        public bool EyesDilation = true; //	Pupil Dilation (0 is Constricted, 1 is Dilated)	Combined
        public bool EyesSqueeze = true; //	Eyelid Squeeze	Combined
        public bool LeftEyeX = true; //	Gaze Direction X	Left
        public bool LeftEyeY = true; //	Gaze Direction Y	Left
        public bool RightEyeX = true; //	Gaze Direction X	Right
        public bool RightEyeY = true; //	Gaze Direction Y	Right
        public bool LeftEyeWiden = true; //	Eye Widen	Left
        public bool RightEyeWiden = true; //	Eye Widen	Right
        public bool LeftEyeSqueeze = true; //	Eyelid Squeeze	Left
        public bool RightEyeSqueeze = true; //	Eyelid Squeeze	Right
        public bool LeftEyeLidExpanded = true; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Left
        public bool RightEyeLidExpanded = true; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Right
        public bool CombinedEyeLidExpanded = true; //	0.0 to 0.8 Eyelid Squeeze, Eyelid Open, 0.8 to 1.0 Eye Widen	Combined
        public bool LeftEyeLidExpandedSqueeze = true; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Left
        public bool RightEyeLidExpandedSqueeze = true; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Right
        public bool CombinedEyeLidExpandedSqueeze = true; //	-1 to 0 Eyelid Squeeze, 0.0 to 0.8 Eyelid Open, 0.8 to 1.0 Eye Widen	Combined

        public CgeVendorGroup GROUP_Lip_Tracking_Parameters = CgeVendorGroup.All;
        public bool JawRight = true; //	Jaw translation right	0.0 - 1.0
        public bool JawLeft = true; //	Jaw translation left	0.0 - 1.0
        public bool JawForward = true; //	Jaw translation jutting out	0.0 - 1.0
        public bool JawOpen = true; //	Jaw open	0.0 - 1.0
        public bool MouthApeShape = true; //	Jaw open, lips sealed closed	0.0 - 1.0
        public bool MouthUpperRight = true; //	Upper lip translate right, and not showing teeth	0.0 - 1.0
        public bool MouthUpperLeft = true; //	Upper lip translate left, and not showing teeth	0.0 - 1.0
        public bool MouthLowerRight = true; //	Lower lip translate right	0.0 - 1.0
        public bool MouthLowerLeft = true; //	Lower lip translate left	0.0 - 1.0
        public bool MouthUpperOverturn = true; //	Pushing top lip out	0.0 - 1.0
        public bool MouthLowerOverturn = true; //	Pouting out lower lip	0.0 - 1.0
        public bool MouthPout = true; //	Both lips pouting forward	0.0 - 1.0
        public bool MouthSmileRight = true; //	Smile right1	0.0 - 1.0
        public bool MouthSmileLeft = true; //	Smile left1	0.0 - 1.0
        public bool MouthSadRight = true; //	Sad Right1	0.0 - 1.0
        public bool MouthSadLeft = true; //	Sad Left1	0.0 - 1.0
        public bool CheekPuffRight = true; //	Cheek puffed out, right	0.0 - 1.0
        public bool CheekPuffLeft = true; //	Cheek puffed out, left	0.0 - 1.0
        public bool CheekSuck = true; //	Both cheeks sucked in	0.0 - 1.0
        public bool MouthUpperUpRight = true; //	Upper right lip drawn up to show teeth	0.0 - 1.0
        public bool MouthUpperUpLeft = true; //	Upper left lip drawn up to show teeth	0.0 - 1.0
        public bool MouthLowerDownRight = true; //	Bottom right lip drawn down to show teeth	0.0 - 1.0
        public bool MouthLowerDownLeft = true; //	Bottom left lip drawn down to show teeth	0.0 - 1.0
        public bool MouthUpperInside = true; //	Upper lip bitten by lower teeth	0.0 - 1.0
        public bool MouthLowerInside = true; //	Bottom lip bitten by upper teeth	0.0 - 1.0
        public bool MouthLowerOverlay = true; //	Upper lip out and over lower	0.0 - 1.0
        public bool TongueLongStep1 = true; //	Seems to be an intermediate out	0.0 - 1.0
        public bool TongueLongStep2 = true; //	Seems to be an intermediate out	0.0 - 1.0
        public bool TongueDown = true; //	Tongue tip angled down	0.0 - 1.0
        public bool TongueUp = true; //	Tongue tip angled up	0.0 - 1.0
        public bool TongueRight = true; //	Tongue tip angled right	0.0 - 1.0
        public bool TongueLeft = true; //	Tongue tip angled left	0.0 - 1.0
        public bool TongueRoll = true; //	Both sides of tongue brought up into "v"	0.0 - 1.0
        public bool TongueUpLeftMorph = true; //	Seems to deform upper left of tongue out of mouth	0.0 - 1.0
        public bool TongueUpRightMorph = true; //	Seems to deform upper right of tongue out of mouth	0.0 - 1.0
        public bool TongueDownLeftMorph = true; //	Seems to deform lower left of tongue out of mouth	0.0 - 1.0
        public bool TongueDownRightMorph = true; //	Seems to deform lower right of tongue out of mouth	0.0 - 1.0
        
        public CgeVendorGroup GROUP_General_Combined_Lip_Parameters = CgeVendorGroup.All;
        public bool JawX = true; //	Jaw translation fully left to fully right	-1.0 - 1.0
        public bool MouthUpper = true; //	MouthUpperLeft to MouthUpperRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLower = true; //	MouthLowerLeft to MouthLowerRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthX = true; //	MouthLeft (Upper/Lower) to MouthRight (Upper/Lower), with 0 being neutral	-1.0 - 1.0
        public bool SmileSadRight = true; //	MouthSadRight to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileSadLeft = true; //	MouthSadLeft to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileSad = true; //	MouthSad (Left/Right) to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool TongueY = true; //	TongueDown to TongueUp, with 0 being neutral	-1.0 - 1.0
        public bool TongueX = true; //	TongueLeft to TongueRight, with 0 being neutral	-1.0 - 1.0
        public bool TongueSteps = true; //	TongueLongStep1 to TongueLongStep2, with -1 being tongue fully in to 1 being fully out	-1.0 - 1.0
        public bool PuffSuckRight = true; //	CheekSuck to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffSuckLeft = true; //	CheekSuck to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffSuck = true; //	CheekSuck to CheekPuff (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Jaw_Open_Combined_Lip_Parameters = CgeVendorGroup.All;
        public bool JawOpenApe = true; //	MouthApeShape to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuff = true; //	CheekPuff (Left/Right) to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuffRight = true; //	CheekPuffRight to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenPuffLeft = true; //	CheekPuffLeft to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenSuck = true; //	CheekSuck to JawOpen, with 0 being neutral	-1.0 - 1.0
        public bool JawOpenForward = true; //	JawForward to JawOpen, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Right_Combined_Lip_Parameters = CgeVendorGroup.All;
        public bool MouthUpperUpRightUpperInside = true; //	MouthUpperInside to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightPuffRight = true; //	CheekPuffRight to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightApe = true; //	MouthApeShape to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightPout = true; //	MouthPout to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpRightOverlay = true; //	MouthLowerOverlay Shape to MouthUpperUpRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Left_Combined_Parameters = CgeVendorGroup.All;
        public bool MouthUpperUpLeftUpperInside = true; //	MouthUpperInside to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftPuffLeft = true; //	CheekPuffLeft to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftApe = true; //	MouthApeShape to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftPout = true; //	MouthPout to MouthUpperUpLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpLeftOverlay = true; //	MouthLowerOverlay Shape to MouthUpperUpLeft (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Upper_Up_Combined_Parameters = CgeVendorGroup.All;
        public bool MouthUpperUpUpperInside = true; //	MouthUpperInside to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpInside = true; //	MouthInside (Upper/Lower) to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuff = true; //	CheekPuff (Left/Right) to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuffLeft = true; //	CheekPuffLeft to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPuffRight = true; //	CheekPuffRight to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpApe = true; //	MouthApeShape to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpPout = true; //	MouthPout to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthUpperUpOverlay = true; //	MouthLowerOverlay Shape to MouthUpperUp (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Right_Combined_Parameters = CgeVendorGroup.All;
        public bool MouthLowerDownRightLowerInside = true; //	MouthLowerInside to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightPuffRight = true; //	CheekPuffRight to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightApe = true; //	MouthApeShape to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightPout = true; //	MouthPout to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownRightOverlay = true; //	MouthLowerOverlay Shape to MouthLowerDownRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Left_Combined_Parameters = CgeVendorGroup.All;
        public bool MouthLowerDownLeftLowerInside = true; //	MouthLowerInside to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftPuffLeft = true; //	CheekPuffLeft to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftApe = true; //	MouthApeShape to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftPout = true; //	MouthPout to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownLeftOverlay = true; //	MouthLowerOverlay Shape to MouthLowerDownLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Mouth_Lower_Down_Combined_Parameters = CgeVendorGroup.All;
        public bool MouthLowerDownLowerInside = true; //	MouthLowerInside to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownInside = true; //	MouthInside (Upper/Lower) to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuff = true; //	CheekPuff (Left/Right) to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuffLeft = true; //	CheekPuffLeft to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPuffRight = true; //	CheekPuffRight to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownApe = true; //	MouthApeShape to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownPout = true; //	MouthPout to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool MouthLowerDownOverlay = true; //	MouthLowerOverlay Shape to MouthLowerDown (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Right_Combined_Parameters = CgeVendorGroup.All;
        public bool SmileRightUpperOverturn = true; //	MouthUpperOverturn to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightLowerOverturn = true; //	MouthLowerOverturn to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightOverturn = true; //	MouthOverturn (Upper/Lower) to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightApe = true; //	MouthApeShape to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightOverlay = true; //	MouthLowerOverlay to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        public bool SmileRightPout = true; //	MouthPout to MouthSmileRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Left_Combined_Parameters = CgeVendorGroup.All;
        public bool SmileLeftUpperOverturn = true; //	MouthUpperOverturn to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftLowerOverturn = true; //	MouthLowerOverturn to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftOverturn = true; //	MouthOverturn (Upper/Lower) to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftApe = true; //	MouthApeShape to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftOverlay = true; //	MouthLowerOverlay to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        public bool SmileLeftPout = true; //	MouthPout to MouthSmileLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Smile_Combined_Parameters = CgeVendorGroup.All;
        public bool SmileUpperOverturn = true; //	MouthUpperOverturn to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileLowerOverturn = true; //	MouthLowerOverturn to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileApe = true; //	MouthApeShape to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmileOverlay = true; //	MouthLowerOverlay to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        public bool SmilePout = true; //	MouthLowerPout to MouthSmile (Left/Right), with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Right_Combined_Parameters = CgeVendorGroup.All;
        public bool PuffRightUpperOverturn = true; //	MouthUpperOverturn to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffRightLowerOverturn = true; //	MouthLowerOverturn to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        public bool PuffRightOverturn = true; //	MouthOverturn (Upper/Lower) to CheekPuffRight, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Left_Combined_Parameters = CgeVendorGroup.All;
        public bool PuffLeftUpperOverturn = true; //	MouthUpperOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLeftLowerOverturn = true; //	MouthLowerOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLeftOverturn = true; //	MouthOverturn (Upper/Lower) to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        
        public CgeVendorGroup GROUP_Cheek_Puff_Combined_Parameters = CgeVendorGroup.All;
        public bool PuffUpperOverturn = true; //	MouthUpperOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffLowerOverturn = true; //	MouthLowerOverturn to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
        public bool PuffOverturn = true; //	MouthOverturn (Upper/Lower) to CheekPuffLeft, with 0 being neutral	-1.0 - 1.0
    }
}