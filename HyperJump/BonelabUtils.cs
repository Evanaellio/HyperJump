extern alias bonelab;
using System;
using System.Collections.Generic;
using System.Reflection;
using bonelab::SLZ.Rig;
using bonelab::SLZ.VRMK;
using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Evanaellio.HyperJump
{
    public class BonelabUtils : IBoneUtils
    {
        public PhysicsRig PhysicsRig => Player.GetPhysicsRig();
        public ControllerRig ControllerRig => Player.controllerRig;
        public PhysGrounder PhysGrounder => Player.physicsRig.physG;

        public MethodInfo GetJumpChargeMethod()
        {
            return typeof(RemapRig).GetMethod("JumpCharge");
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
            PhysicsRig.torso.rbPelvis.AddForce(force, velocityChange);
        }

        public bool IsGrounded()
        {
            return PhysGrounder.isGrounded;
        }

        public void SetupMenu(EditableSetting<Enum> profile, List<CustomProfileSetting> customProfileSettings)
        {
            MenuCategory hyperjumpCategory = MenuManager.CreateCategory("HyperJump", Color.white);
            hyperjumpCategory.CreateEnumElement("Profile", Color.white, profile.Initial, profile.Update);

            var subpanels = new List<SubPanelElement>
            {
                hyperjumpCategory.CreateSubPanel("Custom A", new Color(0.906f, 0.298f, 0.235f)),
                hyperjumpCategory.CreateSubPanel("Custom B", new Color(0.18f, 0.8f, 0.443f)),
                hyperjumpCategory.CreateSubPanel("Custom C", new Color(0.204f, 0.596f, 0.859f))
            };

            for (var i = 0; i < subpanels.Count; i++)
            {
                var panel = subpanels[i];
                var custom = customProfileSettings[i];

                panel.CreateFloatElement("Upward jump multiplier", panel.Color,
                    custom.UpwardJumpMultiplier.Initial, 0.1f, 0f, 5f, custom.UpwardJumpMultiplier.Update);
                panel.CreateFloatElement("Forward leap multiplier", panel.Color,
                    custom.ForwardLeapMultiplier.Initial, 0.1f, 0f, 5f, custom.ForwardLeapMultiplier.Update);
                panel.CreateFloatElement("Jump charge time (seconds)", panel.Color,
                    custom.JumpChargingTime.Initial, 0.1f, 0f, 5f, custom.JumpChargingTime.Update);
            }
        }
    }
}