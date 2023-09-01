using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_Cannon : JH_MapObject
{
    RaycastHit hit;


    [SerializeField] GameObject user;
    [SerializeField] float spinSpeed;
    [SerializeField] float launchPower;
    //[SerializeField] bool isEnter = false;
    [SerializeField] int turn=1;
    [SerializeField] bool SpinStop = false;

    Quaternion fristRotate;

    Move_Object_Manager manager;

    private void Start()
    {
        fristRotate = transform.rotation;
        manager = transform.GetComponentInParent<Move_Object_Manager>();
    }

    void Update()
    {
        Spin();
        Check();
    }

    public override void Drop()
    {
        base.Drop();
        Invoke("Init", 0.2f);
    }

    void Init()
    {
        transform.rotation = fristRotate;
        //isEnter = false;
        turn = -1;
        SpinStop = false;
        GetComponent<CapsuleCollider>().isTrigger = false;
    }

    void Spin()
    {
        Debug.DrawRay(transform.position, Vector3.right * 10, Color.blue);

        if ( manager.usingCannon)
        {
            if(spinSpeed>0)
            {
                if (transform.GetChild(0).transform.position.x <= transform.position.x || transform.GetChild(0).transform.position.y <= transform.position.y)
                {
                    Debug.Log(transform.rotation);
                    turn *= -1;
                    transform.Rotate(new Vector3(turn * (spinSpeed / 25), 0, 0));
                }
            }
            else
            {

                if (transform.GetChild(0).transform.position.x >= transform.position.x || transform.GetChild(0).transform.position.y <= transform.position.y)
                {
                    Debug.Log(transform.rotation);
                    turn *= -1;
                    transform.Rotate(new Vector3(turn * (spinSpeed / 25), 0,0));
                }
            }
            

            if(SpinStop==false)
            {
                transform.Rotate(new Vector3(turn * spinSpeed * Time.deltaTime, 0, 0));
            }

            if (Input.GetKeyDown(KeyCode.X) && manager.usingCannon)
            {
                SpinStop = true;
                Fire();
            }

        }
    }

    void Fire()
    {
        user.GetComponent<Rigidbody>().velocity = Vector3.zero;
        user.GetComponent<Rigidbody>().useGravity = true;
        Vector3 vec = transform.GetChild(0).transform.position - transform.position;
        user.GetComponent<Rigidbody>().AddForce(new Vector3(vec.x, vec.y, 0) * launchPower);

        user = null;

        manager.usingCannon = false;
        manager.Send(this, false);

        Invoke("Init",0.2f);
    }

   void Check()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Player");
        Debug.DrawRay(transform.position, Vector3.up * 10, Color.red);

        if (Physics.Raycast(transform.position, Vector3.up,  out hit, 10, layerMask))
        {
            print("상호작용 누르면 대포 탈수 있음");
            if (Input.GetKeyDown(KeyCode.X) && !manager.usingCannon)
            {
                user = hit.transform.gameObject;
                //isEnter = true;
                transform.GetComponent<CapsuleCollider>().isTrigger = true;
                user.GetComponent<Rigidbody>().useGravity = false;
                user.transform.position = transform.position;
                user.GetComponent<Rigidbody>().velocity = Vector3.zero;
                user.GetComponent<JY_Player>().myState = State.Freeze;

                manager.usingCannon = true;
                manager.Send(this, true);
            }
        }
    }
}
