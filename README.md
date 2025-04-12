# VNyan Responsive Hand Posing

ControllerPose.cs is the main codebase that does everything

ResponsiveControllerPlugin.cs is the portion of code that just sets up the layer to run in VNyan, and get/set parameters based on the UI

the UI-Bones folder has a bunch of code snippets that are used in unity to handle the UI.
mainly, LZ_FingerSliders does most of the work for linking moving a slider to changing the settings dictionaries.
LZ_FingerSliders_MirrorToggle (not a great name) wraps on top of that to handle some settings linked between finger sliders

also note, i use common names for some of the bones/axis to reference them more intuitively. Proximal/Middle/Distal refers to the three bone segments in a finger. If the first bone in your finger was 24, then the corresponding bone numbers will be 24, 25, 26. We also only look at a single axis, like we only want the finger to curl, usually its the Y axis.
Splay and Twist are the other two axis on JUST the proximal bone.