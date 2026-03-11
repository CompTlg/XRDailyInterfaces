using UnityEngine;

public class CreateNewScenePrefab : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OVRSceneManager sceneManager = FindObjectOfType<OVRSceneManager>();

        if (sceneManager != null)
        {
            sceneManager.RequestSceneCapture();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
