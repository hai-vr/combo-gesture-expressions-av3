*ComboGestureExpressions* is an Unity Editor tool that lets you attach face expressions to hand gestures, and make it react to other *Avatars 3.0*'s features, including *Contacts*, *PhysBones* and *OSC*.

It is bundled with *Visual Expressions Editor*, an animation editor that lets you create face expressions with the help of previews.

# [> Download latest version...](https://github.com/hai-vr/combo-gesture-expressions-av3/releases)

- *([Download on github.com](https://github.com/hai-vr/combo-gesture-expressions-av3/releases))*
- *([Download on booth.pm](https://hai-vr.booth.pm/items/2219616))*

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/cge2-rc-github.mp4" type="video/mp4">
</video>
<iframe src="https://streamable.com/e/t19nkm?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe> <iframe src="https://streamable.com/e/bg1uoj?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe>

With *ComboGestureExpressions*:

- PhysBones, Contacts and OSC can be used to blend in face expressions.
- The pressure on your controller triggers can be used to animate your face.
- Attach multiple expressions on a single gesture by switching between different moods using the Expressions Menu.
- Eyes will no longer blink whenever the avatar has a face expression with eyes closed.
- Puppets and blend trees are integrated into the tool.
- Animations triggered by squeezing the controller trigger will look smooth to outside observers (see [corrections](corrections.md)).

*(Most illustrations in this documentation use [Saneko avatar (さねこ) by ひゅうがなつ](https://booth.pm/en/items/2322146))*

<img src="EeCgeLogo3.png" style="width: 200px; height: 200px" />

# Installation for V1 users

The components used in ComboGestureExpressions V1 and V2 are the same. You will not lose your work.

To install ComboGestureExpressions V2, **ComboGestureExpressions V1 must be removed** by hand first if it is installed.
- Delete the `Assets/Hai/ComboGesture` folder.
- Delete the `Assets/Hai/ExpressionsEditor` folder.

You can now install *ComboGestureExpressions*.

Remove the preview dummy in the scene, they are no longer needed.

# What's new in V2?

Find out [what's new in V2](v2.0-whats-new.md).

# Having issues? Join my Discord Server

I will try to provide help on the #cge channel when I can.

[Join the Invitation Discord (https://discord.gg/58fWAUTYF8)](https://discord.gg/58fWAUTYF8).

# Set up the prefab and open the CGE Editor

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_15-59-03_LrodZ6DkxP.mp4" type="video/mp4">
</video>

Add the prefab to the scene located in `Assets/Hai/ComboGesture/ComboGestureExpressions.prefab`. Right-click on the newly inserted prefab and click <span class="hai-btn">Unpack prefab completely</span>.
Select the `Default` object which contains a *Combo Gesture Activity* component, then click the <span class="hai-btn">Open editor</span> button in the Inspector to open the CGE Editor.

# Visualize your animation files and preview using AnimationViewer
![NewInV2TagSmall](https://user-images.githubusercontent.com/60819407/167145432-89428be9-9f16-4795-98ce-093a0c96837c.png)
<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_16-15-03_FyjXwkdmJA.mp4" type="video/mp4">
</video>

Select your avatar which contains your Animator.

In the Animator component, click the vertical dots on the right `⋮` and click *Haï AnimationViewer*. You can now see previews of your animations inside your Project view. Move your scene camera to better see your avatar.

This can cost a little bit of performance, so click the *Activate Viewer* button to toggle it on and off.

For more information, see [AnimationViewer manual](https://hai-vr.notion.site/Animation-Viewer-2a4bc319631c44d383174bd140722e38)

# Create a new set of face expressions
![ChangedInV2TagSmall](https://user-images.githubusercontent.com/60819407/167145687-6fb677af-43e0-473e-ab91-66d000619125.png)
<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_16-41-26_p2kFer4X67.mp4" type="video/mp4">
</video>

To assign animations with hand gestures, Drag and drop animation clips from your Project view to the slots in CGE Editor.

<span class="hai-v2">To make the CGE Editor display the previews, drag and drop your avatar to the <em>Preview setup</em> field at the top.</span>

Fill the first row completely. The first row contains the animations that play when one hand is doing a gesture, but the other hand is doing a gesture that doesn't exist (i.e. the 🤙 sign).

Then, you can click the *Auto-set* button in the diagonal. The diagonal contains the animations that play when both hands are doing the same hand gesture. Feel free to choose another animation.

The empty slots in-between will be filled later in this manual, by using a tool to combine animations.

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

# Edit or create animations with Visual Expressions Editor
![ChangedInV2TagSmall](https://user-images.githubusercontent.com/60819407/167145687-6fb677af-43e0-473e-ab91-66d000619125.png)
<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_16-51-53_07ZREXTDvt.mp4" type="video/mp4">
</video>

If you need to create a new face expression animation, click the *Create* button, or click the *Visual Expressions Editor* button on the top right.

<span class="hai-v2">Another way to open this editor is to go in the Animator component, then click the vertical dots on the right</span> `⋮` and click *Haï VisualExpressionsEditor*. <span class="hai-v2">Move your scene camera to better see your avatar.</span>

<span class="hai-v2">In the CGE Editor, at any point, you can click *Regenerate all previews* to refresh the previews.</span>

For more information, head over to the [Visual Expressions Editor documentation](https://hai-vr.notion.site/Visual-Expressions-Editor-262f0ba4cfe24ba38278d99939a2a018).

# Combining hands

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_17-06-15_fe0loyjRqc.mp4" type="video/mp4">
</video>

When your left hand and right hand are not making the same gesture, the animation in the corresponding slot will play.

You can choose your own animation, or use a tool to combine the corresponding animations from the left and right hand by clicking the <span class="hai-btn">+ Combine</span> button.

When combining, you will see a preview of the two animations mixed together. It is very common for the mixed animation to be conflicting, especially when two animations animate the eyes or the mouth in a different way.

Click the buttons on either side to turn some properties on and off, until you find a face expression that makes sense for that combination of gesture. When satisfied with the result, click <span class="hai-btn">Save and assign</span> in the middle.

You can choose to rename the animation using the field above the button.

It is highly recommended to fill out all slots.
 
# Do not blink when eyes are closed

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_17-15-52_JWZUi936jq.mp4" type="video/mp4">
</video>

Go to <span class="hai-btn">Prevent eyes blinking</span> tab. By selecting which animations have both eyes closed, the blinking animation will be disabled as long as that face expression is active.

It is not recommended selecting animations with only one eye closed such as winking, as this will also cause the avatar to stop eye contact.

<div class="hai-interlude">
<p>In your animations, you should <strong>not</strong> animate the Blink blendshape which is used by the Avatars 3.0 descriptor. If you do, your eyelids will not smoothly animate, and they will not animate on analog Fist gestures.</p>
<p>On many avatar bases, the left eyelid and right eyelid can be animated independently. I would suggest you to animate those two blendshapes instead.</p>
</div>

# Apply to the avatar

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_17-59-15_rmaiRqGhCH.mp4" type="video/mp4">
</video>

Select the `ComboGestureExpressions` object of the prefab which contains a *Combo Gesture Compiler* component. In the inspector:
- Drag and drop your avatar in the `Avatar descriptor` slot. The avatar will *not* be modified, this is only required to read the default animation values of the avatar, and also read the blink blendshapes in the Avatar Descriptor.
- Drag and drop your your [FX playable layer](https://docs.vrchat.com/docs/playable-layers#fx) animator to the `FX Animator Controller` slot. **This asset will be modified: New layers and parameters will be added when synchronizing animations.** I recommend you to **make backups** of that FX Animator Controller!

Depending on how your animator is built, choose the correct setting in `FX Playable Mode`: Choose Write Defaults OFF if you are following VRChat recommendations. Try to be consistent throughout your animator.

<span class="hai-v2"><em>If you use MMD worlds that contain dance animations, please read the MMD worlds section later in this manual.</em></span>

You should now be able to press <span class="hai-btn">Synchronize Animator FX layers</span>, which will modify your animator controller.

Whenever you modify any face expression animation or anything related to ComboGestureExpressions, press that button again to synchronize.

*If you haven't done it already, right-click on the newly created prefab and click <span class="hai-btn">Unpack prefab completely</span>.* 

# Squeezing the trigger

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_18-35-03_yxRmSyqtWe.mp4" type="video/mp4">
</video>

In VRChat, to play the hand animations, make a fist with your hand, and squeeze the trigger. The animation will be blended in the more your press the trigger.

The gesture of the other hand is used as the base animation. For example, if you are making `Point + Fist` gesture:
- The `Point + Fist` animation will be used when the trigger is squeezed.
- The `Point` animation will be used when the trigger is not squeezed.

When both hands of your hand are making a fist, you can select two additional animations to play:
- The `Fist X2` animation will be used when both triggers are squeezed.
- The `Fist X2, LEFT trigger` animation will be used when the left trigger is squeezed.
- The `Fist X2, RIGHT trigger` animation will be used when the right trigger is squeezed.
- The `No gesture` animation will be used when none of the triggers are squeezed.

<div class="hai-interlude">
<video controls width="408" autostart loop>
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/hp17ra-demo-analog.mp4" type="video/mp4">
</video>

<p>Illustration of animation blending in an Analog Fist gesture.</p>
</div>

<div class="hai-interlude">
<p>In your animations, you should <strong>not</strong> animate the Blink blendshape which is used by the Avatars 3.0 descriptor. They will not animate on analog Fist gestures.</p>
<p>On many avatar bases, the left eyelid and right eyelid can be animated independently. I would suggest you to animate those two blendshapes instead.</p>
</div>

# Add some Dynamics using PhysBones, Contacts, OSC, and other Avatars 3.0 parameters
![NewInV2TagSmall](https://user-images.githubusercontent.com/60819407/167145432-89428be9-9f16-4795-98ce-093a0c96837c.png)
<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_20-10-33_YTbmBZpWFo.mp4" type="video/mp4">
</video>

To make your face expression react to interactions and other Dynamics, add the prefab to the scene located in `Assets/Hai/ComboGesture/CGEDynamics.prefab`. Right-click on the newly inserted prefab and click <span class="hai-btn">Unpack prefab completely</span>.

This component lets you define Dynamics. Press the `+` button to add a new element in the list.

In the Compiler, define your *Main Dynamics* object above the Mood sets.

You can individually define *Dynamics* objects in each Mood set, which will only apply it to that Mood set. Make sure you don't have duplicate inside your *Main Dynamics* object!

<div class="hai-interlude">
<video controls width="408" autostart loop>
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/cge-dyn2-f.mp4" type="video/mp4">
</video>

<p>Illustration of a Dynamics contact.</p>
</div>

## Select the Dynamic Expression

Select the effect. It can be either a Clip or a Mood set.

After choosing a clip, check the box if both eyes are closed.

For more information about Mood sets, see the *Mood sets* section later in the manual.

## Using a PhysBone as the Dynamic Condition

To use a PhysBone:
- Configure your PhysBone to have a parameter (see [VRChat documentation](https://docs.vrchat.com/docs/physbones#options))
- Define the Source to be PhysBone
- Define the PhysBone object
- Define the Physbone Source to be either Stretch, Angle, or IsGrabbed

The animation will behave differently depending on the PhysBone source:
- If it's Stretch (which is a Float value), the animation will be blended in by how much it is stretched. It is strongest when fully stretched.
  - You can change the *Upper bound* value to a lower value to make it strongest before being fully stretched. A value of 0.75 would make the animation strongest when the Contact is 75% stetched.
- If it's Angle (which is a Float value), the animation will be blended in by how much the bone is away from the rest position. It is strongest when angled at 180 degrees away from the rest position.
  - You can change the *Upper bound* value to a lower value to make it strongest at an angle smaller than 180 degrees. A value of 0.75 would make the animation strongest when the angle is 135 degrees (0.75 * 180deg).
- If it's IsGrabbed, the animation will play when the PhysBone is grabbed.

## Behavior with multiple Dynamic Expressions

<video controls width="816" autostart="false">
    <source src="https://hai-vr.github.io/combo-gesture-expressions-av3/videos/sx_2022-05-04_21-56-30_f5ToWtGl2m.mp4" type="video/mp4">
</video>

The order of Dynamics in this list matters: When two or more Dynamic Conditions are active, the one which is higher in the list has priority.

## Using a Contact as the Dynamic Condition

To use a Contact Receiver:
- Configure your Contact Receiver (see [VRChat documentation](https://docs.vrchat.com/docs/contacts#receiver))
- Define the Source to be Contact
- Define the Contact Receiver object

The animation will behave differently depending on the kind of Contact Receiver:
- If it's Proximity (which is a Float value), the animation will be blended in by the Proximity amount. It is strongest when fully colliding.
  - You can change the *Upper bound* value to a lower value to make it strongest before fully colliding. A value of 0.75 would make the animation strongest when the Contact is 75% colliding.
- If it's Constant, the animation will play when the Contact being collided with.
- If it's OnEnter, the animation will play for a specified duration the moment the Contact is entered above the velocity. Additional options will show up:
  - Set the animation duration.
  - If necessary, tweak the curve. The animation will be blended by this curve over the animation duration. Here are some examples of how to use it:
    - By default, the curve is shaped in a way that makes the animation blend almost instantly and smoothly fade over time:
![sx_2022-05-06_15-15-17_RLkMuJ8Mue](https://user-images.githubusercontent.com/60819407/167138720-8ffa3446-e2ed-463f-b26a-b8f5d0ae4e01.png)
    - You can change the shape of the curve to make your animation last longer at full strength before blending out:
![sx_2022-05-06_15-16-17_R47eUph0Kl](https://user-images.githubusercontent.com/60819407/167138920-48a4d80d-1990-4f93-badb-5a953978e4f5.png)
    - You can also set the curve not to be at full strength to retain influence from your Mood set face expressions.
![sx_2022-05-06_15-18-29_PqWy4xstDA](https://user-images.githubusercontent.com/60819407/167139155-fac2caf5-f2b2-4416-a85a-383c29f5d36d.png)
  - Tweak the enter transition duration of the expression.
  - *Note: As of version V2.0.0, due to a limitation, you can only have one curve per Contact of type OnEnter. In addition, Dynamic Expressions priorities work slighly differently as a lower priority OnEnter Contact may be able to take over a high priority OnEnter Contact. This may change in future versions.*

## Using OSC or Avatars 3.0 parameter as the Dynamic Condition

To use a OSC or Avatars 3.0 parameter as the Dynamic Condition:
- Add the parameter in your avatar Expressions Parameters, if it's not a built-in Avatars 3.0 parameter.
- Define the Parameter Name
- Choose your Parameter type

The animation will behave differently depending on the Parameter type:
- If it's a Float, the animation will be blended in the closest the value gets to 1.
- If it's a Bool or Int, the animation will play when the Bool is true or when the Int value is strictly above 0.
- If you check *Behaves like OnEnter*, the animation will behave as if this parameter was a Contact of type OnEnter.

## Other options

Dynamics condition with a parameter of type Int, Bool will not blend in.

If it's a Float, it will blend in unless the Hard Threshold option is checked, in which case it will behave like a Bool.

In all of these cases, the dynamic element will be active when the threshold is passed.

For a boolean, *IsAboveThreshold* means the element is active when it's true.

# Using multiple mood sets

<iframe src="https://streamable.com/e/c5x44o?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#using-multiple-mood-sets-tutorial-with-audio-commentary) is available)*

Earlier, you set up face expressions within `Default` object of the prefab. This is the default mood set of face expressions of your avatar that is active after loading. However, you can have any number of mood sets and switch between them using the menu to increase the number of face expressions depending on the situation.

The prefab contains another object called `Smiling` as an example, which contains a separate *Combo Gesture Activity* component. Select that object and rename it; It is up to you to organize the mood sets the way you want it (Smiling, Sad, Eccentric, Drunk, Romantic, ...) and it does not necessarily have to be moods (Sign Language, One-handed, Conversation, Dancing, ...)

Select the `ComboGestureExpressions` object of the prefab. In the inspector, set a `Parameter Name` to that new mood set, leaving the first one blank. The blank mood set will be the default mood set that is active when you load your avatar for the first time, or when you deselect a mood set.

In your Expression Parameters, add a new Parameter of type `Bool`.

In your Expression Menu, create a Toggle to control that Parameter.

Add additional mood sets by clicking <span class="hai-btn">+</span> on the list, then drag-and-drop or select another *ComboGestureActivity* component. Just like the second one, choose another `Parameter Name` for that mood set, create an Expression Parameter and a Expression Menu toggle.

*It is not necessary, you can optionally add a `Parameter Name` to the blank mood set. In that case, the first mood set in the list will be default mood set. This will allow you to add a toggle control to the default mood set in order to have an icon for it.*

# Standalone puppets and blend trees

<iframe src="https://streamable.com/e/ai0fzb?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#standalone-puppets-and-blend-trees-tutorial-with-audio-commentary) is available)*

So far we have set up *Activity* mood sets. Another type of mood set is available: *Puppet*, which can be controlled by an Expression Menu.

The prefab contains an object called *Puppet* which contains a *Combo Gesture Puppet* component. Select it and click the <span class="hai-btn">Open editor</span> button in the Inspector.

<span class="hai-v2">To make the CGE Editor display the previews, drag and drop your avatar to the <em>Preview setup</em> field at the top.</span>

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

# Animate cat ears, wings and more

<iframe src="https://streamable.com/e/uo3kut?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#animate-cat-ears-wings-and-more-tutorial-with-audio-commentary) is available)*

In Avatars 3.0, animations that modify transforms belong in the [Gesture playable layer](https://docs.vrchat.com/docs/playable-layers#gesture). In face expression animations, this is most often used to animate ears, wings, tails...

**Skip this step** if you do not have such animations. You should only enable Gesture Playable Layer Support if you do animate those in your face expressions animations.

Note that finger poses and humanoid muscle poses will be ignored by this process. Animating finger poses is done by modifying the Gesture layers, which is outside the scope of this documentation.

If you do not have a gesture layer, duplicate one of the VRChat SDK examples and assign to the Gesture playable layer of your avatar:
- `Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer2` for feminine hand poses,
- `Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer` for masculine hand poses.

Select the `ComboGestureExpressions` object of the prefab. In the inspector, tick the `Gesture playable layer support` checkbox, and assign your [Gesture playable layer](https://docs.vrchat.com/docs/playable-layers#gesture) animator to the `Gesture Animator Controller` slot. **This asset will be modified: New layers and parameters will be added when synchronizing animations.** I recommend you to **make backups** of a that Gesture Animator Controller!

Depending on how your animator is built, choose the correct setting in `Gesture Playable Mode`: Choose Write Defaults OFF if you are following VRChat recommendations. Try to be consistent throughout your animator.

Handling the Gesture Playable is very tricky, and extra precautions need to be taken:

- **You will see a red warning regarding Avatar Masks if *ComboGestureExpressions* detects that your FX Playable Layer may be incompatible with your Gesture Playable Layer**, in which case it will suggest you a fix. If that's the case, click <span class="hai-btn">Add missing masks</span>. This will add a mask to the layers of your FX Playable Layer that do not yet have an Avatar mask.
- If you add new layers to the FX Playable Layer, you may have to click <span class="hai-btn">Add missing masks</span> if you see the red warning again.
- If you modify the FX Playable Layer, and <span class="hai-btn">Synchronize Animator FX and Gesture layers</span> every time you do a change in the FX Playable Layer. That is because the mask is generated based on the animations within the FX Playable layer.
- You should not share your Gesture Playable Layer between two very different avatars that do not have the same base, because the avatar is being used to capture the default bone positions of the avatar when it is at rest, so that animated transforms can reset to a base position when they are not being used.

*If you would like to know why an Avatar mask is needed on layers of the FX Playable Layer, [you may find additional information here](writedefaults.md).*

# Permutations

<iframe src="https://streamable.com/e/2onv8c?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#permutations-tutorial-with-audio-commentary) is available)*

For simplicity purposes, we've been using combinations of gestures, meaning that `Left Point + Right ThumbsUp` will show the same animation as `Left ThumbsUp + Right Point`. I encourage you [using multiple mood sets](#using-multiple-mood-sets) available in an Expressions menu to expand your expressions repertoire.

If you would like to create permutations of gestures, which I do recommend for asymmetric face expressions or hand-specific Fist animations, <span class="hai-v2">change the <em>Mode</em> dropdown at the top left and select <em>Permutations</em></span>. You will see a colored table split between Left hand permutations (colored in orange) and Right hand permutations (colored in blue).

<span class="hai-v2">When this mode is selected in the dropdown</span>, the Activity will behave as if everything was still a combo: If you don't define a Left hand permutation, the Right hand permutation animation will be used for both.

# Mix puppets and gestures

<iframe src="https://streamable.com/e/nvm1n0?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>
*(A [longer tutorial with audio commentary](tutorial.md#mix-puppets-and-gestures-tutorial-with-audio-commentary) is available)*

Any animation slot can have a blend tree within it instead. This means puppeteering is possible for specific combos of hand gestures.

Analog Fist gesture can be completely customized using it, and it is even possible to simultaneously combine the Fist analog trigger with a puppet menu if you feel like it. Remember puppets retain their values when closing the menu, so you don't necessarily need to have your puppet menu opened.

The blend tree template generator can be accessed in <span class="hai-btn">Additional editors > Create blend trees</span> tab. For puppet menus, use the [previously mentioned templates](#standalone-puppets-and-blend-trees). For Fist gestures, select one of the following templates: Single analog fist with hair trigger, Single analog fist and two directions, Dual analog fist.

When placing a blend tree in a single Fist gesture, the parameter `_AutoGestureWeight` will be how much the trigger is squeezed.

When placing a blend tree in the Fist x2 slot, the parameters `GestureLeftWeight` and `GestureRightWeight` will be how much the left and right triggers are squeezed respectively.

# MMD worlds compatibility option
![NewInV2TagSmall](https://user-images.githubusercontent.com/60819407/167145432-89428be9-9f16-4795-98ce-093a0c96837c.png)

MMD worlds are particular in regards to face expressions. If you are a regular user of MMD worlds and need your avatar to be compatible, the avatar needs to be set up in a specific way in order to get face expressions working. **If you don't use MMD worlds, don't bother with this!.**

For MMD worlds specifically, using **Write Defaults ON** is recommended.

In the Compiler at the bottom, define the field called *MMD compatibility toggle parameter*. Create an Expressions Menu toggling that parameter.

In the FX Playable Layer settings, set the FX Playable Mode to *Use Unsupported Write Defaults On*. Do the same for the Gesture Playable Layer if you have one.

![fpl](https://user-images.githubusercontent.com/60819407/167751873-cfc3bcd0-f82f-4670-8e85-a4fe44f40631.png)

When the parameter is active, the face expressions of your avatar will stop playing whenever your avatar is in a chair or a *station*. *Stations* are what MMD worlds use to play animations on the avatar.

*It is possible to use Write Defaults OFF with a big caveat: you won't be able to use the FX layer, therefore no toggles or other avatar gimmicks. CGE's MMD worlds compatibility will not be able to help you with this.*

# Learn more

- [Corrections](corrections.md) - Learn about the various techniques used to fix animations.
- [Integrator](integrator.md) - Documentation about the Integrator, a module to add Weight Corrections without using ComboGestureExpressions.
- [Write Defaults](writedefaults.md) - Explanation of how the Avatar Mask is built.
- [Download on github.com](https://github.com/hai-vr/combo-gesture-expressions-av3/releases) - Main download location.
- [Download on booth.pm](https://hai-vr.booth.pm/items/2219616) - Alternate download location.

*Most illustrations in this documentation use [Saneko avatar (さねこ) by ひゅうがなつ](https://booth.pm/en/items/2322146)*
