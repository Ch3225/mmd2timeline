using MacGruber;
using System;
using System.Collections.Generic;
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
            _rightTitle = Utils.SetupInfoTextNoScroll(this, "Bend Controls (Select a body part)", 30f, RightSide);
            _rootElements.Add(_rightTitle);

            var spacer = Utils.SetupSpacer(this, 10f, RightSide);
            _rootElements.Add(spacer);

            _rightHint = Utils.SetupInfoTextNoScroll(this, "请选择左侧的身体部位", 30f, RightSide);
            _rootElements.Add(_rightHint);

            ResetRightPanelLabels(_selectedBodyPart);
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

            var bendSlider = Utils.SetupSliderFloat(this, $"{bodyPart} Bend", 0f, -100f, 100f, RightSide, "F1");
            RegisterFloat(bendSlider);
            _sliderElements.Add(bendSlider);

            var strengthSlider = Utils.SetupSliderFloat(this, $"{bodyPart} Strength", 50f, 0f, 100f, RightSide, "F1");
            RegisterFloat(strengthSlider);
            _sliderElements.Add(strengthSlider);

            var spacer = Utils.SetupSpacer(this, 10f, RightSide);
            _sliderElements.Add(spacer);
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
                    : "滑块仅作展示用途，未连接骨骼。";
            }
        }

        void ClearCurrentSliders()
        {
            BaseScript.RemoveUIElements(this, _sliderElements);
        }

        void ClearAllUI()
        {
            BaseScript.RemoveUIElements(this, _sliderElements);
            BaseScript.RemoveUIElements(this, _rootElements);
            _bodyPartButtons.Clear();
            _rightTitle = null;
            _rightHint = null;
        }
    }
}
