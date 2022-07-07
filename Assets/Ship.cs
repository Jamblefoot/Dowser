using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] GameObject wellPrefab;
    [SerializeField] GameObject podPrefab;

    [SerializeField] Transform target;

    float turnSpeed = 20f;
    float moveSpeed = 15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null) return;

        Vector3 offset = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
        if(offset.magnitude > 500)
        {
            float rot = Vector3.SignedAngle(transform.forward, offset, Vector3.up);
            rot = Mathf.Sign(rot) * Mathf.Min(Mathf.Abs(rot), turnSpeed);
            transform.rotation = transform.rotation * Quaternion.Euler(0f, rot * Time.deltaTime, 0);
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
    }

    public GameObject LaunchWellToPosition(Vector3 pos)
    {
        Vector3 offset = pos - transform.position;
        offset.y = 0;
        GameObject well = Instantiate(wellPrefab, transform.position, transform.rotation);
        well.GetComponent<Rigidbody>().AddForce(Vector3.up * 2000f + offset.normalized * 1000f, ForceMode.Force);
        Well w = well.GetComponent<Well>();
        w.GoToLocation(pos);
        w.ship = this;

        return well;
    }

    public GameObject LaunchPodToPosition(Vector3 pos)
    {
        Vector3 offset = pos - transform.position;
        offset.y = 0;
        GameObject pod = Instantiate(podPrefab, transform.position, transform.rotation);
        pod.GetComponent<Rigidbody>().AddForce(Vector3.up * 2000f + offset.normalized * 100f * offset.magnitude, ForceMode.Force);
        //well.GetComponent<Well>().GoToLocation(pos);

        return pod;
    }
}
