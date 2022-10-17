using System.Collections.Generic;
using MelonLoader;
using SLZ.Rig;
using UnityEngine;
using HarmonyLib;
using Il2CppSystem;
using SLZ.VRMK;
using UnityObject = UnityEngine.Object;
using Enum = System.Enum;

namespace Evanaellio.HyperJump
{
    public static class BuildInfo
    {
        public const string Name = "Hyper Jump";
        public const string Author = "Evanaellio";
        public const string Company = null;
        public const string Version = "2.0.0";
        public const string DownloadLink = "https://bonelab.thunderstore.io/package/Evanaellio/HyperJump/";
    }

    public class HyperJump : MelonMod
    {
        private const string HyperJumpCategory = nameof(HyperJump);
        private const string CustomProfileCategory = "HyperJumpCustomProfile";

        public static ProfilesEnum ActiveProfile
        {
            get
            {
                string activeProfile = MelonPreferences.GetEntryValue<string>(HyperJumpCategory, nameof(ActiveProfile));
                if (Enum.TryParse(activeProfile, true, out ProfilesEnum profile))
                {
                    return profile;
                }

                MelonLogger.Warning($"Invalid profile '{activeProfile}', using the default profile instead");
                return ProfilesEnum.Default;
            }

            set => MelonPreferences.SetEntryValue(HyperJumpCategory, nameof(ActiveProfile), value.ToString().ToLowerInvariant());
        }

        public class Profile
        {
            public float UpwardJumpMultiplier { get; set; }
            public float ForwardLeapMultiplier { get; set; }
            public float JumpChargingTime { get; set; }
        }

        private static readonly Profile DefaultProfile = new Profile()
        {
            UpwardJumpMultiplier = 90f, ForwardLeapMultiplier = 18f, JumpChargingTime = 1.5f,
        };

        private static readonly Profile InstantProfile = new Profile()
        {
            UpwardJumpMultiplier = DefaultProfile.UpwardJumpMultiplier, ForwardLeapMultiplier = DefaultProfile.ForwardLeapMultiplier, JumpChargingTime = 0f,
        };

        public static readonly Dictionary<ProfilesEnum, Profile> Profiles = new Dictionary<ProfilesEnum, Profile>()
        {
            {ProfilesEnum.Default, DefaultProfile},
            {ProfilesEnum.Instant, InstantProfile},
        };


        public enum ProfilesEnum
        {
            Default,
            Instant,
            Disabled,
            Custom
        }

        public override void OnInitializeMelon()
        {
            // Create MelonPreferences default values (if they don't exist already)
            MelonPreferences.CreateCategory(HyperJumpCategory);
            MelonPreferences.CreateCategory(CustomProfileCategory);
            MelonPreferences.CreateEntry(HyperJumpCategory, nameof(ActiveProfile), "default");
            MelonPreferences.CreateEntry(CustomProfileCategory, nameof(Profile.UpwardJumpMultiplier), DefaultProfile.UpwardJumpMultiplier);
            MelonPreferences.CreateEntry(CustomProfileCategory, nameof(Profile.ForwardLeapMultiplier), DefaultProfile.ForwardLeapMultiplier);
            MelonPreferences.CreateEntry(CustomProfileCategory, nameof(Profile.JumpChargingTime), DefaultProfile.JumpChargingTime);

            Profiles.Add(ProfilesEnum.Custom, new Profile
            {
                UpwardJumpMultiplier = MelonPreferences.GetEntryValue<float>(CustomProfileCategory, nameof(Profile.UpwardJumpMultiplier)),
                ForwardLeapMultiplier = MelonPreferences.GetEntryValue<float>(CustomProfileCategory, nameof(Profile.ForwardLeapMultiplier)),
                JumpChargingTime = MelonPreferences.GetEntryValue<float>(CustomProfileCategory, nameof(Profile.JumpChargingTime)),
            });
        }
    }

    [HarmonyPatch(typeof(ControllerRig), "JumpCharge")]
    class ControllerRigJumpChargePatch
    {
        public static DateTime? JumpChargeStartedDate = null;

        public static void Postfix(ControllerRig __instance, bool chargeInput)
        {
            // Skip HyperJump behaviour for the disabled profile
            if (HyperJump.ActiveProfile.Equals(HyperJump.ProfilesEnum.Disabled)) return;
            HyperJump.Profile currentProfile = HyperJump.Profiles[HyperJump.ActiveProfile];

            // chargeInput is true when the jump button is being pressed
            if (chargeInput && !JumpChargeStartedDate.HasValue)
            {
                JumpChargeStartedDate = DateTime.Now;
            }
            else if (!chargeInput && JumpChargeStartedDate.HasValue)
            {
                // When jump button is released, trigger hyper jump then reset jump charging 
                TriggerHyperJump(__instance, currentProfile);
                JumpChargeStartedDate = null;
            }
        }

        private static void TriggerHyperJump(ControllerRig controllerRig, HyperJump.Profile profile)
        {
            var physGrounder = UnityObject.FindObjectOfType<PhysGrounder>();
            // Only jump when on the ground
            if (physGrounder.isGrounded)
            {
                PhysicsRig rig = UnityObject.FindObjectOfType<PhysicsRig>();

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
                float walkSpeed = new Vector3(rig.pelvisVelocity.x, 0, rig.pelvisVelocity.z).magnitude;
                Vector3 forwardJump = controllerRig.m_head.forward * walkSpeed * profile.ForwardLeapMultiplier;
                Vector3 verticalJump = Vector3.up * profile.UpwardJumpMultiplier;

                // Apply jump velocity to the player
                rig.torso.rbPelvis.AddForce((verticalJump + forwardJump) * jumpChargeRatio,
                    ForceMode.VelocityChange);
            }
        }
    }
}