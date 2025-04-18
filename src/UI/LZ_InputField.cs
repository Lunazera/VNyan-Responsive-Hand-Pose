﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace ControllerPose
{
    class LZ_InputField : MonoBehaviour
    {
        // Adapted from Sjatar's UI code
        // This input field is tied to a particular bone in our dictionary
        // We will allow to set the bone number its linked to and the default value.
        public string fieldName;
        private float fieldValue = 0;
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

            if (LZ_UI.settings.ContainsKey(fieldName))
            {
                fieldValue = Convert.ToSingle(LZ_UI.settings[fieldName]);
                mainField.text = Convert.ToString(fieldValue);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            } 
            else
            {
                LZ_UI.settings.Add(fieldName, Convert.ToString(fieldValue));
                mainField.text = Convert.ToString(fieldValue);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            }
            
        }

        public void ButtonPressCheck()
        {
            // We need to sanitate the input a bit. Unless the input can be converted to a float we can't use it.
            if (float.TryParse(mainField.text, out float fieldValue))
            {
                LZ_UI.settings[fieldName] = Convert.ToString(fieldValue);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            }
            else
            {
                mainField.text = LZ_UI.settings[fieldName];
            }
        }
    }
}
