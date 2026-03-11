
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.IO;
using System.Collections.Generic;
using AUIT;
using  AUIT.AdaptationObjectives.Objectives;
using AUIT.PropertyTransitions;
using AUIT.ContextSources;
using AUIT.AdaptationObjectives;
public class SimplePrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;
    int layerMask;
    int LayerIgnoreRaycast;
    GameObject[] spawnablePrefabsList;
    string path;
    private SpawnedPrefabDataList spawnedPrefabDataList;
    private GameObject auitGO;

    public CameraContextSource cameraContextSource;

    void Awake()
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //spawnedPrefabDataList = new SpawnedPrefabDataList();

        path = Path.Combine(Application.persistentDataPath, "SpawnedPrefabs.json");
           DeleteJson();
         LoadJson();
        InstantiateSavedPrefabs();

        auitGO = GameObject.Find("AUIT");
    
        spawnablePrefabsList = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        currentPreview = Instantiate(previewPrefab);
        currentPreview.layer = LayerIgnoreRaycast;
        foreach (Transform currentPreviewChild in currentPreview.transform)
        {
            currentPreviewChild.gameObject.layer = LayerIgnoreRaycast;
        }
        
        //layerMask = ~(1 << 2);
        layerMask = LayerMask.GetMask("Default");
        //DeleteJson();

       

    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward);

        Debug.DrawRay(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch),OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*Vector3.forward*10f,Color.red);



        if(Physics.Raycast(ray, out RaycastHit hit,100f,layerMask))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up,hit.normal);
                //Debug.Log("Hit1 " +hit.collider.transform.parent.name);

            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                Debug.Log("Left Pinching detected.");

                GameObject spawnedObject = Instantiate(prefab,hit.point,Quaternion.FromToRotation(Vector3.up, hit.normal));
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
               // Debug.Log("Hit2 " +hit.collider.transform.parent.name);

                OVRAnchor anchorID = hit.collider.transform.parent.GetComponent<MRUKAnchor>().Anchor;
                Debug.Log(anchorID);
                SpawnedPrefabData spawnedPrefabData = new SpawnedPrefabData();
                spawnedPrefabData.name = spawnedObject.name.Replace("(Clone)", "").Trim();
                spawnedPrefabData.parentAnchorID = anchorID.ToString();
                spawnedPrefabData.localPrefabPosition = spawnedObject.transform.localPosition;
                spawnedPrefabData.localPrefabRotation = spawnedObject.transform.localRotation;
                //SpawnedPrefabDataList spawnedPrefabDataList = new SpawnedPrefabDataList();
                Debug.Log(spawnedPrefabDataList);
                //Debug.Log(spawnedPrefabData);
                spawnedPrefabDataList.spawnedPrefabs.Add(spawnedPrefabData);
                SaveJson();
            }
        }

        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 rightControllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*Vector3.forward;
        
        Debug.DrawRay(rightControllerPosition, rightControllerDirection *10f, Color.blue,0.1f);

        Ray rightControllerRay = new Ray(rightControllerPosition,rightControllerDirection);

            if(Physics.Raycast(rightControllerRay, out RaycastHit hit_right, 100f, layerMask))
            {
                GameObject rootObject = hit_right.collider.GetComponentInParent<LocalObjectiveHandler>()?.gameObject;

                if (rootObject != null && rootObject.name.Contains("AUITCube"))
                {
                    if (OVRInput.GetDown(OVRInput.Button.One))
                        {
                            Debug.Log("Pinching detected.");
                            GameObject hand = GameObject.Find("RightControllerAnchor"); 
                                if (hand != null)
                                {
                                    rootObject.transform.SetParent(hand.transform);
                                    //todo currently grabbed object has to be removed from optimization

                                }
                        }
                    if (OVRInput.GetUp(OVRInput.Button.One))
                    {
                        rootObject.transform.SetParent(null);
                    }
                }
}
    }



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

    //method to make prefab child of room->room element

    //TODO new gameobject room element with prefabs as children?
    public void InstantiateSavedPrefabs()
    {
        //take list

        foreach(SpawnedPrefabData spawnedPrefab in spawnedPrefabDataList.spawnedPrefabs)
        {
            Debug.Log("Spawing cube");
            string parentAnchorId = spawnedPrefab.parentAnchorID;
            Vector3 localPrefabPosition = spawnedPrefab.localPrefabPosition;
            Quaternion localPrefabRotation = spawnedPrefab.localPrefabRotation;

            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                MRUKRoom room = MRUK.Instance.GetCurrentRoom();

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
                            GameObject spawnedObject = Instantiate(prefab,roomChild);

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
    public string name;
    public string parentAnchorID;
    public Vector3 localPrefabPosition;
    public Quaternion localPrefabRotation;
}

public class SpawnedPrefabDataList
{
    public List<SpawnedPrefabData> spawnedPrefabs = new List<SpawnedPrefabData>();
}
