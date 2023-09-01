using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item :MonoBehaviour
{
    [SerializeField]
    protected GameObject curItem;
    [SerializeField]
    public int useCount=3;       //  ��밡��Ƚ��
    [SerializeField]
    protected float delayTime;    //  ��� �� ���� ��밡�ɽð������� ������   //  ���� ������ �ð�
    protected Vector3 lookVector;

    public virtual float Init()
    {
        return delayTime;
    }
    /// <summary>
    /// ���Ƚ���� 0�����̸� return false
    /// </summary>
    /// <param name="fireVector"></param>
    /// <param name="lookVector"></param>
    /// <returns></returns>
    public virtual bool UseItem(Vector3 fireVector, Vector3 lookVector)
    {
        Fn(fireVector, lookVector);
        if (--useCount <= 0)
        {
            return false;
        }
        return true;
    }
    public virtual void Fn(Vector3 fireVector, Vector3 lookVector)
    {
    }
    void Update()
    {
    }
}

