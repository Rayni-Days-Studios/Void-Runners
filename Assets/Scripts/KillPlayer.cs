using System.Linq;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    private Camera _cam;
    private Quaternion _from;
    private Quaternion _to;
    public float Progress = 0;
    private float _time;
    private bool _die;

    private void Start()
    {
        _cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (!_die) return;
        Progress += Time.deltaTime/_time;
        _cam.transform.rotation = Quaternion.Lerp(_from, _to, Progress);
    }

    public void Die(Vector3 to, float time, float killTime)
    {
        if (_die) return;

            _die = true;

            foreach (var comp in GetComponents<Behaviour>().Where(comp => comp != this))
            {
                comp.enabled = false;
            }
            foreach (var comp in GetComponentsInChildren<MouseLook>())
            {
                comp.enabled = false;
            }
            _time = time;
            _from = _cam.transform.rotation;
            _cam.transform.LookAt(to);
            _to = _cam.transform.rotation;
            _cam.transform.rotation = _from;
            Destroy(gameObject, killTime);
    }
}
