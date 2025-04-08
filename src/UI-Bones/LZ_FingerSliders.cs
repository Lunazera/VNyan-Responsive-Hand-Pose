using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace ControllerPose
{
    class LZ_FingerSliders : MonoBehaviour
    {
        // Sliders will be numbered/indexed 0 to 4 from proximal, middle, distal, splay, and twist

        // notes
        // the eulers of either left/right side are inverted. We want to store the actual rotations 

        // invert:
        // changes sign of the slider's value

        // flip sides:
        // changes sign of the rotations value coming from the vector


        [Header("Finger Settings")]

        public int FingerBone = 0; // 0, 1, 2, 3, 4
        public string InputCondition = "default";
        
        public float mirrorSidesDefault = 0f;
        public float mirrorSides = 0f;

        public float defaultValue = 0f;

        [Header("Finger Curl")]
        public int CurlAxis;
        public bool invert = false;
        public bool flipSides = false;

        public Slider Proximal;
        public Button ProximalReset;

        public Slider Middle;
        public Button MiddleReset;

        public Slider Distal;
        public Button DistalReset;
        

        [Header("Finger Splay")]
        public int SplayAxis;
        public bool invertSplay = false;
        public bool flipSplaySides = false;

        public Slider Splay;
        public Button SplayReset;
       

        [Header("Finger Twist")]
        public int TwistAxis;
        public bool invertTwist = false;
        public bool flipTwistSides = false;

        public Slider Twist;
        public Button TwistReset;

        [Header("Buttons")]
        public Button RemoveBone;

        private static bool sliderSettingFlag = true;

        private int ProximalBone;
        private int MiddleBone;
        private int DistalBone;

        // Flip is meant to flip the pos/neg sign of the angle when dealing with mirrored mode.
        // If true, then the Right bones will get the inverted sign of the left bones
        // If false, then the Right bones will get the same as the left bones

        public void setFingerBones()
        {
            // Establishes bone number based on finger/digit setting, going thumb, index, middle, ring, and little finger
            switch(FingerBone)
            {
                case 0:
                    ProximalBone = 24;
                    MiddleBone = 25;
                    DistalBone = 26;
                    break;
                case 1:
                    ProximalBone = 27;
                    MiddleBone = 28;
                    DistalBone = 29;
                    break;
                case 2:
                    ProximalBone = 30;
                    MiddleBone = 31;
                    DistalBone = 32;
                    break;
                case 3:
                    ProximalBone = 33;
                    MiddleBone = 34;
                    DistalBone = 35;
                    break;
                case 4:
                    ProximalBone = 36;
                    MiddleBone = 37;
                    DistalBone = 38;
                    break;
            }
            Debug.Log("LZ_Controller: Slider" + FingerBone + " bone numbers set to " + ProximalBone + ", " + MiddleBone + ", " + DistalBone);
        }

        public int getFingerBones(int boneSegment, float MirrorSide)
        {
            // Returns this script instances bone number for each segment, determined by left or right side
            int output = 0;
            switch(boneSegment)
            {
                case 0:
                    output = ProximalBone;
                    break;
                case 1:
                    output = MiddleBone;
                    break;
                case 2:
                    output = DistalBone;
                    break;
            }
            if (MirrorSide == 2f)
            {
                output += 15;
            }

            return output;
        }


        public void Start()
        {
            mirrorSides = mirrorSidesDefault;
            sliderSettingFlag = true;

            setFingerBones();

            // Add slider listeners
            Proximal.onValueChanged.AddListener(delegate { SliderChangeCheck(0); });
            Middle.onValueChanged.AddListener(delegate { SliderChangeCheck(1); });
            Distal.onValueChanged.AddListener(delegate { SliderChangeCheck(2); });
            Splay.onValueChanged.AddListener(delegate { SliderChangeCheck(3); });
            Twist.onValueChanged.AddListener(delegate { SliderChangeCheck(4); });

            // Add reset buttons listners
            ProximalReset.onClick.AddListener(delegate { ResetButtonPressCheck(0); });
            MiddleReset.onClick.AddListener(delegate { ResetButtonPressCheck(1); });
            DistalReset.onClick.AddListener(delegate { ResetButtonPressCheck(2); });
            SplayReset.onClick.AddListener(delegate { ResetButtonPressCheck(3); });
            TwistReset.onClick.AddListener(delegate { ResetButtonPressCheck(4); });

            // Add remove button listners
            RemoveBone.onClick.AddListener(delegate { RemoveButtonClicked(); });

            if (ResponsiveControllerSettings.checkInputCondition(InputCondition))
            {
                loadSliderValues();
            }
            else
            {
                Proximal.value = defaultValue;
                Middle.value = defaultValue;
                Distal.value = defaultValue;
                Splay.value = defaultValue;
                Twist.value = defaultValue;
            }

            sliderSettingFlag = false;
        }
        
        public void SliderChangeCheck(int sliderNum)
        {
            if (!sliderSettingFlag) 
            {
                float slider_value = 0f;
                int sliderAxis = CurlAxis;
                int sliderBoneNum = 0;
                bool valueFlip = flipSides;

                bool invertTemp = invert;
                bool invertSplayTemp = invertSplay;
                bool invertTwistTemp = invertTwist;
                if (mirrorSides == 2f)
                {
                    invertTemp = !invert;
                    invertSplayTemp = !invertSplay;
                    invertTwistTemp = !invertTwist;
                }

                // Check if setting exists & create if not
                ResponsiveControllerSettings.addInputCondition(InputCondition);

                switch (sliderNum)
                {
                    case 0:
                        slider_value = invertTemp ? -Proximal.value : Proximal.value;
                        sliderBoneNum = getFingerBones(0, mirrorSides);
                        sliderAxis = CurlAxis;
                        valueFlip = flipSides;
                        break;
                    case 1:
                        slider_value = invertTemp ? -Middle.value : Middle.value;
                        sliderBoneNum = getFingerBones(1, mirrorSides);
                        sliderAxis = CurlAxis;
                        valueFlip = flipSides;
                        break;
                    case 2:
                        slider_value = invertTemp ? -Distal.value : Distal.value;
                        sliderBoneNum = getFingerBones(2, mirrorSides);
                        sliderAxis = CurlAxis;
                        valueFlip = flipSides;
                        break;
                    case 3:
                        slider_value = invertSplayTemp ? -Splay.value : Splay.value;
                        sliderBoneNum = getFingerBones(0, mirrorSides);
                        sliderAxis = SplayAxis;
                        valueFlip = flipSplaySides;
                        break;
                    case 4:
                        slider_value = invertTwistTemp ? -Twist.value : Twist.value;
                        sliderBoneNum = getFingerBones(0, mirrorSides);
                        sliderAxis = TwistAxis;
                        valueFlip = flipTwistSides;
                        break;
                }

                if (mirrorSides == 0f)
                {
                    Debug.Log("Slider Value change: bones " + sliderBoneNum + " & " + (sliderBoneNum + 15) + "-" + sliderAxis + " -> " + slider_value);
                    ResponsiveControllerSettings.setFingerSettingsAxisMirror(sliderBoneNum, sliderAxis, slider_value, valueFlip, InputCondition);
                }
                else if (mirrorSides == 1f)
                {
                    Debug.Log("Slider Value change: bone " + sliderBoneNum + "-" + sliderAxis + " -> " + slider_value);
                    ResponsiveControllerSettings.setFingerSettingsAxis(sliderBoneNum, sliderAxis, slider_value, InputCondition);
                }
                else
                {
                    slider_value = valueFlip ? slider_value : -slider_value;
                    Debug.Log("Slider Value change: bone " + sliderBoneNum + "-" + sliderAxis + " -> " + slider_value);
                    ResponsiveControllerSettings.setFingerSettingsAxis(sliderBoneNum, sliderAxis, slider_value, InputCondition);
                }
            }
        }

        public void loadSliderValues()
        {
            sliderSettingFlag = true; // flag to stop slider from calling this as we load/change new values

            string condition = "default";
            if (ResponsiveControllerSettings.checkInputCondition(InputCondition))
            {
                condition = InputCondition;
            }

            // Manage the sides inversion for our left and right sides. When we change to right side, we want these inversions to all flip
            bool invertTemp = invert;
            bool invertSplayTemp = invertSplay;
            bool invertTwistTemp = invertTwist;

            if (mirrorSides == 2f)
            {
                invertTemp = !invert;
                invertSplayTemp = !invertSplay;
                // invertTwistTemp = !invertTwist;
            }


            (float proximal, float middle, float distal, float splay, float twist, bool boneFound) sliderCurrentValues;

            sliderCurrentValues = ResponsiveControllerSettings.GetFingerInputRotations(getFingerBones(0, mirrorSides), CurlAxis, SplayAxis, TwistAxis, flipSides, flipSplaySides, flipTwistSides, condition);

            if (invertTemp)
            {
                Proximal.value = -sliderCurrentValues.proximal;
                Middle.value = -sliderCurrentValues.middle;
                Distal.value = -sliderCurrentValues.distal;
            }
            else
            {
                Proximal.value = sliderCurrentValues.proximal;
                Middle.value = sliderCurrentValues.middle;
                Distal.value = sliderCurrentValues.distal;
            }

            if (invertSplayTemp)
            {
                Splay.value = -sliderCurrentValues.splay;
            }
            else
            {
                Splay.value = sliderCurrentValues.splay;
            }

            if (invertTwistTemp)
            {
                Twist.value = -sliderCurrentValues.twist;
            }
            else
            {
                Twist.value = sliderCurrentValues.twist;
            }

            Debug.Log("LZ_Controller: Slider " + FingerBone + " loaded -> " + Proximal.value + ", " + Middle.value + ", " + Distal.value + ", " + Splay.value + ", " + Twist.value);
            sliderSettingFlag = false;
        }

        public void ResetButtonPressCheck(int sliderNum)
        {
            int sliderBoneNum = 0;
            int sliderAxis = CurlAxis;

            switch (sliderNum)
            {
                case 0:
                    Proximal.value = defaultValue;
                    sliderBoneNum = getFingerBones(0, mirrorSides);
                    sliderAxis = CurlAxis;
                    break;
                case 1:
                    Middle.value = defaultValue;
                    sliderBoneNum = getFingerBones(1, mirrorSides);
                    sliderAxis = CurlAxis;
                    break;
                case 2:
                    Distal.value = defaultValue;
                    sliderBoneNum = getFingerBones(2, mirrorSides);
                    sliderAxis = CurlAxis;
                    break;
                case 3:
                    Splay.value = defaultValue;
                    sliderBoneNum = getFingerBones(0, mirrorSides);
                    sliderAxis = SplayAxis;
                    break;
                case 4:
                    Twist.value = defaultValue;
                    sliderBoneNum = getFingerBones(0, mirrorSides);
                    sliderAxis = TwistAxis;
                    break;
            }
            Debug.Log("LZ_Controller: Slider " + FingerBone + " bone " + sliderBoneNum + "-" + sliderAxis + " reset!");
            ResponsiveControllerSettings.resetFingerSettingsAxis(sliderBoneNum, sliderAxis, InputCondition);
        }
        public void RemoveButtonClicked()
        {
            Proximal.value = defaultValue;
            Middle.value = defaultValue;
            Distal.value = defaultValue;
            Splay.value = defaultValue;
            Twist.value = defaultValue;

            // Call reset finger methods depending on finger
            if (mirrorSides == 0f)
            {
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(getFingerBones(0, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(getFingerBones(1, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(getFingerBones(2, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(getFingerBones(0, mirrorSides), SplayAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxisMirror(getFingerBones(0, mirrorSides), TwistAxis, InputCondition);

                Debug.Log("LZ_Controller: Slider " + FingerBone + " L&R bones reset!");
            } 
            else if (mirrorSides == 1f) {
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(1, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(2, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), SplayAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), TwistAxis, InputCondition);

                Debug.Log("LZ_Controller: Slider " + FingerBone + " Left bones reset!");
            }
            else
            {
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(1, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(2, mirrorSides), CurlAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), SplayAxis, InputCondition);
                ResponsiveControllerSettings.resetFingerSettingsAxis(getFingerBones(0, mirrorSides), TwistAxis, InputCondition);

                Debug.Log("LZ_Controller: Slider " + FingerBone + " Right bones reset!");
            }
            
            if ( !(InputCondition == "default") )
            {
                ResponsiveControllerSettings.removeInputCondition(InputCondition);
            }
        }
    }
}
