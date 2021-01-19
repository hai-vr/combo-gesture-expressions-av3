using System;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Reused
{
    public static class Av3Parameterists
    {
        internal static BoolParameterist IsLocal = new BoolParameterist("IsLocal");
        internal static IntParameterist Viseme = new IntParameterist("Viseme");
        internal static IntParameterist GestureLeft = new IntParameterist("GestureLeft");
        internal static IntParameterist GestureRight = new IntParameterist("GestureRight");
        internal static FloatParameterist GestureLeftWeight = new FloatParameterist("GestureLeftWeight");
        internal static FloatParameterist GestureRightWeight = new FloatParameterist("GestureRightWeight");
        internal static FloatParameterist AngularY = new FloatParameterist("AngularY");
        internal static FloatParameterist VelocityX = new FloatParameterist("VelocityX");
        internal static FloatParameterist VelocityY = new FloatParameterist("VelocityY");
        internal static FloatParameterist VelocityZ = new FloatParameterist("VelocityZ");
        internal static FloatParameterist Upright = new FloatParameterist("Upright");
        internal static BoolParameterist Grounded = new BoolParameterist("Grounded");
        internal static BoolParameterist Seated = new BoolParameterist("Seated");
        internal static BoolParameterist Afk = new BoolParameterist("AFK");
        internal static IntParameterist TrackingType = new IntParameterist("TrackingType");
        internal static IntParameterist VrMode = new IntParameterist("VRMode");
        internal static BoolParameterist MuteSelf = new BoolParameterist("MuteSelf");
        internal static BoolParameterist InStation = new BoolParameterist("InStation");

        public static Action<Transitionist.TransitionContinuationist> ItIsRemote()
        {
            return continuationist => continuationist.And(IsLocal).IsFalse();
        }

        public static Action<Transitionist.TransitionContinuationist> ItIsLocal()
        {
            return continuationist => continuationist.And(IsLocal).IsTrue();
        }
    }

    internal abstract class Parameterist
    {
        internal string Name { get; }

        internal Parameterist(string name)
        {
            Name = name;
        }
    }

    internal class FloatParameterist : Parameterist
    {
        internal FloatParameterist(string name) : base(name)
        {
        }
    }

    internal class IntParameterist : Parameterist
    {
        internal IntParameterist(string name) : base(name)
        {
        }
    }

    internal class BoolParameterist : Parameterist
    {
        internal BoolParameterist(string name) : base(name)
        {
        }
    }
}
