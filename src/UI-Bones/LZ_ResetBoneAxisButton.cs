using UnityEngine;
using UnityEngine.UI;

// Removes bone from our setting when clicked
// Cannot apply to base/default settings

namespace ResponsiveControllerPlugin.UI
{
    class LZ_ResetBoneAxis : MonoBehaviour
    {
        private Button removeButton;

        public int boneNum;
        public int axis;
        public bool mirrorSides = false;
        public string conditionName = "default";

        public void Start()
        {
            // Get button!
            removeButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            removeButton.onClick.AddListener(delegate { ResetAxisButtonClicked(); });
        }

        public void ResetAxisButtonClicked()
        {
            if (mirrorSides)
            {
                ResponsiveControllerPlugin.getLayerSettings().resetFingerSettingsAxisMirror(boneNum, axis, conditionName);
            } 
            else
            {
                ResponsiveControllerPlugin.getLayerSettings().resetFingerSettingsAxis(boneNum, axis, conditionName);
            }
        }
    }
}
