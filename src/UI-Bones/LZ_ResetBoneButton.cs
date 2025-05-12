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
            ResponsiveControllerLayerSettings layerSettings = ResponsiveControllerPlugin.getLayerSettings();
            layerSettings.resetPoseBoneAxis(boneNum, 0);
            layerSettings.resetPoseBoneAxis(boneNum, 0);
            layerSettings.resetPoseBoneAxis(boneNum, 0);

        }
    }
}
