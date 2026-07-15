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
- Class selection

- Class toggling
  
- Instance selection
- Instance toggling
- Exploded view
- Bookmark navigation.

The "LLM for Unity" Unity Asset by UndreamAI allowed for the integration of an AI assistant, 
combining their chat system with their Function handler to create an AI assitant that can interpret user input as either point cloud manipulative commands, 
or as general conversation. 

The current prototype uses a point cloud that is separated into 6 classes and 9 instances, and uses a file naming convention of 01_01 or 04_02 to load point cloud files within their proper class, with the first number representing the class and second number the instance. Future work should attempt to integrate more instances to allow for more control when searching for specific information, but can be hindered by the current method of rendering instances, and maybe also the hardware itself. The system currently creates multiple V2renderers that have to operate at the same time (one for each instance), while also having to leave room in the GPU for the LLM model to operate as desired. Other improvements could be to explore other features and tools used within other 3D viewing software, or to make the current system more fluid and dynamic to account for other types of point clouds. 

## Using the Application
The application runs within the "FinalVersion" scene located in the assets folder. Once the scene is loaded, it will run and load clouds in the scene based on the defined path within the "CloudCreator" Object. Ensure the naming convention of the point clouds for the defined directory path match with the previously mentioned format (01_01), for proper separation of classes and instances. If using another point cloud directory, please note that the LLM has a predefined prompt for the LLM character script (LLMCharacterPCComp) related to the already provided point cloud directory. 

To navigate the point clouds while the scene is running, hold right mouse button down and use the "WASD" keys to move around. If you ever get lost in the scene, press "SPACE" while holding right mouse button down to recenter the point cloud. If classes or instances do not spawn properly, hold right mouse button down and press "R" to reload the point clouds. 

With the UI menu on the left you can manipulate the point cloud in the following way:
- Alter the point budget of the point clouds via a slider
  <img width="1917" height="827" alt="image" src="https://github.com/user-attachments/assets/f0656c0b-cd61-414a-924a-7310f7443ff0" />
- Change the applied EDL effect using sliders
  <img width="1919" height="832" alt="image" src="https://github.com/user-attachments/assets/751a03d3-6b17-4817-96e7-36098de7477b" />
- Change background using buttons (Skybox, Black, White)
  <img width="1914" height="867" alt="image" src="https://github.com/user-attachments/assets/03ecbe86-ed75-41e8-9702-bfda3787a2b9" />
- Display Bookmarks button (5 bookmark buttons will become available for selection. Pressing buttons automatically zooms to indicated locations and automatically removes layers)
  <img width="1917" height="830" alt="Skærmbillede 2026-07-15 144501" src="https://github.com/user-attachments/assets/3bf6d1a0-768c-4449-ac59-56225b0a4edd" />
- Access AI Assistant Button (Allows you to use natural language within an input field to execute commands or get answers to questions you might have)
  <img width="1917" height="837" alt="Skærmbillede 2026-07-15 144526" src="https://github.com/user-attachments/assets/75c73b23-8d04-40fb-9370-a4eb0c3a27cd" />
- Toggle Classes on or off
  <img width="1921" height="835" alt="Skærmbillede 2026-07-15 144927" src="https://github.com/user-attachments/assets/d0548fd6-1456-4e66-9206-f2f6cb556f20" />
- Select Classes via buttons
  <img width="1915" height="830" alt="Skærmbillede 2026-07-15 144956" src="https://github.com/user-attachments/assets/d020f013-2b7b-4549-8e2d-aea2dcf96d50" />
- Change the color mode via a dropdown menu (RGB, Classification, Intensity)
  <img width="1921" height="830" alt="image" src="https://github.com/user-attachments/assets/53d3114e-8c67-4455-88a5-6b0347489719" />
- Alter the size of points via buttons
  <img width="1921" height="832" alt="image" src="https://github.com/user-attachments/assets/4b2dc8f2-bfec-4c75-a16a-53858d99c781" />
- Separate classes along the y-axis using a slider (exploded view)
  <img width="1917" height="832" alt="Skærmbillede 2026-07-15 144655" src="https://github.com/user-attachments/assets/b9f2eea7-866e-4870-ae57-3298a845bd74" />

Selecting classes grants access to a menu on the right that allows for selecting or toggling instances within the class. An information window will display when an instance is selected and the camera is moved to the instance.
<img width="1917" height="837" alt="Skærmbillede 2026-07-15 145124" src="https://github.com/user-attachments/assets/7fad689b-0b15-477d-9535-80eb9ea26352" />

Instances can also be selected via mouse click (but this is not as reliable as the buttons).
