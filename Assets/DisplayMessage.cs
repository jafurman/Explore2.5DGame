using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMessage : MonoBehaviour
{

    public GameObject message;

    void Start()
    {
        message.SetActive(false);
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
        message.SetActive(true);
        }
    }

    void OnTriggerExit(Collider col)
    {
        message.SetActive(false);
    }
}
