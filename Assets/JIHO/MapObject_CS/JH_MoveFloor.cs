using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_MoveFloor : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Vector3 moveDir;
    [SerializeField] float changeDirTime;
    void Start()
    {
        StartCoroutine("ChangeDir");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

    }

    IEnumerator ChangeDir()
    {
        yield return new WaitForSeconds(changeDirTime);
        moveDir=new Vector3 (moveDir.x*-1, moveDir.y*-1, moveDir.z*-1);
        StartCoroutine("ChangeDir");
    }
}
