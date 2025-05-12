using System;
using UnityEngine;
using ResponsiveControllerPlugin.UI;

namespace ResponsiveControllerPlugin
{
   public class ResponsiveControllerPlugin : MonoBehaviour
    {
        //Set this to false for production
        public static bool debugEnabled = true;

        // Set up pose layer according to our class
        private static ResponsiveControllerLayer responsiveControllerLayer = new ResponsiveControllerLayer(debugEnabled);

        public string paramNameLayerActive = "LZ_ControllerPoseActive";
        private float LayerActive = 1f;
        private float LayerActive_new = 1f;

        public string paramNameSpeed = "LZ_ControllerPoseSpeed";
        private float Speed = 10f;
        private float Speed_new = 10f;
        public static ResponsiveControllerLayer getLayer()
        {
            return responsiveControllerLayer;
        }

        public static ResponsiveControllerLayerSettings getLayerSettings()
        {
            return responsiveControllerLayer.getSettings();
        }

        public void Start()
        {
            // Register Pose Layer for VNyan to listen to
            VNyanInterface.VNyanInterface.VNyanAvatar.registerPoseLayer(responsiveControllerLayer);

            // Parameter management //
            // Layer Toggle
            if (LZ_UI.settings.ContainsKey(paramNameLayerActive))
            {
                LayerActive = Convert.ToSingle(LZ_UI.settings[paramNameLayerActive]);
            }
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(paramNameLayerActive, LayerActive);
            getLayerSettings().setLayerOnOff(LayerActive);

            // Speed, Slerp Amount
            if (LZ_UI.settings.ContainsKey(paramNameSpeed))
            {
                Speed = Convert.ToSingle(LZ_UI.settings[paramNameSpeed]);
            }
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(paramNameSpeed, Speed);
            getLayerSettings().setSlerpAmount(Speed);

            if(debugEnabled) VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LZ_ResponsiveControllerPluginLoaded", 1.0f);
        }

        public void Update()
        {
            // Only run once avatar is loaded in
            if ( !(VNyanInterface.VNyanInterface.VNyanAvatar == null) )
            {
                getLayerSettings().getInputStatesFromVNyan();
            }

            // Parameter management //
            // Layer Toggle
            LayerActive_new = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(paramNameLayerActive);
            if ( !(LayerActive_new == LayerActive) )
            {
                LayerActive = LayerActive_new;
                getLayerSettings().setLayerOnOff(LayerActive);
            }
            // Speed, Slerp Amount
            Speed_new = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(paramNameSpeed);
            if ( !(Speed_new == Speed) )
            {
                Speed = Speed_new;
                getLayerSettings().setSlerpAmount(Speed);
            }
        }
    }
}
