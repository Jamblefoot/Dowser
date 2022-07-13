using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Connector : MonoBehaviour
{
    public Connection connection;

    Rigidbody rigid;
    //LineRenderer line;
    public Transform tube;
    MeshRenderer tubeRend;

    float searchDistance = 15;
    [SerializeField] LayerMask connectLayer;

    public UnityEvent connectionEstablished;
    public UnityEvent connectionBroken;

    float lastDist;

    Pod pod;

    bool blockConnect;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponentInParent<Rigidbody>();
        //line = GetComponentInChildren<LineRenderer>();
        pod = GetComponentInParent<Pod>();

        if(tube != null)
        {
            Collider tubeCol = tube.GetComponentInChildren<Collider>();
            foreach(Collider col in rigid.GetComponentsInChildren<Collider>())
            {
                if(col != tubeCol)
                    Physics.IgnoreCollision(col, tubeCol, true);
            }

            tubeRend = tube.GetComponentInChildren<MeshRenderer>();
        }

        if(connection == null)
        {
            tube.gameObject.SetActive(false);
            StartCoroutine(ConnectDelay());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(connection == null)
        {
            if (rigid.velocity.sqrMagnitude < 0.05f && !blockConnect)
                LookForConnection();
            tube.localScale = new Vector3(1, 1, 0.1f);
            tube.localRotation = Quaternion.Euler(-90, 0, 0);
            lastDist = 0;
        }
        else
        {
            tube.localScale = new Vector3(1, 1, Vector3.Distance(connection.transform.position, transform.position));
            tube.LookAt(connection.transform.position);
            //line.positionCount = 2;
            //line.SetPosition(0, Vector3.zero);
            //line.SetPosition(1, transform.InverseTransformPoint(connection.transform.position));

            if(pod != null)
            {
                int lt = connection.GetLiquidType();
                if(lt != -1)
                {
                    if(pod.AddLiquid(connection.flow * Time.deltaTime, lt))
                    {
                        connection.WithdrawLiquid(connection.flow * Time.deltaTime, lt);
                        tubeRend.material.color = lt == 0 ? Color.blue : lt == 1 ? Color.black : Color.red;
                    }
                    else tubeRend.material.color = Color.white;
                }
            }
        }
    }

    void LookForConnection()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchDistance, connectLayer, QueryTriggerInteraction.Collide);
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
        //line.SetPositions(pos);

        connectionEstablished.Invoke();

        if(tube != null)
        {
            Collider tubeCol = tube.GetComponentInChildren<Collider>();
            foreach(Collider col in con.root.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(tubeCol, col, true);
            }
        }

        Pod pod = GetComponentInParent<Pod>();
        if(pod != null)
        {
            pod.rigid.isKinematic = true;
            pod.rigid.Sleep();
        }
    }

    public void BreakConnection()
    {
        if(connection == null) return;

        Debug.Log("Should be breaking connection!");

        connection.occupant = null;
        connection.connectionLost.Invoke();
        connection = null;
        connectionBroken.Invoke();

        StartCoroutine(ConnectDelay());
    }

    IEnumerator ConnectDelay()
    {
        blockConnect = true;
        yield return new WaitForSeconds(5);
        blockConnect = false;
    }
}
