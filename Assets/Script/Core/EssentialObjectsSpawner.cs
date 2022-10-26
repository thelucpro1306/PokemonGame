using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject EssentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if(existingObjects.Length == 0)
        {
            Instantiate(EssentialObjectsPrefab, new Vector3(0,0,0),Quaternion.identity);
        }
    }

}
