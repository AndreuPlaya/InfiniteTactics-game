
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
#if (UNITY_EDITOR) 
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator map = target as MapGenerator;
        if (DrawDefaultInspector())
        {
           // map.GenerateMap();
            
        }

        if (GUILayout.Button("Generate map"))
        {
            //map.GenerateMap();

        }
        if (GUILayout.Button("Print Tiles"))
        {
            

        }
        
    }
}
#endif