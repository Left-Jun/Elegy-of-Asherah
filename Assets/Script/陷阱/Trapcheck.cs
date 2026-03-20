using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 伤害判定 : MonoBehaviour
{
    public bool traphurt;//受伤状态
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))//检测到玩家
        {
            Debug.Log("Player Intrap");
            traphurt = true;//受伤状态
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))//离开玩家
        {
            Debug.Log("Player Lefttrap");
            traphurt = false;//受伤状态
        }
    }
}
