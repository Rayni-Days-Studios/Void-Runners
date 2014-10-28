using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour 
{
    public float range = 300;

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, z: 0));

        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction, Color.green);

        if (!Physics.Raycast(ray, out hit, range)) return;

        if (hit.collider.gameObject.GetComponent<AmmoBoxScript>() != null)
        {
            hit.collider.gameObject.GetComponent<AmmoBoxScript>().OnLookEnter();
        }
    }
}