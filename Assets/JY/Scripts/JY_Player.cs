using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JY_Player : MonoBehaviour
{
    public NetworkManager_Server server;
    public NetworkManager_Client client;
    private bool isServer;
    public bool IsServer
    {
        get { return isServer; }
        set { isServer = value; }
    }
    Rigidbody rb;
    BoxCollider boxCollider;
    Animator anim;
    public GameObject gun;
    [SerializeField]
    GameObject player;
    Vector3 startPos;
    [SerializeField]
    private Item item;
    public Text curCoinText;
    private float curCoin;
    //public enum State { Idle, Walk, Freeze, rope, Dead };
    [SerializeField] private State state = State.Idle;
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

    public Vector3 lookVector;

    float h;
    float v;

    private float itemDelay;
    private float curDelayTime;

    public ushort aniNum;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        startPos = transform.position;
        
    }
    void Start()
    {
        server = GameObject.Find("NetworkManager_Server").GetComponent<NetworkManager_Server>(); ;
        isServer = server.serverReady;
    }
    // Update is called once per frame
    void Update()
    {
        Jump();

        aniNum = Animation();
        InputKey();
        if (Input.GetKeyDown(KeyCode.Z)) this.UseItem();
        curDelayTime -= Time.deltaTime;
        gun.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg);

    }
    public void Goal()
    {
        GetComponent<JY_Player>().enabled = false;
        Debug.Log(this.gameObject.name + "골인");
    }
    public void GetItem(Item item)
    {
        this.item = item;
        //아이템 획득
        itemDelay = this.item.Init();
        anim.SetBool("isGun", true);
        gun.SetActive(true);

        if (isServer)
        {
            server.GainServerItem();
        }
        else
        {
            client.GainItem();
        }
    }

    private void Move(float h, float v = 0)
    {
        if (h < 0)
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
            gun.transform.localScale = new Vector3(-1, -1, 1);
        }
        else if (h > 0)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
            gun.transform.localScale = new Vector3(1, 1, 1);
        }
        transform.Translate(new Vector3(h, v, 0) * Time.deltaTime * moveSpeed * 5);
        //transform.position += new Vector3(h, v, 0) * moveSpeed * Time.fixedDeltaTime;
    }
    private void Jump()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        Debug.DrawRay(transform.position, Vector3.down * JumpCheckLen, Color.red);
        if (Physics.Raycast(transform.position, Vector3.down, JumpCheckLen, layerMask))
        {
            //print("점프 가능");
            if (Input.GetKeyDown(KeyCode.C))
            {
                rb.AddForce(Vector3.up * jumpPower);
                anim.SetTrigger("Jump");
            }
        }
    }
    /*private void Sit(float v)
    {
        if (v < 0)
        {
            //Debug.Log("Sit");
            boxCollider.center = new Vector3(0, -0.5f, 0);
            boxCollider.size = new Vector3(1, 1, 1);

        }
        else
        {
            //Debug.Log("Stand");
            boxCollider.center = new Vector3(0, 0, 0);
            boxCollider.size = new Vector3(1, 2, 1);
        }
    }*/
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
            anim.SetBool("isRun", true);
        }
        else anim.SetBool("isRun", false);

        if (state == State.Idle || state == State.Walk)
        {
            Move(h);
        }
        else if (state == State.rope)
        {
            Move(0, v);
        }

    }

    public void UseItem()
    {
        if (curDelayTime > 0 || item == null) return;
        if (!item.UseItem(transform.position, lookVector))
        {
            item = null;
            anim.SetBool("isGun", false);
            gun.SetActive(false);
            return;
        }
        if (isServer)
        {
            server.useItem(lookVector);
        }
        else
        {
            client.UseItem(lookVector);
        }
        if (!item.UseItem(transform.position, lookVector)) item = null;
        curDelayTime = itemDelay;
        if (item == null)
        {
            anim.SetBool("isGun", false);
            gun.SetActive(false);
        }


    }
    private void OnCollisionEnter(Collision collision)
    {
        if (state == State.Freeze)
        {
            state = State.Idle;
        }
        if (collision.gameObject.name == "Coin")
        {
            if (collision.gameObject.GetComponent<Coin>().isBig)
            {
                GetCoin(5);
            }
            else
            {
                GetCoin();
            }
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "Enemy")
        {
            this.myState = State.Freeze;
            StartCoroutine(Hit());
            return;
        }
    }
    public void GetCoin(float coin = 1)
    {
        curCoin += coin;
        curCoinText.text = "curCoin : " + curCoin;
    }

    public IEnumerator Hit()//  피격시 오브젝트 삭제 + 조작 불가 --> 리스폰
    {
        Debug.Log("Hit");
        player.SetActive(false);
        boxCollider.enabled = false;
        yield return new WaitForSeconds(1.5f);

        Debug.Log("리스폰");
        player.SetActive(true);
        boxCollider.enabled = true;
        transform.position = startPos;
    }

    /// <summary>
    /// 애니메이션
    /// </summary>
    public ushort Animation()
    {
        ushort i = 0;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) i += 0;
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run")) i += 1;
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) i += 2;

        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Idle")) i += 10;
        else if (anim.GetCurrentAnimatorStateInfo(1).IsName("Gun")) i += 20;

        return i;

    }
}
