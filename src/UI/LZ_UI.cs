using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VNyanInterface;
using ResponsiveControllerPlugin;
using System;
using System.Security;

// UI Core
// modified from Sjatar's UI code example for VNyan Plugin UI's: https://github.com/Sjatar/Screen-Light

namespace ControllerPose
{
    // VNyanInterface.IButtonClickHandler gives access to pluginButtonClicked
    public class LZ_UI : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        public GameObject windowPrefab;
        private GameObject window;

        public string setting_name;
        public string plugin_name;

        public static Dictionary<string, string> settings = new Dictionary<string, string>();

        // This happens when VNyan starts.
        public void Awake()
        {
            loadSettings();

            // VNyan magic to add a plugin button to it's interface!
            VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton(plugin_name, (IButtonClickedHandler)this);
            this.window = (GameObject)VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab((object)this.windowPrefab);
            if ((UnityEngine.Object)this.window != (UnityEngine.Object)null)
            {
                this.window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                this.window.SetActive(false);
            }
        }

        public void loadSettings()
        {
            if (null != VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(setting_name))
            {
                Debug.Log("LZ_Controller: Settings file found! loading settings...");
                settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(setting_name);
                if (settings.ContainsKey("fingerPoses"))
                {
                    ResponsiveControllerSettings.fingerPoses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, VNyanVector3>>>(settings["fingerPoses"]);
                }
                if ( settings.ContainsKey("fingerInputs") )
                {
                    ResponsiveControllerSettings.fingerInputs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, VNyanVector3>>>(settings["fingerInputs"]);
                }
                if ( settings.ContainsKey("fingerInputStates") )
                {
                    ResponsiveControllerSettings.fingerInputStates = JsonConvert.DeserializeObject<Dictionary<string, float>>(settings["fingerInputStates"]);
                }
                if ( settings.ContainsKey("fingerInputConditions") )
                {
                    ResponsiveControllerSettings.fingerInputConditions = JsonConvert.DeserializeObject<List<string>>(settings["fingerInputConditions"]);

                    foreach (string condition in ResponsiveControllerSettings.fingerInputConditions)
                    {
                        VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(ResponsiveControllerSettings.prefix + condition, 0f);
                    }
                }
            }
        }

        public void pluginButtonClicked()
        {
            if ((UnityEngine.Object)this.window == (UnityEngine.Object)null)
                return;
            this.window.SetActive(!this.window.activeSelf);
            if ( !this.window.activeSelf )
                return;
            this.window.transform.SetAsLastSibling();
        }
    }
}
