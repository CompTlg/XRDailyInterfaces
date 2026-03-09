using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class UISpawner : MonoBehaviour
{
    public GameObject uiPrefab;

    public enum SpawnMode
    {
        MidAir,
        Table,
        Wall
    }

    public SpawnMode spawnMode;

    void Start()
    {

        if (uiPrefab == null)
        {
            Debug.LogError("uiPrefab is not there");
            return;
        }

        if (spawnMode == SpawnMode.MidAir)
        {
            SpawnMidAir();
        }
        else
        {
            // MRUK.Instance or SceneLoadedEvent null possible, was giving error
            StartCoroutine(SubscribeWhenMRUKReady());
        }
    }

    IEnumerator SubscribeWhenMRUKReady()
    {
        // Wait till one of them are not null
        while (MRUK.Instance == null || MRUK.Instance.SceneLoadedEvent == null)
        {
            yield return null;
        }

        MRUK.Instance.SceneLoadedEvent.AddListener(SpawnOnSurface);
    }

    void SpawnMidAir()
    {

        Vector3 spawnPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * 1.5f;

        Quaternion rotation =
            Quaternion.LookRotation(
                spawnPosition - Camera.main.transform.position
            );

        Instantiate(uiPrefab, spawnPosition, rotation);
    }

    void SpawnOnSurface()
    {
        MRUKAnchor.SceneLabels label;

        if (spawnMode == SpawnMode.Table)
            label = MRUKAnchor.SceneLabels.TABLE;
        else
            label = MRUKAnchor.SceneLabels.WALL_FACE;

        // To find the closest anchor with the specified label to the camera
        var anchors = MRUK.Instance.GetCurrentRoom().Anchors;
        MRUKAnchor closestAnchor = null;
        float minDistance = float.MaxValue;
        Vector3 cameraPosition = Camera.main.transform.position;

        // Loop through all anchors to find the closest one with the specified label
        foreach (var anchor in anchors)
        {
            if (anchor.HasAnyLabel(label))
            {
                float distance = Vector3.Distance(anchor.transform.position, cameraPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestAnchor = anchor;
                }
            }
        }

        var surface = closestAnchor;

        if (surface == null)
        {
            Debug.Log("No surface found.");
            return;
        }

        Vector3 spawnPosition = surface.transform.position;

        if (spawnMode == SpawnMode.Table)
        {
            // To spawn slightly above the table to avoid clipping
            spawnPosition += Vector3.up * 0.05f;
        }
        else
        {
            // To spwan slightly in front of the wall to avoid clipping
            spawnPosition += surface.transform.forward * 0.05f;
        }

        Quaternion rotation =
            Quaternion.LookRotation(-surface.transform.forward);

        Instantiate(uiPrefab, spawnPosition, rotation);
    }
}