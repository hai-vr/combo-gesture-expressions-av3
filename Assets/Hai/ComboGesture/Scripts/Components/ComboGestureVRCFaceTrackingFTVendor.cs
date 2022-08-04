using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureVRCFaceTrackingFTVendor : MonoBehaviour
    {
        // VRCFaceTracking Vendor
        // https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters

        public CgeVendorGroup GROUP_NativeEye = CgeVendorGroup.All;
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

        public CgeVendorGroup GROUP_NativeLip = CgeVendorGroup.All;
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
    }
}