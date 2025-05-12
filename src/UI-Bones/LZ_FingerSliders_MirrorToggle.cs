using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

// Currently broken/not updated

namespace ResponsiveControllerPlugin.UI
{
    class LZ_FingerSliders_MirrorToggle: MonoBehaviour
    {
        public LZ_FingerSliders Proximal;
        public LZ_FingerSliders Middle;
        public LZ_FingerSliders Distal;
        public LZ_FingerSliders Splay;
        public LZ_FingerSliders Twist;

        public Button MirrorButton;
        public Button LeftSideButton;
        public Button RightSideButton;

        public InputField InputSettingField;
        private string InputSetting = "default";
        public Button InputApplyButton;

        public float mirrorSides = 0f;
        // 0 = mirror
        // 1 = left
        // 2 = right


        public void Start()
        {
            MirrorButton.onClick.AddListener(delegate { MirrorButtonClicked(); });
            LeftSideButton.onClick.AddListener(delegate { LeftSideButtonClicked(); });
            RightSideButton.onClick.AddListener(delegate { RightSideButtonClicked(); });
            SetMirrorSetting(0f);

            InputApplyButton.onClick.AddListener(delegate { ApplyButtonPressCheck(); });
        }

        public void Awake()
        {
            SetMirrorSetting(0f);
        }

        public void onEnable()
        {
            SetMirrorSetting(0f);
        }

        public void MirrorButtonClicked()
        {
            if ( !(this.mirrorSides == 0f) )
            {
                SetMirrorSetting(0f);
            }
        }
        public void LeftSideButtonClicked()
        {
            if (!(this.mirrorSides == 1f))
            {
                SetMirrorSetting(1f);
            }
        }
        public void RightSideButtonClicked()
        {
            if (!(this.mirrorSides == 2f))
            {
                SetMirrorSetting(2f);
            }
        }

        public void SetMirrorSetting(float setting)
        {
            this.mirrorSides = setting;
            Proximal.mirrorSides = setting;
            Middle.mirrorSides = setting;
            Distal.mirrorSides = setting;
            Splay.mirrorSides = setting;
            Twist.mirrorSides = setting;

            Proximal.loadSliderValues();
            Middle.loadSliderValues();
            Distal.loadSliderValues();
            Splay.loadSliderValues();
            Twist.loadSliderValues();

            if (setting == 0f)
            {
                ChangeButtonColor(true, MirrorButton);
                ChangeButtonColor(false, LeftSideButton);
                ChangeButtonColor(false, RightSideButton);
            }
            else if (setting == 1f)
            {
                ChangeButtonColor(false, MirrorButton);
                ChangeButtonColor(true, LeftSideButton);
                ChangeButtonColor(false, RightSideButton);
            }
            else if (setting == 2f)
            {
                ChangeButtonColor(false, MirrorButton);
                ChangeButtonColor(false, LeftSideButton);
                ChangeButtonColor(true, RightSideButton);
            }

        }

        public void ChangeButtonColor(bool boolbuttonState, Button button)
        {
            // If the input is true make it blue
            if (boolbuttonState)
            {
                ColorBlock cb = button.colors;
                cb.normalColor = new Color(0.7f, 0.7f, 1f);
                cb.highlightedColor = new Color(0.7f, 0.7f, 1f);
                cb.pressedColor = new Color(0.7f, 0.7f, 1f);
                cb.selectedColor = new Color(0.7f, 0.7f, 1f);
                button.colors = cb;
            }
            // Else make it greyed out! 
            else
            {
                ColorBlock cb = button.colors;
                cb.normalColor = new Color(0.8f, 0.8f, 0.8f);
                cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
                cb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
                cb.selectedColor = new Color(0.8f, 0.8f, 0.8f);
                button.colors = cb;
            }
        }

        public void ApplyButtonPressCheck()
        {
            ResponsiveControllerLayerSettings settings = ResponsiveControllerPlugin.getLayerSettings();

            settings.resetInputSimulateStates();
            // We need to sanitate the input a bit. Unless the input can be converted to a float we can't use it.
            if (!(InputSettingField.text == "") )
            {
                settings.addInputCondition(InputSettingField.text);
                Proximal.InputCondition = Convert.ToString(InputSettingField.text);
                Middle.InputCondition = Convert.ToString(InputSettingField.text);
                Distal.InputCondition = Convert.ToString(InputSettingField.text);
                Splay.InputCondition = Convert.ToString(InputSettingField.text);
                Twist.InputCondition = Convert.ToString(InputSettingField.text);

                // VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(InputSettingField.text, 0f);

                Proximal.loadSliderValues();
                Middle.loadSliderValues();
                Distal.loadSliderValues();
                Splay.loadSliderValues();
                Twist.loadSliderValues();
            }
            else
            {
                InputSettingField.text = "default";
                Proximal.InputCondition = Convert.ToString("default");
                Middle.InputCondition = Convert.ToString("default");
                Distal.InputCondition = Convert.ToString("default");
                Splay.InputCondition = Convert.ToString("default");
                Twist.InputCondition = Convert.ToString("default");

                Proximal.loadSliderValues();
                Middle.loadSliderValues();
                Distal.loadSliderValues();
                Splay.loadSliderValues();
                Twist.loadSliderValues();
            }
        }
    }
}
