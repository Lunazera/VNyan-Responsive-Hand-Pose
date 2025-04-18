﻿using System;
using System.Collections.Generic;
using VNyanInterface;
using LZQuaternions;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static UnityEngine.Random;
using System.Security;

namespace ControllerPose
{
    // This is split into two sections, this settings section to call/store all the data, and the Interface implementation below
    public class ResponsiveControllerSettings
    {
        // Layer Settings //
        public static bool layerActive = true; // flag for layer being active or not (when inactive, vnyan will stop reading from the rotations entirely)
        public static void setLayerOnOff(float val) => layerActive = (val == 1f) ? true : false;

        public static float layerActiveLeft = 0f;
        public static float layerActiveRight = 0f;

        public static string prefix = "LZ_ControllerPoseInput_";

        // Bone number lists to index only these bones sto impact
        public static List<int> ListFingers = new List<int> { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };
        public static List<int> ListFingersLeft = new List<int> { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };
        public static List<int> ListFingersRight = new List<int> { 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };
        public static int HandLeft = 17;
        public static int HandRight = 18;
        
        public static float slerpAmount = 1f;

        public static float getSlerpAmount()
        {
            return slerpAmount;
        }

        public static void setSlerpAmount(float val)
        {
            slerpAmount = val;
        }

        // Input Setting Dictionaries // 

        // There are two sets of dictionaries here

        // This first set is to store user input of the bone rotations in x/y/z
        // These are stored as Vector objects (have x/y/z properties)

        // We will maintain "Poses", and each pose will have a number of "Inputs" configured
        // We keep the Input settings in a separate dictionary

        // For example consider holding a controller
        // We need to store the "at rest" pose, and the pose for each button input.

        // Right now this is all set up for working with only one pose, but I want to reconfigure this to handle multiple poses and inputs for each

        // We also have a third dictionary for Input states, mainly it is written and read from to track which button inputs are being pressed (and so which pose inputs to activate)

        // ***
        // *** This whole part is mainly what i need help with. probably makes sense to make into its own class to maintain all the info.
        // *** since we have properties we want associated with each thing that are linked together
        // *** like, we want to create many poses. Each pose has a dictionary of its base finger rotations, AND a dictionary of inputs. Each input has its own small dictionary of finger rotations.
        // *** within each pose, we also want to maintain which input is current on or off. Input names will be vnyan parameters that you should be able to name anything.
        // so end result is, i should be able to say "turn on controller pose, and now listen for the input names within this pose, and turn on/off the input rotations depending on that" 


        // Dictionary of poses
        public static Dictionary<string, Dictionary<int, VNyanVector3>> fingerPoses = new Dictionary<string, Dictionary<int, VNyanVector3>> {
            { "default", new Dictionary<int, VNyanVector3>
                {
                    { 24, new VNyanVector3 {} },
                    { 25, new VNyanVector3 {} },
                    { 26, new VNyanVector3 {} },
                    { 27, new VNyanVector3 {} },
                    { 28, new VNyanVector3 {} },
                    { 29, new VNyanVector3 {} },
                    { 30, new VNyanVector3 {} },
                    { 31, new VNyanVector3 {} },
                    { 32, new VNyanVector3 {} },
                    { 33, new VNyanVector3 {} },
                    { 34, new VNyanVector3 {} },
                    { 35, new VNyanVector3 {} },
                    { 36, new VNyanVector3 {} },
                    { 37, new VNyanVector3 {} },
                    { 38, new VNyanVector3 {} },
                    { 39, new VNyanVector3 {} },
                    { 40, new VNyanVector3 {} },
                    { 41, new VNyanVector3 {} },
                    { 42, new VNyanVector3 {} },
                    { 43, new VNyanVector3 {} },
                    { 44, new VNyanVector3 {} },
                    { 45, new VNyanVector3 {} },
                    { 46, new VNyanVector3 {} },
                    { 47, new VNyanVector3 {} },
                    { 48, new VNyanVector3 {} },
                    { 49, new VNyanVector3 {} },
                    { 50, new VNyanVector3 {} },
                    { 51, new VNyanVector3 {} },
                    { 52, new VNyanVector3 {} },
                    { 53, new VNyanVector3 {} }
                }
            }
        };

        /*
        // Future Expansion:
        // Set up dictionaries so we can have a separate layer for different controller poses
        // Maybe better to even set up an object for each pose?
        // Input conditions should then exist within each pose object

        public static Dictionary<string, Dictionary<string, Dictionary<int, VNyanVector3>>> fingerPoseInputs = new Dictionary<string, Dictionary<string, Dictionary<int, VNyanVector3>>> {
            { "default", new Dictionary<string, Dictionary<int, VNyanVector3>>
                {
                }
            }
        };
        */

        // Dictionary of inputs that will modify the target eulers after the default
        public static Dictionary<string, Dictionary<int, VNyanVector3>> fingerInputs = new Dictionary<string, Dictionary<int, VNyanVector3>> { };

        // Dictionary of input settings states, this will track whether a button is pressed or not
        public static Dictionary<string, float> fingerInputStates = new Dictionary<string, float> { };

        // will be checked for simulated inputs
        // this isn't really being used rn i was just trying to find a way to let the program show the input without having to actually presss the button
        public static Dictionary<string, float> fingerInputSimulate = new Dictionary<string, float> { };

        // This just also keeps a list of the names of the inputs to check. this is messy and unneeded probably
        public static List<string> fingerInputConditions = new List<string> { };


        // Bone Rotation Dictionaries //
        // This is the second set of dictionaries
        // This is mainly a 3-dictionary setup, connecting Euler vectors to a target rotation, and then constantly rotating currently-tracked rotations towards the target

        // This layer of dictionaries should be agnostic to the pose and input settings above. 

        // Basically, we do *whatever* stuff to determine what we want the fingers to look like,
        // and then we write the result to fingerEulersTarget
        // The program will constantly compare between fingerEulersTarget, fingerRotationsTarget and fingerRotationsCurrent and adjust the rotations accordingly.

        // Logic:
        // 1. fingerEulersTarget is updated depending on pose and input settings above
        // 2. Each bone in fingerEulersTarget is written into fingerRotationsTarget, converting to quaternionss
        // 3. fingerRotationsCurrent bones are rotated towards each corresponding target in fingerRotationsTarget
        // 4. fingerRotationsCurrent are pushed to show on your model.

        // Overall idea is that the current bone rotation that we will display every frame. Whenever the targets are changed, the current rotations will rotate towards the target.

        //  Bone vector targets
        public static Dictionary<int, VNyanVector3> fingerEulersTarget = new Dictionary<int, VNyanVector3>
        {
            { 24, new VNyanVector3 {} },
            { 25, new VNyanVector3 {} },
            { 26, new VNyanVector3 {} },
            { 27, new VNyanVector3 {} },
            { 28, new VNyanVector3 {} },
            { 29, new VNyanVector3 {} },
            { 30, new VNyanVector3 {} },
            { 31, new VNyanVector3 {} },
            { 32, new VNyanVector3 {} },
            { 33, new VNyanVector3 {} },
            { 34, new VNyanVector3 {} },
            { 35, new VNyanVector3 {} },
            { 36, new VNyanVector3 {} },
            { 37, new VNyanVector3 {} },
            { 38, new VNyanVector3 {} },
            { 39, new VNyanVector3 {} },
            { 40, new VNyanVector3 {} },
            { 41, new VNyanVector3 {} },
            { 42, new VNyanVector3 {} },
            { 43, new VNyanVector3 {} },
            { 44, new VNyanVector3 {} },
            { 45, new VNyanVector3 {} },
            { 46, new VNyanVector3 {} },
            { 47, new VNyanVector3 {} },
            { 48, new VNyanVector3 {} },
            { 49, new VNyanVector3 {} },
            { 50, new VNyanVector3 {} },
            { 51, new VNyanVector3 {} },
            { 52, new VNyanVector3 {} },
            { 53, new VNyanVector3 {} }
        };

        // Bone Targets
        // this is a dictionary of the target rotations that will be maintained constantly
        public static Dictionary<int, VNyanQuaternion> fingerRotationsTarget = new Dictionary<int, VNyanQuaternion>
        {
            { 24, new VNyanQuaternion{} },
            { 25, new VNyanQuaternion{} },
            { 26, new VNyanQuaternion{} },
            { 27, new VNyanQuaternion{} },
            { 28, new VNyanQuaternion{} },
            { 29, new VNyanQuaternion{} },
            { 30, new VNyanQuaternion{} },
            { 31, new VNyanQuaternion{} },
            { 32, new VNyanQuaternion{} },
            { 33, new VNyanQuaternion{} },
            { 34, new VNyanQuaternion{} },
            { 35, new VNyanQuaternion{} },
            { 36, new VNyanQuaternion{} },
            { 37, new VNyanQuaternion{} },
            { 38, new VNyanQuaternion{} },
            { 39, new VNyanQuaternion{} },
            { 40, new VNyanQuaternion{} },
            { 41, new VNyanQuaternion{} },
            { 42, new VNyanQuaternion{} },
            { 43, new VNyanQuaternion{} },
            { 44, new VNyanQuaternion{} },
            { 45, new VNyanQuaternion{} },
            { 46, new VNyanQuaternion{} },
            { 47, new VNyanQuaternion{} },
            { 48, new VNyanQuaternion{} },
            { 49, new VNyanQuaternion{} },
            { 50, new VNyanQuaternion{} },
            { 51, new VNyanQuaternion{} },
            { 52, new VNyanQuaternion{} },
            { 53, new VNyanQuaternion{} }
        };


        // Bone Current
        public static Dictionary<int, VNyanQuaternion> fingerRotationsCurrent = new Dictionary<int, VNyanQuaternion>
        {
            { 24, new VNyanQuaternion{} },
            { 25, new VNyanQuaternion{} },
            { 26, new VNyanQuaternion{} },
            { 27, new VNyanQuaternion{} },
            { 28, new VNyanQuaternion{} },
            { 29, new VNyanQuaternion{} },
            { 30, new VNyanQuaternion{} },
            { 31, new VNyanQuaternion{} },
            { 32, new VNyanQuaternion{} },
            { 33, new VNyanQuaternion{} },
            { 34, new VNyanQuaternion{} },
            { 35, new VNyanQuaternion{} },
            { 36, new VNyanQuaternion{} },
            { 37, new VNyanQuaternion{} },
            { 38, new VNyanQuaternion{} },
            { 39, new VNyanQuaternion{} },
            { 40, new VNyanQuaternion{} },
            { 41, new VNyanQuaternion{} },
            { 42, new VNyanQuaternion{} },
            { 43, new VNyanQuaternion{} },
            { 44, new VNyanQuaternion{} },
            { 45, new VNyanQuaternion{} },
            { 46, new VNyanQuaternion{} },
            { 47, new VNyanQuaternion{} },
            { 48, new VNyanQuaternion{} },
            { 49, new VNyanQuaternion{} },
            { 50, new VNyanQuaternion{} },
            { 51, new VNyanQuaternion{} },
            { 52, new VNyanQuaternion{} },
            { 53, new VNyanQuaternion{} }
        };

        // Input Settings Methods //
        // Here are the many methods i've set up to do various things, many being getters and setters

        public static void setfingerInputState(string conditionName, float setting)
        {
            // Changes condition setting if input condition exists
            if ( checkInputCondition(conditionName) )
            {
                fingerInputStates[conditionName] = setting;
            }
        }

        public static void setfingerInputStateOn(string conditionName)
        {
            // Sets condition setting on if it exists
            if (checkInputCondition(conditionName))
            {
                fingerInputStates[conditionName] = 1f;
            }
        }

        public static void setfingerInputStateOff(string conditionName)
        {
            // Sets condition setting off if it exists
            if (checkInputCondition(conditionName))
            {
                fingerInputStates[conditionName] = 0f;
            }
        }

        public static float getFingerInputState(string conditionName)
        {
            // Returns the input setting state
            if (checkInputCondition(conditionName))
            {
                return fingerInputStates[conditionName];
            }
            else
            {
                return 0f;
            }
        }

        public static void addInputCondition(string conditionName)
        {
            // Adds input condition if it doesn't exist
            if ( !fingerInputs.ContainsKey(conditionName) )
            {
                Debug.Log("LZ_Controller: Input '" + conditionName + "' Not found, creating entry...");
                fingerInputs.Add(conditionName, new Dictionary<int, VNyanVector3> { });
                fingerInputStates.Add(conditionName, 0f);
                fingerInputConditions.Add(conditionName);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
            }
        }

        public static void addInputBone(string conditionName, int boneNum)
        {
            // Adds bone into input condition if it exists. If condition doesn't exist, creates it first
            if ( fingerInputs.ContainsKey(conditionName) )
            {
                if ( !checkInputConditionBone(conditionName, boneNum) )
                {
                    fingerInputs[conditionName].Add(boneNum, new VNyanVector3 { });
                }
            } 
            else
            {
                addInputCondition(conditionName);
                fingerInputs[conditionName].Add(boneNum, new VNyanVector3 { });
            }
        }
        public static void removeInputCondition(string conditionName)
        {
            if ( fingerInputs.ContainsKey(conditionName) )
            {
                // Removes input condition from settings
                Debug.Log("LZ_Controller: Removing input '" + conditionName + "'...");
                fingerInputs.Remove(conditionName);
                fingerInputStates.Remove(conditionName);
                fingerInputConditions.Remove(conditionName);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(prefix + conditionName, 0f);
            }
        }

        public static void removeInputBone(string conditionName, int boneNum)
        {
            // Removes bone from input condition if it exists
            if ( checkInputConditionBone(conditionName, boneNum) )
            {
                fingerInputs[conditionName].Remove(boneNum);
            }
        }

        public static bool checkInputCondition(string conditionName)
        {
            // Check if setting exists
            return (fingerInputs.ContainsKey(conditionName) && fingerInputStates.ContainsKey(conditionName) );
        }

        public static bool checkInputConditionBone(string conditionName, int boneNum)
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

        public static VNyanVector3 getFingerEuler(int boneNum, string pose = "default")
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

        public static float getFingerEulerAxis(int boneNum, int axis, string pose = "default")
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
        public static (float proximal, float middle, float distal, float splay, float twist, bool boneFound) GetFingerInputRotations(int boneNum, int CurlAxis, int SplayAxis, int TwistAxis, bool flipSides, bool flipSplaySides, bool flipTwistSides, string pose = "default", string condition = "default")
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

        public static (float proximal, float middle, float distal, float splay, float twist, bool boneFound) GetFingerPoseRotations(int boneNum, int CurlAxis, int SplayAxis, int TwistAxis, bool flipSides, bool flipSplaySides, bool flipTwistSides)
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


        public static void setInputBone(string conditionName, int boneNum, int axis, float angle)
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

        public static void setFingerSettingsAxisMirror(int boneNum, int axis, float angle, bool flip,  string conditionName)
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
                Debug.Log("Setting default bone " + boneNum);
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
                Debug.Log("Setting input '" + conditionName + "' bone " + boneNum);
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

        public static void setFingerSettingsAxis(int boneNum, int axis, float angle, string conditionName = "default")
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

        public static void resetFingerSettingsAxisMirror(int boneNum, int axis, string conditionName = "default")
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

        public static void resetFingerSettingsAxis(int boneNum, int axis, string conditionName = "default")
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

        public static bool checkIfFingerCurrentExists(int boneNum)
        {
            // Checks if the bone exists in the current dictionary
            return fingerRotationsCurrent.ContainsKey(boneNum);
        }

        public static void initTargetRotations()
        {
            // Sets all target rotations from euler settings
            foreach (int boneNum in ListFingers)
            {
                VNyanVector3 eulers = getFingerEuler(boneNum);
                fingerRotationsTarget[boneNum] = LZQuaternions.QuaternionMethods.setFromVNyanVector3(eulers);
            }
        }

        public static void initCurrentRotations(Dictionary<int, VNyanQuaternion> boneRotations)
        {
            // Set current pose to target
            foreach (int boneNum in ListFingers)
            {
                fingerRotationsCurrent[boneNum] = boneRotations[boneNum];
            }
        }

        // These three "setEulerTarget" methods connect the input settings to the main Euler Targets dictionary that gets read from
        public static void setEulerTarget(int boneNum, VNyanVector3 vector)
        {
            fingerEulersTarget[boneNum] = vector;
        }

        public static void setEulerTargetsDefault()
        {
            foreach (KeyValuePair<int, VNyanVector3> vector in fingerPoses["default"])
            {
                setEulerTarget(vector.Key, vector.Value);
            }
        }

        public static void setEulerTargetsInputs()
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

        public static void setFingerTargets()
        {
            // Set the target rotations from the target vectors
            foreach (int boneNum in ListFingers)
            {
                VNyanVector3 eulers = getFingerEuler(boneNum);
                VNyanQuaternion targetRotation = LZQuaternions.QuaternionMethods.setFromVNyanVector3(eulers);

                if ( !(targetRotation == fingerRotationsTarget[boneNum]) )
                {
                    fingerRotationsTarget[boneNum] = targetRotation;
                }
            }
        }

        public static void rotateCurrentTowardsTarget()
        {
            // Rotate every bone's current rotation Towards Target
            foreach (int boneNum in ListFingers)
            {
                VNyanQuaternion target = fingerRotationsTarget[boneNum];
                VNyanQuaternion current = fingerRotationsCurrent[boneNum];

                if ( !(current == target) )
                {
                    Quaternion newRotation = Quaternion.Slerp(LZQuaternions.QuaternionMethods.convertQuaternionV2U(current), LZQuaternions.QuaternionMethods.convertQuaternionV2U(target), slerpAmount * Time.deltaTime);
                    fingerRotationsCurrent[boneNum] = LZQuaternions.QuaternionMethods.convertQuaternionU2V(newRotation);
                    // fingerRotationsCurrent[boneNum] = target; 
                }
            }
        }

        // User Input Methods //
        // These are to just get the input states from vnyan parameters

        public static void getInputStatesFromVNyan()
        {
            // Get current input states from VNyan
            foreach (string condition in fingerInputConditions)
            {
                setfingerInputState(condition, VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(condition));
            }
        }

        public static void resetInputSimulateStates()
        {
            // Get current input states from VNyan
            foreach (string condition in fingerInputConditions)
            {
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(condition, 0f);
            }
        }
    }

    // IPoseLayer interface implementation, from VNyanInterface.dll.
    // We only really need to do our work in doUpdate()
    // VNyan itself mainly use these methods
    public class ResponsiveControllerLayer : IPoseLayer
    {
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

        // VNyan Get Methods, VNyan uses these to get the pose after doUpdate()
        VNyanVector3 IPoseLayer.getBonePosition(int i)
        {
            return BonePositions[i];
        }
        VNyanQuaternion IPoseLayer.getBoneRotation(int i)
        {
            return BoneRotations[i];
        }
        VNyanVector3 IPoseLayer.getBoneScaleMultiplier(int i)
        {
            return BoneScales[i];
        }
        VNyanVector3 IPoseLayer.getRootPosition()
        {
            return RootPos;
        }
        VNyanQuaternion IPoseLayer.getRootRotation()
        {
            return RootRot;
        }
        bool IPoseLayer.isActive()
        {
            return ResponsiveControllerSettings.layerActive;
        }

        // doUpdate() will run every frame when the layer is on
        public void doUpdate(in PoseLayerFrame ControllerPoseFrame)
        {
            // Get all current Bone and Root values up to this point from our Layer Frame, and load them in our holdover values.
            BoneRotations = ControllerPoseFrame.BoneRotation;
            BonePositions = ControllerPoseFrame.BonePosition;
            BoneScales = ControllerPoseFrame.BoneScaleMultiplier;
            RootPos = ControllerPoseFrame.RootPosition;
            RootRot = ControllerPoseFrame.RootRotation;

            // Set euler vector targets from settings
            ResponsiveControllerSettings.setEulerTargetsDefault();

            // Set the input condition settings depending on state
            ResponsiveControllerSettings.setEulerTargetsInputs();

            // Set rotation targets from vector targets
            ResponsiveControllerSettings.setFingerTargets();

            // Rotate current quats to rotation target 
            ResponsiveControllerSettings.rotateCurrentTowardsTarget();

            // Only run once avatar is loaded in
            if (!(VNyanInterface.VNyanInterface.VNyanAvatar == null))
            {
                // Get leap motion status
                leftmotiondetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("leftmotiondetect");
                rightmotiondetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("rightmotiondetect");
                MirrorTracking = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("MirrorTracking");

                // Apply pose to fingers
                if ( (leftmotiondetect == 0f) && (MirrorTracking == 0f) || (rightmotiondetect == 0f) && (MirrorTracking == 1f) ) 
                {
                    foreach (int boneNum in ResponsiveControllerSettings.ListFingersLeft)
                    {
                        if (ResponsiveControllerSettings.checkIfFingerCurrentExists(boneNum))
                        {
                            BoneRotations[boneNum] = ResponsiveControllerSettings.fingerRotationsCurrent[boneNum];
                        }
                    }
                }
                if ((rightmotiondetect == 0f) && (MirrorTracking == 0f) || (leftmotiondetect == 0f) && (MirrorTracking == 1f))
                {
                    foreach (int boneNum in ResponsiveControllerSettings.ListFingersRight)
                    {
                        if (ResponsiveControllerSettings.checkIfFingerCurrentExists(boneNum))
                        {
                            BoneRotations[boneNum] = ResponsiveControllerSettings.fingerRotationsCurrent[boneNum];
                        }
                    }
                }
            }  
        }
    }
}
