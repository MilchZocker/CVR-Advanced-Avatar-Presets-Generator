using System.Collections.Generic;
using UnityEngine;
using ABI.CCK.Scripts;

namespace ABI.CCK.Components
{
    [System.Serializable]
    public class CVRPresetParameter
    {
        public string parameterName;
        public CVRAdvancesAvatarSettingBase.ParameterType parameterType;
    }

    [System.Serializable]
    public class CVRPresetParameterValue
    {
        public string parameterName;
        public CVRAdvancesAvatarSettingBase.ParameterType parameterType;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public bool update = false;
    }

    [System.Serializable]
    public class CVRPreset
    {
        public string name = "New Preset";
        public List<CVRPresetParameterValue> parameterValues = new List<CVRPresetParameterValue>();
    }

    [AddComponentMenu("ChilloutVR/CVR Preset Manager")]
    public class CVRPresetManager : MonoBehaviour
    {
        [Header("Preset Configuration")]
        public List<CVRPreset> presets = new List<CVRPreset>();
        public List<CVRPresetParameter> availableParameters = new List<CVRPresetParameter>();
        
        [Header("Generation Settings")]
        public bool useStateMachineMode = false;
        
        [Header("Runtime Settings")]
        public string dropdownParameterName = "PresetSelector";

        // Runtime variables
        private Dictionary<int, List<GameObject>> presetDriversByIndex = new Dictionary<int, List<GameObject>>();
        private int lastValue = -999;
        private CVRAvatar avatar;

        void Start()
        {
            // Only needed for component mode
            if (!useStateMachineMode)
            {
                avatar = GetComponent<CVRAvatar>();
                GatherPresetDrivers();
                UpdatePresetDrivers();
            }
        }

        void Update()
        {
            // Only needed for component mode
            if (!useStateMachineMode)
            {
                UpdatePresetDrivers();
            }
        }

        void GatherPresetDrivers()
        {
            presetDriversByIndex.Clear();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("PresetDriver_"))
                {
                    string[] parts = child.name.Replace("PresetDriver_", "").Split('_');
                    if (int.TryParse(parts[0], out int presetIdx))
                    {
                        if (!presetDriversByIndex.ContainsKey(presetIdx))
                            presetDriversByIndex.Add(presetIdx, new List<GameObject>());
                        presetDriversByIndex[presetIdx].Add(child.gameObject);
                    }
                }
            }
        }

        void UpdatePresetDrivers()
        {
            if (avatar == null || avatar.GetComponent<Animator>() == null)
                return;

            var anim = avatar.GetComponent<Animator>();
            int presetIndex = 0;
            if (anim.HasParameterOfType(dropdownParameterName, AnimatorControllerParameterType.Int))
                presetIndex = anim.GetInteger(dropdownParameterName);

            if (presetIndex == lastValue)
                return;

            foreach (var kvp in presetDriversByIndex)
                foreach (var go in kvp.Value)
                    go.SetActive(kvp.Key == presetIndex);

            lastValue = presetIndex;
        }
    }

    public static class AnimatorExtension
    {
        public static bool HasParameterOfType(this Animator self, string paramName, AnimatorControllerParameterType type)
        {
            foreach (var param in self.parameters)
                if (param.type == type && param.name == paramName) return true;
            return false;
        }
    }
}
