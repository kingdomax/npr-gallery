# NPR Gallery
![0  vr-museum-1](https://user-images.githubusercontent.com/6430428/175570185-b9badbd1-0ef7-4dba-96e5-40bb4c43aac8.PNG)
![0  vr-museum-2](https://user-images.githubusercontent.com/6430428/175570194-9fca51ac-51f1-4e18-8d63-7232de230866.PNG)
![0  vr-museum-3](https://user-images.githubusercontent.com/6430428/175570201-8756b040-1fa9-435e-b477-1ad967b56467.PNG)

- Require only Unity [2021.3.2f1](https://unity3d.com/get-unity/download/archive)
- Please treat main branch as a stable version!!<br />
Having said that, create a separate branch to develop your feature, then make a pull request to merge your code to the main one.

## Development guide 
- Create a new branch from main. `git checkout -b <branch_name>`
- Contribute your features to your new branch.
- While you are developing, don't forget to sync your code with the main branch. `git pull origin main`
- When you finish, create [**pull request**](https://github.com/VRSYS-NPR4VR/npr-gallery/compare) against the main branch, and fill in the pull request's description of what changed you have done. [**(PR example)**](https://github.com/VRSYS-NPR4VR/npr-gallery/pull/1)
- If the code conflicts with the main branch, please resolve it correctly.
- Make sure there is no broken code or errors before attempting to merge.
- Get at least one reviewer approval by our teammate or [@kingdomax](https://github.com/orgs/VRSYS-NPR4VR/people/kingdomax), then it's now possible to merge your branch to the main one. (you can also notify [@kingdomax](https://github.com/orgs/VRSYS-NPR4VR/people/kingdomax) in the discord channel)

## Run & Build 
- In general, you can just press the play button in Unity editor, and then the application will run via Oculus Link (make sure you already connected your Quest device with your laptop before doing so)
- If you want to test the networking scenario manually, you can build this application as a standalone executable file.
On the top menu bar: `File > Build Setting... > Build > Select your built folder (normally it's called 'Build' in the project directory > After the build has done, run NPR Gallery.exe file.` Then you can join the previous instance as 2nd player by pressing the play button from Unity editor

## Controller 
- First, you need to change your name that will display on top of your avatar in this distributed VR app.<br />
`Go to object hierarchy > look for __NETWORK__/Network Setup > In NetworkSetup component, change username field to yours and change selected device type to HMD.` You can also change the device type to desktop, but it only supports navigation.
- After your avatar enters the scene, you can navigate using your left controller.
  - Joystick on the left controller allow you to rotate your avatar. 
  - Trigger button on the left controller allow you to walk forward, and the direction will respect where you point your left controller.
- Right controller give you full functionality on painting / brushing interactions.
  - Press A button to switch between painting equipment and bare hand.
  - Press B button to switch the rendering technique.
  - Aim your ray toward test objects(only the paintable ones), and press your index finger to paint the selected rendering technique to the geometry.

## Networking Stuff 
I hope. What I have tried to set up here should not make you change any code regarding the networking stuff. But basically, I used Vrsys package as a base multiplayer framework in this project. Under the hood, Vrsys package uses Photon's PUN as a core networking engine which you can find helpful links below.
- **PUN doc:** https://doc.photonengine.com/en-us/pun/v2/getting-started/pun-intro
- **API doc:** https://doc-api.photonengine.com/en/pun/v2/group__public_api.html
- **Vrsys framework:** https://github.com/VRSYS-NPR4VR/npr-gallery/blob/main/Documents/Vrsys%20Framework.pdf

## [HOW TO] add your shading  
I wrote some explanation in this [**pull request**](https://github.com/VRSYS-NPR4VR/npr-gallery/pull/1). <br />
Most of the time, you need to make the change in the following files:
- Assets/Scripts/Models/Enum.cs
- Assets/Scripts/Models/RenderingConfig.cs
- Assets/Scripts/RenderingProvider.cs
- Assets/Scripts/PaintInteraction.cs
- Assets/Scripts/PaintHandler.cs

## [HOW TO] add paintable objects 
For dummy objects, we would like them to be paintable:  
1. Add 'TestObject' tags to the object.
2. Add a collider component to the object, letting the avatar's ray interactable.
3. Add PhotonView script component to the object; this will make an object updatable via the network.
Please change ViewID parameter to XXX; the first digit is your group number, and the last digit is your object order in the group.
4. Add PaintHandler script component to the object; this script is where your rendering implementation goes.
5. Add ManipulationHandler script component to the object; this script let user manipulate the object.
6. You can also take a look at an example at `__TEST OBJECTS__/Group1/WaterPump`
7. Subdivide the Prefab by the Duotone algorithm. Add a fbx Prefab of the subdivided Object to "Ressources/DuotonePrefabs/" and Name is simlar to the object.

## APK file
You can find latest release build and standalone apk file [here](https://github.com/VRSYS-NPR4VR/npr-gallery/releases)
