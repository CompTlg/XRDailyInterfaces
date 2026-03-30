
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.IO;
using System.Collections.Generic;
using AUIT;
using  AUIT.AdaptationObjectives.Objectives;
using AUIT.PropertyTransitions;
using AUIT.ContextSources;
using AUIT.AdaptationObjectives;
using System.Linq;
using System;
using UnityEngine.UI;


public class SpatialObjectManager : MonoBehaviour
{
    //public GameObject prefab;
    //public GameObject previewPrefab;
    private GameObject currentPreview;

    int layerMask;
    int interactiveMask;
    int LayerIgnoreRaycast;
    GameObject[] spawnablePrefabsList;
    string path;
    public SpawnedPrefabDataList spawnedPrefabDataList;
    private GameObject auitGO;

    public CameraContextSource cameraContextSource;

    private Dictionary <GameObject, SpawnedPrefabData> table = new Dictionary<GameObject, SpawnedPrefabData>();



        public GameObject[] prefabList;
    public GameObject[] previewPrefabList;
    private int selectedIndex = 0;
    private float nextTimeStep = 0f;
    public float delayTime = 0.3f;
    void Awake()
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //spawnedPrefabDataList = new SpawnedPrefabDataList();

        path = Path.Combine(Application.persistentDataPath, "SpawnedPrefabs.json");
        //DeleteJson();
        LoadJson();
        InstantiateSavedPrefabs();
        MakeRoomMeshInvisible();
        auitGO = GameObject.Find("AUIT");
    
        spawnablePrefabsList = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        LayerIgnoreRaycast = LayerMask.NameToLayer("Spawner Contact");
        currentPreview = prefabList[selectedIndex];
        currentPreview.layer = LayerIgnoreRaycast;
        foreach (Transform currentPreviewChild in currentPreview.transform)
        {
            currentPreviewChild.gameObject.layer = LayerIgnoreRaycast;
        }
        
        //layerMask = ~(1 << 2);
        layerMask = LayerMask.GetMask("Default");
        interactiveMask = LayerMask.GetMask("Spawner Contact");
        //DeleteJson();

       

    }

    GameObject currentlyGrabbedObject;
    GameObject currentlyTargetedObject;

    private GameObject hand;

    /// <summary>
    /// Set the layers of a gameObject and its children to specified layer
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="layer"></param>
    private void SetLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursive(child.gameObject,layer);
            }
    }
    // Update is called once per frame

    void Update()
    {
        if (hand == null)
        {
            hand = GameObject.Find("RightControllerAnchor");
        }

        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward);

        Debug.DrawRay(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch),OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward*10f,Color.red);


        // Left controller raycast
        if(Physics.Raycast(ray, out RaycastHit hit,100f,layerMask))
        {
            currentPreview.SetActive(true);
            currentPreview.layer = LayerIgnoreRaycast;
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up,hit.normal);

            //delay to not continuously switch object when moving left thumbstic
            if (Time.time > nextTimeStep)
            {
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                if (stick.x > 0.8)
                {
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
                    NextPrefab();
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
 
                    currentPreview = previewPrefabList[selectedIndex];
                    
                    nextTimeStep = Time.time + delayTime;
                }
                else if (stick.x < -0.8)
                {
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
                    PreviousPrefab();
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
 
                    currentPreview = previewPrefabList[selectedIndex];
                    //currentPreview.layer = LayerIgnoreRaycast;
                    
                    nextTimeStep = Time.time + delayTime;
                }
            }
            // Press X -> Place object
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {

                // set layer to the spawner contact layer
                GameObject spawnedObject = Instantiate(prefabList[selectedIndex],hit.point,Quaternion.FromToRotation(Vector3.up, hit.normal));
                SetLayerRecursive(spawnedObject,LayerIgnoreRaycast);
                spawnedObject.layer = LayerIgnoreRaycast;
                
                //persistance, create object data to later save it to the json
                OVRAnchor anchorID = hit.collider.transform.parent.GetComponent<MRUKAnchor>().Anchor;
                SpawnedPrefabData spawnedPrefabData = new SpawnedPrefabData();

                string prefabname = spawnedObject.name.Replace("(Clone)","").Trim();
                spawnedPrefabData.index = System.Array.FindIndex(prefabList,p => p.name == prefabname);
                spawnedPrefabData.ID = Guid.NewGuid().ToString();
                spawnedPrefabData.parentAnchorID = anchorID.ToString();
                spawnedObject.SetActive(true);
                spawnedObject.transform.SetParent(hit.collider.transform.parent);
                spawnedPrefabData.localPrefabPosition = spawnedObject.transform.localPosition;
                spawnedPrefabData.localPrefabRotation = spawnedObject.transform.localRotation;
                PrefabSettings prefabSettings = new PrefabSettings();
                
                //assigned toggles, also saved to json to allow "Save Settings" where adaptation preferences are saved
                AssignToggleScript assignedToggleScript = spawnedObject.GetComponent<AssignToggleScript>();


                Toggle fieldOfViewObjectiveToggle = assignedToggleScript.fieldOfViewObjectiveToggle;
                Toggle distanceObjectiveToggle = assignedToggleScript.distanceObjectiveToggle;
                Toggle lookTowardsToggle = assignedToggleScript.lookTowardsToggle;

                prefabSettings.fieldOfViewObjective = fieldOfViewObjectiveToggle.isOn;
                prefabSettings.distanceObjective = distanceObjectiveToggle.isOn;
                prefabSettings.lookTowardsObjective = lookTowardsToggle.isOn;
                spawnedPrefabData.prefabSettings = prefabSettings;
                //SpawnedPrefabDataList spawnedPrefabDataList = new SpawnedPrefabDataList();
                Debug.Log(spawnedPrefabDataList);
                //Debug.Log(spawnedPrefabData);
                table.Add(spawnedObject,spawnedPrefabData);
                spawnedPrefabDataList.spawnedPrefabs.Add(spawnedPrefabData);

                
                //spawnedObject.layer = LayerMask.NameToLayer("Default");

                var interElementObj = spawnedObject.GetComponent<AvoidInterElementOcclusionObjective>();

                 if (interElementObj != null){
                                interElementObj.userContextSource = cameraContextSource;
                                //coordTransition.TorsoContextSource = auit.globalTorsoSource;
                            }
                //add instantiated object to auit optimization loop
               var auit = auitGO.GetComponent<AUIT.AUIT>();
               auit.gameObjectsToOptimize.Add(spawnedObject);
               
                //refresh the optimization, needed as new object has been added
                auit.RefreshOptimizationObjects();
                            
                //spawnedObject.layer = LayerIgnoreRaycast;//TODO change back to avoid prefab flying to you

                
                SaveJson();
            }
        }

        
        //right ray interaction
        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 rightControllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*Vector3.forward;
        
        Debug.DrawRay(rightControllerPosition, rightControllerDirection *10f, Color.blue,0.1f);

        Ray rightControllerRay = new Ray(rightControllerPosition,rightControllerDirection);
        

        if(currentlyGrabbedObject == null){

            // Right raycast
            if(Physics.Raycast(rightControllerRay, out RaycastHit hit_right, 100f,interactiveMask))
            {
                Debug.Log(hit_right.collider.gameObject);

                // to register an object the ray collides with in the current frame (by pointing to the object)
               GameObject rootObject = hit_right.collider.GetComponentInParent<LocalObjectiveHandler>()?.gameObject;



                if (rootObject != null)
                {

                    // Take the settings ui and if you press the grip button (PrimaryHandTrigger) then show the settings
                       GameObject prefabUIManager = rootObject.transform.Find("ContentUIExample1")?.gameObject;

                       if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger,OVRInput.Controller.RTouch) && prefabUIManager != null)
                    {
                        prefabUIManager.SetActive(!prefabUIManager.activeInHierarchy);
                        Debug.Log(prefabUIManager);
                    }

                    var coordTransition = rootObject.GetComponent<CoordinateSystemTransition>();

                    // Press A = Grab the object, only possible if the settings are active, as then the localobjectivehandler is false and optimization for
                    // that particular object is paused
                    if (OVRInput.GetDown(OVRInput.Button.One) && prefabUIManager != null && prefabUIManager.activeInHierarchy)
                        {
                            currentlyGrabbedObject = rootObject;

                            //currentlyGrabbedObject.GetComponent<CoordinateSystemTransition>().isManuallyGrabbed = true;
                            currentlyGrabbedObject.GetComponent<LocalObjectiveHandler>().enabled = false;

                            

                                //needed for grabbing the object and changing its orientation
                                if (hand != null)
                                {
                           
                                    currentlyGrabbedObject.transform.SetParent(hand.transform);
                                   
                                  
                                }
                }



                
                 
                
                    
                }
            }

           
    }else{
        //GameObject prefabUIManager = currentlyGrabbedObject.transform.parent.Find("ContentUIExample1")?.gameObject;
        
        // If you stop holding A, then stop the grab
        if (OVRInput.GetUp(OVRInput.Button.One))
            {

                    
                   // currentlyGrabbedObject.GetComponent<CoordinateSystemTransition>().isManuallyGrabbed = false;
                    
                    //GameObject prefabUIManager = currentlyGrabbedObject.transform.parent.Find("ContentUIExample1")?.gameObject;

                    currentlyGrabbedObject.transform.SetParent(null);
                    
                    currentlyGrabbedObject = null;
                  
            }
    }

    }

    /// <summary>
    /// The data list of objects which is serialized into JSON
    /// </summary>
    /// <returns></returns>
    public SpawnedPrefabDataList GetJson(){
    
        return spawnedPrefabDataList;
    }

    MRUKRoom room;

    /// <summary>
    /// load json from the persistent data path, deserialize it into a list for initialization
    /// </summary>
 public void LoadJson()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            spawnedPrefabDataList = JsonUtility.FromJson<SpawnedPrefabDataList>(json);
            Debug.Log("Prefab locations loaded.");

        }else
        {
             //if there is no file yet create a new one
            spawnedPrefabDataList = new SpawnedPrefabDataList();
            //File.WriteAllText(path, spawnedPrefabDataList);
            Debug.Log("New save file created.");
            
        }
        
    }

    /// <summary>
    /// Left thumbstick to the right, increase the unique index i.e. switch to next object (preview)
    /// </summary>
    public void NextPrefab()
    {
        selectedIndex = (selectedIndex + 1) % prefabList.Length;
        Debug.Log("Selected index: " + prefabList[selectedIndex].name);
    }
 
    /// <summary>
    /// Left thumbstick to the left, decrease the unique index
    /// </summary>
    public void PreviousPrefab()
    {
        if (selectedIndex == 0)
        {
            selectedIndex = prefabList.Length - 1;
        } else
        {
            selectedIndex--;
        }
 
    }

    /// <summary>
    /// If any property of an object has been updated or a new object has been spawned, save the updates into the json
    /// </summary>
    public void SaveJson()
    {
        string spData = JsonUtility.ToJson(spawnedPrefabDataList);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/SpawnedPrefabs.json", spData);
        Debug.Log("Prefab location stored.");
    }

    /// <summary>
    /// Deleting the json removes all saved data hence no objects are instantiated when starting the application
    /// </summary>
    public void DeleteJson()
    {
        File.Delete(path);
        Debug.Log("Save file deleted.");
    }


    /// <summary>
    /// Update the designated location of the spawned object
    /// </summary>
    /// <param name="prefab"></param>
     public void UpdateInitialPrefabLocation(GameObject prefab)
    {
        //get original list and update
        //SpawnedPrefabData prefabData = spawnedPrefabDataList.spawnedPrefabs[prefab];
        GameObject prefabRoot = prefab.transform.GetComponentInParent<LocalObjectiveHandler>().gameObject;

        SpawnedPrefabData prefabData = table[prefabRoot];

        //we only know the anchors if the room has been loaded
        MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
        if (room == null)
        {
            room = MRUK.Instance.GetCurrentRoom();
        }

        //find the parent anchor (e.g., first wall it was spawned on) through the anchor ID saved to the object
        foreach(Transform child in room.transform)
        {
            string anchorID = child.GetComponent<MRUKAnchor>().Anchor.ToString();
            if(prefabData.parentAnchorID == anchorID)
            {
                // Save the new location of the object relative to the local position and rotation of the anchor
                Vector3 relativePos = child.InverseTransformPoint(prefab.transform.position);
                Quaternion relativeRot = Quaternion.Inverse(child.rotation)*prefab.transform.rotation;
                prefabData.localPrefabPosition = relativePos;
                prefabData.localPrefabRotation = relativeRot;
                SaveJson();
                Debug.Log("Prefab location updated.");

            }
        }


    });
    }

    /// <summary>
    /// Delete an object, remove it from the auit optimization loop and from the object to prefabData dictionary
    /// </summary>
    /// <param name="prefab"></param>
    public void DeleteSpawnedObject(GameObject prefab)
    {
        var auit = auitGO.GetComponent<AUIT.AUIT>();
        GameObject prefabRoot = prefab.transform.GetComponentInParent<LocalObjectiveHandler>().gameObject;

        if (auit.gameObjectsToOptimize.Contains(prefabRoot))
        {
            auit.gameObjectsToOptimize.Remove(prefabRoot);
            auit.RefreshOptimizationObjects();
        }

        if (table.ContainsKey(prefabRoot))
        {
            SpawnedPrefabData data = table[prefabRoot];
            spawnedPrefabDataList.spawnedPrefabs.Remove(data);
            table.Remove(prefabRoot);
            SaveJson();
        }

        Destroy(prefabRoot);
    }


    /// <summary>
    /// when activated, prefab jumps back to saved designated location (relative position/rotation to parent anchor)
    /// </summary>
    /// <param name="prefab"></param>
    public void BackToInitialPrefabLocation(GameObject prefab)
    {
       GameObject prefabRoot = prefab.transform.GetComponentInParent<LocalObjectiveHandler>().gameObject;

        SpawnedPrefabData prefabData = table[prefabRoot];

        MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
        if (room == null)
            {
                room = MRUK.Instance.GetCurrentRoom();
            }

        
         foreach(Transform child in room.transform)
        {
            string anchorID = child.GetComponent<MRUKAnchor>().Anchor.ToString();
            if(prefabData.parentAnchorID == anchorID)
            {
                prefab.transform.SetParent(child);
                prefab.transform.localPosition = prefabData.localPrefabPosition;
                prefab.transform.localRotation = prefabData.localPrefabRotation;

                Debug.Log("Prefab back to initial location.");

            }
        }

        

    });
    }
    /// <summary>
    /// Make the virtual room invisible
    /// </summary>
    private void MakeRoomMeshInvisible()
    {
         MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                room = MRUK.Instance.GetCurrentRoom();

                foreach(Transform roomComponent in room.transform)
                {
                    MeshRenderer roomComponentMesh = roomComponent.GetComponentInChildren<MeshRenderer>();
                    if(roomComponentMesh != null)
                    {
                        roomComponentMesh.enabled = false;
                    }
                }

    });
    }

    /// <summary>
    /// Make the virtual room visible
    /// </summary>
    private void MakeRoomMeshVisible()
    {
         MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                room = MRUK.Instance.GetCurrentRoom();

                foreach(Transform roomComponent in room.transform)
                {
                    MeshRenderer roomComponentMesh = roomComponent.GetComponentInChildren<MeshRenderer>();
                    if(roomComponentMesh != null)
                    {
                        roomComponentMesh.enabled = true;
                    }
                }

    });
    }

    /// <summary>
    /// instantiate saved objects from JSON when starting the application
    /// </summary>
    public void InstantiateSavedPrefabs()
    {
        //take list

        foreach(SpawnedPrefabData spawnedPrefab in spawnedPrefabDataList.spawnedPrefabs)
        {
            Debug.Log("Spawing cube");
            string prefabID = Guid.NewGuid().ToString();
            string parentAnchorId = spawnedPrefab.parentAnchorID;
            Vector3 localPrefabPosition = spawnedPrefab.localPrefabPosition;
            Quaternion localPrefabRotation = spawnedPrefab.localPrefabRotation;

           //find the room
            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                room = MRUK.Instance.GetCurrentRoom();

                if(room!= null)
                {
                    GameObject roomGO = room.gameObject;
                    Debug.Log("Room located in scene: " +roomGO);
                    
                    foreach(Transform roomChild in roomGO.transform)
                    {
                        //find its parent anchor through the saved parent anchor id
                        string anchorID = roomChild.GetComponent<MRUKAnchor>().Anchor.ToString();
                        Debug.Log(anchorID + " " + parentAnchorId);

                        if(anchorID == parentAnchorId)
                        {
                            //spawn the object
                            var auit = auitGO.GetComponent<AUIT.AUIT>();
                            GameObject meshGO = roomChild.GetChild(0).gameObject;
                            GameObject spawnedObject = Instantiate(prefabList[spawnedPrefab.index],roomChild);
                            spawnedObject.SetActive(true);
                            AssignToggleScript assignedToggleScript = spawnedObject.GetComponent<AssignToggleScript>();

                            //apply the saved optimization settings (e.g., when you enabled FOV in a previous setting then pressed save settings)
                            Toggle fieldOfViewObjectiveToggle = assignedToggleScript.fieldOfViewObjectiveToggle;
                            Toggle distanceObjectiveToggle = assignedToggleScript.distanceObjectiveToggle;
                            Toggle lookTowardsToggle = assignedToggleScript.lookTowardsToggle;


                            fieldOfViewObjectiveToggle.isOn = spawnedPrefab.prefabSettings.fieldOfViewObjective;
                            distanceObjectiveToggle.isOn = spawnedPrefab.prefabSettings.distanceObjective;
                            lookTowardsToggle.isOn = spawnedPrefab.prefabSettings.lookTowardsObjective;

                            // set the gameobject and its children to spawner contact
                            SetLayerRecursive(spawnedObject, LayerIgnoreRaycast);
                            /*spawnedObject.layer = LayerMask.NameToLayer("Default");
                            foreach (Transform child in spawnedObject.transform)
                            {
                                child.gameObject.layer = LayerMask.NameToLayer("Default");
                            }*/
                            // add the gameobject to the dictionary which matches it together with its SpawnedPrefabData
                            table.Add(spawnedObject,spawnedPrefab);

                            var interElementObj = spawnedObject.GetComponent<AvoidInterElementOcclusionObjective>();
                            var coordTransition = spawnedObject.GetComponent<CoordinateSystemTransition>();

                            if (interElementObj != null){
                                interElementObj.userContextSource = cameraContextSource;
                                //coordTransition.TorsoContextSource = auit.globalTorsoSource;
                            }
                          
                            // set the position of the object to the saved local position relative to the anchor
                            spawnedObject.transform.SetLocalPositionAndRotation(localPrefabPosition,localPrefabRotation);
                            Debug.Log("Loaded Prefab into " + roomChild.name + " " + parentAnchorId);

                            // add instantiated object to auit optimization loop
                            auit.gameObjectsToOptimize.Add(spawnedObject);
                    
                            // refresh optimization as new object has been added to the optimization list
                            auit.RefreshOptimizationObjects();

                        }
                    }


                }else
                {
                    Debug.Log("No room located.");
                    
                }
            }
            
            );

            //foreach()
        }

    }

    /// <summary>
    /// save the adaptation preferences for an object (the first three toggles) to be applied across sessions
    /// </summary>
    /// <param name="prefab"></param>
    public void SaveSettings(GameObject prefab)
    {
        GameObject prefabRoot = prefab.transform.GetComponentInParent<LocalObjectiveHandler>().gameObject;

        SpawnedPrefabData prefabData = table[prefabRoot];

        PrefabSettings prefabSettings = prefabData.prefabSettings;

        AssignToggleScript assignedToggleScript = prefabRoot.GetComponent<AssignToggleScript>();


        Toggle fieldOfViewObjectiveToggle = assignedToggleScript.fieldOfViewObjectiveToggle;
        Toggle distanceObjectiveToggle = assignedToggleScript.distanceObjectiveToggle;
        Toggle lookTowardsToggle = assignedToggleScript.lookTowardsToggle;

        prefabSettings.fieldOfViewObjective = fieldOfViewObjectiveToggle.isOn;
        prefabSettings.distanceObjective = distanceObjectiveToggle.isOn;
        prefabSettings.lookTowardsObjective = lookTowardsToggle.isOn;

        SaveJson();
    }

        
    


}

/// <summary>
/// save data about a spawned object to save it into json and retrieve it when starting the application
/// </summary>
[System.Serializable]
    public class SpawnedPrefabData
{
    public int index;
    public string ID;
    public string parentAnchorID;
    public Vector3 localPrefabPosition;
    public Quaternion localPrefabRotation;
    public PrefabSettings prefabSettings;
}

/// <summary>
/// List of spawned objects necessary to instantiate saved objects in a later session
/// </summary>
[System.Serializable]
public class SpawnedPrefabDataList
{
    public List<SpawnedPrefabData> spawnedPrefabs = new List<SpawnedPrefabData>();
}

/// <summary>
/// Adaptation objective toggle values saved in the json file per object
/// </summary>
[System.Serializable]
public class PrefabSettings
{
    public bool fieldOfViewObjective;
    public bool distanceObjective;
    public bool lookTowardsObjective;
}
