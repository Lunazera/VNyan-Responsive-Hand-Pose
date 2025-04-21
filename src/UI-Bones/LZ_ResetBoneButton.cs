using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;
using ResponsiveControllerPlugin;

// Removes bone from our setting when clicked
// Cannot apply to base/default settings

namespace ResponsiveControllerPlugin.UI
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
            ResponsiveControllerLayerSettings settings = ResponsiveControllerPlugin.getLayerSettings();

            if (mirrorSides)
            {
                settings.resetFingerSettingsAxisMirror(boneNum, 0, conditionName);
                settings.resetFingerSettingsAxisMirror(boneNum, 1, conditionName);
                settings.resetFingerSettingsAxisMirror(boneNum, 2, conditionName);
            } 
            else
            {
                settings.resetFingerSettingsAxis(boneNum, 0, conditionName);
                settings.resetFingerSettingsAxis(boneNum, 1, conditionName);
                settings.resetFingerSettingsAxis(boneNum, 2, conditionName);
            }
        }
    }
}
