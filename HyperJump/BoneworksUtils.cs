extern alias boneworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using boneworks::StressLevelZero.Rig;
using boneworks::StressLevelZero.VRMK;
using ModThatIsNotMod.BoneMenu;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Evanaellio.HyperJump
{
    public class BoneworksUtils : IBoneUtils
    {
        public PhysicsRig PhysicsRig => UnityObject.FindObjectOfType<PhysicsRig>();
        public ControllerRig ControllerRig => UnityObject.FindObjectOfType<ControllerRig>();
        public PhysGrounder PhysGrounder => UnityObject.FindObjectOfType<PhysGrounder>();

        public MethodInfo GetControllerRigMethod(string methodName)
        {
            return typeof(ControllerRig).GetMethod(methodName);
        }

        public Vector3 GetPelvisVelocity()
        {
            return PhysicsRig.pelvisVelocity;
        }

        public Vector3 GetHeadDirection()
        {
            return ControllerRig.m_head.forward;
        }

        public void ApplyForceToPelvis(Vector3 force, ForceMode velocityChange)
        {
            // Reduce force strength for Boneworks to account for different physics
            PhysicsRig.physBody.rbPelvis.AddForce(force * 0.34f, velocityChange);
        }

        public bool IsGrounded()
        {
            return PhysGrounder.isGrounded;
        }

        public void SetupMenu(EditableSetting<Enum> profile, List<CustomProfileSetting> customProfileSettings)
        {
            MenuCategory hyperjumpCategory = MenuManager.CreateCategory("HyperJump", Color.white);
            hyperjumpCategory.CreateEnumElement("Profile", Color.white, profile.Initial, profile.Update);

            var subcategories = new List<MenuCategory>
            {
                hyperjumpCategory.CreateSubCategory("Custom A", new Color(0.906f, 0.298f, 0.235f)),
                hyperjumpCategory.CreateSubCategory("Custom B", new Color(0.18f, 0.8f, 0.443f)),
                hyperjumpCategory.CreateSubCategory("Custom C", new Color(0.204f, 0.596f, 0.859f))
            };

            for (var i = 0; i < subcategories.Count; i++)
            {
                var subcategory = subcategories[i];
                var custom = customProfileSettings[i];

                subcategory.CreateFloatElement("Upward jump multiplier", subcategory.color,
                    custom.UpwardJumpMultiplier.Initial, custom.UpwardJumpMultiplier.Update, 0.1f, 0f, 5f, true);
                subcategory.CreateFloatElement("Forward leap multiplier", subcategory.color,
                    custom.ForwardLeapMultiplier.Initial, custom.ForwardLeapMultiplier.Update, 0.1f, 0f, 5f, true);
                subcategory.CreateFloatElement("Jump charge time (seconds)", subcategory.color,
                    custom.JumpChargingTime.Initial, custom.JumpChargingTime.Update, 0.1f, 0f, 5f, true);
            }
        }
    }
}