using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;

namespace ResponsiveControllerPlugin.UI
{
    class Sja_SaveButton : MonoBehaviour
    {
        private Button mainButton;
        public string setting_name;

        public void Start()
        {
            // Get button!
            mainButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            mainButton.onClick.AddListener(delegate { ButtonPressCheck(); });
        }

        public void ButtonPressCheck()
        {
            ResponsiveControllerLayerSettings layerSettings = ResponsiveControllerPlugin.getLayerSettings();

            // If the dictionary exists, which it always should but just in case.
            if (LZ_UI.settings != null && layerSettings != null)
            {

                LZ_UI.settings["fingerPoses"] = JsonConvert.SerializeObject(layerSettings.getFingerPoses());
                LZ_UI.settings["fingerInputs"] = JsonConvert.SerializeObject(layerSettings.getFingerInputs());
                LZ_UI.settings["fingerInputStates"] = JsonConvert.SerializeObject(layerSettings.getFingerInputStates());
                LZ_UI.settings["fingerInputConditions"] = JsonConvert.SerializeObject(layerSettings.getFingerInputConditions());

                // Write the dictionary to a settings file!
                VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(setting_name, LZ_UI.settings);
                Debug.Log("LZ_Controller: Settings saved!");
            }
        }
    }
}
