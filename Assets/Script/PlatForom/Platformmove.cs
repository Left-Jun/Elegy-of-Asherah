using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformmove : MonoBehaviour
{
    [SerializeField] Transform PiontA,PiontB;
    private Transform Pos;
    [SerializeField] private float speed;

    void Start()
    {
        Pos = PiontA;//初始位置
    }

    void Update()
    {
        //目标位置切换
        if(Vector2.Distance(transform.position,PiontA.position)<0.1f)
            Pos = PiontB;
        else if(Vector2.Distance(transform.position,PiontB.position)<0.1f)
            Pos = PiontA;
        //移动
        transform.position = Vector2.MoveTowards(transform.position,Pos.position,speed*Time.deltaTime);
    }

    //玩家进入平台触发器时，设置玩家为平台的子物体
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided");
        if(collision.gameObject.CompareTag("Player"))
        {
            // 安全起见，检查平台自身是否处于激活状态
            if (this.gameObject.activeInHierarchy)
            {
                collision.transform.SetParent(transform);
            }
            else
            {
                Debug.LogWarning("平台未激活，不会将玩家设为子对象。");
            }
        }
    }
    //玩家离开平台触发器时，取消父子关系
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
