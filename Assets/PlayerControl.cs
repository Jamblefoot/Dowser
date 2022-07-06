using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] Transform neck;
    [SerializeField] Transform body;
    [SerializeField] LayerMask groundLayers;

    float moveSpeed = 10;
    float jumpPower = 10;

    Rigidbody rigid;

    float headXRot;

    float vertical, horizontal;
    bool jumping;

    bool grounded;

    Transform groundAnchor;
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
        headXRot = Mathf.Clamp(headXRot, -80f, 80f);

        neck.localRotation = Quaternion.Euler(headXRot, neck.localRotation.eulerAngles.y, 0)
                            * Quaternion.Euler(0, mouseX, 0);

        body.localRotation = Quaternion.Euler(0, neck.localRotation.eulerAngles.y, 0);

        //neck.localRotation = Quaternion.Euler(headXRot, neck.localRotation.eulerAngles.y, 0);
        //neck.localRotation * Quaternion.Euler(mouseY, mouseX, 0);

        if(Input.GetButtonDown("Jump") && !jumping && grounded)
            StartCoroutine(JumpCo());
    }

    IEnumerator JumpCo()
    {
        jumping = true;
        yield return new WaitForFixedUpdate();
        rigid.AddForce(transform.up * rigid.mass * jumpPower, ForceMode.Impulse);
        jumping = false;
    }

    void FixedUpdate()
    {
        CheckGround();

        Vector3 move = Vector3.ProjectOnPlane(neck.forward, Vector3.up).normalized * vertical
            + Vector3.ProjectOnPlane(neck.right, Vector3.up).normalized * horizontal;
        move = move.normalized;
        move *= moveSpeed;
        //rigid.MovePosition(transform.position + move * Time.deltaTime * moveSpeed);
        float dot = Vector3.Dot(move, rigid.velocity);
        if(rigid.velocity.magnitude < moveSpeed)
            rigid.AddForce(move * Time.deltaTime, ForceMode.VelocityChange);
        else
        {
            if(dot < 0.7f)
                rigid.AddForce(move * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    void CheckGround()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + transform.up * 0.1f, -transform.up, out hit, 0.2f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            grounded = true;

            if(groundAnchor == null)
                groundAnchor = new GameObject("player ground anchor").transform;
            groundAnchor.position = hit.point;
            groundAnchor.rotation = Quaternion.identity;
            groundAnchor.parent = hit.collider.transform;
        }
        else
        {
            grounded = false;

            if(groundAnchor == null)
                groundAnchor = new GameObject("player ground anchor").transform;
            groundAnchor.parent = transform;
            groundAnchor.localPosition = Vector3.zero;
            groundAnchor.rotation = Quaternion.identity;
        }
    }
}
