using System.Collections.Generic;
using VNyanInterface;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Linq;
using System.Dynamic;
using Newtonsoft.Json;
using System;
using System.Xml.Linq;

// TODO: We probably want a way to revert changes??? and not apply them unless we want to save it

namespace ResponsiveControllerPlugin
{
    // This is split into two sections, this settings section to call/store all the data, and the Interface implementation below
    class ResponsiveControllerLayerSettings
    {
        //If true, Debug.Log messages will be printed out
        private bool debug = false;

        // Layer Settings //
        private bool layerActive = true; // flag for layer being active or not (when inactive, vnyan will stop reading from the rotations entirely)

        private float slerpAmount = 1f;

        private LZPose loadedPose;

        public string activesubPose {  get; set; }


        private Dictionary<string, LZPose> LZPoseDictionary = new Dictionary<string, LZPose>();

        
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

        // TODO: do we actually need to "set" these? they are kind of going to be created on the fly based on whatever the pose output is.
        // = PoseUtils.createQuaternionDictionary();
        // Bone Vectors read from chosen LZPose (default mixed with active inputs)
        private Dictionary<int, VNyanVector3> RotationsEuler;

        // Bone rotation targets as Quaternions's to read into the current bone rotations
        private Dictionary<int, VNyanQuaternion> RotationsTarget = PoseUtils.createQuaternionDictionary();

        // Current bone rotations of the avatar model to read into pose layer
        private Dictionary<int, VNyanQuaternion> RotationsCurrent = PoseUtils.createQuaternionDictionary();

        // Dictionary of input states currently active
        private Dictionary<string, int> InputStates = new Dictionary<string, int>();


        /* LoadedPose points to an entry in the LZPoseDictionary
         */
        public LZPose getLoadedPose()
        {
            return loadedPose;
        }

        public void setLoadedPose(string name)
        {
            if (checkLZPose(name))
            {
                this.loadedPose = this.LZPoseDictionary[name];
            } 
            else
            {
                Debug.Log("The pose " + name + " was not found in the Pose dictionary.");
            }
        }

        public Dictionary<string, LZPose> getLZPoseDictionary() 
        { 
            return LZPoseDictionary; 
        }
        public LZPose getLZPoseDictionary(string name)
        {
            return LZPoseDictionary[name];
        }
        public void setLZPoseDictionary(Dictionary<string, LZPose> newDictionary)
        {
            LZPoseDictionary = newDictionary;
        }

        /// <summary>
        /// Saves the full pose dictionary into string JSON
        /// </summary>
        /// <returns></returns>
        public string SerializePoses()
        {
            Debug.Log("Converting Pose dictionary to Json");
            return JsonConvert.SerializeObject(getLZPoseDictionary());
        }

        /// <summary>
        /// Loads full pose dictionary from string JSON
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Dictionary<string, LZPose> DeserializePoses(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, LZPose>>(json);
        }

        public void LoadPosesDictionary(string json)
        {
            setLZPoseDictionary(DeserializePoses(json));
            Debug.Log("Pose dictionary loaded from JSON!");
        }



        // Rotations Euler Dictionary //
        //

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
        /// Updates the bones in RotationsTarget dictionary by converting from the Rotation Eulers
        /// We get the Euler rotation, convert it to a quaternion, then set that quaternion to Rotations Target
        /// </summary>
        public void updateRotationsTarget()
        {
            foreach (int boneNum in PoseUtils.getHandsBoneIndices())
            {
                setRotationsTargetBone(boneNum, QuaternionMethods.setFromVNyanVector3( getRotationsEulersBone(boneNum) ));
            }
        }

        // Rotations Current Dictionary //
        //

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


        // Inputs Dictionary //
        //
        // We will keep a dictionary of all the inputs we need to listen to when an LZ pose is active
        // We are constantly grabbing these from within vnyan parameters, so if we don't need to we don't want to do extra work


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

      
        // Working with LZPoses //

        /*
         * LoadedPost should point to one of the poses within the LZPse dictionary. 
         */

        /// <summary>
        /// Loads a new LZPose, using the Working dictionary to reference the main pose  
        /// </summary>
        /// <param name="pose"></param>
        public void loadLZPose(string name)
        {
            this.setLoadedPose(name);
            loadInputs(getLoadedPose());
            loadTargetPose(getLoadedPose(), new List<string> { });
        }

        /// <summary>
        /// Load LZPose output with single subpose into our RotationsEuler Dict
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="subpose"></param>
        public void loadLZPose(string name, string subpose)
        {
            this.setLoadedPose(name);
            if (checkLZPose(name, subpose))
            {
                loadTargetPose(getLoadedPose(), new List<string> { subpose });
            }
            else
            {
                Debug.Log("The subpose '" + subpose + "' was not found in the pose");
            }
        }

        /// <summary>
        /// Load LZPose output with mixed subpose into our RotationsEuler Dict
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="subposes"></param>
        public void loadLZPose(string name, List<string> subposes)
        {
            this.setLoadedPose(name);
            loadTargetPose(getLoadedPose(), subposes);
        }

        /// <summary>
        /// Sets the InputStates Dictionary from the current pose, converting the list to a dictonary of states
        /// </summary>
        /// <param name="pose"></param>
        public void loadInputs(LZPose pose)
        {
            setInputsFromList(pose.getInputs());
        }

        /// <summary>
        /// Loads the Target Eulers dictionary from the pose based on it's output
        /// </summary>
        /// <param name="pose"></param>
        public void loadTargetPose(LZPose pose, List<string> subposes)
        {
            RotationsEuler = pose.getPoseOutput(subposes);
        }

        public void loadTargetPose( List<string> subposes)
        {
            RotationsEuler = getLoadedPose().getPoseOutput(subposes);
        }


        /// <summary>
        /// Creates new LZ pose if it doesn't exist and adds to dictionary
        /// </summary>
        /// <param name="name"></param>
        public void createLZPose(string name)
        {
            if (!checkLZPose(name))
            {
                LZPose newPose = new LZPose(name);
                this.getLZPoseDictionary().Add(name, newPose);
            }
        }

        // Create new LZPose object with given mainpose and subposes
        // - this will effectively duplicate the currently loaded pose into a new pose
        public void createLZPose(string name, string currentPose)
        {
            if (!checkLZPose(name) && checkLZPose(currentPose))
            {
                LZPose newPose = new LZPose(name, getLZPoseDictionary(currentPose).pose(), getLZPoseDictionary(currentPose).getsubPoses() );
                this.getLZPoseDictionary().Add(name, newPose);
            }
        }

        // Remove LZPose object
        public void removeLZPose(string name)
        {
            if (checkLZPose(name))
            {
                this.getLZPoseDictionary().Remove(name);
            }
        }

        
        /// <summary>
        /// Check if pose exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool checkLZPose(string name)
        {
            return this.getLZPoseDictionary().ContainsKey(name);
        }

        /// <summary>
        /// Check if subpose exists within pose in dictionary
        /// </summary>
        /// <param name="name"></param>
        /// <param name="subpose"></param>
        /// <returns></returns>
        public bool checkLZPose(string name, string subpose)
        {
            if (checkLZPose(name))
            {
                return this.getLZPoseDictionary(name).checksubPose(subpose);
            }
            else
            {
                return false;
            }
        }


        // Current/Target Rotation Methods //
        // Get LZPose euler rotation by x/y/z axis
        // - if no inputs active, this should be the mainpose
        // - if input is active, this should be the active one
        public VNyanVector3 getPoseBone(int boneNum)
        {
            return getLoadedPose().getBoneEulerRotation(boneNum);
        }

        public VNyanVector3 getPoseBone(int boneNum, string subpose)
        {
            return getLoadedPose().getBoneEulerRotation(boneNum, subpose);
        }

        // Set LZPose mainpose or subpose euler rotation by x/y/z axis
        // - if no inputs active, this should be the mainpose
        // - if input is active, this should be the active one
        public void setPoseBone(int boneNum, VNyanVector3 rotation)
        {
            getLoadedPose().setBone(boneNum, rotation);
        }

        public void setPoseBone(int boneNum, VNyanVector3 rotation, string subpose)
        {
            getLoadedPose().setBone(boneNum, rotation, subpose);
        }

        /// <summary>
        /// Removes bone from the pose
        /// </summary>
        /// <param name="boneNum"></param>
        public void removePoseBone(int boneNum)
        {
            getLoadedPose().removeBone(boneNum);
        }

        /// <summary>
        /// Removes bone from the subpose if subpose exists
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="subpose"></param>
        public void removePoseBone(int boneNum, string subpose)
        {
            getLoadedPose().removeBone(boneNum, subpose);
        }

        //
        // METHODS FOR UI
        //
        // note: resetting is really the same thing as deleting.

        // Get pose bone by one axis
        // choose x/y/z = 0/1/2
        public float getPoseBoneAxis(int boneNum, int axis)
        {
            switch(axis)
            {
                case 0:
                   return getPoseBone(boneNum).X;
                case 1:
                    return getPoseBone(boneNum).Y;
                case 2:
                    return getPoseBone(boneNum).Z;
                default:
                    return 0;
            }
        }

        public float getPoseBoneAxis(int boneNum, string name, int axis)
        {
            /**
             * This will return the default pose bone if the subpose isn't set.
             * the context of this is showing what is on screen. if a subpose is active, any bone that isn't set will be
             * defaulted to the default mainpose. This is what the sliders should show, since that's where the fingers are
             * We will have to indicate that the bone is not set within the pose visually
             */
            switch (axis)
            {
                case 0:
                    return getPoseBone(boneNum, name).X;
                case 1:
                    return getPoseBone(boneNum, name).Y;
                case 2:
                    return getPoseBone(boneNum, name).Z;
                default:
                    return 0;
            }
        }

        // Set pose bone by one axis
        // choose x/y/z, 
        public void setPoseBoneAxis(int boneNum, int axis, float angle)
        {
            switch (axis)
            {
                case 0:
                    getPoseBone(boneNum).X = angle;
                    break;
                case 1:
                    getPoseBone(boneNum).Y = angle;
                    break;
                case 2:
                    getPoseBone(boneNum).Z = angle;
                    break;
            }
        }

        public void setPoseBoneAxis(int boneNum, string name, int axis, float angle)
        {
            switch (axis)
            {
                case 0:
                    getPoseBone(boneNum, name).X = angle;
                    break;
                case 1:
                    getPoseBone(boneNum, name).Y = angle;
                    break;
                case 2:
                    getPoseBone(boneNum, name).Z = angle;
                    break;
            }
        }

        public void resetPoseBoneAxis(int boneNum, int axis)
        {
            setPoseBoneAxis(boneNum, axis, 0);
        }

        public void resetPoseBoneAxis(int boneNum, int axis, string name)
        {
            setPoseBoneAxis(boneNum, name, axis, 0);
        }

        /** Finger Bone methods
        * These are unique for fingers based on how they are organized and articulated,
        * mainly to make working with them in the UI easier
        */

        /// <summary>
        /// Helper method to return tuple containing predictable structure of finger bones
        /// A finger is broken into three bones: proximal, intermediate, and distal. If you know the proximal bone, the others are predictable
        /// We can only articulate on one axis, the "Curl" axis
        /// We can also "Splay" our fingers from starting finger bone
        /// We can also kind of "twist" from the same bone (not really, but it's useful in posing)
        /// </summary>
        /// <param name="startingBoneNum"></param>
        /// <param name="curlAxis"></param>
        /// <param name="splayAxis"></param>
        /// <param name="twistAxis"></param>
        /// <returns></returns>
        public (float proximal, float intermediate, float distal, float splay, float twist) getFingerBones(int startingBoneNum, int curlAxis, int splayAxis, int twistAxis)
        {
            float proximal = getPoseBoneAxis(startingBoneNum, curlAxis);
            float intermediate = getPoseBoneAxis(startingBoneNum + 1, curlAxis);
            float distal = getPoseBoneAxis(startingBoneNum + 2, curlAxis);
            float splay = getPoseBoneAxis(startingBoneNum, splayAxis);
            float twist = getPoseBoneAxis(startingBoneNum, twistAxis);

            return (proximal, intermediate, distal, splay, twist);
        }

        public (float proximal, float intermediate, float distal, float splay, float twist) getFingerBones(int startingBoneNum, string name, int curlAxis, int splayAxis, int twistAxis)
        {
            float proximal = getPoseBoneAxis(startingBoneNum, name, curlAxis);
            float intermediate = getPoseBoneAxis(startingBoneNum + 1, name, curlAxis);
            float distal = getPoseBoneAxis(startingBoneNum + 2, name, curlAxis);
            float splay = getPoseBoneAxis(startingBoneNum, name, splayAxis);
            float twist = getPoseBoneAxis(startingBoneNum, name, twistAxis);

            return (proximal, intermediate, distal, splay, twist);
        }
 
    }
}
