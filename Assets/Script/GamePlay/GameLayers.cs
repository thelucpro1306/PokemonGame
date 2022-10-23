using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SoildLayer
    {
        get => solidObjectsLayer;
    }
    public LayerMask GrassLayer
    {
        get => grassLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask FovLayer
    {
        get => fovLayer;
    }
}
