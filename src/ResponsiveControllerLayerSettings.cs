using System.Collections.Generic;
using VNyanInterface;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ResponsiveControllerPlugin
{
    // This is split into two sections, this settings section to call/store all the data, and the Interface implementation below
    public class ResponsiveControllerLayerSettings
    {
        //Prefix for parameters set in VNyan, so they can be easily identified in the Monitor tool
        public static string prefix = "LZ_ControllerPoseInput_";

        //If true, Debug.Log messages will be printed out
        private bool debug = false;

        // Layer Settings //
        private bool layerActive = true; // flag for layer being active or not (when inactive, vnyan will stop reading from the rotations entirely)

        //private static float layerActiveLeft = 0f;
        //private static float layerActiveRight = 0f;

        private float slerpAmount = 1f;

        private LZPose defaultPose = PoseUtils.createNewDefaultHandsPose();

        // Input Setting Dictionaries // 

        ///
        /// bone rotation targets as Vector3's to read into the current bone rotations
        ///
        private Dictionary<int, VNyanVector3> fingerEulersTarget = PoseUtils.createVectorDictionary();

        /**
         * bone rotation targets as Quaternions's to read into the current bone rotations
         */
        private Dictionary<int, VNyanQuaternion> fingerRotationsTarget = PoseUtils.createQuaternionDictionary();

        /**
        * Current bone rotations of the avatar model to read into pose layer
        */
        private Dictionary<int, VNyanQuaternion> fingerRotationsCurrent = PoseUtils.createQuaternionDictionary();


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

        /**
         * Gets the Current rotations dictionary
         */
        public Dictionary<int, VNyanQuaternion> getfingerRotationsCurrent()
        {
            return fingerRotationsCurrent;
        }

        /**
         * Sets the Pose Layer on or off (handled by ResponsiveControllerLayer.isActive() in ResponsiveControlLayer.cs)
         */
        public void setLayerOnOff(float val) => layerActive = (val == 1f) ? true : false;

        /**
         * Gets pose layer state?
         */
        public bool isLayerActive() => layerActive;

        /**
         * TODO Add description
         */
        public void setfingerInputState(string conditionName, float setting)
        {
            // Changes condition setting if input condition exists
            if ( checkInputCondition(conditionName) )
            {
                defaultPose.getFingerInputStates()[conditionName] = setting;
            }
        }

        /**
         * TODO Add description
         */
        public void setfingerInputStateOn(string conditionName)
        {
            // Sets condition setting on if it exists
            if (checkInputCondition(conditionName))
            {
                defaultPose.getFingerInputStates()[conditionName] = 1f;
            }
        }

        /**
         * TODO Add description
         */
        public void setfingerInputStateOff(string conditionName)
        {
            // Sets condition setting off if it exists
            if (checkInputCondition(conditionName))
            {
                defaultPose.getFingerInputStates()[conditionName] = 0f;
            }
        }

        /**
         * TODO Add description
         */
        public float getFingerInputState(string conditionName)
        {
            // Returns the input setting state
            if (checkInputCondition(conditionName))
            {
                return defaultPose.getFingerInputStates()[conditionName];
            }
            else
            {
                return 0f;
            }
        }

        /**
         * TODO Add description
         */
        public void addInputCondition(string conditionName)
        {
            // Adds input condition if it doesn't exist
            if ( !defaultPose.getInputPoses().ContainsKey(conditionName) )
            {
                if(debug) Debug.Log("LZ_Controller: Input '" + conditionName + "' Not found, creating entry...");
                defaultPose.getInputPoses().Add(conditionName, new LZPose { });
                defaultPose.getFingerInputStates().Add(conditionName, 0f);
                defaultPose.getFingerInputConditions().Add(conditionName);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
            }
        }

        /**
         * TODO Add description
         */
        public void addInputBone(string conditionName, int boneNum)
        {
            // Adds bone into input condition if it exists. If condition doesn't exist, creates it first
            if (defaultPose.getInputPoses().ContainsKey(conditionName) )
            {
                if ( !checkInputConditionBone(conditionName, boneNum) )
                {
                    defaultPose.getInputPoses()[conditionName].Add(boneNum, new VNyanVector3 { });
                }
            } 
            else
            {
                addInputCondition(conditionName);
                fingerInputs[conditionName].Add(boneNum, new VNyanVector3 { });
            }
        }

        /**
         * TODO Add description
         */
        public void removeInputCondition(string conditionName)
        {
            if ( fingerInputs.ContainsKey(conditionName) )
            {
                // Removes input condition from settings
                if (debug) Debug.Log("LZ_Controller: Removing input '" + conditionName + "'...");
                fingerInputs.Remove(conditionName);
                fingerInputStates.Remove(conditionName);
                fingerInputConditions.Remove(conditionName);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
            }
        }

        /**
         * TODO Add description
         */
        public void removeInputBone(string conditionName, int boneNum)
        {
            // Removes bone from input condition if it exists
            if ( checkInputConditionBone(conditionName, boneNum) )
            {
                fingerInputs[conditionName].Remove(boneNum);
            }
        }

        /**
         * TODO Add description
         */
        public bool checkInputCondition(string conditionName)
        {
            // Check if setting exists
            return (fingerInputs.ContainsKey(conditionName) && fingerInputStates.ContainsKey(conditionName) );
        }

        /**
         * TODO Add description
         */
        public bool checkInputConditionBone(string conditionName, int boneNum)
        {
            // Check if bone exists in input condition
            if (fingerInputs.ContainsKey(conditionName) )
            {
                return fingerInputs[conditionName].ContainsKey(boneNum);
            } 
            else
            {
                return false;
            }
        }

        // Finger Rotations/Vectors Methods //
        // Here are the methods more related to actually getting and setting bone rotations. Many of these are used by the UI sliders to make changes to the pose/innput settings.
        
        /**
         * TODO Add description
         */
        public VNyanVector3 getFingerEuler(int boneNum, string pose = "default")
        {
            // Get finger vector if it exists
            if ( fingerEulersTarget.ContainsKey(boneNum) )
            {
                return fingerEulersTarget[boneNum];
            }
            else
            {
                return new VNyanVector3 { };
            }
        }

        /**
         * TODO Add description
         */
        public float getFingerEulerAxis(int boneNum, int axis, string pose = "default")
        {
            // Get finger rotation along axis if it exists
            if ( fingerEulersTarget.ContainsKey(boneNum) )
            {
                switch(axis)
                {
                    case 0:
                        return fingerEulersTarget[boneNum].X;
                    case 1:
                        return fingerEulersTarget[boneNum].Y;
                    case 2:
                        return fingerEulersTarget[boneNum].Z;
                    default:
                        return 0f;
                }
            }
            else
            {
                return 0f;
            }
        }

        // This is a tuple return to read in the setting related to any given slider.
        /**
         * TODO Add description
         */
        public (float proximal, float middle, float distal, float splay, float twist, bool boneFound) GetFingerInputRotations(int boneNum, int CurlAxis, int SplayAxis, int TwistAxis, bool flipSides, bool flipSplaySides, bool flipTwistSides, string pose = "default", string condition = "default")
        {
            // Get finger bones rotations if it exists

            // returns the five main rotation values we want to manipulate for the finger
            // Axis determines whether to return the x/y/z of the bone
            // flip determines whether to return the positive or negative of the rotation

            float proximal = 0f;
            float middle = 0f;
            float distal = 0f;
            float splay = 0f;
            float twist = 0f;

            bool boneFound = false;

            VNyanVector3 proximal_bone = new VNyanVector3();
            VNyanVector3 middle_bone = new VNyanVector3();
            VNyanVector3 distal_bone = new VNyanVector3();

            // First load in the bone rotations that are currently being displayed
            if (fingerEulersTarget.ContainsKey(boneNum))
            {
                proximal_bone = fingerEulersTarget[boneNum];
            }
            if (fingerEulersTarget.ContainsKey(boneNum + 1))
            {
                middle_bone = fingerEulersTarget[boneNum + 1];
            }
            if (fingerEulersTarget.ContainsKey(boneNum + 2))
            {
                distal_bone = fingerEulersTarget[boneNum + 2];
            }

            
            // Next, check to see if we can find the bones in the Pose dictionary (which contain the 'base' poses)
            if (fingerPoses.ContainsKey(condition))
            {
                if (fingerPoses[condition].ContainsKey(boneNum))
                {
                    proximal_bone = fingerPoses[condition][boneNum];
                }
                if (fingerPoses[condition].ContainsKey(boneNum + 1))
                {
                    middle_bone = fingerPoses[condition][boneNum + 1];
                }
                if (fingerPoses[condition].ContainsKey(boneNum + 2))
                {
                    distal_bone = fingerPoses[condition][boneNum + 2];
                }
            }


            // Finally, see if we can find the bones in the corresponding input dictionaries
            if (fingerInputs.ContainsKey(condition))
            {
                boneFound = true;
                if (fingerInputs[condition].ContainsKey(boneNum))
                {
                    proximal_bone = fingerInputs[condition][boneNum];
                }
                if (fingerInputs[condition].ContainsKey(boneNum + 1))
                {
                    middle_bone = fingerInputs[condition][boneNum + 1];
                }
                if (fingerInputs[condition].ContainsKey(boneNum + 2))
                {
                    distal_bone = fingerInputs[condition][boneNum + 2];
                }
            } 


            switch (CurlAxis)
            {
                case 0:
                    proximal = proximal_bone.X;
                    middle = middle_bone.X;
                    distal = distal_bone.X;
                    break;
                case 1:
                    proximal = proximal_bone.Y;
                    middle = middle_bone.Y;
                    distal = distal_bone.Y;
                    break;
                case 2:
                    proximal = proximal_bone.Z;
                    middle = middle_bone.Z;
                    distal = distal_bone.Z;
                    break;
            }

            switch (SplayAxis)
            {
                case 0:
                    splay = proximal_bone.X;
                    break;
                case 1:
                    splay = proximal_bone.Y;
                    break;
                case 2:
                    splay = proximal_bone.Z;
                    break;
            }

            switch (TwistAxis)
            {
                case 0:
                    twist = proximal_bone.X;
                    break;
                case 1:
                    twist = proximal_bone.Y;
                    break;
                case 2:
                    twist = proximal_bone.Z;
                    break;
            }

            // Flip is meant to flip the pos/neg sign of the angle when dealing with mirrored mode.
            // If true, then the Right bones will get the inverted sign of the left bones
            // If false, then the Right bones will get the same as the left bones
            if (!flipSides)
            {
                proximal *= -1;
                middle *= -1;
                distal *= -1;
            }
            if (!flipSplaySides)
            {
                splay *= -1;
            }
            if (!flipTwistSides)
            {
                //twist *= -1;
            }

            return (proximal, middle, distal, splay, twist, boneFound);
        }

        /**
         * TODO Add description
         */
        public (float proximal, float middle, float distal, float splay, float twist, bool boneFound) GetFingerPoseRotations(int boneNum, int CurlAxis, int SplayAxis, int TwistAxis, bool flipSides, bool flipSplaySides, bool flipTwistSides)
        {
            // Get finger bones rotations if it exists

            // returns the five main rotation values we want to manipulate for the finger
            // Axis determines whether to return the x/y/z of the bone
            // flip determines whether to return the positive or negative of the rotation

            float proximal = 0f;
            float middle = 0f;
            float distal = 0f;
            float splay = 0f;
            float twist = 0f;

            bool boneFound = false;

            if (fingerEulersTarget.ContainsKey(boneNum))
            {
                boneFound = true;

                switch (CurlAxis)
                {
                    case 0:
                        proximal = fingerEulersTarget[boneNum].X;
                        middle = fingerEulersTarget[boneNum + 1].X;
                        distal = fingerEulersTarget[boneNum + 2].X;
                        break;
                    case 1:
                        proximal = fingerEulersTarget[boneNum].Y;
                        middle = fingerEulersTarget[boneNum + 1].Y;
                        distal = fingerEulersTarget[boneNum + 2].Y;
                        break;
                    case 2:
                        proximal = fingerEulersTarget[boneNum].Z;
                        middle = fingerEulersTarget[boneNum + 1].Z;
                        distal = fingerEulersTarget[boneNum + 2].Z;
                        break;
                }

                switch (SplayAxis)
                {
                    case 0:
                        splay = fingerEulersTarget[boneNum].X;
                        break;
                    case 1:
                        splay = fingerEulersTarget[boneNum].Y;
                        break;
                    case 2:
                        splay = fingerEulersTarget[boneNum].Z;
                        break;
                }

                switch (TwistAxis)
                {
                    case 0:
                        twist = fingerEulersTarget[boneNum].X;
                        break;
                    case 1:
                        twist = fingerEulersTarget[boneNum].Y;
                        break;
                    case 2:
                        twist = fingerEulersTarget[boneNum].Z;
                        break;
                }

                // Flip is meant to flip the pos/neg sign of the angle when dealing with mirrored mode.
                // If true, then the Right bones will get the inverted sign of the left bones
                // If false, then the Right bones will get the same as the left bones
                if (!flipSides)
                {
                    proximal *= -1;
                    middle *= -1;
                    distal *= -1;
                }
                if (!flipSplaySides)
                {
                    splay *= -1;
                }
                if (!flipTwistSides)
                {
                    //twist *= -1;
                }
            }

            return (proximal, middle, distal, splay, twist, boneFound);  
        }

        /**
         * TODO Add description
         */
        public void setInputBone(string conditionName, int boneNum, int axis, float angle)
        {
            // TODO: add in converstion for left/right side
            addInputCondition(conditionName);
            addInputBone(conditionName, boneNum);

            if ( conditionName == "default" )
            {
                switch (axis)
                {
                    case 0:
                        fingerPoses[conditionName][boneNum].X = angle;
                        break;
                    case 1:
                        fingerPoses[conditionName][boneNum].Y = angle;
                        break;
                    case 2:
                        fingerPoses[conditionName][boneNum].Z = angle;
                        break;
                }
            }
             else
            {
                // Sets an input bone setting according to angle
                addInputBone(conditionName, boneNum);
                switch (axis)
                {
                    case 0:
                        fingerInputs[conditionName][boneNum].X = angle;
                        break;
                    case 1:
                        fingerInputs[conditionName][boneNum].Y = angle;
                        break;
                    case 2:
                        fingerInputs[conditionName][boneNum].Z = angle;
                        break;
                }
            } 
        }

        /**
         * TODO Add description
         */
        public void setFingerSettingsAxisMirror(int boneNum, int axis, float angle, bool flip,  string conditionName)
        {
            // Set finger rotation setting according to axis mirrored L and R
            float angleflip = flip ? -angle : angle;
            // Flip is meant to flip the pos/neg sign of the angle when dealing with mirrored mode.
            // If true, then the Right bones will get the inverted sign of the left bones
            // If false, then the Right bones will get the same as the left bones

            

            addInputCondition(conditionName);
            addInputBone(conditionName, boneNum);
            addInputBone(conditionName, boneNum + 15);

            if ( conditionName == "default" )
            {
                if (debug) Debug.Log("Setting default bone " + boneNum);
                switch (axis)
                {
                    case 0:
                        fingerPoses[conditionName][boneNum].X = angle;
                        fingerPoses[conditionName][boneNum + 15].X = angleflip;
                        break;
                    case 1:
                        fingerPoses[conditionName][boneNum].Y = angle;
                        fingerPoses[conditionName][boneNum + 15].Y = angleflip;
                        break;
                    case 2:
                        fingerPoses[conditionName][boneNum].Z = angle;
                        fingerPoses[conditionName][boneNum + 15].Z = angleflip;
                        break;
                }
            }
            else
            {
                if (debug) Debug.Log("Setting input '" + conditionName + "' bone " + boneNum);
                switch (axis)
                {
                    case 0:
                        fingerInputs[conditionName][boneNum].X = angle;
                        fingerInputs[conditionName][boneNum + 15].X = angleflip;
                        break;
                    case 1:
                        fingerInputs[conditionName][boneNum].Y = angle;
                        fingerInputs[conditionName][boneNum + 15].Y = angleflip;
                        break;
                    case 2:
                        fingerInputs[conditionName][boneNum].Z = angle;
                        fingerInputs[conditionName][boneNum + 15].Z = angleflip;
                        break;
                }
            }
        }

        /**
         * TODO Add description
         */
        public void setFingerSettingsAxis(int boneNum, int axis, float angle, string conditionName = "default")
        {
            // Set finger rotation setting according to axis
            addInputCondition(conditionName);
            addInputBone(conditionName, boneNum);

            if (conditionName == "default")
            {
                switch (axis)
                {
                    case 0:
                        fingerPoses[conditionName][boneNum].X = angle;
                        break;
                    case 1:
                        fingerPoses[conditionName][boneNum].Y = angle;
                        break;
                    case 2:
                        fingerPoses[conditionName][boneNum].Z = angle;
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case 0:
                        fingerInputs[conditionName][boneNum].X = angle;
                        break;
                    case 1:
                        fingerInputs[conditionName][boneNum].Y = angle;
                        break;
                    case 2:
                        fingerInputs[conditionName][boneNum].Z = angle;
                        break;
                }
            }
        }

        /**
         * TODO Add description
         */
        public void resetFingerSettingsAxisMirror(int boneNum, int axis, string conditionName = "default")
        {
            // Sets finger rotations in axis to 0 for L and R
            addInputCondition(conditionName);
            addInputBone(conditionName, boneNum);
            addInputBone(conditionName, boneNum + 15);

            if (conditionName == "default")
            {
                
                switch (axis)
                {
                    case 0:
                        fingerPoses[conditionName][boneNum].X = 0;
                        fingerPoses[conditionName][boneNum + 15].X = 0;
                        break;
                    case 1:
                        fingerPoses[conditionName][boneNum].Y = 0;
                        fingerPoses[conditionName][boneNum + 15].Y = 0;
                        break;
                    case 2:
                        fingerPoses[conditionName][boneNum].Z = 0;
                        fingerPoses[conditionName][boneNum + 15].Z = 0;
                        break;
                }
            }
            else
            {
                // Sets finger rotations in axis to 0 for L and R
                switch (axis)
                {
                    case 0:
                        fingerInputs[conditionName][boneNum].X = 0;
                        fingerInputs[conditionName][boneNum + 15].X = 0;
                        break;
                    case 1:
                        fingerInputs[conditionName][boneNum].Y = 0;
                        fingerInputs[conditionName][boneNum + 15].Y = 0;
                        break;
                    case 2:
                        fingerInputs[conditionName][boneNum].Z = 0;
                        fingerInputs[conditionName][boneNum + 15].Z = 0;
                        break;
                }
            }
        }

        /**
         * TODO Add description
         */
        public void resetFingerSettingsAxis(int boneNum, int axis, string conditionName = "default")
        {

            // Sets finger rotations in axis to 0
            addInputCondition(conditionName);
            addInputBone(conditionName, boneNum);

            if (conditionName == "default")
            {
                switch (axis)
                {
                    case 0:
                        fingerPoses[conditionName][boneNum].X = 0;
                        break;
                    case 1:
                        fingerPoses[conditionName][boneNum].Y = 0;
                        break;
                    case 2:
                        fingerPoses[conditionName][boneNum].Z = 0;
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case 0:
                        fingerInputs[conditionName][boneNum].X = 0;
                        break;
                    case 1:
                        fingerInputs[conditionName][boneNum].Y = 0;
                        break;
                    case 2:
                        fingerInputs[conditionName][boneNum].Z = 0;
                        break;
                }
            }  
        }

        // Current/Target Rotation Methods //
        // These methods do the actual rotation of the models bones with those 3 dictionaries

        /**
         * TODO Add description
         */
        public bool checkIfFingerCurrentExists(int boneNum)
        {
            // Checks if the bone exists in the current dictionary
            return fingerRotationsCurrent.ContainsKey(boneNum);
        }

        /**
         * TODO Add description
         */
        public void initTargetRotations()
        {
            // Sets all target rotations from euler settings
            foreach (int boneNum in PoseDictionary.getHandsBoneIndices())
            {
                VNyanVector3 eulers = getFingerEuler(boneNum);
                fingerRotationsTarget[boneNum] = QuaternionMethods.setFromVNyanVector3(eulers);
            }
        }

        /**
         * TODO Add description
         */
        public void initCurrentRotations(Dictionary<int, VNyanQuaternion> boneRotations)
        {
            // Set current pose to target
            foreach (int boneNum in PoseDictionary.getHandsBoneIndices())
            {
                fingerRotationsCurrent[boneNum] = boneRotations[boneNum];
            }
        }

        // These three "setEulerTarget" methods connect the input settings to the main Euler Targets dictionary that gets read from
        /**
         * TODO Add description
         */
        public void setEulerTarget(int boneNum, VNyanVector3 vector)
        {
            fingerEulersTarget[boneNum] = vector;
        }

        /**
         * TODO Add description
         */

        public void setEulerTargetsDefault()
        {
            foreach (KeyValuePair<int, VNyanVector3> vector in fingerPoses["default"])
            {
                setEulerTarget(vector.Key, vector.Value);
            }
        }

        /**
         * TODO Add description
         */

        public void setEulerTargetsInputs()
        {
            // Set the target vector dictionary from settings, including current inputs
            // Go through the input state dictionary, for each if value is on, we will read from the corresponding input settings dictionary
            foreach (string condition in fingerInputConditions)
            {
                // If input state is on
                if ( fingerInputStates[condition] > 0f )
                {
                    // Grab out the inner dictionary, which is a collection of bonenumbers and vectors
                    // Check each bone/vector pair, we will replace the corresponding key/value pair in the target dictionary
                    foreach (KeyValuePair<int, VNyanVector3> vector in fingerInputs[condition])
                    {
                        setEulerTarget(vector.Key, vector.Value);
                    }
                }
            }
        }

        /**
         * TODO Add description
         */
        public void setFingerTargets()
        {
            // Set the target rotations from the target vectors
            foreach (int boneNum in PoseDictionary.getHandsBoneIndices())
            {
                VNyanVector3 eulers = getFingerEuler(boneNum);
                VNyanQuaternion targetRotation = QuaternionMethods.setFromVNyanVector3(eulers);

                if ( !(targetRotation == fingerRotationsTarget[boneNum]) )
                {
                    fingerRotationsTarget[boneNum] = targetRotation;
                }
            }
        }

        /**
         * TODO Add description
         */
        public void rotateCurrentTowardsTarget()
        {
            // Rotate every bone's current rotation Towards Target
            foreach (int boneNum in PoseDictionary.getHandsBoneIndices())
            {
                VNyanQuaternion target = fingerRotationsTarget[boneNum];
                VNyanQuaternion current = fingerRotationsCurrent[boneNum];

                if ( !(current == target) )
                {
                    Quaternion newRotation = Quaternion.Slerp(QuaternionMethods.convertQuaternionV2U(current), QuaternionMethods.convertQuaternionV2U(target), slerpAmount * Time.deltaTime);
                    fingerRotationsCurrent[boneNum] = QuaternionMethods.convertQuaternionU2V(newRotation);
                    // fingerRotationsCurrent[boneNum] = target; 
                }
            }
        }

        // User Input Methods //
        // These are to just get the input states from vnyan parameters

        /**
         * TODO Add description
         */
        public void getInputStatesFromVNyan()
        {
            // Get current input states from VNyan
            foreach (string condition in fingerInputConditions)
            {
                setfingerInputState(condition, VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(condition));
            }
        }

        /**
         * TODO Add description
         */

        public void resetInputSimulateStates()
        {
            // Get current input states from VNyan
            foreach (string condition in fingerInputConditions)
            {
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(condition, 0f);
            }
        }
    }
}
