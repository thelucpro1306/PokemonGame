using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(CutSceneActor))]
public class CutSceneActorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property); 
        position =  EditorGUI.PrefixLabel(position, label);

        var toggerPos = new Rect(position.x, position.y, 60,position.height);
        var fieldPos = new Rect(position.x + 60, position.y, position.width - 60 ,position.height);

        var isPlayerProp = property.FindPropertyRelative("isPlayer");

        isPlayerProp.boolValue = GUI.Toggle(toggerPos, isPlayerProp.boolValue, "isPlayer");
        isPlayerProp.serializedObject.ApplyModifiedProperties();

        if (!isPlayerProp.boolValue)
        {
            EditorGUI.PropertyField(fieldPos, property.FindPropertyRelative("character"), GUIContent.none);
        }

        

    }
}
