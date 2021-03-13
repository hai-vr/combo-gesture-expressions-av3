# Write Defaults

Write Defaults is a tricky topic. According to the VRChat Documentation, it recommends that in general:

- Write Defaults should be OFF.
- Transform animations belong in the Gesture Playable Layer.
- Muscle animations belong in the Gesture Playable Layer.
- Anything that is not a Transform animation nor a Muscle animation belong in the FX Playable Layer. This includes constraint animation curves and GameObject toggles.

For Transform animations to work in the Gesture Playable Layer:

- The first mask of the Gesture Playable Layer must *allow* all Muscles and all Transforms that are animated by all the layers of the Gesture Playable Layer.
- The other masks of the Gesture Playable Layer should *allow* only the Muscles and Transforms that are animated by the layer on which the mask is on.

However, in the real world, Transform animations in the Gesture Playable Layer will not animate if all of the following are true:

- If in the FX layer, there is at least 1 layer with 1 active state having Write Defaults OFF, and
- That layer has an Avatar Mask that does not *deny* all the transforms that are being animated.

This means we need to create an Avatar Mask to add in the FX Layer. However, that Avatar Mask needs to be crafted carefully:

- If a layer has animations that animates references such as Material swaps, then the Avatar Mask must *allow* that transform in the mask.
- In the real world, there are prefabs that animate Transforms in the FX Playable Layer. Thereform the mask must *allow* these unusual transforms in order not to break existing prefabs.
- If there are zero transforms that need to be *allowed* by the above rules, then the Avatar Mask must *allow* at least 1 random transform, because an Avatar Mask that has 0 transforms is considered to be *allowing all* the transforms.

There is an unknown:

- According to the VRChat documentation, the mask of the first layer of the FX Playable Layer will be replaced at runtime. This means I do not know what is the expected behavior of the animator if the base layer has a strange configuration of Write Defaults OFF. (source: Haï)

There are additional precautions that need to be taken for Write Defaults OFF to function properly, but it is outside of the scope of *ComboGestureExpressions*:

- All transition must have an Interruption Source set to None: This is because if a transition from A to B interrupts into a transition from A to C, which then interrupts back to a transition from A to B, it will exhibit a behavior similar to Write Defaults ON.
- All states should have a Motion of at least 2 keyframes for safety, rather than `None (Motion)`.
- Animation clips should not have 0 animated curves.
- Blend trees should not have 0 Motions.
- Avoid Blend trees of type *Direct*.
