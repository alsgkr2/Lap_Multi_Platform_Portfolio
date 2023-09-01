using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { Idle, Walk, Freeze, rope, Dead };

public class JH_Player : MonoBehaviour
{
    Rigidbody rb;

    //public enum State { Idle, Walk, Freeze, rope, Dead };
   [SerializeField]  private State state = State.Idle;
    public State myState
    {
        get { return state; }
        set { state = value; }
    }
    //Move()
    [SerializeField] float moveSpeed;

    //Jump()
    [SerializeField] float jumpPower;
    public GameObject JumpCheck;
    [SerializeField] float JumpCheckLen;

    Vector3 lookVector;

    float h;
    float v;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        InputKey();
    }

    private void FixedUpdate()
    {
        
    }
    private void Move(float h, float v=0)
    {
        transform.Translate(new Vector3(h, v, 0) * moveSpeed * Time.deltaTime);
    }

    private void Jump()
    {


        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        Debug.DrawRay(transform.position, Vector3.down * JumpCheckLen, Color.red);
        if (Physics.Raycast(transform.position, Vector3.down, JumpCheckLen, layerMask))
        {
            print("점프 가능");
            if (Input.GetKeyDown(KeyCode.C))
            {
                rb.AddForce(Vector3.up * jumpPower);
            }

        }

      


    }

    void InputKey()
    {
        if (state == State.Freeze)
        {
            return;
        }

        h = Input.GetAxis("Horizontal");        // 가로축
        v = Input.GetAxis("Vertical");          // 세로축
        
        if (h != 0 || v != 0)
        {
            lookVector = new Vector3(h, v).normalized;
        }
        
       

        if (state == State.Idle || state == State.Walk)
        {
            Move(h);
        }
        else if (state == State.rope)
        {
            Move(0, v);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(state == State.Freeze)
        {
            state = State.Idle;
        }
    }
}