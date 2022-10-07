using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conditions
{
    public string Name { get; set; }
    public string Desription { get; set; }
    public string StartMessage { get; set; }

    public Action<Pokemon> OnAfterTurn { get; set; }
}
