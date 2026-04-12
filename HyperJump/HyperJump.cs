using UnityEngine;
using MelonLoader;
using DateTime = System.DateTime;
using BoneLib;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using static Evanaellio.HyperJump.ProfilePreferences;
using TimeSpan = System.TimeSpan;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Evanaellio.HyperJump;

public static class BuildInfo
{
    public const string Name = "Hyper Jump";
    public const string Author = "Evanaellio";
    public const string Company = null;
    public const string Version = "4.0.0";
    public const string DownloadLink = "https://bonelab.thunderstore.io/package/Evanaellio/HyperJump/";
}

public class HyperJump : MelonMod
{
    public static PhysicsRig PhysicsRig => Player.PhysicsRig;
    public static ControllerRig ControllerRig => Player.ControllerRig;
    public static PhysGrounder PhysGrounder => Player.PhysicsRig.physG;

    public override void OnInitializeMelon()
    {
        InitializePreferencesAndMenu();
        MelonLogger.Msg("HyperJump loaded");
    }

    public override void OnUpdate()
    {
        if (ControllerRig == null)
        {
            return;
        }

        // _secondaryAButtonDown is True only during the frame where the jump button was pressed
        if (ControllerRig._secondaryAButtonDown && !JumpChargeStartedDate.HasValue)
        {
            JumpChargeStartedDate = DateTime.Now;
        }
    }

    private static DateTime? JumpChargeStartedDate = null;

    [HarmonyPatch(typeof(RemapRig))]
    public static class RemapRigPatches
    {
        [HarmonyPatch(nameof(RemapRig.Jump))]
        [HarmonyPrefix]
        public static void Jump()
        {
            // Skip HyperJump behaviour for the disabled profile
            if (ActiveProfile.Equals(ProfilesEnum.Disabled)) return;
            Profile currentProfile = Profiles[ActiveProfile];

            // When jump button is released, trigger hyper jump then reset jump charging 
            TriggerHyperJump(currentProfile);
            JumpChargeStartedDate = null;
        }
    }

    private static void TriggerHyperJump(Profile profile)
    {
        // Only jump when on the ground
        if (PhysGrounder.isGrounded)
        {
            float jumpChargeRatio = 1f;

            // Compute jump charging ratio (between 0 and 1) if charging is enabled (charging time greater than 0)
            if (profile.JumpChargingTime > 0)
            {
                TimeSpan jumpChargeDuration =
                    (DateTime.Now - JumpChargeStartedDate) ?? TimeSpan.Zero;
                jumpChargeRatio = Mathf.InverseLerp(0.0f, profile.JumpChargingTime * 1000,
                    (float) jumpChargeDuration.TotalMilliseconds);
            }

            // Compute velocity vectors for jumping (up) and leaping (forward)
            Vector3 pelvisVelocity = PhysicsRig.pelvisVelocity;
            pelvisVelocity.y = 0;
            float walkSpeed = pelvisVelocity.magnitude;
            Vector3 forwardJump = ControllerRig.m_head.forward * walkSpeed * profile.ForwardLeapMultiplier * 18f;
            Vector3 verticalJump = Vector3.up * profile.UpwardJumpMultiplier * 90f;

            // Apply jump velocity to the player
            PhysicsRig.torso.rbPelvis.AddForce((verticalJump + forwardJump) * jumpChargeRatio, ForceMode.VelocityChange);
        }
    }
}
