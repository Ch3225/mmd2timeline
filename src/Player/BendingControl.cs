using MacGruber;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    internal class BendingControl : BaseScript
    {
        readonly List<object> _rootElements = new List<object>();
        readonly List<object> _sliderElements = new List<object>();
        readonly List<UIDynamicButton> _bodyPartButtons = new List<UIDynamicButton>();

        string _selectedBodyPart = string.Empty;
        bool _lastEnableState;
        bool _uiReady;
        bool _scriptInitialized;
        UIDynamicTextInfo _rightTitle;
        UIDynamicTextInfo _rightHint;
        
        // 总控制相关
        JSONStorableFloat _totalMultiplier;
        readonly Dictionary<string, List<JSONStorableFloat>> _bodyPartSliders = new Dictionary<string, List<JSONStorableFloat>>();

        public override bool ShouldIgnore()
        {
            return false;
        }

        public void Start()
        {
            try
            {
                InitScript();
                _scriptInitialized = true;
                _lastEnableState = config.EnableBending;
                RefreshBendingControlUI();
                _uiReady = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "BendingControl::Start");
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (_scriptInitialized && !_uiReady)
            {
                try
                {
                    RefreshBendingControlUI();
                    _uiReady = true;
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex, "BendingControl::OnEnable");
                }
            }
        }

        public void Update()
        {
            if (!_scriptInitialized || !_uiReady)
            {
                return;
            }

            if (config.EnableBending != _lastEnableState)
            {
                _lastEnableState = config.EnableBending;
                RefreshBendingControlUI();
            }
        }

        public override void OnDisable()
        {
            ClearAllUI();
            _uiReady = false;
            base.OnDisable();
        }

        public override void OnDestroy()
        {
            ClearAllUI();
            _selectedBodyPart = string.Empty;
            _uiReady = false;
            base.OnDestroy();
        }

        void RefreshBendingControlUI()
        {
            string previousSelection = _selectedBodyPart;

            _lastEnableState = config.EnableBending;

            ClearAllUI();

            _selectedBodyPart = previousSelection;

            CreateHeader();

            if (!config.EnableBending)
            {
                CreateDisabledMessage();
                return;
            }

            CreateBodyPartButtons();
            CreateSliderArea();

            if (!string.IsNullOrEmpty(previousSelection))
            {
                OnBodyPartSelected(previousSelection);
            }
            else
            {
                ResetRightPanelLabels();
            }
        }

        void CreateHeader()
        {
            var title = Utils.SetupInfoOneLine(this, "Bending Control", LeftSide);
            _rootElements.Add(title);

            var spacer = Utils.SetupSpacer(this, 10f, LeftSide);
            _rootElements.Add(spacer);
        }

        void CreateDisabledMessage()
        {
            var infoLeft = Utils.SetupInfoTextNoScroll(this, "Enable Bending Control in Settings to activate this page.", 30f, LeftSide);
            _rootElements.Add(infoLeft);

            var infoRight = Utils.SetupInfoTextNoScroll(this, "Open the Settings tab and enable \"Enable Bending Control\".", 30f, RightSide);
            _rootElements.Add(infoRight);
        }

        void CreateBodyPartButtons()
        {
            string[] bodyParts =
            {
                "Jaw", "Head", "Tongue",
                "Arm", "Collar", "Collar", "Arm",
                "Elbow", "Spine", "Elbow",
                "Chest", "Abd L", "Abd H", "Pelvis",
                "Hand", "Thigh", "Thigh", "Hand",
                "", "Penis", "Fingers", "Fingers",
                "", "Knee", "Knee", "",
                "Toe", "Foot", "Foot", "Toe",
                "", "Toes", "Toes", ""
            };

            foreach (string part in bodyParts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    var spacer = Utils.SetupSpacer(this, 25f, LeftSide);
                    _rootElements.Add(spacer);
                    continue;
                }

                var button = Utils.SetupButton(this, part, () => OnBodyPartSelected(part), LeftSide);
                button.height = 40f;
                button.buttonColor = part == _selectedBodyPart ? Color.green : Color.white;

                _bodyPartButtons.Add(button);
                _rootElements.Add(button);
            }
        }

        void CreateSliderArea()
        {
            // 创建总控制滑块（在右上角）
            CreateTotalControlSlider();
            
            _rightTitle = Utils.SetupInfoTextNoScroll(this, "Bend Controls (Select a body part)", 30f, RightSide);
            _rootElements.Add(_rightTitle);

            var spacer = Utils.SetupSpacer(this, 10f, RightSide);
            _rootElements.Add(spacer);

            _rightHint = Utils.SetupInfoTextNoScroll(this, "请选择左侧的身体部位", 30f, RightSide);
            _rootElements.Add(_rightHint);

            ResetRightPanelLabels(_selectedBodyPart);
        }
        
        void CreateTotalControlSlider()
        {
            // 总控制标题
            var totalTitle = Utils.SetupInfoOneLine(this, "总控制 (Total Control)", RightSide);
            _rootElements.Add(totalTitle);
            
            // 总控制倍数滑块 (最大15倍)
            _totalMultiplier = Utils.SetupSliderFloat(this, "Total Multiplier", 1.0f, 0.0f, 15.0f, RightSide, "F1");
            _totalMultiplier.setCallbackFunction += OnTotalMultiplierChanged;
            RegisterFloat(_totalMultiplier);
            _rootElements.Add(_totalMultiplier);
            
            // 分隔符
            var spacer = Utils.SetupSpacer(this, 15f, RightSide);
            _rootElements.Add(spacer);
        }
        
        void OnTotalMultiplierChanged(float multiplier)
        {
            // 应用总倍数到当前显示的滑块
            if (!string.IsNullOrEmpty(_selectedBodyPart) && _bodyPartSliders.ContainsKey(_selectedBodyPart))
            {
                ApplyTotalMultiplier(multiplier);
            }
        }
        
        void ApplyTotalMultiplier(float totalMultiplier)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedBodyPart) || !_bodyPartSliders.ContainsKey(_selectedBodyPart))
                    return;

                var sliders = _bodyPartSliders[_selectedBodyPart];
                
                foreach (var slider in sliders)
                {
                    if (slider.name.Contains("Bend"))
                    {
                        // 对Bend滑块应用倍数 (保持在范围内)
                        var baseValue = slider.val / (totalMultiplier == 0 ? 1 : Math.Max(0.1f, totalMultiplier));
                        var newValue = Mathf.Clamp(baseValue * totalMultiplier, slider.min, slider.max);
                        
                        // 使用基于插件的关节控制逻辑（预留接口）
                        ApplyJointControl(_selectedBodyPart, "Bend", newValue, totalMultiplier);
                    }
                    else if (slider.name.Contains("Strength"))
                    {
                        // 对Strength滑块应用倍数
                        var baseValue = slider.val / (totalMultiplier == 0 ? 1 : Math.Max(0.1f, totalMultiplier));
                        var newValue = Mathf.Clamp(baseValue * totalMultiplier, slider.min, slider.max);
                        
                        // 使用基于插件的关节控制逻辑（预留接口）
                        ApplyJointControl(_selectedBodyPart, "Strength", newValue, totalMultiplier);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "ApplyTotalMultiplier");
            }
        }
        
        // 基于插件架构的关节控制方法（重载版本支持特定映射）
        void ApplyJointControl(string bodyPart, string controlType, float value, float multiplier, JointMapping specificMapping = null)
        {
            try
            {
                // 如果提供了特定映射，使用它；否则获取默认映射
                var jointMappings = specificMapping != null 
                    ? new List<JointMapping> { specificMapping } 
                    : GetJointMappings(bodyPart);
                    
                if (jointMappings == null || jointMappings.Count == 0)
                {
                    LogUtil.Log($"No joint mapping found for body part: {bodyPart}");
                    return;
                }

                var persons = GetTargetPersonAtoms().ToList();
                if (persons.Count == 0)
                {
                    LogUtil.LogWarning("BendingControl could not find any Person atoms in the scene; skipping control application.");
                    return;
                }

                foreach (var person in persons)
                {
                    if (person == null || person.type != "Person")
                    {
                        continue;
                    }

                    var allJoints = GetAllConfigurableJointsLocal(person);
                    if (allJoints == null)
                    {
                        LogUtil.LogWarning($"BendingControl: failed to get configurable joints for {person.uid}.");
                        continue;
                    }

                    foreach (var jointMapping in jointMappings)
                    {
                        var controllerName = GetControllerName(jointMapping.jointKey);
                        if (!string.IsNullOrEmpty(controllerName))
                        {
                            var controller = person.GetStorableByID(controllerName) as FreeControllerV3;
                            if (controller != null)
                            {
                                if (controlType == "Bend" || controlType == "AxisBend")
                                {
                                    ApplyBendingControl(controller, jointMapping, value * multiplier);
                                }
                                else if (controlType == "Strength")
                                {
                                    ApplyStrengthControl(controller, jointMapping, value * multiplier);
                                }
                            }
                        }

                        if (allJoints.ContainsKey(jointMapping.jointKey))
                        {
                            var joint = allJoints[jointMapping.jointKey];
                            if (controlType == "Bend" || controlType == "AxisBend")
                            {
                                ApplyJointBending(joint, jointMapping, value * multiplier);
                            }
                            else if (controlType == "Strength")
                            {
                                ApplyJointStrength(joint, value * multiplier);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyJointControl: {bodyPart} {controlType}");
            }
        }
        
        // 基于插件架构的关节控制方法（原始版本）
        void ApplyJointControl(string bodyPart, string controlType, float value, float multiplier)
        {
            ApplyJointControl(bodyPart, controlType, value, multiplier, null);
        }

        void ApplyBendingControl(FreeControllerV3 controller, JointMapping mapping, float bendValue)
        {
            try
            {
                // 基于插件的旋转目标控制
                var clampedValue = Mathf.Clamp(bendValue, mapping.minBend, mapping.maxBend);
                
                // 应用到主要轴向（基于插件的轴向映射）
                switch (mapping.primaryAxis)
                {
                    case "X":
                        controller.jointRotationDriveXTarget = clampedValue * mapping.multiplier;
                        break;
                    case "Y":
                        controller.jointRotationDriveYTarget = clampedValue * mapping.multiplier;
                        break;
                    case "Z":
                        controller.jointRotationDriveZTarget = clampedValue * mapping.multiplier;
                        break;
                }

                LogUtil.Log($"Applied bending {bendValue} to {controller.name} on axis {mapping.primaryAxis}");
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyBendingControl: {controller.name}");
            }
        }

        void ApplyStrengthControl(FreeControllerV3 controller, JointMapping mapping, float strengthValue)
        {
            try
            {
                // 基于插件的关节强度控制
                var clampedStrength = Mathf.Clamp(strengthValue, 0f, 500f);
                
                controller.jointRotationDriveSpring = clampedStrength;
                controller.jointRotationDriveMaxForce = clampedStrength;
                
                LogUtil.Log($"Applied strength {strengthValue} to {controller.name}");
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyStrengthControl: {controller.name}");
            }
        }

        void ApplyJointBending(ConfigurableJoint joint, JointMapping mapping, float bendValue)
        {
            try
            {
                // 直接关节旋转控制
                var targetRotation = Quaternion.Euler(
                    mapping.primaryAxis == "X" ? bendValue * mapping.multiplier : 0f,
                    mapping.primaryAxis == "Y" ? bendValue * mapping.multiplier : 0f,
                    mapping.primaryAxis == "Z" ? bendValue * mapping.multiplier : 0f
                );
                
                joint.targetRotation = targetRotation;
                
                LogUtil.Log($"Applied joint bending {bendValue} to {joint.name}");
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyJointBending: {joint.name}");
            }
        }

        void ApplyJointStrength(ConfigurableJoint joint, float strengthValue)
        {
            try
            {
                // 调整关节弹簧强度
                var clampedStrength = Mathf.Clamp(strengthValue, 0f, 100f);
                
                var slerpDrive = joint.slerpDrive;
                slerpDrive.positionSpring = clampedStrength;
                slerpDrive.maximumForce = clampedStrength;
                joint.slerpDrive = slerpDrive;
                
                LogUtil.Log($"Applied joint strength {strengthValue} to {joint.name}");
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyJointStrength: {joint.name}");
            }
        }

        IEnumerable<Atom> GetTargetPersonAtoms()
        {
            var visited = new HashSet<Atom>();

            if (containingAtom != null && containingAtom.type == "Person")
            {
                visited.Add(containingAtom);
                yield return containingAtom;
            }

            MotionHelperGroup motionHelperGroup = null;
            try
            {
                motionHelperGroup = MotionHelperGroup.GetInstance();
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, "BendingControl: unable to access MotionHelperGroup");
            }

            if (motionHelperGroup != null)
            {
                foreach (var helper in motionHelperGroup.Helpers)
                {
                    var person = helper?.PersonAtom;
                    if (person != null && person.type == "Person" && visited.Add(person))
                    {
                        yield return person;
                    }
                }
            }

            if (visited.Count == 0)
            {
                foreach (var atom in SuperController.singleton.GetAtoms())
                {
                    if (atom != null && atom.type == "Person" && visited.Add(atom))
                    {
                        yield return atom;
                    }
                }
            }
        }

        // 关节映射类
        class JointMapping
        {
            public string jointKey;
            public string primaryAxis;
            public float multiplier = 1.0f;
            public float minBend = -100f;
            public float maxBend = 100f;
        }

        // 返回一个或多个关节映射（支持对称部位）
        List<JointMapping> GetJointMappings(string bodyPart)
        {
            var list = new List<JointMapping>();
            switch (bodyPart.ToLower())
            {
                case "jaw":
                case "head":
                    list.Add(new JointMapping { jointKey = "head", primaryAxis = "X", multiplier = 0.5f, minBend = -45f, maxBend = 35f });
                    break;
                case "tongue":
                    list.Add(new JointMapping { jointKey = "tongueBase", primaryAxis = "X", multiplier = 0.4f, minBend = -30f, maxBend = 30f });
                    break;
                case "neck":
                    list.Add(new JointMapping { jointKey = "neck", primaryAxis = "X", multiplier = 0.5f, minBend = -35f, maxBend = 35f });
                    break;
                case "chest":
                    list.Add(new JointMapping { jointKey = "chest", primaryAxis = "X", multiplier = 0.5f, minBend = -50f, maxBend = 50f });
                    break;
                case "abd l":
                case "abd h":
                case "abdomen":
                    list.Add(new JointMapping { jointKey = "abdomen", primaryAxis = "X", multiplier = 0.3f, minBend = -30f, maxBend = 20f });
                    break;
                case "pelvis":
                    list.Add(new JointMapping { jointKey = "pelvis", primaryAxis = "X", multiplier = 0.3f, minBend = -30f, maxBend = 15f });
                    break;
                case "arm":
                    list.Add(new JointMapping { jointKey = "lShldr", primaryAxis = "X", multiplier = 0.75f, minBend = -75f, maxBend = 30f });
                    list.Add(new JointMapping { jointKey = "rShldr", primaryAxis = "X", multiplier = -0.75f, minBend = -75f, maxBend = 30f });
                    break;
                case "elbow":
                    list.Add(new JointMapping { jointKey = "lForeArm", primaryAxis = "X", multiplier = 1.3f, minBend = -130f, maxBend = 20f });
                    list.Add(new JointMapping { jointKey = "rForeArm", primaryAxis = "X", multiplier = -1.3f, minBend = -130f, maxBend = 20f });
                    break;
                case "hand":
                    list.Add(new JointMapping { jointKey = "lHand", primaryAxis = "X", multiplier = 0.8f, minBend = -80f, maxBend = 80f });
                    list.Add(new JointMapping { jointKey = "rHand", primaryAxis = "X", multiplier = -0.8f, minBend = -80f, maxBend = 80f });
                    break;
                case "thigh":
                    list.Add(new JointMapping { jointKey = "lThigh", primaryAxis = "X", multiplier = 1.0f, minBend = -25f, maxBend = 100f });
                    list.Add(new JointMapping { jointKey = "rThigh", primaryAxis = "X", multiplier = 1.0f, minBend = -25f, maxBend = 100f });
                    break;
                case "knee":
                    list.Add(new JointMapping { jointKey = "lShin", primaryAxis = "X", multiplier = 1.5f, minBend = -150f, maxBend = 11f });
                    list.Add(new JointMapping { jointKey = "rShin", primaryAxis = "X", multiplier = 1.5f, minBend = -150f, maxBend = 11f });
                    break;
                case "foot":
                    list.Add(new JointMapping { jointKey = "lFoot", primaryAxis = "X", multiplier = 0.65f, minBend = -65f, maxBend = 40f });
                    list.Add(new JointMapping { jointKey = "rFoot", primaryAxis = "X", multiplier = 0.65f, minBend = -65f, maxBend = 40f });
                    break;
                case "toe":
                case "toes":
                    list.Add(new JointMapping { jointKey = "lToe", primaryAxis = "X", multiplier = 0.75f, minBend = -65f, maxBend = 75f });
                    list.Add(new JointMapping { jointKey = "rToe", primaryAxis = "X", multiplier = 0.75f, minBend = -65f, maxBend = 75f });
                    break;
                default:
                    // 尝试直接匹配常见键
                    var direct = GetJointMapping(bodyPart);
                    if (direct != null) list.Add(direct);
                    break;
            }

            return list;
        }

        // 保留的单一映射方法（向后兼容/辅助）
        JointMapping GetJointMapping(string bodyPart)
        {
            switch (bodyPart.ToLower())
            {
                case "head":
                    return new JointMapping { jointKey = "head", primaryAxis = "X", multiplier = 0.5f, minBend = -45f, maxBend = 35f };
                case "neck":
                    return new JointMapping { jointKey = "neck", primaryAxis = "X", multiplier = 0.5f, minBend = -35f, maxBend = 35f };
                case "chest":
                    return new JointMapping { jointKey = "chest", primaryAxis = "X", multiplier = 0.5f, minBend = -50f, maxBend = 50f };
                case "abdomen":
                    return new JointMapping { jointKey = "abdomen", primaryAxis = "X", multiplier = 0.3f, minBend = -30f, maxBend = 20f };
                case "pelvis":
                    return new JointMapping { jointKey = "pelvis", primaryAxis = "X", multiplier = 0.3f, minBend = -30f, maxBend = 15f };
                default:
                    return null;
            }
        }

        // 基于插件的控制器名称映射
        string GetControllerName(string jointKey)
        {
            // 基于J2C.Joint2ctrls的映射
            var joint2Controllers = new Dictionary<string, string>
            {
                {"head", "headControl"},
                {"neck", "neckControl"},
                {"abdomen", "abdomenControl"},
                {"abdomen2", "abdomen2Control"},
                {"pelvis", "pelvisControl"},
                {"lThigh", "lThighControl"},
                {"rThigh", "rThighControl"},
                {"lShin", "lKneeControl"},
                {"rShin", "rKneeControl"},
                {"lFoot", "lFootControl"},
                {"rFoot", "rFootControl"},
                {"lCollar", "lShoulderControl"},
                {"rCollar", "rShoulderControl"},
                {"lShldr", "lArmControl"},
                {"rShldr", "rArmControl"},
                {"lForeArm", "lElbowControl"},
                {"rForeArm", "rElbowControl"},
                {"lHand", "lHandControl"},
                {"rHand", "rHandControl"},
                {"chest", "chestControl"},
                {"lToe", "lToeControl"},
                {"rToe", "rToeControl"}
            };

            return joint2Controllers.ContainsKey(jointKey) ? joint2Controllers[jointKey] : null;
        }

        void OnBodyPartSelected(string bodyPart)
        {
            _selectedBodyPart = bodyPart;

            foreach (var button in _bodyPartButtons)
            {
                button.buttonColor = button.label == bodyPart ? Color.green : Color.white;
            }

            CreateSlidersForBodyPart(bodyPart);
        }

        void CreateSlidersForBodyPart(string bodyPart)
        {
            ResetRightPanelLabels(bodyPart);

            ClearCurrentSliders();

            if (string.IsNullOrEmpty(bodyPart))
            {
                return;
            }

            // 尝试使用参考插件的多轴控制
            var referenceAxes = GetReferencePluginAxes(bodyPart);
            
            // 创建该身体部位的滑块列表
            var sliders = new List<JSONStorableFloat>();

            if (referenceAxes != null && referenceAxes.Count > 0)
            {
                // 使用参考插件的多轴控制
                foreach (var axis in referenceAxes)
                {
                    var slider = Utils.SetupSliderFloat(this, 
                        $"{bodyPart} {axis.Label}", 
                        axis.Default, 
                        axis.Min, 
                        axis.Max, 
                        RightSide, 
                        "F1");
                        
                    slider.setCallbackFunction += (value) => OnAxisSliderChanged(bodyPart, axis.Label, axis.Axis, value);
                    RegisterFloat(slider);
                    _sliderElements.Add(slider);
                    sliders.Add(slider);
                    
                    // 在每个滑块后添加小间距
                    var spacer = Utils.SetupSpacer(this, 5f, RightSide);
                    _sliderElements.Add(spacer);
                }
            }
            else
            {
                // 回退到简单的Bend控制（已知工作）
                var bendSlider = Utils.SetupSliderFloat(this, $"{bodyPart} Bend", 0f, -100f, 100f, RightSide, "F1");
                bendSlider.setCallbackFunction += (value) => OnSliderChanged(bodyPart, "Bend", value);
                RegisterFloat(bendSlider);
                _sliderElements.Add(bendSlider);
                sliders.Add(bendSlider);
            }
            
            // 添加强度控制滑块（通用且已知工作）
            var strengthSlider = Utils.SetupSliderFloat(this, $"{bodyPart} Strength", 50f, 0f, 100f, RightSide, "F1");
            strengthSlider.setCallbackFunction += (value) => OnSliderChanged(bodyPart, "Strength", value);
            RegisterFloat(strengthSlider);
            _sliderElements.Add(strengthSlider);
            sliders.Add(strengthSlider);

            var finalSpacer = Utils.SetupSpacer(this, 10f, RightSide);
            _sliderElements.Add(finalSpacer);

            // 存储滑块引用
            _bodyPartSliders[bodyPart] = sliders;
        }
        
        // 基于参考插件的轴控制定义
        class ReferenceAxis
        {
            public string Label;
            public string Axis;
            public float Min;
            public float Max;
            public float Default;
        }
        
        List<ReferenceAxis> GetReferencePluginAxes(string bodyPart)
        {
            var axes = new List<ReferenceAxis>();
            
            switch (bodyPart.ToLower())
            {
                case "head":
                    axes.Add(new ReferenceAxis { Label = "Fwd ↔ Back", Axis = "X", Min = -35f, Max = 45f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Turn Left ↔ Right", Axis = "Y", Min = -35f, Max = 35f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Lean Left ↔ Right", Axis = "Z", Min = -30f, Max = 30f, Default = 0f });
                    break;
                case "arm":
                    axes.Add(new ReferenceAxis { Label = "Down ↔ Up", Axis = "X", Min = -75f, Max = 30f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Forw ↔ Back", Axis = "Y", Min = -80f, Max = 80f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist In ↔ Out", Axis = "Z", Min = -70f, Max = 70f, Default = 0f });
                    break;
                case "elbow":
                    axes.Add(new ReferenceAxis { Label = "Bend ↔ Straight", Axis = "X", Min = -130f, Max = 20f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Y", Axis = "Y", Min = -65f, Max = 65f, Default = 0f });
                    break;
                case "hand":
                    axes.Add(new ReferenceAxis { Label = "In ↔ Out", Axis = "X", Min = -80f, Max = 80f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Y", Axis = "Y", Min = -40f, Max = 40f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Z", Axis = "Z", Min = -40f, Max = 40f, Default = 0f });
                    break;
                case "thigh":
                    axes.Add(new ReferenceAxis { Label = "Back ↔ Forw", Axis = "X", Min = -25f, Max = 100f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Toes In ↔ Out", Axis = "Y", Min = -75f, Max = 75f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Straight ↔ Spread", Axis = "Z", Min = -85f, Max = 85f, Default = 0f });
                    break;
                case "foot":
                    axes.Add(new ReferenceAxis { Label = "Toe ↔ Heal", Axis = "X", Min = -65f, Max = 40f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Toes In ↔ Out", Axis = "Y", Min = -20f, Max = 20f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Lean In ↔ Out", Axis = "Z", Min = -20f, Max = 20f, Default = 0f });
                    break;
                case "collar":
                    axes.Add(new ReferenceAxis { Label = "Down ↔ Up", Axis = "X", Min = -15f, Max = 50f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Forw ↔ Back", Axis = "Y", Min = -20f, Max = 20f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Rotate", Axis = "Z", Min = -15f, Max = 15f, Default = 0f });
                    break;
                case "spine":
                case "chest":
                    axes.Add(new ReferenceAxis { Label = "Forw ↔ Back", Axis = "X", Min = -50f, Max = 50f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Right↔Left", Axis = "Y", Min = -40f, Max = 40f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Z", Axis = "Z", Min = -40f, Max = 40f, Default = 0f });
                    break;
                case "pelvis":
                    axes.Add(new ReferenceAxis { Label = "Forw ↔ Back", Axis = "X", Min = -30f, Max = 15f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Right↔Left", Axis = "Y", Min = -15f, Max = 15f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Z", Axis = "Z", Min = -10f, Max = 10f, Default = 0f });
                    break;
                case "abd l":
                case "abd h":
                    axes.Add(new ReferenceAxis { Label = "Forw ↔ Back", Axis = "X", Min = -30f, Max = 20f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Right↔Left", Axis = "Y", Min = -15f, Max = 15f, Default = 0f });
                    axes.Add(new ReferenceAxis { Label = "Twist Z", Axis = "Z", Min = -10f, Max = 10f, Default = 0f });
                    break;
                // 单轴控制
                case "knee":
                    axes.Add(new ReferenceAxis { Label = "Bend ↔ Straight", Axis = "X", Min = -150f, Max = 11f, Default = 0f });
                    break;
                case "toe":
                case "toes":
                    axes.Add(new ReferenceAxis { Label = "Down ↔ Up", Axis = "X", Min = -65f, Max = 75f, Default = 0f });
                    break;
            }
            
            return axes;
        }
        
        void OnAxisSliderChanged(string bodyPart, string axisLabel, string axis, float sliderValue)
        {
            try
            {
                // 获取当前总倍数
                float totalMultiplier = _totalMultiplier?.val ?? 1.0f;
                
                // 基于已知工作的ApplyJointControl逻辑，创建轴向特定的JointMapping
                var jointMappings = GetJointMappings(bodyPart);
                if (jointMappings != null && jointMappings.Count > 0)
                {
                    // 为每个关节映射设置轴向和值
                    foreach (var mapping in jointMappings)
                    {
                        // 临时修改映射的轴向以应用特定轴向控制
                        var originalAxis = mapping.primaryAxis;
                        mapping.primaryAxis = axis;
                        
                        // 使用已知工作的ApplyJointControl逻辑
                        ApplyJointControl(bodyPart, "AxisBend", sliderValue, totalMultiplier, mapping);
                        
                        // 恢复原始轴向
                        mapping.primaryAxis = originalAxis;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"OnAxisSliderChanged: {bodyPart} {axisLabel}");
            }
        }
        
        void OnSliderChanged(string bodyPart, string controlType, float sliderValue)
        {
            try
            {
                // 获取当前总倍数
                float totalMultiplier = _totalMultiplier?.val ?? 1.0f;
                
                // 应用滑块值到关节控制
                ApplyJointControl(bodyPart, controlType, sliderValue, totalMultiplier);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"OnSliderChanged: {bodyPart} {controlType}");
            }
        }

        void ResetRightPanelLabels(string bodyPart = null)
        {
            if (_rightTitle?.text != null)
            {
                _rightTitle.text.text = string.IsNullOrEmpty(bodyPart)
                    ? "Bend Controls (Select a body part)"
                    : $"Bend Controls - {bodyPart}";
            }

            if (_rightHint?.text != null)
            {
                _rightHint.text.text = string.IsNullOrEmpty(bodyPart)
                    ? "请选择左侧的身体部位"
                    : "调整弯曲和强度参数，总控制滑块可放大效果。";
            }
        }

        void ClearCurrentSliders()
        {
            BaseScript.RemoveUIElements(this, _sliderElements);
            
            // 清理当前选中身体部位的滑块引用
            if (!string.IsNullOrEmpty(_selectedBodyPart))
            {
                _bodyPartSliders.Remove(_selectedBodyPart);
            }
        }

        void ClearAllUI()
        {
            BaseScript.RemoveUIElements(this, _sliderElements);
            BaseScript.RemoveUIElements(this, _rootElements);
            _bodyPartButtons.Clear();
            _bodyPartSliders.Clear();
            _rightTitle = null;
            _rightHint = null;
            _totalMultiplier = null;
        }

        // 本地实现：从 Atom 的 forceReceivers 构建可配置关节字典，避免依赖外部扩展方法
        Dictionary<string, ConfigurableJoint> GetAllConfigurableJointsLocal(Atom atom)
        {
            try
            {
                var joints = new Dictionary<string, ConfigurableJoint>();

                if (atom == null) return joints;

                // 首先从 forceReceivers 收集主关节
                foreach (var receiver in atom.forceReceivers)
                {
                    if (receiver == null) continue;
                    var joint = receiver.GetComponent<ConfigurableJoint>();
                    if (joint != null && !joints.ContainsKey(receiver.name))
                    {
                        joints[receiver.name] = joint;
                    }
                }

                // 其次收集每个关节的子 ConfigurableJoint（例如手指、脚趾等）
                var atomJoints = atom.GetComponentsInChildren<ConfigurableJoint>(true);
                foreach (var cj in atomJoints)
                {
                    if (cj == null || string.IsNullOrEmpty(cj.name)) continue;
                    if (!joints.ContainsKey(cj.name))
                    {
                        joints[cj.name] = cj;
                    }
                }

                return joints;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "GetAllConfigurableJointsLocal");
                return null;
            }
        }
    }
}
