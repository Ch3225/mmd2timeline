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
    // Map from button label to its actual Button component for precise highlighting
    readonly Dictionary<string, UnityEngine.UI.Button> _buttonByLabel = new Dictionary<string, UnityEngine.UI.Button>(StringComparer.OrdinalIgnoreCase);

        string _selectedBodyPart = string.Empty;
        bool _lastEnableState;
        bool _uiReady;
        bool _scriptInitialized;
        UIDynamicTextInfo _rightTitle;
        UIDynamicTextInfo _rightHint;
        
        // 总控制相关
        JSONStorableFloat _totalMultiplier;
        JSONStorableBool _symmetricControl;  // 对称控制开关
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
            // 严格按照截图逐行布局
            // 第1行: Jaw  Head  Tongue  (三按钮)
            var row1 = Utils.SetupTripleButton(
                this,
                "Jaw", () => OnBodyPartSelected("Jaw"),
                "Head", () => OnBodyPartSelected("Head"),
                "Tongue", () => OnBodyPartSelected("Tongue"),
                LeftSide
            );
            _rootElements.Add(row1);
            RegisterButtonsFromRow(row1, new []{"Jaw","Head","Tongue"});
            // 第2行: Arm  Collar  Collar  Arm  (四按钮)
            var row2 = Utils.SetupQuadButton(
                this,
                "Arm", () => OnBodyPartSelected("L.Arm"),
                "Collar", () => OnBodyPartSelected("L.Collar"),
                "Collar", () => OnBodyPartSelected("R.Collar"),
                "Arm", () => OnBodyPartSelected("R.Arm"),
                LeftSide
            );
            _rootElements.Add(row2);
            RegisterButtonsFromRow(row2, new []{"L.Arm","L.Collar","R.Collar","R.Arm"});
            // 第3行: Elbow  Spine  Elbow
            var row3 = Utils.SetupTripleButton(
                this,
                "Elbow", () => OnBodyPartSelected("L.Elbow"),
                "Spine", () => OnBodyPartSelected("Spine"),
                "Elbow", () => OnBodyPartSelected("R.Elbow"),
                LeftSide
            );
            _rootElements.Add(row3);
            RegisterButtonsFromRow(row3, new []{"L.Elbow","Spine","R.Elbow"});
            // 第4行: Chest  Abd L  Abd H  Pelvis
            var row4 = Utils.SetupQuadButton(
                this,
                "Chest", () => OnBodyPartSelected("Chest"),
                "Abd L", () => OnBodyPartSelected("Abd L"),
                "Abd H", () => OnBodyPartSelected("Abd H"),
                "Pelvis", () => OnBodyPartSelected("Pelvis"),
                LeftSide
            );
            _rootElements.Add(row4);
            RegisterButtonsFromRow(row4, new []{"Chest","Abd L","Abd H","Pelvis"});
            // 第5行: Hand  Thigh  Thigh  Hand
            var row5 = Utils.SetupQuadButton(
                this,
                "Hand", () => OnBodyPartSelected("L.Hand"),
                "Thigh", () => OnBodyPartSelected("L.Thigh"),
                "Thigh", () => OnBodyPartSelected("R.Thigh"),
                "Hand", () => OnBodyPartSelected("R.Hand"),
                LeftSide
            );
            _rootElements.Add(row5);
            RegisterButtonsFromRow(row5, new []{"L.Hand","L.Thigh","R.Thigh","R.Hand"});
            // 第6行: Fingers  Penis  Fingers
            var row6 = Utils.SetupTripleButton(
                this,
                "Fingers", () => OnBodyPartSelected("L.Fingers"),
                "Penis", () => OnBodyPartSelected("Penis"),
                "Fingers", () => OnBodyPartSelected("R.Fingers"),
                LeftSide
            );
            _rootElements.Add(row6);
            RegisterButtonsFromRow(row6, new []{"L.Fingers","Penis","R.Fingers"});
            // 第7行: Knee  Knee
            AddTwinRow("L.Knee", "R.Knee");
            // 第8行: Toe  Foot  Foot  Toe
            var row8 = Utils.SetupQuadButton(
                this,
                "Toe", () => OnBodyPartSelected("L.Toe"),
                "Foot", () => OnBodyPartSelected("L.Foot"),
                "Foot", () => OnBodyPartSelected("R.Foot"),
                "Toe", () => OnBodyPartSelected("R.Toe"),
                LeftSide
            );
            _rootElements.Add(row8);
            RegisterButtonsFromRow(row8, new []{"L.Toe","L.Foot","R.Foot","R.Toe"});
            // 第9行: Toes  Toes
            AddTwinRow("L.Toe", "R.Toe");
            
            // 将总控制移到左侧底部
            CreateTotalControlOnLeft();
        }

        // 兼容旧版 C#（不支持本地函数），抽成类级方法
        void AddTwinRow(string left, string right)
        {
            var twin = Utils.SetupTwinButton(
                this,
                string.IsNullOrEmpty(left) ? "" : left,
                () => { if (!string.IsNullOrEmpty(left)) OnBodyPartSelected(left); },
                string.IsNullOrEmpty(right) ? "" : right,
                () => { if (!string.IsNullOrEmpty(right)) OnBodyPartSelected(right); },
                LeftSide
            );
            _rootElements.Add(twin);
            // capture labels and buttons
            var tb = twin as UIDynamicTwinButton;
            if (tb != null)
            {
                if (!string.IsNullOrEmpty(left)) _buttonByLabel[left] = tb.buttonLeft;
                if (!string.IsNullOrEmpty(right)) _buttonByLabel[right] = tb.buttonRight;
            }
        }

        void RegisterButtonsFromRow(UIDynamic row, string[] logicalLabels)
        {
            var multi = row as MacGruber.Utils.UIDynamicMultiButton;
            if (multi == null || multi.buttons == null) return;
            for (int i = 0; i < logicalLabels.Length && i < multi.buttons.Count; i++)
            {
                var key = logicalLabels[i];
                if (!string.IsNullOrEmpty(key))
                {
                    _buttonByLabel[key] = multi.buttons[i];
                }
            }
        }

        void CreateSliderArea()
        {
            _rightTitle = Utils.SetupInfoTextNoScroll(this, "Bend Controls (Select a body part)", 30f, RightSide);
            _rootElements.Add(_rightTitle);

            var spacer = Utils.SetupSpacer(this, 10f, RightSide);
            _rootElements.Add(spacer);

            _rightHint = Utils.SetupInfoTextNoScroll(this, "请选择左侧的身体部位", 30f, RightSide);
            _rootElements.Add(_rightHint);

            ResetRightPanelLabels(_selectedBodyPart);
        }
        
        void CreateTotalControlOnLeft()
        {
            // 分隔符
            var spacer1 = Utils.SetupSpacer(this, 10f, LeftSide);
            _rootElements.Add(spacer1);
            
            // 总控制标题
            var totalTitle = Utils.SetupInfoOneLine(this, "总控制 (Total Control)", LeftSide);
            _rootElements.Add(totalTitle);
            
            // 对称控制开关
            _symmetricControl = Utils.SetupToggle(this, "Symmetric Control", false, LeftSide);
            _symmetricControl.setCallbackFunction += OnSymmetricControlChanged;
            RegisterBool(_symmetricControl);
            _rootElements.Add(_symmetricControl);
            
            // 总控制倍数滑块 (最大15倍)
            _totalMultiplier = Utils.SetupSliderFloat(this, "Total Multiplier", 1.0f, 0.0f, 15.0f, LeftSide, "F1");
            _totalMultiplier.setCallbackFunction += OnTotalMultiplierChanged;
            RegisterFloat(_totalMultiplier);
            _rootElements.Add(_totalMultiplier);
        }
        
        void OnSymmetricControlChanged(bool isSymmetric)
        {
            // 对称控制开关状态改变时的处理
            LogUtil.Debug($"对称控制: {(isSymmetric ? "启用" : "禁用")}");
            UpdateButtonHighlights();
        }
        
        void OnTotalMultiplierChanged(float multiplier)
        {
            // 重新应用所有滑块值，使用新的倍数
            if (!string.IsNullOrEmpty(_selectedBodyPart) && _bodyPartSliders.ContainsKey(_selectedBodyPart))
            {
                var sliders = _bodyPartSliders[_selectedBodyPart];
                foreach (var slider in sliders)
                {
                    if (slider.name.EndsWith("Strength"))
                    {
                        OnSliderChanged(_selectedBodyPart, "Strength", slider.val);
                    }
                    // 对于轴向滑块，重新触发轴向控制
                    var bodyPartMapping = BodyPartControlMapping.GetBodyPartMapping();
                    var mappingKey = GetBodyPartMappingKey(_selectedBodyPart);
                    if (!string.IsNullOrEmpty(mappingKey) && bodyPartMapping.ContainsKey(mappingKey))
                    {
                        var partControls = bodyPartMapping[mappingKey];
                        foreach (var axis in partControls.Axes)
                        {
                            if (slider.name.Contains(axis.Label))
                            {
                                OnNewAxisSliderChanged(mappingKey, axis, slider.val);
                                break;
                            }
                        }
                    }
                }
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
                // 基于参考插件的强化控制逻辑
                var clampedValue = Mathf.Clamp(bendValue, mapping.minBend, mapping.maxBend);
                
                // 关键修复：确保驱动力足够强
                SetJointDriveStrength(controller, 10000f, 10000f); // 增加驱动力
                
                // 应用到主要轴向
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
                
                // 如果有ConfigurableJoint，也同时设置
                var joint = controller.GetComponent<ConfigurableJoint>();
                if (joint != null)
                {
                    ApplyJointBending(joint, mapping, bendValue);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyBendingControl: {controller.name}");
            }
        }
        
        void SetJointDriveStrength(FreeControllerV3 controller, float spring, float maxForce)
        {
            try
            {
                // 设置足够的弹簧力和最大力来确保控制有效
                controller.jointRotationDriveSpring = spring;
                controller.jointRotationDriveMaxForce = maxForce;
                
                // 确保关节没有被其他约束限制
                controller.jointRotationDriveDamper = 0f; // 减少阻尼
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"SetJointDriveStrength: {controller.name}");
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
            UpdateButtonHighlights();
            CreateSlidersForBodyPart(bodyPart);
        }
        
        void UpdateButtonHighlights()
        {
            // 高亮选中的按钮和对称按钮
            // 重置所有按钮颜色
            foreach (var kv in _buttonByLabel)
            {
                if (kv.Value != null)
                {
                    var colors = kv.Value.colors;
                    // NormalColor 不能直接设置，这里通过目标图像颜色
                    var img = kv.Value.targetGraphic as UnityEngine.UI.Graphic;
                    if (img != null) img.color = Color.white;
                }
            }

            // 高亮当前选中
            UnityEngine.UI.Button btn;
            if (!string.IsNullOrEmpty(_selectedBodyPart) && _buttonByLabel.TryGetValue(_selectedBodyPart, out btn))
            {
                var img = btn?.targetGraphic as UnityEngine.UI.Graphic;
                if (img != null) img.color = Color.green;
            }

            // 高亮对称
            if (_symmetricControl?.val == true && !string.IsNullOrEmpty(_selectedBodyPart))
            {
                var symmetricPart = GetSymmetricBodyPart(_selectedBodyPart);
                UnityEngine.UI.Button sbtn;
                if (!string.IsNullOrEmpty(symmetricPart) && _buttonByLabel.TryGetValue(symmetricPart, out sbtn))
                {
                    var img = sbtn?.targetGraphic as UnityEngine.UI.Graphic;
                    if (img != null) img.color = Color.green;
                }
            }
        }

        void CreateSlidersForBodyPart(string bodyPart)
        {
            ResetRightPanelLabels(bodyPart);
            ClearCurrentSliders();

            if (string.IsNullOrEmpty(bodyPart))
            {
                return;
            }

            // 右侧暂不需要数值输入框，隐藏它
            bool prevHide = MacGruber.Utils.HideSliderNumericInput;
            MacGruber.Utils.HideSliderNumericInput = true;

            // 使用新的身体部位控制映射
            var bodyPartMapping = BodyPartControlMapping.GetBodyPartMapping();
            var mappingKey = GetBodyPartMappingKey(bodyPart);
            
            var sliders = new List<JSONStorableFloat>();

            if (!string.IsNullOrEmpty(mappingKey) && bodyPartMapping.ContainsKey(mappingKey))
            {
                // 使用新的多轴控制系统
                var partControls = bodyPartMapping[mappingKey];
                
                foreach (var axis in partControls.Axes)
                {
                    var slider = Utils.SetupSliderFloat(this, 
                        $"{partControls.Name} {axis.Label}", 
                        axis.Default, 
                        axis.Min, 
                        axis.Max, 
                        RightSide, 
                        "F1");
                        
                    slider.setCallbackFunction += (value) => OnNewAxisSliderChanged(mappingKey, axis, value);
                    RegisterFloat(slider);
                    _sliderElements.Add(slider);
                    sliders.Add(slider);
                    
                    var spacer = Utils.SetupSpacer(this, 5f, RightSide);
                    _sliderElements.Add(spacer);
                }
            }
            else
            {
                // 未实现的部位或未映射，显示提示
                var info = Utils.SetupInfoTextNoScroll(this, $"{bodyPart} 暂无可用的轴向控制", 30f, RightSide);
                _sliderElements.Add(info);
            }
            
            // 添加强度控制滑块
            var strengthSlider = Utils.SetupSliderFloat(this, $"{bodyPart} Strength", 50f, 0f, 100f, RightSide, "F1");
            strengthSlider.setCallbackFunction += (value) => OnSliderChanged(bodyPart, "Strength", value);
            RegisterFloat(strengthSlider);
            _sliderElements.Add(strengthSlider);
            sliders.Add(strengthSlider);

            var finalSpacer = Utils.SetupSpacer(this, 10f, RightSide);
            _sliderElements.Add(finalSpacer);

            _bodyPartSliders[bodyPart] = sliders;

            // 恢复全局设置（避免影响其他页面）
            MacGruber.Utils.HideSliderNumericInput = prevHide;
        }
        
        // 映射UI显示名称到内部控制键
        string GetBodyPartMappingKey(string displayName)
        {
            switch (displayName.ToLower())
            {
                // 中央/非对称部位
                case "head": return "Head";
                case "spine": return "Spine";
                case "chest": return "Chest";
                case "pelvis": return "Pelvis";
                case "abd": return "Abd";
                case "abd l": return "Abd L";
                case "abd h": return "Abd H";
                case "hip": return "Pelvis";
                case "jaw": return "Jaw";
                case "tongue": return "Tongue";
                
                // 左侧部位
                case "l.collar": return "L.Collar";
                case "l.arm": return "L.Arm";
                case "l.elbow": return "L.Elbow";
                case "l.hand": return "L.Hand";
                case "l.thigh": return "L.Thigh";
                case "l.knee": return "L.Knee";
                case "l.foot": return "L.Foot";
                case "l.toe": return "L.Toe";
                
                // 右侧部位
                case "r.collar": return "R.Collar";
                case "r.arm": return "R.Arm";
                case "r.elbow": return "R.Elbow";
                case "r.hand": return "R.Hand";
                case "r.thigh": return "R.Thigh";
                case "r.knee": return "R.Knee";
                case "r.foot": return "R.Foot";
                case "r.toe": return "R.Toe";
                
                // 通用映射（不区分左右）
                case "knee": case "l knee": return "L.Knee";
                case "r knee": return "R.Knee";
                case "hand": case "l hand": return "L.Hand";
                case "r hand": return "R.Hand";
                case "l toe": return "L.Toe";
                case "r toe": return "R.Toe";
                
                // 通用/复数形式
                case "toes": return "Toes";
                case "toe": return "Toe";
                case "foot": return "Foot";
                case "thigh": return "Thigh";
                
                default: return null;
            }
        }
        
        // 新的轴控制处理方法
        void OnNewAxisSliderChanged(string bodyPartKey, BodyPartControlMapping.ControlAxis axis, float value)
        {
            try
            {
                var totalMultiplier = _totalMultiplier?.val ?? 1f;
                var finalValue = value * axis.Neg; // 应用对称性乘数
                
                // 直接控制关节
                ApplyDirectJointControl(bodyPartKey, axis.Axis, finalValue, totalMultiplier);
                
                // 对称控制逻辑
                if (_symmetricControl?.val == true)
                {
                    ApplySymmetricControl(bodyPartKey, axis, value, totalMultiplier);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"OnNewAxisSliderChanged: {bodyPartKey} {axis.Label}");
            }
        }
        
        // 对称控制方法
        void ApplySymmetricControl(string bodyPartKey, BodyPartControlMapping.ControlAxis axis, float value, float totalMultiplier)
        {
            try
            {
                string symmetricPartKey = GetSymmetricBodyPart(bodyPartKey);
                if (string.IsNullOrEmpty(symmetricPartKey)) return;

                // 获取对称部位的映射
                var mapping = BodyPartControlMapping.GetBodyPartMapping();
                if (!mapping.ContainsKey(symmetricPartKey)) return;

                var symmetricControls = mapping[symmetricPartKey];
                var symmetricAxis = symmetricControls.Axes.FirstOrDefault(a => a.Axis == axis.Axis);
                if (symmetricAxis == null) return;

                // 应用对称值（考虑对称部位的Neg乘数）
                var symmetricValue = value * symmetricAxis.Neg;
                ApplyDirectJointControl(symmetricPartKey, axis.Axis, symmetricValue, totalMultiplier);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplySymmetricControl: {bodyPartKey}");
            }
        }
        
        // 获取对称身体部位
        string GetSymmetricBodyPart(string bodyPart)
        {
            if (string.IsNullOrEmpty(bodyPart)) return null;

            // 标准化前缀（支持多种大小写/格式）
            var normalized = bodyPart.Trim();
            if (normalized.StartsWith("L.", StringComparison.OrdinalIgnoreCase))
                return normalized.Replace("L.", "R.");
            if (normalized.StartsWith("R.", StringComparison.OrdinalIgnoreCase))
                return normalized.Replace("R.", "L.");

            // 支持常见无前缀的名称或不同大小写
            switch (normalized.ToLower())
            {
                case "r.knee": return "L.Knee";
                case "l.hand": return "R.Hand";
                case "r.hand": return "L.Hand";
                case "l.arm": return "R.Arm";
                case "r.arm": return "L.Arm";
                case "l.thigh": return "R.Thigh";
                case "r.thigh": return "L.Thigh";
                case "l.foot": return "R.Foot";
                case "r.foot": return "L.Foot";
                case "l.toe": return "R.Toe";
                case "r.toe": return "L.Toe";
                // 通用名
                case "l.knee":
                case "knee":
                    return "R.Knee";
            }

            return null; // 非对称部位返回 null
        }
        
        // 直接关节控制方法 - 基于参考插件的实现
        void ApplyDirectJointControl(string bodyPartKey, string axis, float value, float multiplier)
        {
            try
            {
                var persons = GetTargetPersonAtoms().ToList();
                if (persons.Count == 0) return;

                foreach (var person in persons)
                {
                    if (person == null || person.type != "Person") continue;

                    var controllerName = GetControllerNameFromBodyPart(bodyPartKey);
                    if (string.IsNullOrEmpty(controllerName)) continue;

                    var controller = person.GetStorableByID(controllerName) as FreeControllerV3;
                    if (controller == null) continue;

                    // 关键：使用倍数调节刚度，而不是放大角度
                    float baseStiffness = 200f;
                    float spring = Mathf.Clamp(baseStiffness * Mathf.Max(0.1f, multiplier), 50f, 1500f);
                    controller.jointRotationDriveSpring = spring;
                    controller.jointRotationDriveMaxForce = spring;
                    controller.jointRotationDriveDamper = 1f;

                    var finalValue = value; // 角度直接使用滑块值（度）
                    
                    // 直接设置目标旋转
                    switch (axis.ToUpper())
                    {
                        case "X":
                            controller.jointRotationDriveXTarget = finalValue;
                            break;
                        case "Y":
                            controller.jointRotationDriveYTarget = finalValue;
                            break;
                        case "Z":
                            controller.jointRotationDriveZTarget = finalValue;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyDirectJointControl: {bodyPartKey} {axis}");
            }
        }
        
        // 从身体部位键获取控制器名称
        string GetControllerNameFromBodyPart(string bodyPartKey)
        {
            switch (bodyPartKey)
            {
                // 头部和面部
                case "Head": return "headControl";
                case "Jaw": return "jawControl";
                case "Tongue": return "tongueControl";
                case "Abd L": return "abdomenControl";
                case "Abd H": return "abdomen2Control";
                
                // 肩膀和锁骨
                case "L.Collar": return "lShoulderControl";
                case "R.Collar": return "rShoulderControl";
                
                // 手臂
                case "L.Arm": return "lArmControl";
                case "R.Arm": return "rArmControl";
                
                // 肘部
                case "L.Elbow": return "lElbowControl";
                case "R.Elbow": return "rElbowControl";
                
                // 手部
                case "L.Hand": return "lHandControl";
                case "R.Hand": return "rHandControl";
                
                // 躯干
                case "Spine": return "chestControl";
                case "Chest": return "chestControl";
                case "Abd": return "abdomenControl";
                case "Pelvis": return "pelvisControl";
                
                // 大腿
                case "L.Thigh": return "lThighControl";
                case "R.Thigh": return "rThighControl";
                case "Thigh": return "lThighControl"; // 通用大腿
                
                // 膝盖
                case "L.Knee": return "lKneeControl";
                case "R.Knee": return "rKneeControl";
                case "Knee": return "lKneeControl"; // 通用膝盖
                
                // 脚部
                case "L.Foot": return "lFootControl";
                case "R.Foot": return "rFootControl";
                case "Foot": return "lFootControl"; // 通用脚部
                
                // 脚趾
                case "L.Toe": return "lToeControl";
                case "R.Toe": return "rToeControl";
                case "Toe": return "lToeControl"; // 通用脚趾
                case "Toes": return "lToeControl"; // Toes 也映射到左脚趾控制器
                
                // 旧的映射保持兼容性
                case "head": return "headControl";
                case "lCollar": return "lShoulderControl";
                case "rCollar": return "rShoulderControl";
                case "lShldr": return "lArmControl";
                case "rShldr": return "rArmControl";
                case "lForeArm": return "lElbowControl";
                case "rForeArm": return "rElbowControl";
                case "lHand": return "lHandControl";
                case "rHand": return "rHandControl";
                case "lThigh": return "lThighControl";
                case "rThigh": return "rThighControl";
                case "lShin": return "lKneeControl";
                case "rShin": return "rKneeControl";
                case "lFoot": return "lFootControl";
                case "rFoot": return "rFootControl";
                case "lToe": return "lToeControl";
                case "rToe": return "rToeControl";
                
                default: return null;
            }
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
            }

            // 结束 switch，并返回构建的轴列表
            return axes;
        }

        void OnSliderChanged(string bodyPart, string controlType, float sliderValue)
        {
            try
            {
                // 获取当前总倍数
                float totalMultiplier = _totalMultiplier?.val ?? 1.0f;
                
                if (controlType == "Strength")
                {
                    // Strength控制：使用新的直接控制方法
                    ApplyStrengthDirectly(bodyPart, sliderValue, totalMultiplier);
                }
                else
                {
                    // 其他控制：使用传统映射方法
                    ApplyJointControl(bodyPart, controlType, sliderValue, totalMultiplier);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"OnSliderChanged: {bodyPart} {controlType}");
            }
        }
        
        void ApplyStrengthDirectly(string bodyPart, float strengthValue, float multiplier)
        {
            try
            {
                var persons = GetTargetPersonAtoms().ToList();
                if (persons.Count == 0) return;

                foreach (var person in persons)
                {
                    if (person == null || person.type != "Person") continue;

                    var controllerName = GetControllerNameFromBodyPart(bodyPart);
                    if (string.IsNullOrEmpty(controllerName)) continue;

                    var controller = person.GetStorableByID(controllerName) as FreeControllerV3;
                    if (controller == null) continue;

                    // 应用强度控制
                    float finalStrength = Mathf.Clamp(strengthValue * multiplier, 0f, 1000f);
                    controller.jointRotationDriveSpring = finalStrength;
                    controller.jointRotationDriveMaxForce = finalStrength;

                    // 同步更新底层关节（如果存在），确保物理驱动也反映强度
                    var joint = controller.GetComponent<ConfigurableJoint>();
                    if (joint != null)
                    {
                        var drive = joint.slerpDrive;
                        drive.positionSpring = Mathf.Clamp(finalStrength, 0f, 1000f);
                        drive.maximumForce = Mathf.Clamp(finalStrength, 0f, 1000f);
                        joint.slerpDrive = drive;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ApplyStrengthDirectly: {bodyPart}");
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
            _buttonByLabel.Clear();
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
