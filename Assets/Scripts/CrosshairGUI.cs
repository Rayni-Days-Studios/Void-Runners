using UnityEngine;

[ExecuteInEditMode]
public class CrosshairGUI : MonoBehaviour 
{
    public Texture2D crosshair;
    public GUIStyle noGuiStyle;
    public Color guiColor = Color.white;

    // Use this for initialization
    void Start () 
    {
        useGUILayout = false;
    }
	
    // Update is called once per frame
    void OnGUI () 
    {
        GUI.color = guiColor;
        GUI.Box(new Rect((Screen.width / 2) - (crosshair.width / 2), 
            (Screen.height / 2) - (crosshair.height / 2), 
            crosshair.width, crosshair.height), 
            crosshair, noGuiStyle);
    }
}