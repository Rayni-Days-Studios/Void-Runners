using UnityEngine;

public class Crosshair : MonoBehaviour 
{
    public float Range = 300;

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));

        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction, Color.green);

        if (!Physics.Raycast(ray, out hit, Range)) return;

        if (hit.collider.gameObject.GetComponent<AmmoBoxScript>() != null)
        {
            hit.collider.gameObject.GetComponent<AmmoBoxScript>().OnLookEnter();
        }
    }
}