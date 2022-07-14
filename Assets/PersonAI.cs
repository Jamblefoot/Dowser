using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAI : MonoBehaviour
{
    [SerializeField] Transform armature;
    [SerializeField] Transform head;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] LayerMask rockLayer;
    float lookRange = 50f;
    float moveSpeed = 5f;

    Animator anim;
    Rigidbody rigid;
    public Rigidbody[] ragdoll;
    CapsuleCollider capsule;
    Vector3 capsuleInit;

    bool ragdolling;
    bool grounded;

    Collider target;
    Vector3 targetPos;

    Transform tran;

    bool lookingForTarget;


    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
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

        if(ragdolling) return;

        if(target != null)
        {
            tran.LookAt(targetPos, Vector3.up);
            float dist = Vector3.Distance(tran.position, targetPos);
            if (dist > 1f)
            {
                if (grounded)
                {
                    MoveTowardPosition(targetPos);
                }
            }
        }
        else
        {
            if(!lookingForTarget)
                StartCoroutine(LookForRock());
        }
    }

    void MoveTowardPosition(Vector3 pos)
    {
        anim.SetBool("walking", true);

        tran.LookAt(new Vector3(pos.x, tran.position.y, pos.z), Vector3.up);
        if (grounded)
        {
            if (rigid.velocity.sqrMagnitude < moveSpeed * moveSpeed)
                rigid.AddForce(tran.forward * moveSpeed, ForceMode.VelocityChange);
        }
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
        foreach (Transform child in tran)
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

        //anim.CrossFade("Base Layer.getUp", 1f);
        //anim.SetLayerWeight(0, 0);
        //StartCoroutine(Recover());
        //rigid.isKinematic = false;
        ragdolling = false;

        
    }

    /*IEnumerator Recover()
    {
        while(anim.GetLayerWeight(0) < 1)
        {
            anim.SetLayerWeight(0, anim.GetLayerWeight(0) + Time.deltaTime);
            yield return null;
        }
    }*/

    void FollowRagdoll(List<Transform> children)
    {
        foreach (Transform child in children)
        {
            child.parent = null;
        }
        tran.position = ragdoll[0].position;
        tran.LookAt(tran.position + Vector3.ProjectOnPlane(head.position - tran.position, Vector3.up).normalized);//transform.position + Vector3.ProjectOnPlane(ragdoll[0].transform.forward, Vector3.up).normalized);
        foreach (Transform child in children)
        {
            child.parent = tran;
        }
    }

    bool CheckGrounded()
    {
        RaycastHit hit;
        if(Physics.Raycast(tran.position + Vector3.up * 0.1f, Vector3.down, out hit, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            if(hit.distance <= 0.2f)
                return true;
        }
        else
        {
            if(Physics.Raycast(tran.position + Vector3.up * 1000f, Vector3.down, out hit, 1000, groundLayers, QueryTriggerInteraction.Ignore))
            {
                tran.position = hit.point;
                Debug.Log("PersonAI was underground!");
                return true;
            }
        }

        return false;
    }

    IEnumerator LookForRock()
    {
        lookingForTarget = true;
        while(target == null)
        {
            Collider[] hitColliders = new Collider[5];
            int numOfCols = Physics.OverlapSphereNonAlloc(tran.position, lookRange, hitColliders, rockLayer, QueryTriggerInteraction.Collide);
            if(numOfCols > 0)
            {
                target = hitColliders[0];
                targetPos = target.ClosestPoint(tran.position);
            }

            yield return new WaitForSeconds(Random.Range(2f, 10f));
        }
        lookingForTarget = false;
    }
}
