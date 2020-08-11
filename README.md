# ComboGestureExpressions for Avatars 3.0

### [> Download latest version...](https://github.com/hai-vr/combo-gesture-expressions-av3/releases)

*ComboGestureExpressions* is an Unity Editor tool that lets you attach face expressions to hand gestures, and automatically generate an *Avatars 3.0* animator to match these gestures.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/illustration.gif)

Using this tool:

- Face expressions are fully predictable when combining different gestures.
  This resolves an issue with classic avatars where two face expressions would conflict and mess with the appearance of the eyes.
- Eyes will no longer blink whenever the avatar has a face expression with eyes closed.
- Multiple face expressions can be bound to a single gesture, using the expressions menu to switch between sets of face expressions.
- Enjoy the new Avatars 3.0 finer face expression controls alongside the classic gesture face expression controls.

This tool should NOT be used for:

- ❌ Animating hand and finger positions.
  To animate hand and finger positions, use the Avatars 3.0's Gesture layer which is made for this purpose.

## How to use

A common issue with classic avatar face expression are conflicting face expressions. For instance, combining two expressions will result in a combined face expression that isn't aesthetic pleasing.

In *ComboGestureExpressions*, you can create up to 36 animations in the *Combo Gesture Activity* component to support every single possible gesture combo.
Every combo will result in a predictable face. You are free to reuse animations between combos or leave some gestures blank.

Then, you can use that *Combo Gesture Activity* in a *Combo Gesture Compiler*, which will add the face expression layers into your FX animator.

If you wish to have multiple sets of face expressions, add additional *Combo Gesture Activity* in the *Combo Gesture Compiler* so that you can switch between them using the Action menu (or other means).

### Combo Gesture Activity

- Create a GameObject with a *Combo Gesture Activity* component.
- Add the 36 animations for each combination of gestures, leaving some of them blank if needed.
  - Duplicate animations are allowed.
  - The folding menus lets you filter by a specific gesture.
- Reference:
  - *Exactly one* / *No gesture*: 🤙 (Neutral)
  - FIST: ✊ (Fist)
  - OPEN: ✋ (HandOpen)
  - POINT: ☝️ (FingerPoint)
  - PEACE: ✌️(Victory)
  - ROCKNROLL: 🤘 (RockNRoll)
  - GUN: 🎯👈 (HandGun)
  - THUMBSUP: 👍 (ThumbsUp)
  - *...on both hands*: 🙌

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-activity-default_illustrated.png)

- If you leave blank (*None (Animation Clip)*), the animation in *No Gesture* will be used instead if present.
  Otherwise, the face will fall back to the default expression of the skinned mesh renderer.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-activity-fallback_illustrated.png)

- In *Closed eyes animations* list, add face expressions that have closed eyes.
  This will disable the blinking animation whenever any of these animations are active.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-activity-closed_illustrated.png)

### Combo Gesture Compiler

- Create a GameObject with a *Combo Gesture Compiler* component.
- Set the FX animator in the *FX Animator Controller* slot.
- In *Gesture Combo Activities* list, add the *Combo Gesture Activity* GameObject.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-compiler-single.png)

- Press *Synchronize* to generate the layers.

### Using multiple activities

If you want to switch between multiple sets of face expressions from the Action menu:

- Make sure you have a stage parameter and an expression menu set up to switch between face expressions.
- Set that stage parameter name in *Activity Stage name*.
- In *Gesture Combo Activities* list, set the number values on the right to match the menu.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-compiler-multiple_illustrated.png)

### Mixing with Avatars 3.0's Puppet menu

*ComboGestureExpressions* was designed to combine the best of both worlds: The spontaneity of gestures, and the expressiveness of Avatars 3.0's Puppet menus.

🤘 + 🕹️ = ❤️

If you would like to add a puppet menu for use simultaneously with Avatars 3.0:

- Follow the **"Using multiple activities" 🔼** guide above to add an additional *Gesture Combo Activity*.
  You can switch to that activity using the expression menu whenever you want to use Puppet menus to control your face expressions.
- Configure that *Gesture Combo Activity* with face expressions that will not conflict with your blend trees.
  - You can also leave all animations of that *Gesture Combo Activity* blank, so that your face will only be controlled by blend trees.
- About blend trees:
  - It is recommended that if one animation in the blend tree changes a property, then all animations should also change that property to a default value when unused.
  - You may need to reset the properties affected by that face expression back to defaults when switching activities.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/coexist-blend-tree.gif)

- Design your own Animator layer so that the blend tree plays only when the value of the given *Activity Stage name*'s parameter matches.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/animator-blend-tree-condition.png)

### Advanced: Internal parameters

A few internal parameters are exposed, allowing other Animator layers to take control over the behavior of *ComboGesturesExpressions*.

These may be used to disable the logic that drives eyes blinking Animator Tracking, disable all face expressions, or read whether the current face expression is meant to close the eyes.

This is an advanced and experimental feature. Head over to the [Internal Parameters guide](GUIDE_internal_parameters.md).
