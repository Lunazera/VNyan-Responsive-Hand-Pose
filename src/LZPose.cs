﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    /* TODO
    * getting input list...
    * - Needs a way to include Sub-Sub poses in the list 
    * - Make sure that when we are getting the output vector, we are going into sub-sub poses
    * - We should take input int or float as scalar multiplier to vectors
    * - We should mix/average overlapping bone vectors when they are present
    */

    class LZPose
    {
        // Name of the pose. 
        private string name;

        // A dictionary of bone rotations that apply to this pose
        private Dictionary<int, BoneRotation> mainPose = new Dictionary<int, BoneRotation>();

        private Dictionary<string, LZPose> subPoses = new Dictionary<string, LZPose> { };
        
        /// <summary>
        /// Creates a new pose with no bones.
        /// </summary>
        /// <param name="name">Pose name</param>
        public LZPose(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a new pose with a list of bones, but no finger inputs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bones"></param>
        public LZPose(string name, ICollection<int> bones)
        {
            this.name = name;
            foreach (var ele in bones)
            {
                this.mainPose.Add(ele, new BoneRotation(ele));
            }
        }

        /// <summary>
        /// Creates a new pose with a given set of finger inputs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mainPose"></param>
        public LZPose(string name, ICollection<BoneRotation> mainPose)
        {
            this.name = name;
            foreach (var ele in mainPose)
            {
                this.mainPose.Add(ele.getBoneIndex(), ele);
            }
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs.
         */
        public LZPose(string name, Dictionary<int, BoneRotation> mainPose)
        {
            this.name = name;
            this.mainPose = mainPose;
        }

        /**
         * Creates a new pose with a list of bones, but no finger inputs, and a set of input poses.
         */
        public LZPose(string name, ICollection<int> bones, Dictionary<string, LZPose> subPoses) : this(name, bones)
        {
            this.subPoses = subPoses;
        }

        /**
         * Creates a new pose with a given set of finger inputs, and a set of input poses.
         */
        public LZPose(string name, ICollection<BoneRotation> mainPose, Dictionary<string, LZPose> subPoses): this(name, mainPose)
        {
            this.subPoses = subPoses;
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs, and a set of input poses.
         */
        public LZPose(string name, Dictionary<int, BoneRotation> mainPose, Dictionary<string, LZPose> subPoses) : this(name, mainPose)
        {
            this.subPoses = subPoses;
        }

        /// <summary>
        /// Gets BoneRotation from mainpose
        /// </summary>
        /// <param name="boneNum">Bone Number</param>
        /// <returns></returns>
        public BoneRotation getBone(int boneNum)
        {
            return this.mainPose[boneNum];
        }

        /// <summary>
        /// Gets BoneRotation checking for a subpose. If the subpose is found in the pose, it grabs that bone. Otherwise it gets the main pose bone
        /// </summary>
        /// <param name="boneNum">Bone Number</param>
        /// <param name="subpose">Subpose name</param>
        /// <returns></returns>
        public BoneRotation getBone(int boneNum, string subpose)
        {
            if (checksubPose(subpose))
            {
                return getsubPose(subpose).getBone(boneNum);
            } 
            else
            {
                Debug.Log("The sub pose '" + subpose + "' wasn't found, using default pose");
                return this.mainPose[boneNum];
            }
        }

        public VNyanVector3 getBoneEulerRotation(int boneNum)
        {
            return getBone(boneNum).getEulerRotation();
        }

        public VNyanVector3 getBoneEulerRotation(int boneNum, string subpose)
        {
            return getBone(boneNum, subpose).getEulerRotation();
        }

        public VNyanQuaternion getBoneRotation(int boneNum)
        {
            return getBone(boneNum).getRotation();
        }

        public VNyanQuaternion getBoneRotation(int boneNum, string subpose)
        {
            return getBone(boneNum, subpose).getRotation();
        }

        /// <summary>
        /// If the bone doesn't already exist, adds new blank bone to the pose dictionary
        /// </summary>
        /// <param name="boneNum"></param>
        public void setBone(int boneNum)
        {
            if (!checkBone(boneNum))
            {
                this.mainPose.Add(boneNum, new BoneRotation(boneNum));
            }
            
        }

        /// <summary>
        /// Sets bone to subpose if subpose exists, otherwise does nothing
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="name"></param>
        public void setBone(int boneNum, string subpose)
        {
            if (checksubPose(name))
            {
                this.getsubPose(subpose).setBone(boneNum);
            }
        }

        /// <summary>
        /// Adds bone with rotation to pose if bone doesn't exist. If it does, it set's that bone to this value
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="rotation"></param>
        public void setBone(int boneNum, VNyanVector3 rotation)
        {
            if (!checkBone(boneNum))
            {
                this.mainPose.Add(boneNum, new BoneRotation(boneNum, rotation));
            } 
            else
            {
                this.mainPose[boneNum] = new BoneRotation(boneNum, rotation);
            }
            
        }

        /// <summary>
        /// Sets bone with rotation to subpose if subpose exists, otherwise does nothing
        /// </summary>
        /// <param name="boneNum"></param>
        /// <param name="rotation"></param>
        /// <param name="name"></param>
        public void setBone(int boneNum, VNyanVector3 rotation, string subpose)
        {
            if (checksubPose(subpose))
            {
                this.getsubPose(subpose).setBone(boneNum, rotation);
            }
        }

        /// <summary>
        /// Removes bone from the dictonary if it exists
        /// </summary>
        /// <param name="boneNum"></param>
        public void removeBone(int boneNum)
        {
            if (checkBone(boneNum))
            {
                this.mainPose.Remove(boneNum);
            }
        }

        public void removeBone(int boneNum, string subpose)
        {
            if (checksubPose(subpose))
            {
                this.removeBone(boneNum);
            }
        }

        /// <summary>
        /// Checks if bone exists in dictonary
        /// </summary>
        /// <param name="boneNum"></param>
        /// <returns></returns>
        public bool checkBone(int boneNum)
        {
            return this.mainPose.ContainsKey(boneNum);
        }

        /// <summary>
        /// Creates new subpose within pose
        /// </summary>
        /// <param name="subpose"></param>
        public void setsubPose(string subpose)
        {
            if (!checksubPose(subpose))
            {
                this.subPoses.Add(subpose, new LZPose(subpose));
            }
        }

        /// <summary>
        /// Adds LZpose to subposes if it doesnt exist. If it does, then sets that pose to the new pose
        /// </summary>
        /// <param name="subpose"></param>
        public void setsubPose(string subpose, LZPose pose)
        {
            if (!checksubPose(subpose))
            {
                this.subPoses.Add(subpose, pose);
            }
            else
            {
                this.subPoses[subpose] = pose;
            }
        }

        /// <summary>
        /// Removes subpose within LZPose
        /// </summary>
        /// <param name="subpose"></param>
        public void removesubPose(string subpose)
        {
            if (checksubPose(subpose))
            {
                this.subPoses.Remove(subpose);
            }
        }

        /// <summary>
        /// Check if the subpose exists within the pose
        /// </summary>
        /// <returns>true or false</returns>
        public bool checksubPose(string subpose)
        {
            return subPoses.ContainsKey(subpose);
        }


        //Getters

        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Gets the mainPose containing all the bone rotations.
        /// Subposes will also contain their own mainPose, 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, BoneRotation> pose()
        {
            return mainPose;
        }

        public int[] getBoneNumbers()
        {

            return mainPose.Keys.ToArray();
        }

        /// <summary>
        /// Gets Sub Poses Dictionary
        /// </summary>
        /// <returns>Sub Pose Dictionary</returns>
        public Dictionary<string, LZPose> getsubPoses()
        {
            return subPoses;
        }

        /// <summary>
        /// Gets out a single subpose
        /// </summary>
        /// <returns>single sub pose within LZpose</returns>
        public LZPose getsubPose(string subpose)
        {
            return subPoses[subpose];
        }

        /// <summary>
        /// Gets array of bone numbers within the subpose
        /// [Maybe not needed?]
        /// </summary>
        /// <param name="subposeName">string name of the subpose</param>
        /// <returns>int array of bone numbers</returns>
        public int[] getsubPoseBoneNumbers(string subpose)
        {
            return getsubPose(subpose).pose().Keys.ToArray();
        }

        

        /// <summary>
        /// Gets a string list of all the sub pose names
        /// </summary>
        /// <returns>list of strings containing sub pose names</returns>
        public List<string> getInputs()
        {
            return new List<string>(subPoses.Keys);
        }

        //Setters

        public void setName(string name)
        {
            this.name = name;
        }

        /**
         * TODO:
         * - Setters to change Bone Rotations within either the mainPose or SubPoses
         *      - Once an LZPose is created, we will want to actually modify the pose in real time
         *      - Once Subposes are created, we will want to modify those poses too.
         * - Maybe rather than constantly changing the LZPose object itself...
         *      - we can create a copy of the object that is used to do the rotations and things, maybe that's basically just the "output" from getPoseOutput().
         *      - when changing settings of the pose using the UI, we just change that "output".
         *      - Then, when we're done we "save" it, which then takes the output dictionary and saves it under the subpose if any inputs are active, and if not saves it under the default/main pose
         */

        //Methods

        /** TODO
         * - We want to filter "sub poses" dictionary with the active poses 
         *      - ie: subposes has a, b, c. We activate a and c. so the filter should be a container with only a and c
         * - We want to handle what happens when multiple subposes are active
         *      - subposes = Dictionary<string, LZPose>, where each LZPose has Dictionary<int, BoneRotation>. Each BoneRotation has an int and Vector3/Quaternion.
         *      - We want to collapse or aggregate the subposes dictionary
         *          - The end result should be a Dictionary<int, BoneRotation>, where each entry is some combination of the different BoneRotations set for each bone int.
         *          - ie: Say subpose a and c both have a BoneRotation setting for bone 24. We want to take both of those and come up with a single setting for bone 24 to store
         *      - If the BoneRotations are Vector3, the combination will be an average between all the vectors, which is just (x1+x2)/2, (y1+y2)/2, (z1+z2)/2
         *      - If the BoneRotations are Quaternion, just take the last entry for now (averaging will be harder)
         * - We want a method to return the end result of the dictionary of BoneRotations, which will be mainPose combined with the aggregated subPoses
         *      - Here we will overwrite the bones in mainPose with the bone's set in subPoses if they are present.
         */

        /// <summary>
        /// Filters the subPoses based on an a list of active input states.
        /// </summary>
        /// <param name="ActiveInputs">List of strings of inputs that are "on", set externally</param>
        /// <returns>Filtered subPoses dictionary</returns>
        public Dictionary<string, LZPose> filterSubPoses(List<string> ActiveInputs)
        {
            return subPoses.Where(x => ActiveInputs.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Calculate the average of a list of BoneRotations and applies the new rotation to each bone.
        /// </summary>
        /// <param name="boneRotationList"></param>
        /// /// <returns>A new list with all the bones from the input list, and the rotation set to the average vector</returns>
        public List<BoneRotation> averageBoneRotations(List<BoneRotation> boneRotationList)
        {
            int numBones = boneRotationList.Count();

            float meanX = boneRotationList.Sum(rot => rot.getRotation().X)/ numBones;
            float meanY = boneRotationList.Sum(rot => rot.getRotation().Y)/ numBones;
            float meanZ = boneRotationList.Sum(rot => rot.getRotation().Z)/ numBones;
            float meanW = boneRotationList.Sum(rot => rot.getRotation().W)/ numBones;

            VNyanQuaternion meanRot = new VNyanQuaternion { X = meanX, Y = meanY, Z = meanZ, W = meanW };

            List<BoneRotation> meanBones = new List<BoneRotation> { };

            boneRotationList.ForEach(rot => meanBones.Add(new BoneRotation(rot.getBoneIndex(), meanRot)));

            return meanBones;
        }

        /* TODO
         * - Vector interpolation should be an option/setting for subposes in some way
         * - if unchecked, when overlapping rotations, take only last in list
         * - if checked, linearly combine the vectors
         * We need to know which parameter to check against
         * - Everything that uses this input will get interpolated together.
         * 
         * IE Joystick
         * - DefaultRight
         * - - x+ x- y+ y-
         * - - These four subposes will set 'DefaultRight' as their comparison
         * note... luna meant "compare against name of parameter"
         * 
         * BUT 
         * we can do sub-poses of sub-poses 
         */




        /// <summary>
        /// 
        /// We want to collect all the bone rotations according to each bone number for each sub pose, so we can mix the values together.
        /// We start off with a dictionary of poses per input state.
        /// We'll ignore the input state part for now, and just say "we want to collaps the dictionaries => <int, List<LZPose>>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, LZPose> mixSubPoses()
        {
            Dictionary<string, LZPose> output = new Dictionary<string, LZPose> { };

            List<BoneRotation> allSubBones = new List<BoneRotation>();

            //Iterate through each subpose and collect all the bones
            foreach (LZPose subPose in subPoses.Values)
            {
                //Ignore subposes inside subposes for now, as that'll need to be a recursive method
                foreach (KeyValuePair<int, BoneRotation> kvp in mainPose)
                {
                    //Add all bones that haven't already been added
                    allSubBones.Add(kvp.Value);
                }
            }

            List<BoneRotation> meanBones = averageBoneRotations(allSubBones);

            //TODO actually do something with meanBones

            //List<Dictionary<int,BoneRotation>> subPosesWithBone = subPoses.Select(a => a.Value.getMainPose().Where(b => b.Key == boneNum).ToDictionary(b => b.Key, b => b.Value)).ToList();

            return output;
        }

        /// <summary>
        /// This should return the final result of the Pose, taking the default/main pose + the mixed-together subposes
        /// - subpose bones will mix together by taking average when the same bones are set
        /// - Default bones will be replaced by mixed subpose bones when they are set, otherwise pass through the default bones
        /// - This needs to be a dictionary of VnyanVectory3, not BoneRotation, because it is going to be used by our vector/quaternion methods
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, VNyanVector3> getPoseOutput(List<string> subposes)
        {
            Dictionary<int, VNyanVector3> output = new Dictionary<int, VNyanVector3> { };

            //TODO Implement me

            return output;
        }
    }
}
