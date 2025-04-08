using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;
using ControllerPose;

// Removes bone from our setting when clicked
// Cannot apply to base/default settings

namespace TrackingSmoothing
{
    class LZ_RemoveInputCondition : MonoBehaviour
    {
        private Button removeButton;
        public string conditionName = "default";

        public void Start()
        {
            // Get button!
            removeButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            removeButton.onClick.AddListener(delegate { RemoveButtonClicked(); });
        }

        public void RemoveButtonClicked()
        {
            if (conditionName != "default")
            {
                ResponsiveControllerSettings.removeInputCondition(conditionName);
            }
        }
    }
}
