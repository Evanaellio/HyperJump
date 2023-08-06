using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Evanaellio.HyperJump
{
    public class EditableSetting<T>
    {
        public T Initial;
        public Action<T> Update;
    }

    public class CustomProfileSetting
    {
        public EditableSetting<float> UpwardJumpMultiplier;
        public EditableSetting<float> ForwardLeapMultiplier;
        public EditableSetting<float> JumpChargingTime;
    }

    public interface IBoneUtils
    {
        MethodInfo GetControllerRigMethod(string methodName);
        Vector3 GetPelvisVelocity();
        Vector3 GetHeadDirection();
        void ApplyForceToPelvis(Vector3 force, ForceMode velocityChange);
        bool IsGrounded();
        void SetupMenu(EditableSetting<Enum> profile, List<CustomProfileSetting> customProfileSettings);
    }
}