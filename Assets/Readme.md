# MR Home Assistant - README

## Start
To start, create an empty GameObject and attach the SpatialObjectManager script. Add objects you want to spawn to the Prefab List and their corresponding preview to the Preview List. 

## Create a new object to spawn

### Create a UIParent
Before adding an object to the list, create an empty GameObject which parents the object. To ensure that the right controller ray hits the Object, set its layer to "Spawner Contact". Attach all AUIT objectives to the parent GameObject. Add the AssignToggleScript to the parent to bind the toggles of the attached settings to the objectives. Add the PrefabUIManager script to the parent to enable and disable the settings. Add the XRGrabInteractable component to the parent. Create a BoxCollider for your UIElement and attach it to the XRGrabInteractable enable grabbing.

### Attach Settings to Object
To attach settings onto an object, add the settings panel (currently named "ContentUIExample1") as a child. Manually move the settings next to the GameObject. 

### Bind Toggles and Buttons
The first three toggles can be found under "Horizontal (1)" in the Settings' hierarchy. Bind the toggles to the AUIT objectives attached to the parent. Assign each toggle to one slot in the AssignToggleScript. 

To bind the "Update Spawn", "Back to Spawn", "Update Settings" and "Delete Object" buttons, navigate to Horizontal(5) in the Settings hierarchy and select the following:

"Update Spawn" -> in the toggle component of "ButtonShelf_IconAndLabel_Toggle (1)", find "On Value Changed" assign "SpatialObjectManager.UpdateInitialPrefabLocation", drag the object parent to the gameObject field.

"Back to Spawn", -> in the toggle component of "ButtonShelf_IconAndLabel_Toggle (2)", find "On Value Changed" assign 
"SpatialObjectManager.BackToInitialPrefabLocation", drag the object parent to the gameObject field.

"Update Settings" -> in the toggle component of "ButtonShelf_IconAndLabel_Toggle (3)", find "On Value Changed" assign 
"SpatialObjectManager.SaveSettings", drag the object parent to the gameObject field.

"Delete Object"  -> in the toggle component of "ButtonShelf_IconAndLabel_Toggle (4)", find "On Value Changed" assign "SpatialObjectManager.DeleteSpawnedObject", drag the object parent to the gameObject field.

### Ray Interaction
To ensure that the ray is visible if it collides with the object, add the RayInteraction gameObject as a child and assign your parent to the "Pointable element" slot in the Ray Interactable component.