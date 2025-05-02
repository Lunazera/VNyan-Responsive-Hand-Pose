using System;
using System.Collections.Generic;
using System.Text;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    class BoneRotation
    {
        //The index of the bone
        private int boneIndex;

        public int getBoneIndex()
        {
            return boneIndex;
        }

        //The 3D (Euler) rotation applied to the bone
        private VNyanVector3 eulerRotation;

        public VNyanVector3 getEulerRotation()
        {
            return eulerRotation;
        }

        //The 4D rotation applied to the bone
        private VNyanQuaternion rotation;

        public VNyanQuaternion getRotation()
        {
            return rotation;
        }

        /**
         * Creates a new instance with no defined rotations.
         */
        public BoneRotation(int boneIndex)
        {
            this.boneIndex = boneIndex;
            this.eulerRotation = new VNyanVector3 { };
            this.rotation = new VNyanQuaternion { };
        }

        /**
         * Creates a new pose with a given set of 3D (Euler) rotations.
         */
        public BoneRotation(int boneIndex, VNyanVector3 eulerRotation)
        {
            this.boneIndex = boneIndex;
            this.eulerRotation = eulerRotation;
            this.rotation = new VNyanQuaternion { };
        }

        /**
         * Creates a new pose with a given set of 4D rotations.
         */
        public BoneRotation(int boneIndex, VNyanQuaternion rotation)
        {
            this.boneIndex = boneIndex;
            this.eulerRotation = new VNyanVector3 { };
            this.rotation = rotation;
        }

        /**
         * Creates a new pose with a given set of 3D and 4D rotations.
         */
        public BoneRotation(int boneIndex, VNyanVector3 eulerRotation, VNyanQuaternion rotation)
        {
            this.boneIndex = boneIndex;
            this.eulerRotation = eulerRotation;
            this.rotation = rotation;
        }
    }
}
