# Hyper Jump

Jump higher and leap further.

To install the mod, copy the Mods folder inside the zip into your BONEWORKS or BONELAB directory.

Hyper jumps and leaps are best used in exterior environments (unless you like fracturing your skull on the ceiling or smashing your face against a wall).

Hyper leaps only apply when walking/running, and will propel you towards where you are looking.

Hyper jumps/leaps can be charged so that the jump power scales with how long the jump button was pressed for.

You can use BoneMenu to edit up to 3 custom profiles with different jump/leap strength.

ActiveProfile can take the following values :
- `default` the default HyperJump profile  
- `instant` same as the default but all jumps will be performed at max power  
- `disabled` disables HyperJump  
- `customa` uses the parameters from the `[HyperJumpCustomA]` section, which is initially the same as the default profile  
- `customb` uses the parameters from the `[HyperJumpCustomB]` section, which is initially the same as the default profile  
- `customc` uses the parameters from the `[HyperJumpCustomC]` section, which is initially the same as the default profile


| Custom property       | Description                                                                                               |
|-----------------------|-----------------------------------------------------------------------------------------------------------|
| UpwardJumpMultiplier  | How high the hyper jumps will go                                                                          |
| ForwardLeapMultiplier | How far the hyper leaps will go                                                                           |
| JumpChargingTime      | How many seconds to hold the jump button until you reach max jump power, set to 0 for instant hyper jumps |

## Changelogs

#### v3.0.1

- Fix dependency to BoneLib for compatibility with Patch 2

#### v3.0.0

- Make compatible with both BONEWORKS and BONELAB
- Add more custom profiles slots
- Add BoneMenu support for switching and editing profiles

#### v2.0.0

- Port from BONEWORKS to BONELAB
- Add configurable profiles

#### v1.1.0:

- Add jump charging
- Improve hyper jump physics

#### v1.0.0:

- Initial version