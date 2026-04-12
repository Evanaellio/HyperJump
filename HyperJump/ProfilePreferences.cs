using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using Enum = System.Enum;
using BoneLib.BoneMenu;

namespace Evanaellio.HyperJump;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static class ProfilePreferences
{
    private const string HyperJumpCategory = nameof(HyperJump);

    public class EditableSetting<T>
    {
        public T Initial;
        public Action<T> Update;
    }

    private static readonly string[] CustomProfileCategories =
    {
        "HyperJumpCustomA", "HyperJumpCustomB", "HyperJumpCustomC"
    };

    public class CustomProfileSetting
    {
        public EditableSetting<float> UpwardJumpMultiplier;
        public EditableSetting<float> ForwardLeapMultiplier;
        public EditableSetting<float> JumpChargingTime;
    }

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

    public static readonly Dictionary<ProfilesEnum, Profile> Profiles = new()
    {
        {ProfilesEnum.Default, DefaultProfile},
        {ProfilesEnum.Instant, InstantProfile},
    };


    // ReSharper disable UnusedMember.Global
    public enum ProfilesEnum
    {
        Default,
        Instant,
        Disabled,
        CustomA,
        CustomB,
        CustomC
    }
    // ReSharper restore UnusedMember.Global

    public static void InitializePreferencesAndMenu()
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

        // Create BoneLib menus 
        EditableSetting<Enum> profileSetting = new EditableSetting<Enum>
            {Initial = ActiveProfile, Update = profileEnum => ActiveProfile = (ProfilesEnum) profileEnum};

        Page hyperjumpPage = Page.Root.CreatePage("HyperJump", Color.white);
        hyperjumpPage.CreateEnum("Profile", Color.white, profileSetting.Initial, profileSetting.Update);

        var subpages = new List<Page>
        {
            hyperjumpPage.CreatePage("Custom A", new Color(0.906f, 0.298f, 0.235f)),
            hyperjumpPage.CreatePage("Custom B", new Color(0.18f, 0.8f, 0.443f)),
            hyperjumpPage.CreatePage("Custom C", new Color(0.204f, 0.596f, 0.859f))
        };

        for (var i = 0; i < subpages.Count; i++)
        {
            var category = subpages[i];
            var custom = customProfileSettings[i];

            category.CreateFloat("Upward jump multiplier", category.Color,
                custom.UpwardJumpMultiplier.Initial, 0.2f, 0f, 5f, custom.UpwardJumpMultiplier.Update);
            category.CreateFloat("Forward leap multiplier", category.Color,
                custom.ForwardLeapMultiplier.Initial, 0.2f, 0f, 5f, custom.ForwardLeapMultiplier.Update);
            category.CreateFloat("Jump charge time (seconds)", category.Color,
                custom.JumpChargingTime.Initial, 0.2f, 0f, 5f, custom.JumpChargingTime.Update);
        }
    }
}