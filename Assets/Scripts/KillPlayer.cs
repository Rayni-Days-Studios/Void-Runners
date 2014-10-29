using System.Linq;
using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour {
    Camera _cam;
    Quaternion _from;
    Quaternion _to;
    public float Progress = 0;
    float _time;
    bool _die;

    void Start()
    {
        _cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!_die) return;

        Progress += Time.deltaTime / _time;
        _cam.transform.rotation = Quaternion.Lerp(_from, _to, Progress);
    }

    public IEnumerator Die(Vector3 to, float time, float killTime)
    {
        if (_die) yield break;

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
        yield return new WaitForSeconds(killTime);
        Application.LoadLevel(Application.loadedLevel);
    }
}
