using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;

// Simply adds a listener to the button object it's attached to that will close the selected window prefab object
// NOTE: I am not 100% certain if this actually properly closes the window in the same way that clicking the plugin button does
// It depends on how the plugin button actually works when it creates the ui window.
// If it creates a parent 'window' gameobject and puts the prefab inside, then this close button is only de-activating the child prefab, whereas the
// plugin button seems like it's pointed at the parent window.
// But since this does work, it might be doing doing the same as the plugin window.

namespace ResponsiveControllerPlugin.UI
{
    class LZ_CloseButton : MonoBehaviour
    {
        public GameObject windowPrefab;
        private Button closeButton;
        public string setting_name;

        public void Start()
        {
            // Get button!
            closeButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            closeButton.onClick.AddListener(delegate { CloseButtonClicked(); });
        }

        public void CloseButtonClicked()
        {
            ResponsiveControllerLayerSettings settings = ResponsiveControllerPlugin.getLayerSettings();

            settings.resetInputSimulateStates();
            // If the dictionary exists, which it always should but just in case.
            if (LZ_UI.settings != null)
            {
                //LZ_UI.settings["fingerPoses"] = JsonConvert.SerializeObject(ResponsiveControllerSettings.fingerPoses);
                //LZ_UI.settings["fingerInputs"] = JsonConvert.SerializeObject(ResponsiveControllerSettings.fingerInputs);
                //LZ_UI.settings["fingerInputStates"] = JsonConvert.SerializeObject(ResponsiveControllerSettings.fingerInputStates);
                //LZ_UI.settings["fingerInputConditions"] = JsonConvert.SerializeObject(ResponsiveControllerSettings.fingerInputConditions);

                // Write the dictionary to a settings file!
                //VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(setting_name, LZ_UI.settings);
                //Debug.Log("LZ_Controller: Settings saved!");
            }
            this.windowPrefab.SetActive(false);
        }
    }
}
