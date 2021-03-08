# Integrator

This is the documentation for the Integrator.

The Integrator is a module that lets you integrate the Weight Correction layers onto your Animator controller **even if you don't use ComboGestureExpressions to manage your face expressions**.

If you do use ComboGestureExpressions to manage your face expressions, you should ignore the Integrator documentation and [return to the main documentation](index.md).

ComboGestureExpressions generates animator layers that may be interesting to users who use the Analog Fist gesture weight (`GestureLeftWeight` and `GestureRightWeight`).

In particular, when other players look at you, the animation looks laggy and jerky. The generated layers can help address this issue *(For more information, see [Corrections](corrections.md#gestureweight-smoothing-correction))*.

<iframe src="https://streamable.com/e/42360m?loop=0" width="408" height="256" frameborder="0" allowfullscreen></iframe> 

The Integrator will generate those layers without requiring you to manage any face expression.

## Usage

Reminder: If you use ComboGestureExpressions, you do not need to use the Integrator.

Create a GameObject, and add a *Combo Gesture Integrator* component.

Assign your animator to the *Animator Controller* slot, and press <span class="hai-btn">Synchronize Animator layers</span>.

After synchronizing, you will need to edit the animator locations which make use of the Gesture weight manually. Usually those are either the Normalized Time of animator states or Blend Tree parameters:

- Replace `GestureLeftWeight` with `_Hai_GestureLWSmoothing`
- Replace `GestureRightWeight` with `_Hai_GestureRWSmoothing`

You usually need to synchronize only once.

If you use gesture weight parameters in multiple Playable layers such as both Gesture Playable layer and FX Playable layer, you will need to do this operation with each animator controller.

---

- [Back to main page](index.md)