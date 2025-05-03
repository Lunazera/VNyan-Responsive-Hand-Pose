using System;
using System.Collections.Generic;
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
        private Dictionary<int, BoneRotation> defaultPose = new Dictionary<int, BoneRotation>();

        private Dictionary<string, Pose> inputPoses = new Dictionary<string, Pose> { };
        
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
                this.defaultPose.Add(ele, new BoneRotation(ele));
            }
        }

        /// <summary>
        /// Creates a new pose with a given set of finger inputs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultPose"></param>
        public LZPose(String name, ICollection<BoneRotation> defaultPose)
        {
            this.name = name;
            foreach (var ele in defaultPose)
            {
                this.defaultPose.Add(ele.getBoneIndex(), ele);
            }
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs.
         */
        public LZPose(String name, Dictionary<int, BoneRotation> defaultPose)
        {
            this.name = name;
            this.defaultPose = defaultPose;
        }

        /**
         * Creates a new pose with a list of bones, but no finger inputs, and a set of input poses.
         */
        public LZPose(String name, ICollection<int> bones, Dictionary<string, Pose> inputPoses) : this(name, bones)
        {
            this.inputPoses = inputPoses;
        }

        /**
         * Creates a new pose with a given set of finger inputs, and a set of input poses.
         */
        public LZPose(String name, ICollection<BoneRotation> defaultPose, Dictionary<string, Pose> inputPoses): this(name, defaultPose)
        {
            this.inputPoses = inputPoses;
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs, and a set of input poses.
         */
        public LZPose(String name, Dictionary<int, BoneRotation> defaultPose, Dictionary<string, Pose> inputPoses) : this(name, defaultPose)
        {
            this.inputPoses = inputPoses;
        }

        //Getters

        public String getName()
        {
            return name;
        }

        public Dictionary<int, BoneRotation> getdefaultPose()
        {
            return defaultPose;
        }

        public Dictionary<string, Pose> getInputPoses()
        {
            return inputPoses;
        }

        //Setters

        public void setName(String name)
        {
            this.name = name;
        }
    }
}
