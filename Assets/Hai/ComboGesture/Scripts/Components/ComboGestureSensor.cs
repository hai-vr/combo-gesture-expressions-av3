using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureSensor : MonoBehaviour
    {
        public CgeSensorCone mouth;
        public CgeSensorCone jaw;
        public CgeSensorCone puff;
        public CgeSensorLipElement lipElement;
        public CgeSensorEyeElement eyeElement;
    }

    public struct CgeSensorCone
    {
        public Motion idle;
        public Motion center;
        public Motion up;
        public Motion down;
        public Motion left;
        public Motion right;
    }
    
    public struct CgeSensorLipElement
    {
        public SkinnedMeshRenderer[] renderersByConvention;
        
        public Motion None; // GearBell 0:0 RESTING_FACE_EXAMPLE
        //public Motion None; // GearBell 8:0 TONGUE_REST_COMPARISON_EXAMPLE
        
        public Motion Jaw_Right; // GearBell 1:2 JAW_RIGHT
        public Motion Jaw_Left; // GearBell 1:3 JAW_LEFT
        public Motion Jaw_Forward; // GearBell 1:4 JAW_FORWARD
        public Motion Jaw_Open;// GearBell 1:1 JAW_OPEN
        public Motion Mouth_Ape_Shape; // GearBell 1:0 JAW_DOWN ???
        public Motion Mouth_Upper_Right; // GearBell 3:0 UPPER_LIP_RIGHT
        public Motion Mouth_Upper_Left; // GearBell 3:1 UPPER_LIP_LEFT
        public Motion Mouth_Lower_Right; // GearBell 3:3 LOWER_LIP_RIGHT
        public Motion Mouth_Lower_Left; // GearBell 3:2 LOWER_LIP_LEFT
        public Motion Mouth_Upper_Overturn; // GearBell 5:3 UPPER_LIP_OVERTURN
        public Motion Mouth_Lower_Overturn; // GearBell 5:2 LOWER_LIP_OVERTURN
        public Motion Mouth_Pout; // GearBell 0:1 POUT_PUCKER
        public Motion Mouth_Smile_Right; // GearBell 2:3 RIGHT_SMILE
        public Motion Mouth_Smile_Left; // GearBell 2:2 LEFT_SMILE
        public Motion Mouth_Sad_Right; // GearBell 2:1 RIGHT_SAD
        public Motion Mouth_Sad_Left; // GearBell 2:0 LEFT_SAD
        public Motion Cheek_Puff_Right; // GearBell 6:0 CHEEK_PUFF_RIGHT
        public Motion Cheek_Puff_Left; // GearBell 6:1 CHEEK_PUFF_LEFT
        public Motion Cheek_Suck; // GearBell 6:2 CHEEKS_SUCK
        public Motion Mouth_Upper_UpRight; // GearBell 3:0 UPPER_LIP_UP_RIGHT
        public Motion Mouth_Upper_UpLeft; // GearBell 3:1 UPPER_LIP_UP_LEFT
        public Motion Mouth_Lower_DownRight;// GearBell 3:3 UPPER_LIP_DOWN_RIGHT
        public Motion Mouth_Lower_DownLeft; // GearBell 3:2 UPPER_LIP_DOWN_LEFT
        public Motion Mouth_Upper_Inside; // GearBell 5:0 UPPER_LIP_INSIDE
        public Motion Mouth_Lower_Inside; // GearBell 5:1 LOWER_LIP_INSIDE
        public Motion Mouth_Lower_Overlay; // GearBell 5:4 LOWER_LIP_OVERLAY
        public Motion Tongue_LongStep1; // GearBell 8:1 TONGUE_LONG_FORWARD (???)
        public Motion Tongue_LongStep2; // GearBell 8:2 TONGUE_FORWARD (???)
        public Motion Tongue_Down; // GearBell 8:4 TONGUE_DOWN
        public Motion Tongue_Up; // GearBell 8:3 TONGUE_UP
        public Motion Tongue_Right; // GearBell 9:1 TONGUE_RIGHT
        public Motion Tongue_Left; // GearBell 9:0 TONGUE_LEFT
        public Motion Tongue_Roll; // GearBell 9:2 TONGUE_ROLL
        public Motion Tongue_UpLeft_Morph; // GearBell 10:0 TONGUE_UP_LEFT_MORPH
        public Motion Tongue_UpRight_Morph; // GearBell 10:1 TONGUE_UP_RIGHT_MORPH
        public Motion Tongue_DownLeft_Morph; // GearBell 10:2 TONGUE_DOWN_LEFT_MORPH
        public Motion Tongue_DownRight_Morph; // GearBell 10:3 TONGUE_DOWN_RIGHT_MORPH
        // public Motion Max; // enum marker
    }

    public struct CgeSensorEyeElement
    {
        public SkinnedMeshRenderer[] renderersByConvention;
        
        public Motion None;
        public Motion Eye_Left_Blink;
        public Motion Eye_Left_Wide;
        public Motion Eye_Left_Right;
        public Motion Eye_Left_Left;
        public Motion Eye_Left_Up;
        public Motion Eye_Left_Down;
        public Motion Eye_Right_Blink;
        public Motion Eye_Right_Wide;
        public Motion Eye_Right_Right;
        public Motion Eye_Right_Left;
        public Motion Eye_Right_Up;
        public Motion Eye_Right_Down;
        public Motion Eye_Frown;
        public Motion Eye_Left_Squeeze;
        public Motion Eye_Right_Squeeze;
        // public Motion Max; // enum marker
    }
}