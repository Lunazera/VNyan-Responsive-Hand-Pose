﻿using System.Collections.Generic;
using VNyanInterface;

namespace ResponsiveControllerPlugin
{
    // IPoseLayer interface implementation, from VNyanInterface.dll.
    // We only really need to do our work in doUpdate()
    // VNyan itself mainly use these methods
    class ResponsiveControllerLayer : IPoseLayer
    {
        private ResponsiveControllerLayerSettings settings = new ResponsiveControllerLayerSettings();

        //Prefix for parameters set in VNyan, so they can be easily identified in the Monitor tool
        public static string prefix = "LZ_ControllerPoseInput_";

        // Set up our frame-by-frame information
        public PoseLayerFrame ControllerPoseFrame = new PoseLayerFrame();

        // Create containers to load pose data each frame
        public Dictionary<int, VNyanQuaternion> BoneRotations;
        public Dictionary<int, VNyanVector3> BonePositions;
        public Dictionary<int, VNyanVector3> BoneScales;
        public VNyanVector3 RootPos;
        public VNyanQuaternion RootRot;

        public float leftmotiondetect = 0f;
        public float rightmotiondetect = 0f;
        public float MirrorTracking = 0f;

        
        private string currentPoseName;
        private string currentSubposeName;


        // VNyan Get Methods, VNyan uses these to get the pose after doUpdate()

        /**
         * TODO Add description
         */
        VNyanVector3 IPoseLayer.getBonePosition(int i)
        {
            return BonePositions[i];
        }

        /**
         * TODO Add description
         */
        VNyanQuaternion IPoseLayer.getBoneRotation(int i)
        {
            return BoneRotations[i];
        }

        /**
         * TODO Add description
         */
        VNyanVector3 IPoseLayer.getBoneScaleMultiplier(int i)
        {
            return BoneScales[i];
        }

        /**
         * TODO Add description
         */
        VNyanVector3 IPoseLayer.getRootPosition()
        {
            return RootPos;
        }

        /**
         * TODO Add description
         */
        VNyanQuaternion IPoseLayer.getRootRotation()
        {
            return RootRot;
        }

        /**
         * TODO Add description
         */
        bool IPoseLayer.isActive()
        {
            return settings.isLayerActive();
        }

        /**
         * TODO Add description
         */
        public ResponsiveControllerLayer()
        {
            settings.setDebug(false);
        }

        /**
         * TODO Add description
         */
        public ResponsiveControllerLayer(bool debug)
        {
            settings.setDebug(debug);
        }

        /**
         * TODO Add description
         */
        public ResponsiveControllerLayerSettings getSettings()
        {
            return settings;
        }

        /**
         * Updates the layer.
         * 
         * doUpdate() will run every frame when the layer is on
         */
        public void doUpdate(in PoseLayerFrame ControllerPoseFrame)
        {
            // Get all current Bone and Root values up to this point from our Layer Frame, and load them in our holdover values.
            BoneRotations = ControllerPoseFrame.BoneRotation;
            BonePositions = ControllerPoseFrame.BonePosition;
            BoneScales = ControllerPoseFrame.BoneScaleMultiplier;
            RootPos = ControllerPoseFrame.RootPosition;
            RootRot = ControllerPoseFrame.RootRotation;

            /**
             * We are going to assume that the Rotations Eulers target is already loaded.
             * In truth this will be loaded and changed elsewhere, either according to button inputs or 
             * UI settings
            */

            string incomingPose = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString("LZPose");

            // If pose name changes, we will load that new pose
            if (!(currentPoseName == incomingPose)) 
            {
                settings.loadLZPose(incomingPose);
                currentPoseName = incomingPose;
            }


            // Get in the target Euler dictionary, calls "getposeoutput"
            settings.loadTargetPose(new List<string> { incomingsubPose });

            // Convert Target Eulers to Target Quaternion
            settings.updateRotationsTarget();

            // Rotate current quats to rotation target 
            settings.rotateCurrentTowardsTarget();

            // Only run once avatar is loaded in
            if (!(VNyanInterface.VNyanInterface.VNyanAvatar == null))
            {
                // Get leap motion status
                leftmotiondetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("leftmotiondetect");
                rightmotiondetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("rightmotiondetect");
                MirrorTracking = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("MirrorTracking");

                // Apply pose to fingers
                if ( ((leftmotiondetect == 0f) && (MirrorTracking == 0f)) || ((rightmotiondetect == 0f) && (MirrorTracking == 1f)) )
                {
                    foreach (int boneNum in PoseUtils.getLeftHandBoneIndices())
                    {
                        BoneRotations[boneNum] = settings.getRotationsCurrentBone(boneNum);
                    }
                }
                if ( ((rightmotiondetect == 0f) && (MirrorTracking == 0f)) || ((leftmotiondetect == 0f) && (MirrorTracking == 1f)) )
                {
                    foreach (int boneNum in PoseUtils.getRightHandBoneIndices())
                    {

                        BoneRotations[boneNum] = settings.getRotationsCurrentBone(boneNum);
                    }
                }
            }
        }
    }
}
