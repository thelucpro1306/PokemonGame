using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    Image image;

    public static Fader Instance { get; private set; }  

    private void Awake()
    {
        image = GetComponent<Image>();
        Instance = this;    

    }

    public IEnumerator FaderIn(float time)
    {
        yield return image.DOFade(1f,time).WaitForCompletion();
    }
    public IEnumerator FaderOut(float time)
    {
        yield return image.DOFade(0f, time).WaitForCompletion();
    }

}
