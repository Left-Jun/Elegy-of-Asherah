using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRespawn : MonoBehaviour
{
    private Rigidbody2D rbody;
    private Vector2 respawnPosition;//重生位置
    // Start is called before the first frame update

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        respawnPosition = GetComponent<Checkpoints>().checkPoint.position;//重生位置初始化
    }

    // Update is called once per frame
    void Update()
    {
        respawnPosition = GetComponent<Checkpoints>().checkPoint.position;//重生位置更新
        Debug.Log("Respawn Position: " + respawnPosition);
    }
    public void Respawn()
    {
        transform.position = respawnPosition;//重生位置
    }
}
