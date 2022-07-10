using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAI : MonoBehaviour
{
    [SerializeField] Transform armature;
    [SerializeField] Transform head;
    [SerializeField] LayerMask groundLayers;

    Animator anim;
    Rigidbody rigid;
    public Rigidbody[] ragdoll;
    CapsuleCollider capsule;
    Vector3 capsuleInit;

    bool ragdolling;
    bool grounded;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        capsuleInit = new Vector3(capsule.radius, capsule.center.y, capsule.height);
        rigid = GetComponent<Rigidbody>();
        ragdoll = armature.GetComponentsInChildren<Rigidbody>();
        ActivateRagdoll(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        grounded = CheckGrounded();
    }

    public void ActivateRagdoll(bool setting)
    {
        foreach (Rigidbody rb in ragdoll)
        {
            rb.isKinematic = !setting;
        }

        anim.enabled = !setting;
        //rigid.isKinematic = setting;

        if(setting && !ragdolling)
            StartCoroutine(RagdollCo());
    }

    IEnumerator RagdollCo()
    {
        ragdolling = true;

        /*capsule.center = new Vector3(0f, 0.5f, 0f);
        capsule.radius = 0.5f;
        capsule.height = 1f;*/

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        float prelim = 1f;
        while(prelim > 0f)
        {
            yield return new WaitForFixedUpdate();
            FollowRagdoll(children);
            prelim -= Time.deltaTime;
        }

        while(ragdoll[0].velocity.sqrMagnitude > 0.01f)
        {
            yield return new WaitForFixedUpdate();

            FollowRagdoll(children);
            
        }

        ActivateRagdoll(false);
        anim.SetTrigger("getup");
        //rigid.isKinematic = false;
        ragdolling = false;

        
    }

    void FollowRagdoll(List<Transform> children)
    {
        foreach (Transform child in children)
        {
            child.parent = null;
        }
        transform.position = ragdoll[0].position;
        transform.LookAt(transform.position + Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up).normalized);//transform.position + Vector3.ProjectOnPlane(ragdoll[0].transform.forward, Vector3.up).normalized);
        foreach (Transform child in children)
        {
            child.parent = transform;
        }
    }

    bool CheckGrounded()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            if(hit.distance <= 0.2f)
                return true;
        }
        else
        {
            if(Physics.Raycast(transform.position + Vector3.up * 1000f, Vector3.down, out hit, 1000, groundLayers, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
                return true;
            }
        }

        return false;
    }
}
