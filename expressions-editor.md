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

## Based blendshapes

Some blendshapes will have a preview that is either completely gray, or barely changes anything. These blendshapes are likely changing meshes that are not currently visible. For instance, it could be teeth variations such as fangs, tongue variations, tears positions, etc.

In order to generate more relevant previews, you need to make this blenshape generate a preview **based** on what another blendshape is animating. To do this:

- In *Property Explorer*, open Other tools.
- Click *Fix Tooth and oher hidden blendshapes*.
- Click *Select* on all the blendshapes of the same category that have incorrect previews.
- When done selecting, click *Assign* on the blendshape that you want be **based** on. For instance, to fix teeth blendshapes, click *Assign* on a blendshape that has an open mouth with teeth showing.
- The selected blendshapes now have the word **Based** on them, indicating that the preview only reflects what that blendshape is doing based on something else.
