using System.Collections.Generic;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureAkaneFacialOSCFTVendor : ComboGestureFTVendor
    {
        // AkaneFacialOSC Vendor
        // https://github.com/azwjp/AkaneFacialOSC/releases
        // https://azw.booth.pm/items/3686598
        // https://github.com/azwjp/AkaneFacialOSC/blob/v2.1.0/AkaneFacialOSC/Document/HowToUse_ja.md
        
        public CgeVendorGroup GROUP_目の周りのデータ__SDKの計算方法と同様に計算した値 = CgeVendorGroup.Some;
        public bool Eye_Left_Blink; //	0	自然な状態	左目を閉じる	左目のまばたき
        public bool Eye_Left_Wide; //	0	自然な状態	眉が上がって左目を大きく開ける	目を大きく見開いた状態．eye_wide の値．
        public bool Eye_Left_Right; //	0	自然な状態	右を見ようとして左目の右側に力を入れる	瞳孔の位置が中央より右にあるときに送信される
        public bool Eye_Left_Left; //	0	自然な状態	左を見ようとして左目の左側に力を入れる	瞳孔の位置が中央より左にあるときに送信される
        public bool Eye_Left_Up; //	0	自然な状態	上を見ようとしてうわまぶたが上がる	Eye_Left_Wide と全く同一の値が送信される
        public bool Eye_Left_Down; //	0	自然な状態	左眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる	目が中央より下に向いている時に送信される．vrc.lookingdown のイメージ
        public bool Eye_Right_Blink; //	0	自然な状態	右目を閉じる	右目のまばたき
        public bool Eye_Right_Wide; //	0	自然な状態	眉が上がって右目を大きく開ける	目を大きく見開いた状態．eye_wide の値．
        public bool Eye_Right_Right; //	0	自然な状態	右を見ようとして右目の右側に力を入れる	瞳孔の位置が中央より右にあるときに送信される
        public bool Eye_Right_Left; //	0	自然な状態	左を見ようとして右目の左側に力を入れる	瞳孔の位置が中央より左にあるときに送信される
        public bool Eye_Right_Up; //	0	自然な状態	上を見ようとして右目のうわまぶたが上がる	Eye_Left_Wide と全く同一の値が送信される
        public bool Eye_Right_Down; //	0	自然な状態	右眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる	目が中央より下に向いている時に送信される．vrc.lookingdown のイメージ
        public bool Eye_Left_Frown; //	0	自然な状態	左目をしかめて顔の中央に力を入れる	eye_frown の値
        public bool Eye_Right_Frown; //	0	自然な状態	右目をしかめて顔の中央に力を入れる	eye_frown の値
        public bool Eye_Left_Squeeze; //	0	自然な状態	左眉をしかめて下に押し下げる	eye_squeeze の値
        public bool Eye_Right_Squeeze; //	0	自然な状態	右眉をしかめて下に押し下げる	eye_squeeze の値
        
        //
        
        public CgeVendorGroup GROUP_視線__アプリ内で計算された値 = CgeVendorGroup.Some;
        public bool Gaze_Left_Vertical; //	0.5 (0)	左目が下を向く	左目が上を向く	範囲を [0, 1] と [-1, 1] の間で変更可能
        public bool Gaze_Left_Horizontal; //	0.5 (0)	左目が左を向く	左目が右を向く	範囲を [0, 1] と [-1, 1] の間で変更可能
        public bool Gaze_Right_Vertical; //	0.5 (0)	右目が下を向く	右目が上を向く	範囲を [0, 1] と [-1, 1] の間で変更可能
        public bool Gaze_Right_Horizontal; //	0.5 (0)	右目が左を向く	右目が右を向く	範囲を [0, 1] と [-1, 1] の間で変更可能
        public bool Gaze_Vertical; //	0.5 (0)	両目が下を向く	両目が上を向く	範囲を [0, 1] と [-1, 1] の間で変更可能．左右の目の平均
        public bool Gaze_Horizontal; //	0.5 (0)	両目が左を向く	両目が右を向く	範囲を [0, 1] と [-1, 1] の間で変更可能．左右の目の平均
        
        //
        
        public CgeVendorGroup GROUP_目__計算処理済みの値 = CgeVendorGroup.Some;
        public bool Eye_Blink; //	0	自然な状態	両目を閉じる	Eye_Left_Blink と Eye_Right_Blink の平均
        public bool Eye_Wide; //	0	自然な状態	眉が上がって両目を大きく開ける	Eye_Left_Wide と Eye_Right_Wide の平均
        public bool Eye_Right; //	0	自然な状態	右を見ようとして両目の右側に力を入れる	Eye_Left_Right と Eye_Right_Right の平均
        public bool Eye_Left; //	0	自然な状態	左を見ようとして両目の左側に力を入れる	Eye_Left_Left と Eye_Right_Left の平均
        public bool Eye_Up; //	0	自然な状態	上を見ようとしてうわまぶたが上がる	Eye_Left_Up と Eye_Right_Up の平均
        public bool Eye_Down; //	0	自然な状態	眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる	Eye_Left_Down と Eye_Right_Down の平均
        public bool Eye_Frown; //	0	自然な状態	両目をしかめて顔の中央に力が入る	Eye_Left_Frown と Eye_Right_Frown の平均．
        public bool Eye_Squeeze; //	0	自然な状態	両眉をしかめて下に押し下げる	Eye_Left_Squeeze と Eye_Right_Squeeze の平均．
        
        //
        
        public CgeVendorGroup GROUP_顔__トラッカで取得した生の値 = CgeVendorGroup.Some;
        public bool Jaw_Right; //	0	自然な状態	顎を右に動かす	Jaw_Left と同時に 1 に設定した場合は元の状態に戻る
        public bool Jaw_Left; //	0	自然な状態	顎を左に動かす	Jaw_Right と同時に 1 に設定した場合は元の状態に戻る
        public bool Jaw_Forward; //	0	自然な状態	顎を前に突き出す	
        public bool Jaw_Open; //	0	自然な状態	顎を開く	
        public bool Mouth_Ape_Shape; //	0	自然な状態	口を閉じたまま顎を開く	
        public bool Mouth_Upper_Right; //	0	自然な状態	口を閉じたまま上唇を右に動かす	Mouth_Upper_Left と同時に 1 に設定した場合は元の状態に戻る
        public bool Mouth_Upper_Left; //	0	自然な状態	口を閉じたまま上唇を左に動かす	Mouth_Upper_Right と同時に 1 に設定した場合は元の状態に戻る
        public bool Mouth_Lower_Right; //	0	自然な状態	口を閉じたまま下唇を右に動かす	Mouth_Lower_Left と同時に 1 に設定した場合は元の状態に戻る
        public bool Mouth_Lower_Left; //	0	自然な状態	口を閉じたまま下唇を左に動かす	Mouth_Lower_Right と同時に 1 に設定した場合は元の状態に戻る
        public bool Mouth_Upper_Overturn; //	0	自然な状態	口を閉じたまま上唇を突き出す	
        public bool Mouth_Lower_Overturn; //	0	自然な状態	口を閉じたまま下唇を突き出す	
        public bool Mouth_Pout; //	0	自然な状態	口を閉じたまま口をすぼめる	vrc.v_ou 用のシェイプでも代用できなくはないが，口の開き方が意図しない形になる可能性あり
        public bool Mouth_Smile_Right; //	0	自然な状態	口を閉じたまま右側の口角を上げる	
        public bool Mouth_Smile_Left; //	0	自然な状態	口を閉じたまま左側の口角を上げる	
        public bool Mouth_Sad_Right; //	0	自然な状態	口を閉じたまま右側の口角を下げる	
        public bool Mouth_Sad_Left; //	0	自然な状態	口を閉じたまま左側の口角を下げる	
        public bool Cheek_Puff_Right; //	0	自然な状態	右側の頬を膨らます	
        public bool Cheek_Puff_Left; //	0	自然な状態	左側の頬を膨らます	
        public bool Cheek_Suck; //	0	自然な状態	両側の頬をすぼます	
        public bool Mouth_Upper_UpRight; //	0	自然な状態	口を閉じたまま右側の上唇を上げて歯が見える	
        public bool Mouth_Upper_UpLeft; //	0	自然な状態	口を閉じたまま左側の上唇を上げて歯が見える	
        public bool Mouth_Lower_DownRight; //	0	自然な状態	口を閉じたまま右側の下唇を下げて歯が見える	
        public bool Mouth_Lower_DownLeft; //	0	自然な状態	口を閉じたまま左側の下唇を下げて歯が見える	
        public bool Mouth_Upper_Inside; //	0	自然な状態	上唇を前歯に挟み込むように丸める	
        public bool Mouth_Lower_Inside; //	0	自然な状態	下唇を前歯に挟み込むように丸める	
        public bool Mouth_Lower_Overlay; //	0	自然な状態	下唇が上唇の前に被さる	上唇が前にくる信号はない
        public bool Tongue_LongStep1; //	0	自然な状態	舌を前に出す	th の発音程度．口を閉じた状態では舌が外に出ない
        public bool Tongue_LongStep2; //	0	自然な状態	舌を大きく前に出す	舌を前に大きく突き出した状態
        public bool Tongue_Down; //	0	自然な状態	舌の先端を下に動かす	Tongue_LongStep2 と組み合わせたとき，あかんべーをしている状態
        public bool Tongue_Up; //	0	自然な状態	舌の先端を上に動かす	R の発音の舌
        public bool Tongue_Right; //	0	自然な状態	舌を右に動かす	
        public bool Tongue_Left; //	0	自然な状態	舌を左に動かす	
        public bool Tongue_Roll; //	0	自然な状態	舌をストローのように筒状に丸める	
        public bool Tongue_UpLeft_Morph; //	0	自然な状態	舌の先端を左上に曲げる	
        public bool Tongue_UpRight_Morph; //	0	自然な状態	舌の先端を右上に曲げる	
        public bool Tongue_DownLeft_Morph; //	0	自然な状態	舌の先端を左下に曲げる	
        public bool Tongue_DownRight_Morph; //	0	自然な状態	舌の先端を右下に曲げる	
        
        //
        
        public CgeVendorGroup GROUP_顔__アプリ内で計算_統合したデータ = CgeVendorGroup.Some;
        public bool Jaw_Left_Right; //	0.5 (0)	顎を左に動かす	顎を右に動かす	Jaw_Left と Jaw_Right から計算
        public bool Mouth_Sad_Smile_Right; //	0.5 (0)	口を閉じたまま右側の口角を上げる	口を閉じたまま右側の口角を上げる	Mouth_Sad_Right と Mouth_Smile_Right から計算
        public bool Mouth_Sad_Smile_Left; //	0.5 (0)	口を閉じたまま左側の口角を上げる	口を閉じたまま左側の口角を上げる	Mouth_Sad_Left と Mouth_Smile_Left から計算
        public bool Mouth_Smile; //	0	自然な状態	口を閉じたまま両方の口角を上げる	Mouth_Smile_Left と Mouth_Smile_Right の平均
        public bool Mouth_Sad; //	0	自然な状態	口を閉じたまま両方の口角を下げる	Mouth_Sad_Left と Mouth_Sad_Right の平均
        public bool Mouth_Sad_Smile; //	0.5 (0)	両方の口角を下げる	両方の口角を上げる	Mouth_Sad と Mouth_Smile から計算
        public bool Mouth_Upper_Left_Right; //	0.5 (0)	口を閉じたまま上唇を左に動かす	口を閉じたまま上唇を右に動かす	Mouth_Upper_Left と Mouth_Upper_Right から計算
        public bool Mouth_Lower_Left_Right; //	0.5 (0)	口を閉じたまま下唇を左に動かす	口を閉じたまま下唇を右に動かす	Mouth_Lower_Left と Mouth_Lower_Right から計算
        public bool Mouth_Left_Right; //	0.5 (0)	口を閉じたまま上下の唇を左に動かす	口を閉じたまま上下の唇を右に動かす	Mouth_Upper_Left_Right と Mouth_Lower_Left_Right の平均
        public bool Mouth_Upper_Inside_Overturn; //	0.5 (0)	上唇を前歯に挟み込むように丸める	口を閉じたまま上唇を突き出す	Mouth_Upper_Inside と Mouth_Upper_Overturn から計算
        public bool Mouth_Lower_Inside_Overturn; //	0.5 (0)	下唇を前歯に挟み込むように丸める	口を閉じたまま下唇を突き出す	Mouth_Lower_Inside と Mouth_Lower_Overturn から計算
        public bool Cheek_Puff; //	0	自然な状態	両方の頬を膨らます	Cheek_Puff_Left と Cheek_Puff_Right の平均
        public bool Cheek_Suck_Puff; //	0.5 (0)	両側の頬をすぼます	両方の頬を膨らます	Cheek_Suck と Cheek_Puff から計算
        public bool Mouth_Upper_Up; //	0	自然な状態	上唇を上げて歯が見える	Mouth_Upper_UpLeft と Mouth_Upper_UpRight の平均
        public bool Mouth_Lower_Down; //	0	自然な状態	下唇を下げて歯が見える	Mouth_Lower_DownLeft と Mouth_Lower_DownRight の平均
        public bool Tongue_Left_Right; //	0.5 (0)	舌を左に動かす	舌を右に動かす	Tongue_Left　と Tongue_Right から計算
        public bool Tongue_Down_Up; //	0.5 (0)	舌の先端を下に動かす	舌の先端を上に動かす	Tongue_Down　と Tongue_Up から計算
        
        protected override Dictionary<string, CgeElementActuator[]> ExposeMap()
        {
            throw new System.NotImplementedException();
        }
    }
}