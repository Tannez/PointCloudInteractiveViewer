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

The current prototype uses a point cloud that is separated into 6 classes and 9 instances, and uses a file naming convention of 01_01 or 04_02 to load point cloud files within their proper class, with the first number representing the class and second number the instance. Future work should attempt to integrate more instances to allow for more control when searching for specific information, but can be hindered by the current method of rendering instances, and maybe also the hardware itself. The system currently creates multiple V2renderers that have to operate at the same time (one for each instance), while also having to leave room in the GPU for the LLM model to operate as desired. Other improvements could be to explore other features and tools used within other 3D viewing software, or to make the current system more fluid and dynamic to account for other types of point clouds. 

## Using the Application
The application runs within the "FinalVersion" scene located in the assets folder. Once the scene is loaded, it will run and load clouds in the scene based on the definded path within the "CloudCreator" Object. Ensure the naming convention of the point clouds for the definded directory path match with the previously mentioned format (01_01), for proper separation of classes and instances. If using another point cloud directory, please note that the LLM has a predefined prompt for the LLM character script (LLMCharacterPCComp) related to the already provided point cloud directory. 

To navigate the ppint clouds while the scene is running, hold right mouse button down and use the "WASD" keys to move around. If you ever get lost in the scene, press "SPACE" while holding right mouse button down, to recenter the point cloud. If classes or instances do not spawn properly, hold right mouse button down and press "R" to reload the point clouds. 

With the UI menu on the left you can manipulate the point cloud in the following way:
- Alter the point budget of the point clouds via a slider
- Change the applied EDL effect using sliders
- Change background using buttons (Skybox, Black, White)
- Display Bookmarks button (5 bookmark buttons will become available for selection. Pressing buttons automatically zooms to indicated locations and automatically removes layers)
- Access AI Assistant Button (Allows you to use natural language within an input field to execute commands or get answers to questions you might have)
- Toggle Classes on or off
- Select Classes via buttons
- Change the color mode via a dropdown menu (RGB, Classification, Intensity)
- Alter the size of points via buttons
- Separate classes along the y-axis using a slider (exploded view)

Selecting classes grants access to a menu on the right that allows for selecting or toggling instances within the class. An information window will display when an instance is selected and the camera is moved to the instance.

Instances can also be selected via mouse click (but this is not as reliable as the buttons).
