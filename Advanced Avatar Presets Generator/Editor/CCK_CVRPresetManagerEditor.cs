#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using ABI.CCK.Components;
using ABI.CCK.Scripts;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditorInternal;

// Add these aliases to resolve ambiguity
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using AnimatorStateMachine = UnityEditor.Animations.AnimatorStateMachine;
using AnimatorState = UnityEditor.Animations.AnimatorState;
using AnimatorStateTransition = UnityEditor.Animations.AnimatorStateTransition;

namespace ABI.CCK.Components
{
    [CustomEditor(typeof(CVRPresetManager))]
    public class CCK_CVRPresetManagerEditor : Editor
    {
        private CVRPresetManager _presetManager;
        private CVRAvatar _avatar;
        private ReorderableList _presetsList;
        private ReorderableList _parametersList;

        private void OnEnable()
        {
            _presetManager = (CVRPresetManager)target;
            _avatar = _presetManager.GetComponent<CVRAvatar>();
            InitializePresetsList();
            InitializeParametersList();
        }

        private void InitializePresetsList()
        {
            _presetsList = new ReorderableList(_presetManager.presets, typeof(CVRPreset),
                true, true, true, true)
            {
                drawHeaderCallback = (rect) => GUI.Label(rect, "Presets"),
                drawElementCallback = DrawPresetElement,
                elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 2.5f,
                onAddCallback = (list) => _presetManager.presets.Add(new CVRPreset { name = "New Preset" }),
                onRemoveCallback = (list) =>
                {
                    if (list.index >= 0 && list.index < _presetManager.presets.Count)
                        _presetManager.presets.RemoveAt(list.index);
                }
            };
        }

        private void InitializeParametersList()
        {
            _parametersList = new ReorderableList(_presetManager.availableParameters,
                typeof(CVRPresetParameter), true, true, true, true)
            {
                drawHeaderCallback = (rect) => GUI.Label(rect, "Available Parameters"),
                drawElementCallback = DrawParameterElement,
                elementHeightCallback = (index) => EditorGUIUtility.singleLineHeight * 1.5f
            };
        }

        private void DrawPresetElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= _presetManager.presets.Count) return;
            var preset = _presetManager.presets[index];
            rect.y += 2;
            var nameRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            preset.name = EditorGUI.TextField(nameRect, "Preset Name", preset.name);
            rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
            var countRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(countRect, $"Parameters: {preset.parameterValues.Count}");
        }

        private void DrawParameterElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= _presetManager.availableParameters.Count) return;
            var parameter = _presetManager.availableParameters[index];
            rect.y += 2;
            var nameRect = new Rect(rect.x, rect.y, rect.width * 0.7f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(nameRect, parameter.parameterName);
            var typeRect = new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(typeRect, parameter.parameterType.ToString());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("CVR Advanced Avatar Preset Generator must be on the same GameObject as CVRAvatar", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();
            
            // Mode selection toggle
            EditorGUILayout.LabelField("Generation Mode", EditorStyles.boldLabel);
            _presetManager.useStateMachineMode = EditorGUILayout.Toggle("Use State Machine Mode", _presetManager.useStateMachineMode);
            EditorGUILayout.HelpBox(
                _presetManager.useStateMachineMode
                    ? "State Machine Mode: Uses AnimatorDriver behaviour on animator states (cleaner, more efficient, recommended)"
                    : "Component Mode: Uses CVRAnimatorDriver components on GameObjects (compatible with all CCK versions)",
                MessageType.Info
            );
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh Available Parameters"))
                RefreshAvailableParameters();

            EditorGUILayout.Space();
            if (_presetManager.availableParameters.Count > 0)
            {
                _parametersList.DoLayoutList();
                EditorGUILayout.Space();
            }

            _presetsList.DoLayoutList();

            EditorGUILayout.Space();
            if (_presetsList.index >= 0 && _presetsList.index < _presetManager.presets.Count)
                DrawPresetEditor(_presetManager.presets[_presetsList.index]);

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Preset System", GUILayout.Height(30)))
                GeneratePresetSystem();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPresetEditor(CVRPreset preset)
        {
            EditorGUILayout.LabelField($"Edit Preset: {preset.name}", EditorStyles.boldLabel);
            foreach (var availableParam in _presetManager.availableParameters)
            {
                var existingValue = preset.parameterValues.FirstOrDefault(pv => pv.parameterName == availableParam.parameterName);
                if (existingValue == null)
                {
                    existingValue = new CVRPresetParameterValue
                    {
                        parameterName = availableParam.parameterName,
                        parameterType = availableParam.parameterType,
                        floatValue = 0f,
                        update = false
                    };
                    preset.parameterValues.Add(existingValue);
                }

                EditorGUILayout.BeginHorizontal();
                existingValue.update = EditorGUILayout.Toggle(existingValue.update, GUILayout.Width(18));
                EditorGUILayout.LabelField(availableParam.parameterName, GUILayout.Width(140));
                switch (availableParam.parameterType)
                {
                    case CVRAdvancesAvatarSettingBase.ParameterType.Float:
                        existingValue.floatValue = EditorGUILayout.FloatField(existingValue.floatValue);
                        break;
                    case CVRAdvancesAvatarSettingBase.ParameterType.Int:
                        existingValue.intValue = EditorGUILayout.IntField(existingValue.intValue);
                        break;
                    case CVRAdvancesAvatarSettingBase.ParameterType.Bool:
                        existingValue.boolValue = EditorGUILayout.Toggle(existingValue.boolValue);
                        break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RefreshAvailableParameters()
        {
            _presetManager.availableParameters.Clear();
            if (_avatar?.avatarSettings?.settings == null)
                return;

            foreach (var setting in _avatar.avatarSettings.settings)
            {
                if (string.IsNullOrEmpty(setting.machineName))
                    continue;

                switch (setting.type)
                {
                    case CVRAdvancedSettingsEntry.SettingsType.Toggle:
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName,
                            parameterType = setting.setting.usedType
                        });
                        break;
                    case CVRAdvancedSettingsEntry.SettingsType.Slider:
                    case CVRAdvancedSettingsEntry.SettingsType.Dropdown:
                    case CVRAdvancedSettingsEntry.SettingsType.InputSingle:
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName,
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        break;
                    case CVRAdvancedSettingsEntry.SettingsType.Color:
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-r",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-g",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-b",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        break;
                    case CVRAdvancedSettingsEntry.SettingsType.Joystick2D:
                    case CVRAdvancedSettingsEntry.SettingsType.InputVector2:
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-x",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-y",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        break;
                    case CVRAdvancedSettingsEntry.SettingsType.Joystick3D:
                    case CVRAdvancedSettingsEntry.SettingsType.InputVector3:
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-x",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-y",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        _presetManager.availableParameters.Add(new CVRPresetParameter
                        {
                            parameterName = setting.machineName + "-z",
                            parameterType = CVRAdvancesAvatarSettingBase.ParameterType.Float
                        });
                        break;
                }
            }
        }

        private void GeneratePresetSystem()
        {
            // Ensure Default Preset exists
            if (_presetManager.presets.Count == 0 || _presetManager.presets[0].name != "Default Preset")
            {
                var defaultPreset = new CVRPreset { name = "Default Preset" };
                foreach (var param in _presetManager.availableParameters)
                {
                    defaultPreset.parameterValues.Add(new CVRPresetParameterValue
                    {
                        parameterName = param.parameterName,
                        parameterType = param.parameterType,
                        floatValue = 0f,
                        update = false
                    });
                }
                _presetManager.presets.Insert(0, defaultPreset);
            }

            CreatePresetDropdown();

            if (_presetManager.useStateMachineMode)
            {
                CleanupOldDrivers();
                CreateAnimatorLogicWithStateMachineBehaviour();
            }
            else
            {
                CreateAnimatorDrivers();
                CreateAnimatorLogicWithComponents();
            }

            EditorUtility.SetDirty(_presetManager);
            EditorUtility.DisplayDialog("Success", "Preset system generated successfully!", "OK");
            AssetDatabase.SaveAssets();
        }

        private void CreatePresetDropdown()
        {
            var dropdownEntry = new CVRAdvancedSettingsEntry
            {
                name = "Preset Selector",
                machineName = "PresetSelector",
                type = CVRAdvancedSettingsEntry.SettingsType.Dropdown,
                dropDownSettings = new CVRAdvancesAvatarSettingGameObjectDropdown
                {
                    defaultValue = 0,
                    options = new List<CVRAdvancedSettingsDropDownEntry>()
                }
            };

            foreach (var preset in _presetManager.presets)
            {
                dropdownEntry.dropDownSettings.options.Add(new CVRAdvancedSettingsDropDownEntry { name = preset.name });
            }

            // Remove existing preset selector if it exists
            _avatar.avatarSettings.settings.RemoveAll(s => s.machineName == "PresetSelector");
            _avatar.avatarSettings.settings.Insert(0, dropdownEntry);
        }

        private void CleanupOldDrivers()
        {
            // Remove old driver GameObjects
            var drivers = _presetManager.transform.GetComponentsInChildren<CVRAnimatorDriver>(true)
                .Where(d => d.name.StartsWith("PresetDriver_")).ToList();
            foreach (var d in drivers)
                DestroyImmediate(d.gameObject);
        }

        #region Component Mode

        private void CreateAnimatorDrivers()
        {
            CleanupOldDrivers();

            // Create drivers for each preset
            for (int presetIndex = 0; presetIndex < _presetManager.presets.Count; presetIndex++)
            {
                var preset = _presetManager.presets[presetIndex];
                var updatedParams = preset.parameterValues.Where(pv => pv.update).ToList();
                int chunks = Mathf.CeilToInt((float)updatedParams.Count / 16f);

                for (int chunkIndex = 0; chunkIndex < Mathf.Max(1, chunks); chunkIndex++)
                {
                    var chunkParams = updatedParams.Skip(chunkIndex * 16).Take(16).ToList();
                    string chunkSuffix = chunks > 1 ? $"_{chunkIndex:00}" : "";
                    var driverGO = new GameObject($"PresetDriver_{presetIndex:D2}{chunkSuffix}");
                    driverGO.transform.SetParent(_presetManager.transform, false);
                    var driver = driverGO.AddComponent<CVRAnimatorDriver>();

                    // Set the target animator to the avatar's animator
                    var targetAnimator = _avatar.GetComponent<Animator>();
                    for (int i = 0; i < chunkParams.Count && i < 16; i++)
                    {
                        var paramValue = chunkParams[i];
                        driver.animators.Add(targetAnimator);
                        driver.animatorParameters.Add(paramValue.parameterName);
                        driver.animatorParameterType.Add(GetParameterTypeIndex(paramValue.parameterType));
                        SetDriverParameterValue(driver, i, paramValue);
                    }

                    // Enable only Default preset drivers by default
                    driverGO.SetActive(presetIndex == 0);
                }
            }
        }

        private void CreateAnimatorLogicWithComponents()
        {
            if (_avatar?.avatarSettings?.baseController == null)
            {
                EditorUtility.DisplayDialog("Error", "Base controller not set in avatar settings!", "OK");
                return;
            }

            var controller = _avatar.avatarSettings.baseController as AnimatorController;
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Base controller is not an AnimatorController!", "OK");
                return;
            }

            // Add PresetSelector parameter if it doesn't exist
            if (!controller.parameters.Any(p => p.name == "PresetSelector"))
            {
                controller.AddParameter(new UnityEngine.AnimatorControllerParameter
                {
                    name = "PresetSelector",
                    type = UnityEngine.AnimatorControllerParameterType.Int,
                    defaultInt = 0
                });
            }

            // Remove or update preset system layer
            string layerName = "PresetSystem";
            int existingLayerIdx = System.Array.FindIndex(controller.layers, l => l.name == layerName);
            if (existingLayerIdx >= 0)
                controller.RemoveLayer(existingLayerIdx);

            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1f,
                stateMachine = new AnimatorStateMachine()
            };
            layer.stateMachine.name = layerName;
            AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);

            // For each preset, only one state and one transition, and the clip combines all drivers of this preset
            for (int presetIdx = 0; presetIdx < _presetManager.presets.Count; presetIdx++)
            {
                var state = layer.stateMachine.AddState($"Preset_{presetIdx:D2}");
                var clip = CreateCombinedPresetAnimationClip(presetIdx);
                state.motion = clip;

                var transition = layer.stateMachine.AddAnyStateTransition(state);
                transition.AddCondition(AnimatorConditionMode.Equals, presetIdx, "PresetSelector");
                transition.duration = 0f;
                transition.hasExitTime = false;
                transition.canTransitionToSelf = false;
            }

            controller.AddLayer(layer);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        private AnimationClip CreateCombinedPresetAnimationClip(int presetIdx)
        {
            var clip = new AnimationClip();
            clip.name = $"Preset_{presetIdx:D2}";

            // Find all driver GameObjects for this preset
            var driverGOs = new List<Transform>();
            foreach (Transform child in _presetManager.transform)
            {
                if (child.name == $"PresetDriver_{presetIdx:D2}" ||
                    (child.name.StartsWith($"PresetDriver_{presetIdx:D2}_")))
                {
                    driverGOs.Add(child);
                }
            }

            // Animate activation for each driver (only these drivers should be enabled)
            foreach (var driverGO in driverGOs)
            {
                var activeCurve = new AnimationCurve();
                activeCurve.AddKey(0f, 1f);
                activeCurve.AddKey(1f / 60f, 1f);
                clip.SetCurve(AnimationUtility.CalculateTransformPath(driverGO, _avatar.transform),
                    typeof(GameObject), "m_IsActive", activeCurve);

                var driver = driverGO.GetComponent<CVRAnimatorDriver>();
                if (driver == null) continue;

                // Animate all parameter fields for this driver
                for (int paramFieldIdx = 0; paramFieldIdx < 16; paramFieldIdx++)
                {
                    float outVal = 0f;
                    if (paramFieldIdx < driver.animatorParameters.Count)
                    {
                        var paramName = driver.animatorParameters[paramFieldIdx];
                        if (!string.IsNullOrEmpty(paramName))
                        {
                            var paramValue = _presetManager.presets[presetIdx].parameterValues
                                .FirstOrDefault(pv => pv.parameterName == paramName && pv.update);
                            if (paramValue != null)
                            {
                                outVal = paramValue.parameterType switch
                                {
                                    CVRAdvancesAvatarSettingBase.ParameterType.Float => paramValue.floatValue,
                                    CVRAdvancesAvatarSettingBase.ParameterType.Int => paramValue.intValue,
                                    CVRAdvancesAvatarSettingBase.ParameterType.Bool => paramValue.boolValue ? 1f : 0f,
                                    _ => paramValue.floatValue
                                };
                            }
                        }
                    }

                    string floatFieldName = $"animatorParameter{(paramFieldIdx + 1):D2}";
                    var paramCurve = new AnimationCurve();
                    paramCurve.AddKey(0f, outVal);
                    paramCurve.AddKey(1f / 60f, outVal);
                    clip.SetCurve(AnimationUtility.CalculateTransformPath(driverGO, _avatar.transform),
                                 typeof(CVRAnimatorDriver), floatFieldName, paramCurve);
                }
            }

            // Animate all other drivers (not part of this preset) to be inactive
            for (int i = 0; i < _presetManager.presets.Count; i++)
            {
                if (i == presetIdx) continue;
                foreach (Transform child in _presetManager.transform)
                {
                    if (child.name == $"PresetDriver_{i:D2}" ||
                        (child.name.StartsWith($"PresetDriver_{i:D2}_")))
                    {
                        var inactiveCurve = new AnimationCurve();
                        inactiveCurve.AddKey(0f, 0f);
                        inactiveCurve.AddKey(1f / 60f, 0f);
                        clip.SetCurve(AnimationUtility.CalculateTransformPath(child, _avatar.transform),
                            typeof(GameObject), "m_IsActive", inactiveCurve);
                    }
                }
            }

            var clipPath = $"Assets/PresetClip_{presetIdx:D2}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        #endregion

        #region State Machine Behaviour Mode

        private void CreateAnimatorLogicWithStateMachineBehaviour()
        {
            if (_avatar?.avatarSettings?.baseController == null)
            {
                EditorUtility.DisplayDialog("Error", "Base controller not set in avatar settings!", "OK");
                return;
            }

            var controller = _avatar.avatarSettings.baseController as AnimatorController;
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Base controller is not an AnimatorController!", "OK");
                return;
            }

            // Add PresetSelector parameter if it doesn't exist
            if (!controller.parameters.Any(p => p.name == "PresetSelector"))
            {
                controller.AddParameter(new UnityEngine.AnimatorControllerParameter
                {
                    name = "PresetSelector",
                    type = UnityEngine.AnimatorControllerParameterType.Int,
                    defaultInt = 0
                });
            }

            // Remove or update preset system layer
            string layerName = "PresetSystem";
            int existingLayerIdx = System.Array.FindIndex(controller.layers, l => l.name == layerName);
            if (existingLayerIdx >= 0)
                controller.RemoveLayer(existingLayerIdx);

            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1f,
                stateMachine = new AnimatorStateMachine()
            };
            layer.stateMachine.name = layerName;
            AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);

            // Create empty animation clip for states
            var emptyClip = new AnimationClip { name = "Empty" };
            var emptyClipPath = "Assets/PresetEmpty.anim";
            if (!AssetDatabase.LoadAssetAtPath<AnimationClip>(emptyClipPath))
                AssetDatabase.CreateAsset(emptyClip, emptyClipPath);
            else
                emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(emptyClipPath);

            // For each preset, create state(s) with AnimatorDriver behaviour (chunked if needed)
            for (int presetIdx = 0; presetIdx < _presetManager.presets.Count; presetIdx++)
            {
                var preset = _presetManager.presets[presetIdx];
                var updatedParams = preset.parameterValues.Where(pv => pv.update).ToList();

                // Still need to chunk since AnimatorDriverTask list might get long
                // and we want to keep states manageable
                int totalParams = updatedParams.Count;
                int chunksNeeded = Mathf.Max(1, totalParams); // One state per preset, all params in EnterTasks
                
                var state = layer.stateMachine.AddState($"Preset_{presetIdx:D2}");
                state.motion = emptyClip;

                // Add AnimatorDriver StateMachineBehaviour
                var animatorDriver = state.AddStateMachineBehaviour<AnimatorDriver>();

                // Create EnterTasks for all parameters
                foreach (var paramValue in updatedParams)
                {
                    var task = new AnimatorDriverTask
                    {
                        targetName = paramValue.parameterName,
                        targetType = ConvertToAnimatorDriverParameterType(paramValue.parameterType),
                        op = AnimatorDriverTask.Operator.Set,
                        aType = AnimatorDriverTask.SourceType.Static,
                        aValue = GetParameterValueAsFloat(paramValue)
                    };

                    animatorDriver.EnterTasks.Add(task);
                }

                // Add transition
                var transition = layer.stateMachine.AddAnyStateTransition(state);
                transition.AddCondition(AnimatorConditionMode.Equals, presetIdx, "PresetSelector");
                transition.duration = 0f;
                transition.hasExitTime = false;
                transition.canTransitionToSelf = false;
            }

            controller.AddLayer(layer);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        private AnimatorDriverTask.ParameterType ConvertToAnimatorDriverParameterType(CVRAdvancesAvatarSettingBase.ParameterType paramType)
        {
            return paramType switch
            {
                CVRAdvancesAvatarSettingBase.ParameterType.Float => AnimatorDriverTask.ParameterType.Float,
                CVRAdvancesAvatarSettingBase.ParameterType.Int => AnimatorDriverTask.ParameterType.Int,
                CVRAdvancesAvatarSettingBase.ParameterType.Bool => AnimatorDriverTask.ParameterType.Bool,
                _ => AnimatorDriverTask.ParameterType.Float
            };
        }

        #endregion

        #region Helper Methods

        private int GetParameterTypeIndex(CVRAdvancesAvatarSettingBase.ParameterType paramType)
        {
            return paramType switch
            {
                CVRAdvancesAvatarSettingBase.ParameterType.Float => 0,
                CVRAdvancesAvatarSettingBase.ParameterType.Int => 1,
                CVRAdvancesAvatarSettingBase.ParameterType.Bool => 2,
                _ => 0
            };
        }

        private void SetDriverParameterValue(CVRAnimatorDriver driver, int index, CVRPresetParameterValue paramValue)
        {
            float value = GetParameterValueAsFloat(paramValue);
            switch (index)
            {
                case 0: driver.animatorParameter01 = value; break;
                case 1: driver.animatorParameter02 = value; break;
                case 2: driver.animatorParameter03 = value; break;
                case 3: driver.animatorParameter04 = value; break;
                case 4: driver.animatorParameter05 = value; break;
                case 5: driver.animatorParameter06 = value; break;
                case 6: driver.animatorParameter07 = value; break;
                case 7: driver.animatorParameter08 = value; break;
                case 8: driver.animatorParameter09 = value; break;
                case 9: driver.animatorParameter10 = value; break;
                case 10: driver.animatorParameter11 = value; break;
                case 11: driver.animatorParameter12 = value; break;
                case 12: driver.animatorParameter13 = value; break;
                case 13: driver.animatorParameter14 = value; break;
                case 14: driver.animatorParameter15 = value; break;
                case 15: driver.animatorParameter16 = value; break;
            }
        }

        private float GetParameterValueAsFloat(CVRPresetParameterValue paramValue)
        {
            return paramValue.parameterType switch
            {
                CVRAdvancesAvatarSettingBase.ParameterType.Float => paramValue.floatValue,
                CVRAdvancesAvatarSettingBase.ParameterType.Int => paramValue.intValue,
                CVRAdvancesAvatarSettingBase.ParameterType.Bool => paramValue.boolValue ? 1f : 0f,
                _ => paramValue.floatValue,
            };
        }

        #endregion
    }
}
#endif
