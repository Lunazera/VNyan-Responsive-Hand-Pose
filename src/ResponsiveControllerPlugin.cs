using System;
using UnityEngine;
using VNyanInterface;
using ControllerPose;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ResponsiveControllerPlugin
{
   public class ResponsiveControllerPlugin : MonoBehaviour
    {
        // Set up pose layer according to our class
        IPoseLayer ResponsiveControllerLayer = new ControllerPose.ResponsiveControllerLayer();

        public string paramNameLayerActive = "LZ_ControllerPoseActive";
        private float LayerActive = 1f;
        private float LayerActive_new = 1f;

        public string paramNameSpeed = "LZ_ControllerPoseSpeed";
        private float Speed = 10f;
        private float Speed_new = 10f;

        public void Start()
        {
            // Register Pose Layer for VNyan to listen to
            VNyanInterface.VNyanInterface.VNyanAvatar.registerPoseLayer(ResponsiveControllerLayer);

            // Parameter management //
            // Layer Toggle
            if (LZ_UI.settings.ContainsKey(paramNameLayerActive))
            {
                LayerActive = Convert.ToSingle(LZ_UI.settings[paramNameLayerActive]);
            }
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(paramNameLayerActive, LayerActive);
            ResponsiveControllerSettings.setLayerOnOff(LayerActive);

            // Speed, Slerp Amount
            if (LZ_UI.settings.ContainsKey(paramNameSpeed))
            {
                Speed = Convert.ToSingle(LZ_UI.settings[paramNameSpeed]);
            }
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(paramNameSpeed, Speed);
            ResponsiveControllerSettings.setSlerpAmount(Speed);
        }

        public void Update()
        {
            // Only run once avatar is loaded in
            if ( !(VNyanInterface.VNyanInterface.VNyanAvatar == null) )
            {
                ResponsiveControllerSettings.getInputStatesFromVNyan();
            }

            // Parameter management //
            // Layer Toggle
            LayerActive_new = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(paramNameLayerActive);
            if ( !(LayerActive_new == LayerActive) )
            {
                LayerActive = LayerActive_new;
                ResponsiveControllerSettings.setLayerOnOff(LayerActive);
            }
            // Speed, Slerp Amount
            Speed_new = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(paramNameSpeed);
            if ( !(Speed_new == Speed) )
            {
                Speed = Speed_new;
                ResponsiveControllerSettings.setSlerpAmount(Speed);
            }
        }
    }
}
