using UnityEngine;
using System.Collections;

public class MenuScript : Photon.MonoBehaviour
{


    void OnGUI()
    {

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 225, 0, 450, Screen.height));

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Select a scene");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();


        GUILayout.BeginVertical();

        GUILayout.Space(10);

        if (GUILayout.Button("Tutorial 2B - Instantiating"))
        {
            Application.LoadLevel("Tutorial_2B");
        }

        GUILayout.Space(10);

        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();

    }
}