using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Connector : MonoBehaviour
{
    Connection connection;

    Rigidbody rigid;
    LineRenderer line;

    [SerializeField] LayerMask connectLayer;

    public UnityEvent connectionEstablished;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponentInParent<Rigidbody>();
        line = GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(connection == null)
        {
            if (rigid.velocity.sqrMagnitude < 0.5f)
                LookForConnection();
        }
        else
        {
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, transform.InverseTransformPoint(connection.transform.position));
        }
    }

    void LookForConnection()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f, connectLayer, QueryTriggerInteraction.Collide);
        foreach(Collider col in hitColliders)
        {
            Connection con = col.GetComponent<Connection>();
            if(con != null && con.occupant == null)
            {
                MakeConnection(con);

                break;
            }
        }
    }

    public void MakeConnection(Connection con)
    {
        con.occupant = this;
        con.connectionEstablished.Invoke();
        connection = con;

        Vector3[] pos = new Vector3[2];
        pos[0] = Vector3.zero;
        pos[1] = transform.InverseTransformPoint(connection.transform.position);
        line.SetPositions(pos);

        connectionEstablished.Invoke();
    }
}
