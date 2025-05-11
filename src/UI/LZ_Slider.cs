using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace ResponsiveControllerPlugin.UI
{
    class LZ_Slider : MonoBehaviour
    {
        public int boneNum;
        public int axis;
        public bool invert = false;
        public bool flipSides = false;
        public bool mirrorSides = false;

        public Button mainButton;

        // If you want the first time value to be different change it here.
        public float default_value = 0f;
        
        private Slider mainSlider;

        public void Start()
        {
            // Get the slider component of the object the script is attached to!
            mainSlider = GetComponent(typeof(Slider)) as Slider;

            mainButton.onClick.AddListener(delegate { ButtonPressCheck(); });

            // We add a listener, it will run ValueChangeCheck when the value changes on the slider!
            mainSlider.onValueChanged.AddListener(delegate {ValueChangeCheck(); });

            ResponsiveControllerLayerSettings settings = ResponsiveControllerPlugin.getLayerSettings();

            if (settings.activesubPose != null)
            {
                mainSlider.value = settings.getPoseBoneAxis(boneNum, settings.activesubPose, axis);
            }
            else
            {
                mainSlider.value = settings.getPoseBoneAxis(boneNum, axis);
            }

        }
        
        public void ValueChangeCheck()
        {
            float slider_value = invert ? -mainSlider.value : mainSlider.value;

            // Check if setting exists & create if not
            ResponsiveControllerLayerSettings settings = ResponsiveControllerPlugin.getLayerSettings();

            // Sets bone within subpose if its there
            if (settings.activesubPose != null)
            {
                settings.setPoseBoneAxis(boneNum, settings.activesubPose, axis, slider_value);
            }
            else
            {
                settings.setPoseBoneAxis(boneNum, axis, slider_value);
            }
            
        }

        public void ButtonPressCheck()
        {
            mainSlider.value = default_value;
        }
    }
}
