using System.Collections.Generic;
using VNyanInterface;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Linq;

namespace ResponsiveControllerPlugin
{
    // This is split into two sections, this settings section to call/store all the data, and the Interface implementation below
    class ResponsiveControllerLayerSettings
    {
        //Prefix for parameters set in VNyan, so they can be easily identified in the Monitor tool
        public static string prefix = "LZ_ControllerPoseInput_";

        //If true, Debug.Log messages will be printed out
        private bool debug = false;

        // Layer Settings //
        private bool layerActive = true; // flag for layer being active or not (when inactive, vnyan will stop reading from the rotations entirely)

        private float slerpAmount = 1f;

        private LZPose defaultPose = PoseUtils.createNewDefaultHandsPose();

        /**
         * Sets the Pose Layer on or off (handled by ResponsiveControllerLayer.isActive() in ResponsiveControlLayer.cs)
         */
        public void setLayerOnOff(float val) => layerActive = (val == 1f) ? true : false;

        /**
         * Gets pose layer state?
         */
        public bool isLayerActive() => layerActive;

        // Input Settings Methods //
        /**
         * Sets whether to print debug messages to the log
         */
        public void setDebug(bool val)
        {
            debug = val;
        }

        /**
         * Gets the current debug value.
         */
        public bool isDebug()
        {
            return debug;
        }

        /**
         * Gets the Slerp value, which is used by Quaternion.Slerp() to rotate the Current quaternions towards the Targets
         */
        public float getSlerpAmount()
        {
            return slerpAmount;
        }

        /**
         * Sets the Slerp value, which is used by Quaternion.Slerp() to rotate the Current quaternions towards the Targets
         */
        public void setSlerpAmount(float val)
        {
            slerpAmount = val;
        }


        


        // Rotations Euler Dictionary //
        //
        // bone rotation targets as Vector3's to read into the current bone rotations

        private Dictionary<int, VNyanVector3> RotationsEuler = PoseUtils.createVectorDictionary();

        /// <summary>
        /// Gets bone vector in the Target Rotations Vector3 dictionary
        /// </summary>
        /// <param name="boneNum"> Bone Number</param>
        /// <param name="vector"></param>
        public VNyanVector3 getRotationsEulersBone(int boneNum)
        {
            return RotationsEuler[boneNum];
        }

        /// <summary>
        /// Sets a bone vector in the Target Rotations Vector3 dictionary
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="vector"></param>
        public void setRotationsEulersBone(int boneNum, VNyanVector3 vector)
        {
            RotationsEuler[boneNum] = vector;
        }


        // Rotations Target Dictionary //
        //
        // bone rotation targets as Quaternions's to read into the current bone rotations
        private Dictionary<int, VNyanQuaternion> RotationsTarget = PoseUtils.createQuaternionDictionary();

        /// <summary>
        /// Gets bone quaternion in the Target Rotations
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public VNyanQuaternion getRotationsTargetBone(int boneNum)
        {
            return RotationsTarget[boneNum];
        }

        /// <summary>
        /// Sets bone quaternion in Target Rotations
        /// </summary>
        public void setRotationsTargetBone(int boneNum, VNyanQuaternion quaternion)
        {
            RotationsTarget[boneNum] = quaternion;
        }

        /// <summary>
        /// Updates the RotationsTarget dictionary by converting from the Rotation Eulers
        /// We get the Euler rotation, convert it to a quaternion, then set that quaternion to Rotations Target
        /// </summary>
        public void updateRotationsTarget()
        {
            foreach (int boneNum in PoseUtils.getHandsBoneIndices())
            {
                setRotationsTargetBone(boneNum, QuaternionMethods.setFromVNyanVector3( getRotationsEulersBone(boneNum) ));
            }
        }

        public bool checkIfTargetBoneExists(int boneNum)
        {
            return RotationsTarget.ContainsKey(boneNum);
        }
       

        // Rotations Current Dictionary //
        //
        // Current bone rotations of the avatar model to read into pose layer
        private Dictionary<int, VNyanQuaternion> RotationsCurrent = PoseUtils.createQuaternionDictionary();

        /// <summary>
        /// Gets Single Bone from Current Rotations
        /// </summary>
        /// <returns></returns>
        public VNyanQuaternion getRotationsCurrentBone(int boneNum)
        {
            return RotationsCurrent[boneNum];
        }

        /// <summary>
        /// Sets single Bone in the Current Rotations
        /// </summary>
        public void setRotationsCurrentBone(int boneNum, VNyanQuaternion quaternion)
        {
            RotationsCurrent[boneNum] = quaternion;
        }

        /// <summary>
        /// Updates the RotationsCurrent Dictionary by calling rotateCurrentTowardsTarget()
        /// </summary>
        public void updateRotationsCurrent()
        {
            rotateCurrentTowardsTarget();
        }

        /// <summary>
        /// Rotates every bone's Current rotation towards the Target using SLERP (spherical linear interpolation)
        /// This only should happen if the current rotation is actually different to the target.
        /// We convert to unity Quaternions to do the calculation, then convert back to VNyanQuaternions to save it.
        /// </summary>
        public void rotateCurrentTowardsTarget()
        {
            foreach (int boneNum in PoseUtils.getHandsBoneIndices())
            {
                VNyanQuaternion target = getRotationsTargetBone(boneNum);
                VNyanQuaternion current = getRotationsCurrentBone(boneNum);

                if (!(current == target)) // Only apply if the target is different from the current
                {
                    Quaternion newRotation = Quaternion.Slerp(QuaternionMethods.convertQuaternionV2U(current), QuaternionMethods.convertQuaternionV2U(target), slerpAmount * Time.deltaTime);
                    setRotationsCurrentBone(boneNum, QuaternionMethods.convertQuaternionU2V(newRotation));
                }
            }
        }

        public bool checkIfCurrentBoneExists(int boneNum)
        {
            return RotationsCurrent.ContainsKey(boneNum);
        }


        // Inputs Dictionary //
        //
        // We will keep a dictionary of all the inputs we need to listen to when an LZ pose is active
        // We are constantly grabbing these from within vnyan parameters, so if we don't need to we don't want to do extra work

        private Dictionary<string, int> InputStates = new Dictionary<string, int>();


        /// <summary>
        /// Sets the InputStates dictionary from a new incoming one, usually from LZPose
        /// </summary>
        /// <param name="InputsDict"></param>
        public void setInputs(Dictionary<string, int> InputsDict)
        {
            InputStates = InputsDict;
        }

        /// <summary>
        /// Checks if Input State exists in current dictionary
        /// </summary>
        /// <param name="input">name of input or subpose</param>
        /// <returns>True or False if input name exists in dictionary</returns>
        public bool checkInputState(string input)
        {
            return InputStates.ContainsKey(input);
        }

        /// <summary>
        /// Sets the state of the input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="state"></param>
        public void setInputState(string input, int state)
        {
            InputStates[input] = state;
        }

        /// <summary>
        /// Sets the state of the input, casting a float into int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="state"></param>
        public void setInputStateFloat(string input, float state)
        {
            InputStates[input] = (int)state;
        }

        /// <summary>
        /// Gets input state value
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int getInputState(string input)
        {
            return InputStates[input];
        }

        /// <summary>
        /// Gets the input state and converts it to float
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float getInputStateFloat(string input)
        {
            return (float)getInputState(input);
        }

        /// <summary>
        /// Gets on/off bool state of input
        /// </summary>
        /// <param name="input">string input name</param>
        /// <returns>True or False if the value in on or not</returns>
        public bool getInputStateBool(string input) => (getInputState(input) >= 1) ? true : false;

        /// <summary>
        /// Gets chosen button inputs from VNyan, based on the list current active for the pose.
        /// </summary>
        public void updateInputStatesFromVNyan()
        {
            foreach (KeyValuePair<string, int> input in InputStates)
            {
                setInputStateFloat(input.Key, VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(input.Key));
            }
        }

        /// <summary>
        /// Sets the InputStates Dictionary from an incoming list converting into dictionary of states
        /// </summary>
        /// <param name="InputsList"></param>
        public void setInputsFromList(List<string> InputsList)
        {
            setInputs(InputsList.ToDictionary(x => x, x => 0));
        }

        /// <summary>
        /// Sets the InputStates Dictionary from the current pose, converting the list to a dictonary of states
        /// </summary>
        /// <param name="pose"></param>
        public void setInputsFromPose(LZPose pose)
        {
            setInputsFromList(pose.getInputs());
        }


        // Current/Target Rotation Methods //
        // These methods do the actual rotation of the models bones with those 3 dictionaries

        /**
         * TODO Add description
         */


        /**
         * Initialize target rotation dictionary from LZPose
         */
        //public void initTargetRotations()
        //{
        //    // Sets all target rotations from euler settings
        //    foreach (int boneNum in PoseUtils.getHandsBoneIndices())
        //    {
        //        VNyanVector3 eulers = getFingerEuler(boneNum);
        //        fingerRotationsTarget[boneNum] = QuaternionMethods.setFromVNyanVector3(eulers);
        //    }
        //}

        // These three "setEulerTarget" methods connect the input settings to the main Euler Targets dictionary that gets read from
       

        /**
         * Loops through bones in LZPose output, sets the Target Rotations Vector3 to this
         */
        //public void setEulerTargetsDefault()
        //{
        //    foreach (KeyValuePair<int, VNyanVector3> vector in fingerPoses["default"])
        //    {
        //        setEulerTarget(vector.Key, vector.Value);
        //    }
        //}  





        /**
         * TODO Add description
         */




        /**
         * Functions to manage LZPoses
         * - Create new LZPose
         * - Create new subPose within LZPose
         * - Manage Input States/Conditions
         * - - create input state
         * - - get input state
         * - - set on/off
         * - - delete input state
         * - Add bone setting to LZPose
         * - - main pose or subpose
         * 
         * - Set Bone Rotation in one axis
         * - Reset bone rotations in one axis
         */





        /**
         * Create Input Condition
         */
        //public void addInputCondition(string conditionName)
        //{
        //    // Adds input condition if it doesn't exist
        //    if ( !defaultPose.getInputPoses().ContainsKey(conditionName) )
        //    {
        //        if(debug) Debug.Log("LZ_Controller: Input '" + conditionName + "' Not found, creating entry...");
        //        defaultPose.getInputPoses().Add(conditionName, new LZPose { });
        //        defaultPose.getFingerInputStates().Add(conditionName, 0f);
        //        defaultPose.getFingerInputConditions().Add(conditionName);
        //        VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
        //    }
        //}

        /**
         * Remove Input State
         */
        //public void removeInputCondition(string conditionName)
        //{
        //    if ( fingerInputs.ContainsKey(conditionName) )
        //    {
        //        // Removes input condition from settings
        //        if (debug) Debug.Log("LZ_Controller: Removing input '" + conditionName + "'...");
        //        fingerInputs.Remove(conditionName);
        //        fingerInputStates.Remove(conditionName);
        //        fingerInputConditions.Remove(conditionName);
        //        VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
        //    }
        //}



        /**
         * Add bone to LZPose (subpose or mainpose)
         */
        //public void addInputBone(string conditionName, int boneNum)
        //{
        //    // Adds bone into input condition if it exists. If condition doesn't exist, creates it first
        //    if (defaultPose.getInputPoses().ContainsKey(conditionName) )
        //    {
        //        if ( !checkInputConditionBone(conditionName, boneNum) )
        //        {
        //            defaultPose.getInputPoses()[conditionName].Add(boneNum, new VNyanVector3 { });
        //        }
        //    } 
        //    else
        //    {
        //        addInputCondition(conditionName);
        //        fingerInputs[conditionName].Add(boneNum, new VNyanVector3 { });
        //    }
        //}


        /**
         * Remove bone from LZPose
         */
        //public void removeInputBone(string conditionName, int boneNum)
        //{
        //    // Removes bone from input condition if it exists
        //    if ( checkInputConditionBone(conditionName, boneNum) )
        //    {
        //        fingerInputs[conditionName].Remove(boneNum);
        //    }
        //}




        // Finger Rotations/Vectors Methods //
        // Here are the methods more related to actually getting and setting bone rotations. Many of these are used by the UI sliders to make changes to the pose/innput settings.

        /**
         * Gets Target Rotations Vector3
         */
        

        /**
         * Gets Target Rotation according to one axis
         */
        //public float getFingerEulerAxis(int boneNum, int axis, string pose = "default")
        //{
        //    // Get finger rotation along axis if it exists
        //    if ( fingerEulersTarget.ContainsKey(boneNum) )
        //    {
        //        switch(axis)
        //        {
        //            case 0:
        //                return fingerEulersTarget[boneNum].X;
        //            case 1:
        //                return fingerEulersTarget[boneNum].Y;
        //            case 2:
        //                return fingerEulersTarget[boneNum].Z;
        //            default:
        //                return 0f;
        //        }
        //    }
        //    else
        //    {
        //        return 0f;
        //    }
        //} 
    }
}
