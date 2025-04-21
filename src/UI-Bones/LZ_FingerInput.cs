using System;
using UnityEngine;
using UnityEngine.UI;

namespace ResponsiveControllerPlugin.UI
{
    class LZ_FingerInput : MonoBehaviour
    {
        // Adapted from Sjatar's UI code
        // This input field is tied to a particular bone in our dictionary
        // We will allow to set the bone number its linked to and the default value.
        public int boneNum;
        public int axis;
        public float fieldValue = 0;
        public bool flipSides = false;
        public string conditionName = "default";
        private InputField mainField;
        private Button mainButton;

        public void Start()
        {
            // We add the inputfield as the mainfield
            mainField = GetComponent(typeof(InputField)) as InputField;
            // We add a button as confirmation to change the inputted value
            mainButton = GetComponentInChildren(typeof(Button)) as Button;

            // We add a listener that will run ButtonPressCheck if the button is pressed.
            mainButton.onClick.AddListener(delegate { ButtonPressCheck(); });

            if (ResponsiveControllerPlugin.getLayerSettings().checkInputCondition(conditionName)) 
            {
                fieldValue = ResponsiveControllerPlugin.getLayerSettings().getFingerEulerAxis(boneNum, axis, conditionName);
                mainField.text = Convert.ToString(fieldValue);
            }
            else
            {
                mainField.text = Convert.ToString(fieldValue);
            }

        }

        public void ButtonPressCheck()
        {
            // We need to sanitate the input a bit. Unless the input can be converted to a float we can't use it.
            if (float.TryParse(mainField.text, out float fieldValue))
            {
                // Set the new value!
                // ResponsiveControllerSettings.setFingerSettingsAxis(boneNum, axis, fieldValue, flipSides, conditionName);
            }
            else
            {
                // If the value was not able to be converted we just want to show the current value.
                // This overwrites what the user typed.
                mainField.text = Convert.ToString(ResponsiveControllerPlugin.getLayerSettings().getFingerEulerAxis(boneNum, axis, conditionName));
            }
        }
    }
}
