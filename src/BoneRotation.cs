using System;
using System.Collections.Generic;
using System.Text;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    class BoneRotation
    {
        //The index of the bone
        private int boneIndex

        //The rotation applied to the bone
        private VNyanQuaternion rotation;

        /**
         * Creates a new instance with no defined rotations.
         */
        public BoneRotation(int boneIndex)
        {
            this.boneIndex = boneIndex;
            this.rotation = new VNyanQuaternion { };
        }

        /**
         * Creates a new pose with a given 3D rotation.
         */
        public BoneRotation(int boneIndex, VNyanVector3 rot)
        {
            this.boneIndex = boneIndex;
            this.rotation = new VNyanQuaternion { X = rot.X, Y = rot.Y, Z = rot.Z, W = 0 };
        }

        /**
         * Creates a new pose with a given 4D rotation.
         */
        public BoneRotation(int boneIndex, VNyanQuaternion rot)
        {
            this.boneIndex = boneIndex;
            this.rotation = rot;
        }

        // Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns>boneIndex</returns>
        public int getBoneIndex()
        {
            return this.boneIndex;
        }

        /// <summary>
        /// Returns the rotation as a Vector3 by chopping off the W component.
        /// </summary>
        /// <returns>Rotation as a VNyanVector3</returns>
        public VNyanVector3 getEulerRotation()
        {
            return new VNyanVector3 { X = this.rotation.X, Y = this.rotation.Y, Z = this.rotation.Z};
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>rotation</returns>
        public VNyanQuaternion getRotation()
        {
            return this.rotation;
        }

        public void setRotation(VNyanVector3 rot)
        {
            this.rotation = new VNyanQuaternion { X = rot.X, Y = rot.Y, Z = rot.Z, W = 0 };
        }

        public void setRotation(VNyanQuaternion rot)
        {
            this.rotation = rot;
        }

    }
}
