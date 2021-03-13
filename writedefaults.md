# Write Defaults

Write Defaults is a tricky topic. According to the VRChat Documentation, it recommends that in general:

- Write Defaults should be OFF.
- Transform animations belong in the Gesture Playable Layer.
- Muscle animations belong in the Gesture Playable Layer.
- Anything that is not a Transform animation nor a Muscle animation belong in the FX Playable Layer. This includes constraint animation curves and GameObject toggles.

However, in the real world, Transform animations in the Gesture Playable Layer will not animate if all of the following are true:

- If in the FX layer, there is at least 1 layer with 1 active state having Write Defaults OFF, and
- That layer has an Avatar Mask that does not *deny* all the transforms that are being animated.

This means we need to create an Avatar Mask to add in the FX Layer. However, that Avatar Mask needs to be crafted carefully:

- If a layer has animations that animates references such as Material swaps, then the Avatar Mask must *allow* that transform in the mask.
- In the real world, there are prefabs that animate Transforms in the FX Playable Layer. Thereform the mask must *allow* these unusual transforms in order not to break existing prefabs.
- If there are zero transforms that need to be *allowed* by the above rules, then the Avatar Mask must *allow* at least 1 random transform, because an Avatar Mask that has 0 transforms is considered to be *allowing all* the transforms.
