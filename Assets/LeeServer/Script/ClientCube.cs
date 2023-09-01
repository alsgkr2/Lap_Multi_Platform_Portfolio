using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCube : MonoBehaviour
{
    public enum animState
    {
        Idle, Run, Jump
    }
    Item item;
    int useCount;
    int curUseCount;
    private NetworkManager_Client client;
    public NetworkManager_Client Client { set { client = value; } }
    public ushort clientName;
    public ushort ClientName { set { clientName = value; } }

    private int itemInteger;
    [SerializeField] GameObject bullet;
    public GameObject gun;
    Animator anim;
    [SerializeField] GameObject player;
    BoxCollider boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        //useCount = item.useCount;
        this.client.reset += DestroySelf;
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach (var i in client.tuples)
        {
            if (i.Item1 == this.clientName)
            {
                AnimChange(i.Item5);
                transform.position = i.Item2;
                //gun.transform.localScale = new Vector3(i.Item3, 1, 1);
                transform.localScale = new Vector3(i.Item3, 1, 1);
                gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, i.Item4));
                gun.transform.localScale = new Vector3(i.Item3, 1, 1);
                if (client.getItems[clientName] != 0)
                {
                    GetItem();
                    client.getItems[clientName] = 0;
                }
                continue;
            }
        }
        */
    }
    /// <summary>
    /// 0 : Idle���� / 1 : Run���� / 2 : Jump����
    /// </summary>
    public void AnimChange(int i = 0)
    {

        switch (i / 10)
        {

            case 1:
                anim.SetBool("isGun", false);
                gun.SetActive(false);
                switch (i % 10)
                {
                    case 0:
                        anim.SetBool("isRun", false);
                        return;
                    case 1:
                        //anim.Play("Run");
                        anim.SetBool("isRun", true);
                        return;
                    case 2:
                        anim.SetTrigger("Jump");
                        return;

                }
                return;
            case 2:
                anim.SetBool("isGun", true);
                gun.SetActive(true);
                switch (i % 10)
                {
                    case 0:
                        anim.SetBool("isRun", false);
                        return;
                    case 1:
                        //anim.Play("Run");
                        anim.SetBool("isRun", true);
                        return;
                    case 2:
                        anim.SetTrigger("Jump");
                        return;

                }
                return;
        }
    }
    public void GetItem()
    {
        anim.SetBool("isGun", true);
        gun.SetActive(true);
    }

    private void DestroySelf()
    {
        client.reset -= DestroySelf;
        Destroy(gameObject);
    }

    public IEnumerator Hit()//  �ǰݽ� ������Ʈ ���� + ���� �Ұ� --> ������
    {
        Debug.Log("Hit");
        player.transform.localScale = Vector3.zero;
        boxCollider.enabled = false;
        yield return new WaitForSeconds(1.5f);

        Debug.Log("������");
        player.transform.localScale = Vector3.one * 2;
        boxCollider.enabled = true;
    }
}