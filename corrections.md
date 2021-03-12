# Corrections

This section is not a tutorial, but an explanation of the various corrections that ComboGestureExpressions provides.

#### Exhaustive animation curves

The animations you provide are not used directly in animators: I create modified copies of those animations in order to prevent several issues from happening:

- There is an issue if the FX layer contains specific states which will mess up the appearance of your face expressions [depending on the framerate](https://github.com/hai-vr/combo-gesture-expressions-av3/issues/22) of the person looking at you.
- There is an issue when wiggling a puppet menu too quickly, which will mess up the appearance of your face expressions.

The applied fix is to make sure all FX layer animations will always modify all animated curves. 

#### GestureWeight transition correction

GestureWeight value depends on how much the trigger is squeezed when the hand is doing a Fist gesture. The value of GestureWeight will always be 1.0 if the hand is not a Fist.

This causes an issue right when the hand stops doing a Fist gesture: The animation will transition to another state, but the blend tree read the new GestureWeight value of 1.0 instantly. This causes the face expression to instantly change before the transition starts.

The applied fix is to copy the GestureWeight value only as long as the hand is doing a Fist gesture, effectively freezing the last known value when the Fist is no longer doing that gesture. This is done by using a technique with *Animated Animator Parameters*, where an Animator parameter is animated using itself as the Normalized Time input.

This correction can be installed without ComboGestureExpressions using the [Integrator](integrator.md).

#### GestureWeight smoothing correction

The trigger squeezes smoothly when looking at yourself on mirrors and cameras.

However, when other players look at you, the animation looks laggy and jerky, it is not interpolated.

The applied fix is to smooth that value by performing a mathematical operation on the received GestureWeight:

> SmoothedValue ← TargetValue * SmoothingFactor + SmoothedValue * (1 - SmoothingFactor)

This mathematical operation is implemented using several blend trees.

This correction can be installed without ComboGestureExpressions using the [Integrator](integrator.md).

#### Disabling blinking animation

Blinking is detected by attaching an *Animated Animator Parameter* to the animation. This parameter is set to 1.0 on all animations that are tagged as blinking, which is then read by a dedicated layer to disable eye tracking.

#### Single-keyframe animations correction

Some face expressions are animated over several frames, but some aren't when it's just a static face. For the latter, it is common practice to create animations with a one frame long, which consists on making sure the animation has two keyframes. This is done manually by duplicating the first keyframe.

If the animation only has one keyframe, then most of the times it makes the animation last 1 second, which creates an anomaly where face expression transitions last too long.

ComboGestureExpressions automatically creates a modified copy of those animations to make sure all animations curves have at least two keyframes. You can safely create animations with only one keyframe.

---

- [Back to main page](index.md)
