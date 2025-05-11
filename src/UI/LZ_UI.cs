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

namespace ResponsiveControllerPlugin.UI
{
    // VNyanInterface.IButtonClickHandler gives access to pluginButtonClicked
    public class LZ_UI : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        public GameObject windowPrefab;
        private GameObject window;

        public string setting_name;
        public string plugin_name;

        public static Dictionary<string, string> settingsJSON = new Dictionary<string, string>();

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
                ResponsiveControllerLayerSettings layerSettings = ResponsiveControllerPlugin.getLayerSettings();

                Debug.Log("LZ_Controller: Settings file found! loading settings...");
                settingsJSON = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(setting_name);
                if (settingsJSON.ContainsKey("LZPoseDictionary"))
                {
                    layerSettings.LoadPosesDictionary(settingsJSON["LZPoseDictionary"]);
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
