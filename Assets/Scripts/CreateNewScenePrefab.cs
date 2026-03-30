using UnityEngine;

/// <summary>
/// Initial room scan. Once a scan has been performed, the capture is not requested anymore as the scene is stored on the device
/// and loaded when starting the application
/// </summary>
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
