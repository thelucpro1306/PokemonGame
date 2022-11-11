using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable 
{
    IEnumerator Interact(Transform initiator);
}
