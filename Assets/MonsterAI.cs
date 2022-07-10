using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    Animator anim;
    Rigidbody rigid;

    public Transform target;
    public LayerMask targetLayers;
    public LayerMask groundLayers;
    bool grounded = false;

    float moveSpeed = 10;
    bool jumping;

    float lookRange = 100f;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        grounded = CheckGrounded();

        if(target != null)
        {
            transform.LookAt(target, Vector3.up);
            float dist = Vector3.Distance(transform.position, target.position);
            if(dist > 1f && grounded)
            {
                if(!jumping)
                {
                    if(Random.value > 0.99f)
                        StartCoroutine(JumpCo(target.position));
                    else MoveTowardPosition(target.position);
                }
            }
        }
        else LookForTarget();
    }

    void MoveTowardPosition(Vector3 pos)
    {
        anim.SetBool("walking", true);

        transform.LookAt(new Vector3(pos.x, transform.position.y, pos.z), Vector3.up);
        if(grounded)
        {
            if(rigid.velocity.sqrMagnitude < moveSpeed * moveSpeed)
            rigid.AddForce(transform.forward * moveSpeed, ForceMode.VelocityChange);
        }
    }
    
    bool CheckGrounded()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }

    IEnumerator JumpCo(Vector3 pos)
    {
        jumping = true;

        Vector3 move = pos - transform.position;
        
        anim.SetBool("crouch", true);
        anim.SetBool("walking", false);
        yield return new WaitForSeconds(0.5f + Random.value);
        anim.SetBool("jump", true);
        anim.SetBool("crouch", false);
        rigid.AddForce((Vector3.up * 10 + move.normalized * Mathf.Min(10f, move.magnitude)) * rigid.mass, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("jump", false);
        while(!grounded)
        {
            anim.SetBool("rear", true);
            yield return null;
        }

        anim.SetBool("rear", false);
        jumping = false;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void LookForTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, lookRange, targetLayers, QueryTriggerInteraction.Collide);
        foreach(Collider col in hitColliders)
        {
            if(col.GetComponent<PlayerControl>())
            {
                target = col.transform;
                break;
            }
        }
    }
}
