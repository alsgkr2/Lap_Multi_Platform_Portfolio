using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JH_FallFloor : JH_MapObject
{
    // Start is called before the first frame update
    Vector3 firstPos;
    [SerializeField] float WaitTime;//떨어지기전 기다려주는 시간
    [SerializeField] float fallSpeed;
    bool isFall = false;
    [SerializeField] float timer;

    Move_Object_Manager manager;
    bool flag;

    private void Start()
    {
        firstPos = transform.position;
        manager = transform.GetComponentInParent<Move_Object_Manager>();
    }

    private void Update()
    {
        if(flag) Timer();
    }

    public override void Drop()
    {
        base.Drop();
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    void Timer()
    {
        timer += Time.deltaTime;
        if (timer >= WaitTime && isFall)
        {
            isFall = false;
            flag = false;
            manager.Send(this);
            Drop();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.transform.position.y > transform.position.y) && collision.transform.CompareTag("Player") && !isFall)
        {
            print("플레이어가 위에서 밟음");
            isFall = true;
            flag = true;
        }

        if (collision.transform.CompareTag("Ground"))
        {
            timer = 0;
            transform.position = firstPos;
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}