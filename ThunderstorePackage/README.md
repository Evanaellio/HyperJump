# Hyper Jump

Jump higher and leap further.

To install the mod, copy the Mods folder inside the zip into your BONELAB directory.

Hyper jumps and leaps are best used in exterior environments (unless you like fracturing your skull on the ceiling or smashing your face against a wall).

Hyper leaps only apply when walking/running, and will propel you towards where you are looking.

Hyper jumps/leaps can be charged so that the jump power scales with how long the jump button was pressed for.

You can edit your MelonPreferences.cfg file in order to customize your HyperJump experience. By default, it's configured like this :

```ini
[HyperJump]
ActiveProfile = "default"

[HyperJumpCustomProfile]
UpwardJumpMultiplier = 90.0
ForwardLeapMultiplier = 18.0
JumpChargingTime = 1.5
```

ActiveProfile can take the following values :  
`default` the default HyperJump profile  
`instant` same as the default but all jumps will be performed at max power  
`disabled` disables HyperJump  
`custom` uses the parameters from the `[HyperJumpCustomProfile]` section, which is initially the same as the default profile  

| Custom property       | Description                                                                                               |
|-----------------------|-----------------------------------------------------------------------------------------------------------|
| UpwardJumpMultiplier  | How high the hyper jumps will go                                                                          |
| ForwardLeapMultiplier | How far the hyper leaps will go                                                                           |
| JumpChargingTime      | How many seconds to hold the jump button until you reach max jump power, set to 0 for instant hyper jumps |

## Changelogs

#### v2.0.0

- Port from BONEWORKS to BONELAB
- Add configurable profiles

#### v1.1.0:

- Add jump charging
- Improve hyper jump physics

#### v1.0.0:

- Initial version