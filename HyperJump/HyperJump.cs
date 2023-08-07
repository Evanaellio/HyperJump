using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Il2CppSystem;
using MelonLoader;
using UnityObject = UnityEngine.Object;
using Enum = System.Enum;

namespace Evanaellio.HyperJump
{
    extern alias bonelab;

    public static class BuildInfo
    {
        public const string Name = "Hyper Jump";
        public const string Author = "Evanaellio";
        public const string Company = null;
        public const string Version = "3.1.0";
        public const string DownloadLink = "https://bonelab.thunderstore.io/package/Evanaellio/HyperJump/";
    }

    public class HyperJump : MelonMod
    {
        private const string HyperJumpCategory = nameof(HyperJump);

        private static readonly string[] CustomProfileCategories =
        {
            "HyperJumpCustomA", "HyperJumpCustomB", "HyperJumpCustomC"
        };

        public static IBoneUtils BoneUtils;

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
            public virtual float UpwardJumpMultiplier { get; set; }
            public virtual float ForwardLeapMultiplier { get; set; }
            public virtual float JumpChargingTime { get; set; }
        }

        public class CustomProfile : Profile
        {
            public string Category { get; set; }

            public override float UpwardJumpMultiplier
            {
                get => MelonPreferences.GetEntryValue<float>(Category, nameof(UpwardJumpMultiplier));
                set => MelonPreferences.SetEntryValue(Category, nameof(UpwardJumpMultiplier), value);
            }

            public override float ForwardLeapMultiplier
            {
                get => MelonPreferences.GetEntryValue<float>(Category, nameof(ForwardLeapMultiplier));
                set => MelonPreferences.SetEntryValue(Category, nameof(ForwardLeapMultiplier), value);
            }

            public override float JumpChargingTime
            {
                get => MelonPreferences.GetEntryValue<float>(Category, nameof(JumpChargingTime));
                set => MelonPreferences.SetEntryValue(Category, nameof(JumpChargingTime), value);
            }
        }

        private static readonly Profile DefaultProfile = new Profile()
        {
            UpwardJumpMultiplier = 1f, ForwardLeapMultiplier = 1f, JumpChargingTime = 1.5f,
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
            CustomA,
            CustomB,
            CustomC
        }

        public override void OnInitializeMelon()
        {
            // Create MelonPreferences default values (if they don't exist already)
            MelonPreferences.CreateCategory(HyperJumpCategory);
            MelonPreferences.CreateEntry(HyperJumpCategory, nameof(ActiveProfile), "default");

            List<CustomProfileSetting> customProfileSettings = new List<CustomProfileSetting>();

            foreach (var customProfileCategory in CustomProfileCategories)
            {
                MelonPreferences.CreateCategory(customProfileCategory);
                MelonPreferences.CreateEntry(customProfileCategory, nameof(Profile.UpwardJumpMultiplier), DefaultProfile.UpwardJumpMultiplier);
                MelonPreferences.CreateEntry(customProfileCategory, nameof(Profile.ForwardLeapMultiplier), DefaultProfile.ForwardLeapMultiplier);
                MelonPreferences.CreateEntry(customProfileCategory, nameof(Profile.JumpChargingTime), DefaultProfile.JumpChargingTime);

                var profile = new CustomProfile {Category = customProfileCategory};
                Enum.TryParse(customProfileCategory.Remove(0, "HyperJump".Length), true, out ProfilesEnum customProfileEnum);
                Profiles.Add(customProfileEnum, profile);

                var customProfileSetting = new CustomProfileSetting
                {
                    UpwardJumpMultiplier = new EditableSetting<float> {Initial = profile.UpwardJumpMultiplier, Update = f => profile.UpwardJumpMultiplier = f},
                    ForwardLeapMultiplier = new EditableSetting<float>
                        {Initial = profile.ForwardLeapMultiplier, Update = f => profile.ForwardLeapMultiplier = f},
                    JumpChargingTime = new EditableSetting<float> {Initial = profile.JumpChargingTime, Update = f => profile.JumpChargingTime = f},
                };

                customProfileSettings.Add(customProfileSetting);
            }


            BoneUtils = MelonLoader.InternalUtils.UnityInformationHandler.GameName.Equals("BONEWORKS") ? (IBoneUtils) new BoneworksUtils() : new BonelabUtils();

            EditableSetting<Enum> profileSetting = new EditableSetting<Enum>
                {Initial = ActiveProfile, Update = profileEnum => ActiveProfile = (ProfilesEnum) profileEnum};

            BoneUtils.SetupMenu(profileSetting, customProfileSettings);

            HarmonyMethod jumpChargePostfix = new HarmonyMethod(typeof(HyperJump).GetMethod(nameof(JumpChargePostfix)));
            MethodInfo jumpChargeMethod = BoneUtils.GetJumpChargeMethod();
            HarmonyInstance.Patch(jumpChargeMethod, null, jumpChargePostfix);
        }

        private static DateTime? JumpChargeStartedDate = null;

        public static void JumpChargePostfix(bool chargeInput = true)
        {
            // Skip HyperJump behaviour for the disabled profile
            if (ActiveProfile.Equals(ProfilesEnum.Disabled)) return;
            Profile currentProfile = Profiles[ActiveProfile];

            // chargeInput is true when the jump button is being pressed
            if (chargeInput && !JumpChargeStartedDate.HasValue)
            {
                JumpChargeStartedDate = DateTime.Now;
            }
            else if (!chargeInput && JumpChargeStartedDate.HasValue)
            {
                // When jump button is released, trigger hyper jump then reset jump charging 
                TriggerHyperJump(currentProfile);
                JumpChargeStartedDate = null;
            }
        }

        // UnityObject controllerRig, 
        private static void TriggerHyperJump(Profile profile)
        {
            // Only jump when on the ground
            if (BoneUtils.IsGrounded())
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
                Vector3 pelvisVelocity = BoneUtils.GetPelvisVelocity();
                pelvisVelocity.y = 0;
                float walkSpeed = pelvisVelocity.magnitude;
                Vector3 forwardJump = BoneUtils.GetHeadDirection() * walkSpeed * profile.ForwardLeapMultiplier * 18f;
                Vector3 verticalJump = Vector3.up * profile.UpwardJumpMultiplier * 90f;

                // Apply jump velocity to the player
                BoneUtils.ApplyForceToPelvis((verticalJump + forwardJump) * jumpChargeRatio, ForceMode.VelocityChange);
            }
        }
    }
}