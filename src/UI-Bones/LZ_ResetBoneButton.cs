using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;
using ControllerPose;

// Removes bone from our setting when clicked
// Cannot apply to base/default settings

namespace TrackingSmoothing
{
    class LZ_ResetBone : MonoBehaviour
    {
        private Button removeButton;

        public int boneNum;
        public bool mirrorSides = false;
        public string conditionName = "default";

        public void Start()
        {
            // Get button!
            removeButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            removeButton.onClick.AddListener(delegate { ResetButtonClicked(); });
        }

        public void ResetButtonClicked()
        {
            if (mirrorSides)
            {
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(boneNum, 0, conditionName);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(boneNum, 1, conditionName);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(boneNum, 2, conditionName);
            } 
            else
            {
                ResponsiveControllerSettings.resetFingerSettingsAxis(boneNum, 0, conditionName);
                ResponsiveControllerSettings.resetFingerSettingsAxis(boneNum, 1, conditionName);
                ResponsiveControllerSettings.resetFingerSettingsAxis(boneNum, 2, conditionName);
            }
        }
    }
}
