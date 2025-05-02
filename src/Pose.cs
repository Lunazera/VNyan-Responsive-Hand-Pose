using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    class Pose
    {
        //Name of the pose. 
        private String name;

        //A dictionary of bone rotations that apply to this pose
        private Dictionary<int, BoneRotation> fingerInputs = new Dictionary<int, BoneRotation>();

        private Dictionary<string, Pose> inputPoses = new Dictionary<string, Pose> { };

        private List<string> fingerInputConditions = new List<string> { };

        private Dictionary<string, float> fingerInputStates = new Dictionary<string, float> { };
        
        /**
         * Creates a new pose with no bones.
         */
        public Pose(String name)
        {
            this.name = name;
        }

        /**
         * Creates a new pose with a list of bones, but no finger inputs.
         */
        public Pose(String name, ICollection<int> bones)
        {
            this.name = name;
            foreach (var ele in bones)
            {
                this.fingerInputs.Add(ele, new BoneRotation(ele));
            }
        }

        /**
         * Creates a new pose with a given set of finger inputs.
         */
        public Pose(String name, ICollection<BoneRotation> fingerInputs)
        {
            this.name = name;
            foreach (var ele in fingerInputs)
            {
                this.fingerInputs.Add(ele.getBoneIndex(), ele);
            }
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs.
         */
        public Pose(String name, Dictionary<int, BoneRotation> fingerInputs)
        {
            this.name = name;
            this.fingerInputs = fingerInputs;
        }

        /**
         * Creates a new pose with a list of bones, but no finger inputs, and a set of input poses.
         */
        public Pose(String name, ICollection<int> bones, Dictionary<string, Pose> inputPoses) : this(name, bones)
        {
            this.inputPoses = inputPoses;
        }

        /**
         * Creates a new pose with a given set of finger inputs, and a set of input poses.
         */
        public Pose(String name, ICollection<BoneRotation> fingerInputs, Dictionary<string, Pose> inputPoses): this(name, fingerInputs)
        {
            this.inputPoses = inputPoses;
        }

        /**
         * Creates a new pose with a given dictionary of finger inputs, and a set of input poses.
         */
        public Pose(String name, Dictionary<int, BoneRotation> fingerInputs, Dictionary<string, Pose> inputPoses) : this(name, fingerInputs)
        {
            this.inputPoses = inputPoses;
        }

        //Getters

        public String getName()
        {
            return name;
        }

        public Dictionary<int, BoneRotation> getFingerInputs()
        {
            return fingerInputs;
        }

        public List<string> getFingerInputConditions()
        {
            return fingerInputConditions;
        }

        public Dictionary<string, float> getFingerInputStates()
        {
            return fingerInputStates;
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
