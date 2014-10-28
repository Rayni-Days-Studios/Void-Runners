using UnityEngine;
using System.Collections.Generic;

public class TriggerScript : MonoBehaviour 
{

    public List<GameObject> List = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        List.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        List.Remove(other.gameObject);
    }
}
