using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    Animator anim;
    Rigidbody rigid;
    AudioSource audioSource;

    public Transform target;
    public LayerMask targetLayers;
    public LayerMask groundLayers;
    bool grounded = false;

    float moveSpeed = 10;
    bool jumping;

    bool lookingForTarget;
    bool attacking;

    float lookRange = 100f;

    [System.Serializable]
    public class Piece
    {
        public Transform bone;
        public GameObject limbPrefab;
    }
    public Piece[] pieces;

    Transform tran;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();

        tran = transform;

        audioSource = GetComponentInChildren<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        grounded = CheckGrounded();

        if(target != null)
        {
            tran.LookAt(target, Vector3.up);
            float dist = Vector3.Distance(tran.position, target.position);
            if(dist > 2.1f)
            {
                if(grounded && !jumping)
                {
                    if(Random.value > 0.99f)
                        StartCoroutine(JumpCo(target.position));
                    else MoveTowardPosition(target.position);
                }
            }
            else
            {
                if(!attacking)
                {
                    StartCoroutine(Attack());
                }
            }
        }
        else
        {
            if(!lookingForTarget)
                StartCoroutine(LookForTarget());
        } 

        if((GameControl.instance.player.transform.position - tran.position).sqrMagnitude > 500 * 500 && target == null)
        {
            Destroy(gameObject);
        }
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

    IEnumerator LookForTarget()
    {
        lookingForTarget = true;
        while(target == null)
        {
            Collider[] hitColliders = new Collider[10];
            int numOfColliders = Physics.OverlapSphereNonAlloc(transform.position, lookRange, hitColliders, targetLayers, QueryTriggerInteraction.Collide);
            for(int i = 0; i < numOfColliders; i++)
            {
                if(hitColliders[i].GetComponent<PlayerControl>() || hitColliders[i].GetComponent<PersonAI>())
                {
                    target = hitColliders[i].transform;
                    break;
                }
            }

            yield return new WaitForSeconds(Random.Range(1f, 5f));
        }

        lookingForTarget = false;
    }

    public void Burst()
    {
        foreach(Piece piece in pieces)
        {
            GameObject p = Instantiate(piece.limbPrefab, piece.bone.position, transform.rotation);
            p.transform.localScale = transform.localScale;
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if(rb != null)
                rb.AddForce(rigid.velocity, ForceMode.Force);
            Destroy(p, Random.Range(10f, 30f));
            
        }
        Destroy(gameObject);
    }

    IEnumerator Attack()
    {
        attacking = true;
        
        PersonAI pai= target.GetComponentInParent<PersonAI>();
        if(pai != null)
        {
            pai.ActivateRagdoll(true);
            //TAKE HEALTH
        }
        else //at this point else is the player
        {
            target.GetComponent<PlayerControl>().TakeDamage(Random.Range(3,8), tran.position);
        }
        yield return new WaitForSeconds(1f);

        attacking = false;
    }

    public void PlayFootstep()
    {
        if(audioSource != null)
        {
            audioSource.PlayOneShot(GameControl.instance.GetFootstep());
        }
    }
}
