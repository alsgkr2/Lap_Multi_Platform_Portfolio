using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JH_IceSpike : JH_MapObject
{
    [SerializeField] float Speed;
    Move_Object_Manager manager;
    public bool isPlayer = false;
    Vector3 firstPos;

    private void Start()
    {
        firstPos = transform.position;
        manager = transform.GetComponentInParent<Move_Object_Manager>();
    }

    private void Update()
    {
        CheckPlayer();
    }

    void CheckPlayer()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Player");
        Debug.DrawRay(firstPos, Vector3.down * 20, Color.blue);

        if (Physics.Raycast(firstPos, Vector3.down, 20, layerMask) && !isPlayer)
        {
            Drop();
            manager.Send(this);
        }
    }

    public override void Drop()
    {
        base.Drop();
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
        isPlayer = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject)
        {
            transform.GetComponent<BoxCollider>().isTrigger = true;
            transform.GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;  
            Invoke("Init", 0.5f);
        }
    }

    void Init()
    {
        print("오브젝트와 충돌");
        transform.GetComponent<BoxCollider>().isTrigger = false;
        transform.GetComponent<MeshRenderer>().enabled = true;
        transform.position = firstPos;
        isPlayer = false;
    }
}
