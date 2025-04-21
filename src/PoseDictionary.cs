using System;
using System.Collections.Generic;
using System.Linq;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    class PoseDictionary
    {
        //Indices of bones on the left hand
        private static List<int> ListFingersLeft = new List<int> { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };
        //Indices of bones on the right hand
        private static List<int> ListFingersRight = new List<int> { 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };
        //Indices of bones on both hands, with left hand listed first
        private static List<int> ListFingers = new List<int> {}.Concat(ListFingersLeft).Concat(ListFingersRight).ToList();

        private static int HandLeftIndex = 17;
        private static int HandRightIndex = 18;

        // Input Setting Dictionaries // 

        // There are two sets of dictionaries here

        // This first set is to store user input of the bone rotations in x/y/z
        // These are stored as Vector objects (have x/y/z properties)

        // We will maintain "Poses", and each pose will have a number of "Inputs" configured
        // We keep the Input settings in a separate dictionary

        // For example consider holding a controller
        // We need to store the "at rest" pose, and the pose for each button input.

        // Right now this is all set up for working with only one pose, but I want to reconfigure this to handle multiple poses and inputs for each

        // We also have a third dictionary for Input states, mainly it is written and read from to track which button inputs are being pressed (and so which pose inputs to activate)

        // ***
        // *** This whole part is mainly what i need help with. probably makes sense to make into its own class to maintain all the info.
        // *** since we have properties we want associated with each thing that are linked together
        // *** like, we want to create many poses. Each pose has a dictionary of its base finger rotations, AND a dictionary of inputs. Each input has its own small dictionary of finger rotations.
        // *** within each pose, we also want to maintain which input is current on or off. Input names will be vnyan parameters that you should be able to name anything.
        // so end result is, i should be able to say "turn on controller pose, and now listen for the input names within this pose, and turn on/off the input rotations depending on that"

        public static Dictionary<string, Dictionary<int, VNyanVector3>> createPoseDictionary()
        {
            Dictionary<string, Dictionary<int, VNyanVector3>> fingerPoses = new Dictionary<string, Dictionary<int, VNyanVector3>> { };
            Dictionary<int, VNyanVector3> defaultDic = PoseDictionary.createVectorDictionary();
            fingerPoses.Add("default", defaultDic);
            return fingerPoses;
        }

        public static Dictionary<int, VNyanVector3> createVectorDictionary()
        {
            Dictionary<int, VNyanVector3> posDic = new Dictionary<int, VNyanVector3>();
            foreach (var ele in ListFingers)
            {
                posDic.Add(ele, new VNyanVector3 { });
            }
            return posDic;
        }

        public static Dictionary<int, VNyanQuaternion> createQuaternionDictionary()
        {
            Dictionary<int, VNyanQuaternion> rotDic = new Dictionary<int, VNyanQuaternion>();
            foreach (var ele in ListFingers)
            {
                rotDic.Add(ele, new VNyanQuaternion { });
            }
            return rotDic;
        }


        /**
         * Return a list of indices of bones on the left hand
         */
        public static List<int> getLeftHandBoneIndices()
        {
            return ListFingersLeft;
        }

        /**
         * Return a list of indices of bones on the right hand
         */
        public static List<int> getRightHandBoneIndices()
        {
            return ListFingersRight;
        }

        /**
         * Return a list of indices of bones on both hands, with left hand listed first
         */
        public static List<int> getHandsBoneIndices()
        {
            return ListFingers;
        }

        public static int getHandLeftIndex()
        {
            return HandLeftIndex;
        }

        public static int getHandRightIndex()
        {
            return HandRightIndex;
        }
    }
}
