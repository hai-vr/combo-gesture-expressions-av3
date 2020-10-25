# Limited Lipsync

You may have noticed a tab called Limited Lipsync.

This feature has been developed in order to prevent the avatar's lipsync blendshapes from interfering with face expressions where the mouth shape is known to cause conflict, especially when its shape is a wide mouth, or the teeth is grinning, or the mouth in a O shape.

We would detect whenever the face is doing these face expressions, and interrupt VRChat's default high fidelity lipsync and switch over to low fidelity lipsync that we would generate.

The current issue is that this feature, when enabled, behaves unexpectedly and causes lipsync to stop working altogether.

For this reason, you should not use it (do not enable the `Integrate limited lipsync` in the Compiler).

I am confident that future VRChat updates will eventually allow this behavior to function, which is the reason I kept this feature available as a preview.

---

- [Back to main page](index.md)