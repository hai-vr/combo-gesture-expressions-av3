*ComboGestureExpressions* is an Unity Editor tool that lets you attach face expressions to hand gestures and take as much advantage of *Avatars 3.0*'s quality of life features.

# [> Download latest version...](https://github.com/hai-vr/combo-gesture-expressions-av3/releases)

<iframe src="https://streamable.com/e/t19nkm?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe>

(**Full introduction video to be done**)

A common issue with classic avatars are face expressions that conflict when both hands are combined.
For instance, if a face expression closes the eyes on the left hand, but lowers the eyelids on the other hand, the face will look wrong. 

*ComboGestureExpressions* takes advantage of Avatars 3.0 animators to fix this using predictable combo animations, but also introduces new features that will expand the expressions of your avatar:

- Using the expressions menu, attach multiple expressions on a single gesture by switching between entire sets of face expressions representing different moods.
- Eyes will no longer blink whenever the avatar has a face expression with eyes closed.
- Puppets and blend trees are integrated into the tool, with automatic correction of incorrect blending.
- ...and more tweaks.

This tool should NOT be used for:

- ❌ Animating hand and finger positions.
  To animate hand and finger positions, use the Avatars 3.0's Gesture layer which is made for this purpose.

# Create a new set of face expressions

<iframe src="https://streamable.com/e/iycnko?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#create-a-new-set-of-face-expressions-tutorial-with-audio-commentary) is available)*

Add the prefab to the scene located in `Assets/Hai/ComboGesture/ComboGestureExpressions.prefab`. Right-click on the newly inserted prefab and click <span class="hai-btn">Unpack prefab completely</span>.
Select the `Default` object which contains a *Combo Gesture Activity* component, then click the <span class="hai-btn">Open editor</span> button in the Inspector.

If your avatar is already set up in the scene and visible, click <span class="hai-btn">Automatically setup preview</span>.
A duplicate of the last avatar and a camera will be created in the scene in order to preview the face expressions.
The camera position can be adjusted if needed.

Afterwards, you will be able to click <span class="hai-btn">Generate preview</span> to preview animations.

*(All illustrations in this documentation use [Saneko avatar (さねこ) by ひゅうがなつ](https://booth.pm/en/items/2322146))*

# Combining hands

<iframe src="https://streamable.com/e/44azm7?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#combining-hands-tutorial-with-audio-commentary) is available)*

Go to the <span class="hai-btn">Set face expressions >Singles</span> tab, then drag-and-drop or select face expressions for the gestures in the first row. In the second row, do the same for x2 gestures when both hands are doing that gesture. If you want the animation for both hands to be the same, click <span class="hai-btn">Auto-set</span> button.

Remember to click <span class="hai-btn">Generate preview</span> to preview animations.

<div class="hai-interlude">
<iframe src="https://streamable.com/e/pzbd3w?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe>

<p>If by any chance the camera is not aligned, look at the hierarchy of the scene and double-click on the <code>CGEPreviewSetup</code> object. Enable the <code>CGEPreviewCamera</code> object and <code>CGEPreviewDummy</code> object and align the camera to the face of the dummy avatar.</p>
</div>

Then, go to the <span class="hai-btn">Set face expressions > Combos</span> tab. This is a table of all possible combinations of those gestures. Drag-and-drop or select face expressions for these slots. Alternatively, you can choose to try combining animations by clicking the <span class="hai-btn">+ Combine</span> button.

When combining, you will see a preview of the two animations mixed together. It is very common for the mixed animation to be conflicting, especially when two animations animate the eyes or the mouth in a different way.

Click the buttons on either side to turn some properties on and off, until you find a face expression that makes sense for that combination of gesture. When satisfied with the result, click <span class="hai-btn">Save and assign</span> in the middle. You can choose to rename the animation using the field above the button.

Gesture names for reference ([VRChat documentation](https://docs.vrchat.com/docs/animator-parameters#gestureleft-and-gestureright-values)):
  - *No gesture* / *None*: 🤙 (*Neutral* in VRChat docs)
  - Fist: ✊
  - Open: ✋ (*HandOpen* in VRChat docs)
  - Point: ☝️ (*FingerPoint* in VRChat docs)
  - Victory: ✌️
  - RockNRoll: 🤘 
  - Gun: 🎯👈 (*HandGun* in VRChat docs)
  - ThumbsUp: 👍 
  - *...on both hands* / *x2*: 🙌

The animation defined in *No gesture* will be used on all empty slots.
 
# Do not blink when eyes are closed

<iframe src="https://streamable.com/e/egz72f?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#do-not-blink-when-eyes-are-closed) is available)*

Go to <span class="hai-btn">Prevent eyes blinking</span> tab. By selecting which animations have both eyes closed, the blinking animation will be disabled as long as that face expression is active.

It is not recommended selecting animations with only one eye closed such as winking, as this will also cause the avatar to stop eye contact.

# Apply to the avatar

<iframe src="https://streamable.com/e/igwote?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#apply-to-the-avatar-tutorial-with-audio-commentary) is available)*

Select the `ComboGestureExpressions` object of the prefab which contains a *Combo Gesture Compiler* component. In the inspector, assign your [FX playable layer](https://docs.vrchat.com/docs/playable-layers#fx) animator to the `FX Animator Controller` slot. **This asset will be modified: New layers and parameters will be added when synchronizing animations.** I recommend you to **make backups** of that FX Animator Controller!

Drag and drop your avatar in the `Avatar descriptor` slot. The avatar will *not* be modified, this is only required to verify conflicts regarding lipsync and blink blendshape settings.

You should now be able to press <span class="hai-btn">Synchronize Animator FX layers</span>, which will modify your animator controller.

Whenever you modify any face expression animation or anything related to ComboGestureExpressions, press that button again to synchronize.

*If you haven't done it already, right-click on the newly prefab and click <span class="hai-btn">Unpack prefab completely</span>.* 

# Squeezing the trigger

<iframe src="https://streamable.com/e/7eimot?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#squeezing-the-trigger-tutorial-with-audio-commentary) is available)*

Fist animations are blended in when squeezing the trigger while the hand is doing a fist. The gesture of the other hand is used as the base animation.

For example, a `POINT + FIST` will use that animation when the trigger is squeezed, but when the trigger is not squeezed the animation defined in `POINT` will be used instead.

When both hands are doing a Fist gesture, you are able to define an animation for when the Left trigger is squeezed, another when the Right trigger is squeezed, and another for when both triggers are squeezed.

<div class="hai-interlude">
<iframe src="https://streamable.com/e/hp17ra?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe>

<p>Illustration of animation blending in an Analog Fist gesture.</p>
</div>

# Animate cat ears, wings and more

<iframe src="https://streamable.com/e/uo3kut?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#animate-cat-ears-wings-and-more-tutorial-with-audio-commentary) is available)*

In Avatars 3.0, animations that modify transforms belong in the [Gesture playable layer](https://docs.vrchat.com/docs/playable-layers#gesture). In face expression animations, this is most often used to animate ears, wings, tails...

**Skip this step** if you do not have such animations.

Note that finger poses and humanoid muscle poses will be ignored by this process. Animating finger poses is done by modifying the Gesture layers, which is outside the scope of this documentation.

If you do not have a gesture layer, duplicate one of the VRChat SDK examples and assign to the Gesture playable layer of your avatar:
- `Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer2` for feminine hand poses,
- `Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer` for masculine hand poses.

Select the `ComboGestureExpressions` object of the prefab. In the inspector, tick the `Gesture playable layer support` checkbox, and assign your [Gesture playable layer](https://docs.vrchat.com/docs/playable-layers#gesture) animator to the `Gesture Animator Controller` slot. **This asset will be modified: New layers and parameters will be added when synchronizing animations.** I recommend you to **make backups** of a that Gesture Animator Controller!

# Using multiple mood sets

<iframe src="https://streamable.com/e/c5x44o?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#using-multiple-mood-sets-tutorial-with-audio-commentary) is available)*

Earlier, you set up face expressions within `Default` object of the prefab. This is the default mood set of face expressions of your avatar that is active after loading. However, you can have any number of mood sets and switch between them using the menu to increase the number of face expressions depending on the situation.

The prefab contains another object called `Smiling` as an example, which contains a separate *Combo Gesture Activity* component. Select that object and rename it; It is up to you to organize the mood sets the way you want it (Smiling, Sad, Eccentric, Drunk, Romantic, ...) and it does not necessarily have to be moods (Sign Language, One-handed, Conversation, Dancing, ...)

Select the `ComboGestureExpressions` object of the prefab. In the inspector, set the `Parameter Name` to a parameter name of your choice.

In your Avatar Parameters, add that parameter name as an Int.

In your Expression Menu, create a Toggle control to switch that Parameter name to the value of 1.

Add additional mood sets by clicking <span class="hai-btn">+</span> on the list, then drag-and-drop or select another *ComboGestureActivity* component. The number on the right is the value you need to set in your Expressions Menu for that Parameter Name.

It is not necessary, but I also like to create a Toggle control to switch the Parameter name to the value of 0 in order to have an icon for it.

# Standalone puppets and blend trees

<iframe src="https://streamable.com/e/ai0fzb?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#standalone-puppets-and-blend-trees-tutorial-with-audio-commentary) is available)*

So far we have set up *Activity* mood sets. Another type of mood set is available: *Puppet*, which can be controlled by an Expression Menu.

The prefab contains an object called *Puppet* which contains a *Combo Gesture Puppet* component. Select it and click the <span class="hai-btn">Open editor</span> button in the Inspector. Since we already set up a preview earlier when setting up hand gestures, that preview will be reused when you click <span class="hai-btn">Automatically setup preview</span>.

Create a blend tree using the tool. Select one of the following basic templates: Four directions, Eight directions, Six directions pointing forward, Six directions pointing sideways.

In Joystick center animation, add an animation that will be used when the joystick of the puppet menu is resting at the center. Click <span class="hai-btn">Create a new blend tree asset</span> to select a location where to save that blend tree.

There are two additional options when generating the blend tree that should be left at their default values:
- Fix joystick snapping creates 4 additional animations for the resting pose near the center. This is because joystick of VR controllers have a dead zone in the middle. This means the animation will snap when exiting that dead zone.
- Joystick maximum tilt brings the outer animation points slightly closer to the middle. This is because joystick of VR controllers can not always be tilted all the way in every direction. This can also be used to avoid tilting the joystick all the way.

After generating the blend tree, edit it in the inspector to assign the face expressions in it. After it is done, select which face expressions have eyes closed by going to <span class="hai-btn">Prevent eyes blinking</span> tab.

Select the `ComboGestureExpressions` object of the prefab. In the inspector, add a mood set by clicking <span class="hai-btn">+</span> on the list. On the left in the dropdown menu, switch from *Activity* to *Puppet*, then drag-and-drop or select the `Puppet` object. Just like *Activity* mood sets, you can create more *Puppet* mood sets by creating additional *ComboGesturePuppet* components.

I recommend creating two controls in your Expression Menu to control the puppet: A Toggle control to switch to the Puppet mood set, and separate Two-Axis Puppet to control the two parameters of your blend tree.

<div class="hai-interlude">
<iframe src="https://streamable.com/e/8u2sd5?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe>

<p>Illustration of a puppet mood set.</p>
</div>

# Permutations

<iframe src="https://streamable.com/e/2onv8c?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#permutations-tutorial-with-audio-commentary) is available)*

For simplicity purposes, we've been using combinations of gestures, meaning that `Left POINT + Right THUMBSUP` will show the same animation as `Left THUMBSUP + Right POINT`. I encourage you [using multiple mood sets](#using-multiple-mood-sets) available in an Expressions menu to expand your expressions repertoire.

If you would like to create permutations of gestures, which I do recommend for asymmetric face expressions or hand-specific Fist animations, you may go to <span class="hai-btn">Set face expressions > Permutations</span> tab and click on <span class="hai-btn">Enable permutations for this Activity</span>. You will see a colored table split between Left hand permutations (colored in orange) and Right hand permutations (colored in blue).

When enabling permutations, the Activity will behave as if everything was still a combo: If you don't define a Left hand permutation, the Right hand permutation animation will be used for both.

# Mix puppets and gestures

<iframe src="https://streamable.com/e/nvm1n0?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#mix-puppets-and-gestures-tutorial-with-audio-commentary) is available)*

Any animation slot can have a blend tree within it instead. This means puppeteering is possible for specific combos of hand gestures.

Analog Fist gesture can be completely customized using it, and it is even possible to simultaneously combine the Fist analog trigger with a puppet menu if you feel like it. Remember puppets retain their values when closing the menu, so you don't necessarily need to have your puppet menu opened.

The blend tree template generator can be accessed in <span class="hai-btn">Additional editors > Create blend trees</span> tab. For puppet menus, use the [previously mentioned templates](#standalone-puppets-and-blend-trees). For Fist gestures, select one of the following templates: Single analog fist with hair trigger, Single analog fist and two directions, Dual analog fist.

When placing a blend tree in a single Fist gesture, the parameter `_AutoGestureWeight` will be how much the trigger is squeezed.

When placing a blend tree in the Fist x2 slot, the parameters `GestureLeftWeight` and `GestureRightWeight` will be how much the left and right triggers are squeezed respectively.

# Learn more

- [Corrections](corrections.md) - Learn about the various techniques used to fix animations.
- [Tutorials](tutorial.md) - Video tutorials with audio commentary.
- [Unavailable feature: Limited Lipsync](limited-lipsync.md) - An explanation of what *Make lipsync movement subtle* is.
- [Download on github.com](https://github.com/hai-vr/combo-gesture-expressions-av3/releases) - Main download location.
- [Download on booth.pm](https://hai-vr.booth.pm/items/2219616) - Alternate download location.

*All illustrations in this documentation use [Saneko avatar (さねこ) by ひゅうがなつ](https://booth.pm/en/items/2322146)*