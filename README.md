# Point Cloud Interactive Viewer

The following project was created as part of an ongoing project at the VUDP union (a union supporting projects improving effectiveness and quality within the Danish water sector), 
and explores various new features within 3D point cloud viewing software to aid usability when exploring point cloud data.

The design was inspired by other 3D viewing software such as Potree, PointView, CloudCompare and SAMP's Shared Reality, 
and explores features (not currently utilized within the 3D point cloud viewing software used by the Danish water sector) such as: 
- Bookmark Navigation
- Exploded View
- Instance Selection
- AI Assistance (LLM integration)

The implementation has used other libraries and repositories for rendering point clouds within the Unity game engine, as well as for integrating the LLM capabilities. 
Primarily this prototype uses the BA_PointClouds scripts and shaders by Simon Fraiss for visualising point clouds within the Unity environment. 
Some of the scripts have been modified to allow point clouds to be loaded as classes and instances, which in turn allowed for feature integrations within a UI menu such as: 
- Class toggling
- Class selection
- Instance toggling
- Instance selection
- Exploded view
- Bookmark navigation. 

The "LLM for Unity" Unity Asset by UndreamAI allowed for the integration of an AI assistant, 
combining their chat system with their Function handler to create an AI assitant that can interpret user input as either point cloud manipulative commands, 
or as general conversation. 

The current prototype uses a point cloud that is separated into 6 classes and 9 instances, and uses a file naming convention of 01_01 or 04_02 to load point cloud files within their proper class,
with the first number representing the class and second number the instance. Future work should attempt to enable more instance for more specific control, 
but can be hindered by the current method of rendering instances (or the hardware itself), as it creates multiple V2renderers that have to operate at the same time for each instance, 
while also having to leave room in the GPU for the LLM model to operate as desired. 
