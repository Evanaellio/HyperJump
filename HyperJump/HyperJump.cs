using MelonLoader;
using StressLevelZero.Rig;
using UnityEngine;
using HarmonyLib;
using StressLevelZero.VRMK;

namespace Evanaellio.HyperJump
{
    public static class BuildInfo
    {
        public const string Name = "Hyper Jump";
        public const string Author = "Evanaellio";
        public const string Company = null;
        public const string Version = "1.0";
        public const string DownloadLink = null;
    }

    public class HyperJump : MelonMod
    {
        private const string HyperJumpCategory = nameof(HyperJump);

        public static float UpwardJumpMultiplier
        {
            get => MelonPreferences.GetEntryValue<float>(HyperJumpCategory, nameof(UpwardJumpMultiplier));
            set => MelonPreferences.SetEntryValue(HyperJumpCategory, nameof(UpwardJumpMultiplier), value);
        }

        public static float ForwardLeapMultiplier
        {
            get => MelonPreferences.GetEntryValue<float>(HyperJumpCategory, nameof(ForwardLeapMultiplier));
            set => MelonPreferences.SetEntryValue(HyperJumpCategory, nameof(ForwardLeapMultiplier), value);
        }

        public static bool HyperJumpEnabled
        {
            get => MelonPreferences.GetEntryValue<bool>(HyperJumpCategory, nameof(HyperJumpEnabled));
            set => MelonPreferences.SetEntryValue(HyperJumpCategory, nameof(HyperJumpEnabled), value);
        }

        public override void OnApplicationStart()
        {
            // Create MelonPreferences default values (if they don't exist already)
            MelonPreferences.CreateCategory(HyperJumpCategory);
            MelonPreferences.CreateEntry(HyperJumpCategory, nameof(UpwardJumpMultiplier), 16f);
            MelonPreferences.CreateEntry(HyperJumpCategory, nameof(ForwardLeapMultiplier), 3f);
            MelonPreferences.CreateEntry(HyperJumpCategory, nameof(HyperJumpEnabled), true);

            // ModThatIsNotMod Menu
            ModThatIsNotMod.BoneMenu.MenuCategory category =
                ModThatIsNotMod.BoneMenu.MenuManager.CreateCategory("Hyper Jump", Color.green);
            category.CreateFloatElement("Upward Jump Multiplier", Color.white, UpwardJumpMultiplier,
                newValue => UpwardJumpMultiplier = newValue, 2f, 0f, 40f, true);
            category.CreateFloatElement("Forward Leap Multiplier", Color.white, ForwardLeapMultiplier,
                newValue => ForwardLeapMultiplier = newValue, 0.5f, 0f, 8f, true);
            category.CreateBoolElement("Enable Hyper Jump", Color.white, HyperJumpEnabled,
                newValue => HyperJumpEnabled = newValue);
        }
    }

    [HarmonyPatch(typeof(ControllerRig), "Jump")]
    class ControllerRigJumpPatch
    {
        public static void Prefix(ControllerRig __instance)
        {
            if (!HyperJump.HyperJumpEnabled) return;

            var physGrounder = Object.FindObjectOfType<PhysGrounder>();
            
            // Only jump when on the ground
            if (physGrounder.isGrounded)
            {
                PhysicsRig rig = Object.FindObjectOfType<PhysicsRig>();
                
                // Compute velocity vectors for jumping (up) and leaping (forward)
                float walkSpeed = new Vector3(rig.pelvisVelocity.x, 0, rig.pelvisVelocity.z).magnitude;
                Vector3 forwardJump = __instance.m_head.forward * walkSpeed * HyperJump.ForwardLeapMultiplier;
                Vector3 verticalJump = Vector3.up * HyperJump.UpwardJumpMultiplier;
                
                // Apply jump velocity to the player
                rig.physBody.AddVelocityChange(verticalJump + forwardJump);
            }
        }
    }
}