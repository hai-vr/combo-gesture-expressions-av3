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

- Press *Create/Overwrite* to generate the layers.

### Using multiple activities

If you want to switch between multiple sets of face expressions from the Action menu:

- Make sure you have a stage parameter and an expression menu set up to switch between face expressions.
- Set that stage parameter name in *Activity Stage name*.
- In *Gesture Combo Activities* list, set the number values on the right to match the menu.

![](https://github.com/hai-vr/combo-gesture-expressions-av3/raw/z-res-pictures/Documentation/inspector-compiler-multiple_illustrated.png)
