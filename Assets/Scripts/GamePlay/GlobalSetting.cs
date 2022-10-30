using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSetting : MonoBehaviour
{
    [SerializeField] Color hightlightColor;


    public Color HighlightedColor => hightlightColor;

    public static GlobalSetting i { get; private set; }

    private void Awake()
    {
        i = this;
    }

}
