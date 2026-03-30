using UnityEngine;
using AUIT.AdaptationObjectives;

/// <summary>
/// disables localobjectivehandler on object to allow interaction with settings and grabbing
/// </summary>
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

       

       
    }

   

     
}
