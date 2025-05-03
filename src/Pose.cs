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

        /// <summary>
        /// Filters the subPoses based on an a list of active input states.
        /// </summary>
        /// <param name="inputStates">List of strings of inputs, set externally</param>
        /// <returns>Filtered subPoses dictionary</returns>
        public Dictionary<string, LZPose> filterSubPoses(List<string> inputStates)
        {
            return subPoses.Where(x => inputStates.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }


        public BoneRotation averageBoneRotations(List<BoneRotation> boneRotationList)
        {
            return null;
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
