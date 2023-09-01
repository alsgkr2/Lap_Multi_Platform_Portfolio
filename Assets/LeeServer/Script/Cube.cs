using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private NetworkManager_Server server;
    public NetworkManager_Server Server { set { server = value; } }
    public ushort clientName;
    public ushort ClientName { set { clientName = value; } }
    public int itemInteger;
    //private float[] itemVector;
    [SerializeField] GameObject bullet;
    public GameObject gun;
    Animator anim;
    [SerializeField] GameObject player;
    BoxCollider boxCollider;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Cube 클래스 초기화
    /// </summary>
    /// <param name="server"></param>
    /// <param name="clientName"></param>
    public void Init(NetworkManager_Server server, ushort clientName)
    {
        this.server = server;
        this.clientName = clientName;
        this.server.reset += DestroySelf;
        boxCollider = GetComponent<BoxCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach(var i in server.ConnectedClients)
        {
            ServerClient client = i.Value;
            if (client.clientname == this.clientName)
            {
                transform.position = client.position;
                transform.localScale = client.scale;
                AnimChange(client.animation);
                gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, client.gun));
                gun.transform.localScale = client.scale;
                if (itemInteger != client.item)
                {
                    itemInteger = client.item;
                    if (itemInteger != 0)
                    {
                        GetItem();
                    }
                }
                continue;
            }
        }
        */
    }

    public void GetItem()
    {
        anim.SetBool("isGun", true);
        gun.SetActive(true);
    }

    private void DestroySelf()
    {
        server.reset -= DestroySelf;
        Destroy(gameObject);
    }
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
    public IEnumerator Hit()//  피격시 오브젝트 삭제 + 조작 불가 --> 리스폰
    {
        Debug.Log("Hit");
        player.transform.localScale = Vector3.zero;
        boxCollider.enabled = false;
        yield return new WaitForSeconds(1.5f);

        Debug.Log("리스폰");
        player.transform.localScale = Vector3.one * 2;
        boxCollider.enabled = true;
    }
}
