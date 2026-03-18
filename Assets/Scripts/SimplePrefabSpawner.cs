
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



public class SimplePrefabSpawner : MonoBehaviour
{
    //public GameObject prefab;
    //public GameObject previewPrefab;
    private GameObject currentPreview;
    int layerMask;
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

        auitGO = GameObject.Find("AUIT");
    
        spawnablePrefabsList = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        currentPreview = prefabList[selectedIndex];
        currentPreview.layer = LayerIgnoreRaycast;
        foreach (Transform currentPreviewChild in currentPreview.transform)
        {
            currentPreviewChild.gameObject.layer = LayerIgnoreRaycast;
        }
        
        //layerMask = ~(1 << 2);
        layerMask = LayerMask.GetMask("Default");
        //DeleteJson();

       

    }

    GameObject currentlyGrabbedObject;
    GameObject currentlyTargetedObject;

    private GameObject hand;
    // Update is called once per frame
    void Update()
    {
        if (hand == null)
        {
            hand = GameObject.Find("RightControllerAnchor");
        }

        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward);

        Debug.DrawRay(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch),OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward*10f,Color.red);



        if(Physics.Raycast(ray, out RaycastHit hit,100f,layerMask))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up,hit.normal);

            /*
            if (Time.time > nextTimeStep)
            {
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                if (stick.x > 0.8)
                {
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
                    NextPrefab();
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
 
                    currentPreview = previewPrefabList[selectedIndex];
                    currentPreview.layer = LayerIgnoreRaycast;
                    foreach (Transform currentPreviewChild in currentPreview.transform)
                    {
                        currentPreviewChild.gameObject.layer = LayerIgnoreRaycast;
                    }
                    nextTimeStep = Time.time + delayTime;
                }
                else if (stick.x < -0.8)
                {
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
                    PreviousPrefab();
                    previewPrefabList[selectedIndex].SetActive(!previewPrefabList[selectedIndex].activeSelf);
 
                    currentPreview = previewPrefabList[selectedIndex];
                    currentPreview.layer = LayerIgnoreRaycast;
                    foreach (Transform currentPreviewChild in currentPreview.transform)
                    {
                        currentPreviewChild.gameObject.layer = LayerIgnoreRaycast;
                    }
                    nextTimeStep = Time.time + delayTime;
                }
            }
*/
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {

                GameObject spawnedObject = Instantiate(prefabList[selectedIndex],hit.point,Quaternion.FromToRotation(Vector3.up, hit.normal));


                spawnedObject.SetActive(true);
                spawnedObject.transform.SetParent(hit.collider.transform.parent);
                //spawnedObject.layer = LayerMask.NameToLayer("Default");

                var interElementObj = spawnedObject.GetComponent<AvoidInterElementOcclusionObjective>();

                 if (interElementObj != null){
                                interElementObj.userContextSource = cameraContextSource;
                                //coordTransition.TorsoContextSource = auit.globalTorsoSource;
                            }

               var auit = auitGO.GetComponent<AUIT.AUIT>();
               auit.gameObjectsToOptimize.Add(spawnedObject);
               
                
                auit.RefreshOptimizationObjects();
                            
                //spawnedObject.layer = LayerIgnoreRaycast;//TODO change back to avoid prefab flying to you

                OVRAnchor anchorID = hit.collider.transform.parent.GetComponent<MRUKAnchor>().Anchor;
                Debug.Log(anchorID);
                SpawnedPrefabData spawnedPrefabData = new SpawnedPrefabData();
                spawnedPrefabData.ID = Guid.NewGuid().ToString();
                spawnedPrefabData.parentAnchorID = anchorID.ToString();
                spawnedPrefabData.localPrefabPosition = spawnedObject.transform.localPosition;
                spawnedPrefabData.localPrefabRotation = spawnedObject.transform.localRotation;
                //SpawnedPrefabDataList spawnedPrefabDataList = new SpawnedPrefabDataList();
                Debug.Log(spawnedPrefabDataList);
                //Debug.Log(spawnedPrefabData);
                table.Add(spawnedObject,spawnedPrefabData);
                spawnedPrefabDataList.spawnedPrefabs.Add(spawnedPrefabData);
                SaveJson();
            }
        }

        

        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 rightControllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*Vector3.forward;
        
        Debug.DrawRay(rightControllerPosition, rightControllerDirection *10f, Color.blue,0.1f);

        Ray rightControllerRay = new Ray(rightControllerPosition,rightControllerDirection);
        bool objectIsHit = Physics.Raycast(rightControllerRay, out RaycastHit hit_right, 100f,(1<<2));
        GameObject currentlyTargetedObject = null;
        if(objectIsHit){
        
        GameObject rootObject = hit_right.collider.GetComponentInParent<LocalObjectiveHandler>()?.gameObject;
        /*
        if(rootObject != null)
        {
            Debug.Log("pointing to: " + rootObject.name);
            //rootObject.GetComponent<LocalObjectiveHandler>().enabled = false;
            currentlyTargetedObject = rootObject;
            Debug.Log("Local objective handler off" + rootObject.name);


        }
        if(rootObject!= null && currentlyTargetedObject != rootObject)
            {
                //currentlyTargetedObject.GetComponent<LocalObjectiveHandler>().enabled = true;
            Debug.Log("Local objective handler on" + rootObject.name);

            }*/
        }

        if(currentlyGrabbedObject == null){

            if(Physics.Raycast(rightControllerRay, out  hit_right, 100f,(1<<2)))
            {
                //Debug.Log(hit_right.collider.gameObject.name);
               GameObject rootObject = hit_right.collider.GetComponentInParent<LocalObjectiveHandler>()?.gameObject;



                if (rootObject != null && rootObject.name.Contains("AUITCube"))
                {
                       // GameObject prefabUIManager = currentlyGrabbedObject.transform.parent.Find("ContentUIExample1")?.gameObject;

                    //rootObject.transform.GetComponent<LocalObjectiveHandler>().enabled = false;
                    var coordTransition = rootObject.GetComponent<CoordinateSystemTransition>();
                    if (OVRInput.GetDown(OVRInput.Button.One) )
                        {
                            currentlyGrabbedObject = rootObject;

                            //currentlyGrabbedObject.GetComponent<CoordinateSystemTransition>().isManuallyGrabbed = true;
                            currentlyGrabbedObject.GetComponent<LocalObjectiveHandler>().enabled = false;

                            

                            coordTransition.isManuallyGrabbed= true;

                            //GameObject hand = GameObject.Find("RightControllerAnchor"); 
                                if (hand != null)
                                {
                                    //rootObject.transform.GetComponent<LocalObjectiveHandler>().enabled = false;

                                    //rootObject.transform.SetParent(hand.transform);
                                    // currently grabbed object has to be removed from optimization
                                   
                                    // Add these lines to snap the object to the controller's center
                                  
                                    currentlyGrabbedObject.transform.SetParent(hand.transform);
                                     // currentlyGrabbedObject.transform.localPosition = Vector3.zero;
                                    //currentlyGrabbedObject.transform.localRotation = Quaternion.identity;
                                  
                                }
                }
                 
                
                    
                }
            }

           
    }else{
        //GameObject prefabUIManager = currentlyGrabbedObject.transform.parent.Find("ContentUIExample1")?.gameObject;
        
        if (OVRInput.GetUp(OVRInput.Button.One))
            {

                    
                    currentlyGrabbedObject.GetComponent<CoordinateSystemTransition>().isManuallyGrabbed = false;
                    
                    //GameObject prefabUIManager = currentlyGrabbedObject.transform.parent.Find("ContentUIExample1")?.gameObject;

                    currentlyGrabbedObject.transform.SetParent(null);
                    currentlyGrabbedObject.transform.GetComponent<CurvePositionTransition>().enabled = true;
                                    currentlyGrabbedObject.transform.GetComponent<CurveRotationTransition>().enabled = true;
                    currentlyGrabbedObject = null;
                    /*
                if(!prefabUIManager.activeInHierarchy){
                    currentlyGrabbedObject.GetComponent<LocalObjectiveHandler>().enabled = true;

                }*/

                    
                    /*
                GameObject prefabUIManager = rootObject.transform.parent.Find("ContentUIExample1").gameObject;
                if(!prefabUIManager.activeInHierarchy){
                rootObject.transform.GetComponent<LocalObjectiveHandler>().enabled = true;
                }
                coordTransition = rootObject.GetComponent<CoordinateSystemTransition>();
                coordTransition.isManuallyGrabbed=false;

                rootObject.transform.SetParent(null);
                */

            }
    }

    }

        public SpawnedPrefabDataList GetJson(){
       
            return spawnedPrefabDataList;
        }

    MRUKRoom room;

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

    public void NextPrefab()
    {
        selectedIndex = (selectedIndex + 1) % prefabList.Length;
        Debug.Log("Selected index: " + prefabList[selectedIndex].name);
    }
 
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

    public void SaveJson()
    {
        string spData = JsonUtility.ToJson(spawnedPrefabDataList);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/SpawnedPrefabs.json", spData);
        Debug.Log("Prefab location stored.");
    }

    public void DeleteJson()
    {
        File.Delete(path);
        Debug.Log("Save file deleted.");
    }

     public void UpdateInitialPrefabLocation(GameObject prefab)
    {
        //get original list and update
        //SpawnedPrefabData prefabData = spawnedPrefabDataList.spawnedPrefabs[prefab];
        SpawnedPrefabData prefabData = table[prefab];
        foreach(Transform child in room.transform)
        {
            string anchorID = child.GetComponent<MRUKAnchor>().Anchor.ToString();
            if(prefabData.parentAnchorID == anchorID)
            {
                Vector3 relativePos = child.InverseTransformPoint(prefab.transform.position);
                Quaternion relativeRot = Quaternion.Inverse(child.rotation)*prefab.transform.rotation;
                prefabData.localPrefabPosition = relativePos;
                prefabData.localPrefabRotation = relativeRot;
                SaveJson();
                Debug.Log("Prefab location updated.");

            }
        }


    }

    public void BackToInitialPrefabLocation(GameObject prefab)
    {
        SpawnedPrefabData prefabData = table[prefab];


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

        

    }

    //method to make prefab child of room->room element

    //TODO new gameobject room element with prefabs as children?
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


            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                room = MRUK.Instance.GetCurrentRoom();

                if(room!= null)
                {
                    GameObject roomGO = room.gameObject;
                    Debug.Log("Room located in scene: " +roomGO);
                    foreach(Transform roomChild in roomGO.transform)
                    {
                        string anchorID = roomChild.GetComponent<MRUKAnchor>().Anchor.ToString();
                        Debug.Log(anchorID + " " + parentAnchorId);

                        if(anchorID == parentAnchorId)
                        {
                            var auit = auitGO.GetComponent<AUIT.AUIT>();
                            GameObject meshGO = roomChild.GetChild(0).gameObject;
                            GameObject spawnedObject = Instantiate(prefabList[selectedIndex],roomChild);

                            table.Add(spawnedObject,spawnedPrefab);

                            var interElementObj = spawnedObject.GetComponent<AvoidInterElementOcclusionObjective>();
                            var coordTransition = spawnedObject.GetComponent<CoordinateSystemTransition>();

                            if (interElementObj != null){
                                interElementObj.userContextSource = cameraContextSource;
                                //coordTransition.TorsoContextSource = auit.globalTorsoSource;
                            }
                          
                            auitGO.GetComponent<AUIT.AUIT>().gameObjectsToOptimize.Add(spawnedObject);
                            spawnedObject.transform.SetLocalPositionAndRotation(localPrefabPosition,localPrefabRotation);
                            Debug.Log("Loaded Prefab into " + roomChild.name + " " + parentAnchorId);

                            auit.gameObjectsToOptimize.Add(spawnedObject);
                    

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


}
[System.Serializable]
    public class SpawnedPrefabData
{
    public string ID;
    public string parentAnchorID;
    public Vector3 localPrefabPosition;
    public Quaternion localPrefabRotation;
}

public class SpawnedPrefabDataList
{
    public List<SpawnedPrefabData> spawnedPrefabs = new List<SpawnedPrefabData>();
}
