using System.Linq;
using UnityEngine;


public class KillPlayer : MonoBehaviour
{
    private Camera cam;
    private Quaternion fromQuaternion;
    private Quaternion toPoint;
    private float timer;
    private bool die;
    public float progress = 0;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (!die) return;
            progress += Time.deltaTime/timer;
            cam.transform.rotation = Quaternion.Lerp(fromQuaternion, toPoint, progress);
    }

    public void Die(Vector3 to, float time, float killTime)
    {
        if (die) return;

        die = true;

        foreach (var comp in GetComponents<Behaviour>().Where(comp => comp != this))
        {
            comp.enabled = false;
        }
        foreach (var comp in GetComponentsInChildren<MouseLook>())
        {
            comp.enabled = false;
        }

        this.timer = time;
        fromQuaternion = cam.transform.rotation;
        cam.transform.LookAt(to);
        this.toPoint = cam.transform.rotation;
        cam.transform.rotation = fromQuaternion;
        Destroy(gameObject, killTime);
    }
}