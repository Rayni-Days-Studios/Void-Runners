using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour 
{

    public List<GameObject> list = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        list.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        list.Remove(other.gameObject);
    }
}