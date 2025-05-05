using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    class LZPose
    {
        //Name of the pose. 
        private String name;

        //A dictionary of bone rotations that apply to this pose
        private Dictionary<int, BoneRotation> mainPose = new Dictionary<int, BoneRotation>();

        private Dictionary<string, LZPose> subPoses = new Dictionary<string, LZPose> { };
        
        /// <summary>
        /// Creates a new pose with no bones.
        /// </summary>
        /// <param name="name">Pose name</param>
        public LZPose(String name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a new pose with a list of bones, but no finger inputs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bones"></param>
        public LZPose(String name, ICollection<int> bones)
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
        public LZPose(String name, ICollection<BoneRotation> mainPose)
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
        public LZPose(String name, Dictionary<int, BoneRotation> mainPose)
        {
            this.name = name;
            this.mainPose = mainPose;
        }

        /**
         * Creates a new pose with a list of bones, but no finger inputs, and a set of input poses.
         */
        public LZPose(String name, ICollection<int> bones, Dictionary<string, LZPose> subPoses) : this(name, bones)
        {
            this.subPoses = subPoses;
        }

        /**
         * Creates a new pose with a given set of finger inputs, and a set of input poses.
         */
        public LZPose(String name, ICollection<BoneRotation> mainPose, Dictionary<string, LZPose> subPoses): this(name, mainPose)
        {
            this.subPoses = subPoses;
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs, and a set of input poses.
         */
        public LZPose(String name, Dictionary<int, BoneRotation> mainPose, Dictionary<string, LZPose> subPoses) : this(name, mainPose)
        {
            this.subPoses = subPoses;
        }

        //Getters

        public String getName()
        {
            return name;
        }

        public Dictionary<int, BoneRotation> getmainPose()
        {
            return mainPose;
        }

        public int[] getBoneNumbers()
        {

            return mainPose.Keys.ToArray();
        }

        public Dictionary<string, LZPose> getsubPoses()
        {
            return subPoses;
        }

        //Setters

        public void setName(String name)
        {
            this.name = name;
        }

        //Methods

        /* TODO
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
        /// <param name="inputStates">List of strings of inputs, set externally</param>
        /// <returns>Filtered subPoses dictionary</returns>
        public Dictionary<string, LZPose> filterSubPoses(List<string> inputStates)
        {
            return subPoses.Where(x => inputStates.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
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

            /* Attempt 1:
            foreach (int boneNum in getBoneNumbers())
            {
                foreach (KeyValuePair<string, LZPose> subPose in subPoses)
                {
                    Dictionary<int, BoneRotation> subPoseRotations = subPose.Value.getmainPose();
                    if (subPoseRotations.ContainsKey(boneNum))
                    {

                    }
                }
                
            }
            */

            /* Attempt 2 */
            foreach (int boneNum in getBoneNumbers())
            {
                // Get list of all the subpose dictionaries that contain an entry for the bone number
                List<Dictionary<int,BoneRotation>> subPosesWithBone = subPoses.Select(a => a.Value.getmainPose().Where(b => b.Key == boneNum).ToDictionary(b => b.Key, b => b.Value)).ToList();

                // 

            }
            
            return output;
        }
    }
}
