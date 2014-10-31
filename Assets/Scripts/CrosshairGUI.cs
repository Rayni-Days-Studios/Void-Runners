using UnityEngine;

namespace _GUI
{
    [ExecuteInEditMode]
    public class CrosshairGUI : MonoBehaviour 
    {
        public Texture2D Crosshair;
        public GUIStyle NoGuiStyle;
        public Color GuiColor = Color.white;

        // Use this for initialization
        void Start () 
        {
            useGUILayout = false;
        }
	
        // Update is called once per frame
        void OnGUI () 
        {
            GUI.color = GuiColor;
            GUI.Box(new Rect((Screen.width / 2) - (Crosshair.width / 2), 
                (Screen.height / 2) - (Crosshair.height / 2), 
                Crosshair.width, Crosshair.height), 
                Crosshair, NoGuiStyle);
        }
    }
}
