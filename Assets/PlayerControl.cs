using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] HurtUI hurtUI;

    [SerializeField] Transform neck;
    [SerializeField] Transform body;
    [SerializeField] Transform legLeft;
    [SerializeField] Transform legRight;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] LayerMask kickLayers;

    float moveSpeed = 10;
    float slowSpeed = 2f;
    float stepSpeed = 300;
    float jumpPower = 10;
    float kickPower = 100;
    float upMult = 4f;
    float kickTime;
    float kickWait;

    Rigidbody rigid;

    float headXRot;

    float vertical, horizontal;
    bool jumping;

    bool grounded;
    Vector3 groundNormal;

    Transform groundAnchor;

    int leftStep = 1;
    float legRot = 0;
    bool stepSwitch;

    public Transform tran;

    float health = 100f;

    void Awake()
    {
        tran = transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        Vector3 mouse = Input.mousePosition;
        float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");
        headXRot -= Input.GetAxis("Mouse Y");
        headXRot = Mathf.Clamp(headXRot, -80f, 70f);

        neck.localRotation = Quaternion.Euler(headXRot, neck.localRotation.eulerAngles.y, 0)
                            * Quaternion.Euler(0, mouseX, 0);

        body.localRotation = Quaternion.Euler(0, neck.localRotation.eulerAngles.y, 0);

        if(kickWait > 0) kickWait -= Time.deltaTime;
        if(kickWait <= 0)
        {
        if(Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f)
        {
            //DO WALKING ANIMATION
            //if(leftStep)
            //{
                legRot += leftStep * Time.deltaTime * stepSpeed;// / (Quaternion.Angle(legLeft.localRotation, Quaternion.Euler(leftStep * 60, 0, 0)) / 180);
                if(Mathf.Abs(legRot) >= 60)
                {
                    if(!stepSwitch)
                    {
                        leftStep = -leftStep;
                        stepSwitch = true;
                    }
                }
                else stepSwitch = false;
                legLeft.localRotation = Quaternion.Euler(legRot, 0, 0);//Quaternion.Slerp(legLeft.localRotation, Quaternion.Euler(legRot, 0, 0), Time.deltaTime * 50f);
                legRight.localRotation = Quaternion.Euler(-legRot, 0, 0);//Quaternion.Slerp(legRight.localRotation, Quaternion.Euler(-legRot, 0, 0), Time.deltaTime * 50f);
                /*legLeft.localRotation = Quaternion.Slerp(legLeft.localRotation, Quaternion.Euler(-60, 0, 0), Time.deltaTime * moveSpeed * 100 / Quaternion.Angle(legLeft.localRotation, Quaternion.Euler(-60, 0, 0)));
                legRight.localRotation = Quaternion.Slerp(legRight.localRotation, Quaternion.Euler(60, 0, 0), Time.deltaTime * moveSpeed * 100 / Quaternion.Angle(legLeft.localRotation, Quaternion.Euler(60, 0, 0)));
                if(legLeft.localRotation.eulerAngles.x <= -60)
                    leftStep = false;*/
            /*}
            else
            {
                legLeft.localRotation = Quaternion.Slerp(legLeft.localRotation, Quaternion.Euler(60, 0, 0), Time.deltaTime * moveSpeed * 100 / Quaternion.Angle(legLeft.localRotation, Quaternion.Euler(60, 0, 0)));
                legRight.localRotation = Quaternion.Slerp(legRight.localRotation, Quaternion.Euler(-60, 0, 0), Time.deltaTime * moveSpeed * 100 / Quaternion.Angle(legLeft.localRotation, Quaternion.Euler(-60, 0, 0)));
                if (legLeft.localRotation.eulerAngles.x >= 60)
                    leftStep = true;
            }*/
        }
        else
        {
            legLeft.localRotation = Quaternion.Slerp(legLeft.localRotation, Quaternion.identity, Time.deltaTime);
            legRight.localRotation = Quaternion.Slerp(legRight.localRotation, Quaternion.identity, Time.deltaTime);
        }
        }


        //neck.localRotation = Quaternion.Euler(headXRot, neck.localRotation.eulerAngles.y, 0);
        //neck.localRotation * Quaternion.Euler(mouseY, mouseX, 0);

        if(Input.GetButtonDown("Jump") && !jumping && grounded)
            StartCoroutine(JumpCo());

        

        if(Input.GetButtonUp("Fire1"))
        {
            Kick();
        }

        if(Input.GetButton("Fire1"))
            kickTime += Time.deltaTime;
        else kickTime = 1f;
    }

    IEnumerator JumpCo()
    {
        jumping = true;
        yield return new WaitForFixedUpdate();
        rigid.AddForce(tran.up * rigid.mass * jumpPower, ForceMode.Impulse);
        jumping = false;
    }

    void FixedUpdate()
    {
        CheckGround();

        if(Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f)
        {
        Vector3 move = Vector3.ProjectOnPlane(neck.forward, Vector3.up).normalized * vertical
            + Vector3.ProjectOnPlane(neck.right, Vector3.up).normalized * horizontal;
        move = move.normalized;
        move *= moveSpeed;
        move = Vector3.ProjectOnPlane(move, groundNormal);
        //rigid.MovePosition(transform.position + move * Time.deltaTime * moveSpeed);
        float dot = Vector3.Dot(move, rigid.velocity);
        if(Vector3.ProjectOnPlane(rigid.velocity, Vector3.up).magnitude < moveSpeed)
            rigid.AddForce(move * Time.deltaTime, ForceMode.VelocityChange);
        else
        {
            if(dot < 0.7f)
                rigid.AddForce(move * Time.deltaTime, ForceMode.VelocityChange);
        }
        }
        else
        {
            if(grounded)
            {
                rigid.AddForce(-rigid.velocity * Time.deltaTime * slowSpeed, ForceMode.VelocityChange);
            }
        }
    }

    void CheckGround()
    {
        RaycastHit hit;
        if(Physics.SphereCast(tran.position + tran.up * 0.4f, 0.3f, -tran.up, out hit, 0.2f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            grounded = true;

            if(groundAnchor == null)
                groundAnchor = new GameObject("player ground anchor").transform;
            groundAnchor.position = hit.point;
            groundAnchor.rotation = Quaternion.identity;
            groundAnchor.parent = hit.collider.transform;

            groundNormal = hit.normal;
        }
        else
        {
            grounded = false;

            if(groundAnchor == null)
                groundAnchor = new GameObject("player ground anchor").transform;
            groundAnchor.parent = tran;
            groundAnchor.localPosition = Vector3.zero;
            groundAnchor.rotation = Quaternion.identity;

            groundNormal = Vector3.up;
        }
    }

    public void Kick()
    {
        //DO ANIMATION STUFF
        if(legRight.localRotation.eulerAngles.x < legLeft.localRotation.eulerAngles.x)
        {
            legRight.localRotation = Quaternion.Euler(-90, 0, 0);
            legLeft.localRotation = Quaternion.identity;
            leftStep = 1;
        }
        else
        {
            legLeft.localRotation = Quaternion.Euler(-90, 0, 0);
            legRight.localRotation = Quaternion.identity;
            leftStep = -1;
        }

        kickWait = 0.2f;


        RaycastHit hit;
        if(Physics.SphereCast(tran.position + tran.up, 0.25f, body.forward, out hit, 2f, kickLayers, QueryTriggerInteraction.Ignore))
        {
            if(hit.collider.attachedRigidbody != null)// && !hit.collider.attachedRigidbody.isKinematic)
            {
                Rigidbody rb = hit.collider.attachedRigidbody;

                PersonAI pai = hit.collider.GetComponentInParent<PersonAI>();
                if(pai != null)
                {
                    pai.ActivateRagdoll(true);
                    rb = pai.ragdoll[0];
                }
                else
                {
                    Pod pod = hit.collider.GetComponentInParent<Pod>();
                    if(pod != null)
                        pod.Fall();
                }

                Vector3 force = body.forward + (rb.worldCenterOfMass - hit.point).normalized;//rb.worldCenterOfMass - hit.point;
                force.y = Mathf.Max(0f, force.y * upMult);
                force = force.normalized;
                force *= kickPower * Mathf.Min(10f, kickTime);
                //force += Vector3.up * upMult;
                rb.AddForceAtPosition(force, hit.point, ForceMode.Impulse);

                MonsterAI mai = hit.collider.GetComponent<MonsterAI>();
                if (mai != null)
                    mai.Burst();
            }
        }

    }

    public void TakeDamage(float amount, Vector3 fromPos)
    {
        health -= amount;
        //if(health <= 0) Die();

        //SHOW SLASH ON SCREEN IN DIRECTION OF fromPos
        if(hurtUI != null)
        {
            Vector3 hitVector = fromPos - tran.position;
            float frontDot = Vector3.Dot(-neck.forward, hitVector);
            float sideDot = Vector3.Dot(neck.right, hitVector) + 1f;
            float upDot = Vector3.Dot(neck.up, hitVector) + 1f;
            Vector2 slashPos = new Vector2(sideDot, upDot);
            hurtUI.SlashAtPosition(slashPos);
        }
    }
}
