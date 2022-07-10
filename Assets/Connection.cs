using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Connection : MonoBehaviour
{
    public Connector occupant;

    public UnityEvent connectionEstablished;
    public UnityEvent connectionLost;

    public Transform root;

    public void BreakConnection()
    {
        if(occupant != null)
            occupant.BreakConnection();
    }
}
