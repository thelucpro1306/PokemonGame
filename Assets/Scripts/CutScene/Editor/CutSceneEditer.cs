using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CutScene))]
public class CutSceneEditer : Editor
{
    public override void OnInspectorGUI()
    {
        var cutScene = target as CutScene;

        if(GUILayout.Button("Thêm hội thoại"))
        {
            cutScene.AddAction(new DialogAction());
        }
        else
        {
            if (GUILayout.Button("Thêm Actor"))
            {
                cutScene.AddAction(new MoveActorAction());
            }
        }
        

        base.OnInspectorGUI();
    }
}
