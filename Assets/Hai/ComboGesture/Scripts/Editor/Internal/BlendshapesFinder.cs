using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class BlendshapesFinder
    {
        private readonly VRCAvatarDescriptor _avatar;

        public BlendshapesFinder(VRCAvatarDescriptor avatar)
        {
            _avatar = avatar;
        }

        public List<BlendShapeKey> FindBlink()
        {
            if (_avatar == null)
            {
                return new List<BlendShapeKey>();
            }

            var eyeLook = _avatar.customEyeLookSettings;
            if (eyeLook.eyelidsSkinnedMesh == null || eyeLook.eyelidsSkinnedMesh.sharedMesh == null)
            {
                return new List<BlendShapeKey>();
            }

            var relativePathToSkinnedMesh = SharedLayerUtils.ResolveRelativePath(_avatar.transform, eyeLook.eyelidsSkinnedMesh.transform);
            return eyeLook.eyelidsBlendshapes
                .Select(i => BlendShapeNameIfValid(i, eyeLook))
                .Where(blendShapeName => blendShapeName != null)
                .Select(blendShapeName => new BlendShapeKey(relativePathToSkinnedMesh, blendShapeName))
                .ToList();
        }

        public List<BlendShapeKey> FindLipsync()
        {
            if (_avatar == null)
            {
                return new List<BlendShapeKey>();
            }

            if (_avatar.lipSync != VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape
                || _avatar.VisemeSkinnedMesh == null
                || _avatar.VisemeSkinnedMesh.sharedMesh == null)
            {
                return new List<BlendShapeKey>();
            }

            var relativePathToSkinnedMesh = SharedLayerUtils.ResolveRelativePath(_avatar.transform, _avatar.VisemeSkinnedMesh.transform);
            return _avatar.VisemeBlendShapes
                .Where(blendShapeName => blendShapeName != null)
                .Select(blendShapeName => new BlendShapeKey(relativePathToSkinnedMesh, blendShapeName))
                .ToList();
        }

        private static string BlendShapeNameIfValid(int index, VRCAvatarDescriptor.CustomEyeLookSettings settings)
        {
            var count = settings.eyelidsSkinnedMesh.sharedMesh.blendShapeCount;
            return index >= 0 && index < count ? settings.eyelidsSkinnedMesh.sharedMesh.GetBlendShapeName(index) : null;
        }
    }

    public readonly struct BlendShapeKey
    {
        public BlendShapeKey(string path, string blendShapeName)
        {
            Path = path;
            BlendShapeName = blendShapeName;
        }

        public CurveKey AsCurveKey()
        {
            return new CurveKey(this.Path, typeof(SkinnedMeshRenderer), "blendShape." + this.BlendShapeName);
        }

        public string Path { get; }
        public string BlendShapeName { get; }

        public static bool operator ==(BlendShapeKey left, BlendShapeKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlendShapeKey left, BlendShapeKey right)
        {
            return !left.Equals(right);
        }

        public bool Equals(BlendShapeKey other)
        {
            return Path == other.Path && BlendShapeName == other.BlendShapeName;
        }

        public override bool Equals(object obj)
        {
            return obj is BlendShapeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (BlendShapeName != null ? BlendShapeName.GetHashCode() : 0);
            }
        }
    }
}
