using UnityEngine;
using AUIT.AdaptationObjectives;
public class PrefabUIManager : MonoBehaviour
{


     GameObject prefabUI;



     AUIT.AUIT auit;

    int layerMask;

        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
     

    void Start()
    {

        layerMask = LayerMask.GetMask("Default");
       // Button button = prefabUI.transform.Find("Horizontal10_\")
       prefabUI = gameObject.transform.Find("ContentUIExample1").gameObject;
        auit = GameObject.Find("AUIT").GetComponent<AUIT.AUIT>();
    }
    public bool settingsOpen;
    LocalObjectiveHandler localObjectiveHandler;
    // Update is called once per frame
    void Update()
    {
        
       
        localObjectiveHandler = GetComponent<LocalObjectiveHandler>();
        var coordTransition = GetComponent<AUIT.PropertyTransitions.CoordinateSystemTransition>();

        

        if (prefabUI.activeInHierarchy)
        {
            //remove from gameObjects to optimize
            //auit.gameObjectsToOptimize.Remove(gameObject);
            //settingsOpen = true;
            localObjectiveHandler.enabled = false;
            
        }else if (!prefabUI.activeInHierarchy)
        {
            //settingsOpen = false;
            //auit.gameObjectsToOptimize.Add(gameObject);
            localObjectiveHandler.enabled = true;

            

        }

        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 rightControllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*Vector3.forward;
        

           Ray ray = new Ray(rightControllerPosition,rightControllerDirection);

       // Debug.DrawRay(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*Vector3.forward*10f,Color.red);



       
    }

   

     
}
