using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class AmmoGUI : MonoBehaviour {
        private ShootingScript _spawnPoint;
        public GUIStyle AmmoCount;
 
        void Awake()
        {
            _spawnPoint = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootingScript>();
        }
    
        void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 0, 0), "" + _spawnPoint.gun.Ammo + "  " + _spawnPoint.LightGun.Ammo, AmmoCount);
        }
    }
}