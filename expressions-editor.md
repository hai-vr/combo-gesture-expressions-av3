*ExpressionsEditor* does not require any SDK of any platform to be installed. In particular, VRChat is not required for the standalone version of *ExpressionsEditor* to run; it can be used along with other applications, social platforms, or your own games and applications.

## Generate a Preview dummy

A preview dummy is a duplicate copy of your avatar that EE will use to generate previews of your animations.

Making a duplicate copy ensures that EE does not modify the original avatar by accident. In addition you are free to modify the preview dummy to make previews look nice for you to work with.

For best results, it is recommended that you:

- Make sure *Legacy Blendshape Normals* options is check in the import settings of your avatar mesh (on the VRChat platform, the upload process will force you to check this box).
- If you use glitter or any sort of animated skin, remove them by creating a new material that does not have those.
- Disable or remove particle systems.

## Reset blendshapes

Depending on the platform you're working on, the animations may need to reset the unused blendshape values to the default values in order to prevent conflicts between animations. They are referred to as *Reset blendshapes* in EE.

Reset blendshapes are not shown in the EE even if they are present in the animation.

If you want to edit those anyways, you need to add them through the *Property Explorer* window.

Note: If you use *ComboGestureExpressions*, your animations do not need *Reset blendshapes*, as they are automatically generated.
