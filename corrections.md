# Corrections

This section is not a tutorial, but an explanation of the various corrections that ComboGestureExpressions provides.

#### Exhaustive animation curves

The animations you provide are not used directly in animators: I create modified copies of those animations in order to prevent several issues from happening:

- There is an issue if the FX layer contains specific states which will mess up the appearance of your face expressions [depending on the framerate](https://github.com/hai-vr/combo-gesture-expressions-av3/issues/22) of the person looking at you.
- There is an issue when wiggling a puppet menu too quickly, which will mess up the appearance of your face expressions.

The applied fix is to make sure all FX layer animations will always modify all animated curves. 

#### GestureWeight correction

GestureWeight value depends on how much the trigger is squeezed when the hand is doing a Fist gesture. The value of GestureWeight will always be 1.0 if the hand is not a Fist.

This causes an issue right when the hand stops doing a Fist gesture: The animation will transition to another state, but the blend tree read the new GestureWeight value of 1.0 instantly. This causes the face expression to instantly change before the transition starts.

The applied fix is to copy the GestureWeight value only as long as the hand is doing a Fist gesture, effectively freezing the last known value when the Fist is no longer doing that gesture. This is done by using a technique with *Animated Animator Parameters*, where an Animator parameter is animated using itself as the Normalized Time input.

#### Disabling blinking animation

Blinking is detected by attaching an *Animated Animator Parameter* to the animation. This parameter is set to 1.0 on all animations that are tagged as blinking, which is then read by a dedicated layer to disable eye tracking.

---

- [Back to main page](index.md)