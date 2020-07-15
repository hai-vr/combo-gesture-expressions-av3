# Internal parameters

*ComboGestureExpressions* exposes a few non-synced parameters that are local to the animator.

⚠️*Internal parameters are experimental: I cannot guarantee that the behavior for these internal parameters will remain stable in future versions of ComboGestureExpressions.*

## Write-only

These write-only parameters lets you control the behavior of the *ComboGestureExpressions*.

### `_Hai_GestureComboDisableExpressions` (Int)

Allows disabling the face expressions, resetting it back to a neutral state.

This is useful if you want to play an animation where the face expression will be completely overriden.

Behavior:

- While the value is equal to 0, face expressions will play normally according to the current Activity and Gestures.
- When the value **becomes** equal to 1, the face expression will be set to a neutral state, and it will remain this way as long as the value remains equal to 1.

### `_Hai_GestureComboDisableBlinkingOverride` (Int)

Allows disabling blinking. This is useful if you have a puppet menu that needs to override the behavior of eyes blinking by independently triggering Animator Tracking Control behaviors for Eyes & Eyelids.

Behavior:

- While the value is equal to 0, eyes blinking will be enabled or disabled according to the current activity and gesture combo.
- When the value **becomes** equal to 1, the logic that drives the eyes blinking will become suspended. The Animator Tracking for the Eyes & Eyelids will NOT be reset back to a default value.
- When the value **becomes** equal to 0, the logic that drives the eyes blinking will become resume, and the Animator Tracking for the Eyes & Eyelids is immediately overriden.


## Read-only

All of the read-only internal parameters have their state derived from:
- `GestureLeft` Animator parameter (IK Sync)
- `GestureRight` Animator parameter (IK Sync)
- `_Hai_GestureComboDisableBlinkingOverride` Internal write-only parameter (Not synced)

In order to make sure your avatar is synced to everyone, make sure that the write-only properties are written from a state that is naturally synced.

### `_Hai_GestureComboValue` (Int, IK frequency)

Exposes one of 36 values, representing the current gesture combo. This value is derived from:

- `GestureLeft` Animator parameter (IK Sync)
- `GestureRight` Animator parameter (IK Sync)

Because it is derived from IK Synced parameters, this value updates at the same rate as IK.

The possible values are:
```
 0,  1,  2,  3,  4,  5,  6,  7,
    11, 12, 13, 14, 15, 16, 17,
        22, 23, 24, 25, 26, 27,
            33, 34, 35, 36, 37,
                44, 45, 46, 47,
                    55, 56, 57,
                        66, 67,
                            77
```

### `_Hai_GestureComboAreEyesClosed` (Int, IK frequency)

Exposes whether the currently playing face expression's eyes are considered to be closed in the currently active Activity.

This can be read from another animator layer in order to blend a hand-made blinking animation.
Remember that it is possible to convert an Int to a Float by using two Parameter Drivers.

This value is derived from:
                                                  
- `_Hai_GestureComboValue` Internal parameter (IK frequency)
- `_Hai_GestureComboDisableBlinkingOverride` Internal parameter (Write-only)

Because it is derived from a parameter updating at IK frequency, this value updates at the same rate as IK, and is synced as long as the Write-only internal parameter is naturally synced.

The possible values are:
- `0` when eyes are open.
- `1` when eyes are closed.

The value will cease to update if the Write-only internal property `_Hai_GestureComboDisableBlinkingOverride` is equal to 1. 
