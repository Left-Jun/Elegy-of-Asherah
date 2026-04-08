using System.Collections;
using UnityEngine;

public class Platformmove : MonoBehaviour
{
    [SerializeField] Transform PiontA, PiontB;
    private Transform Pos;
    [SerializeField] private float speed;

    private bool isBeingDestroyed = false;

    void Start()
    {
        if (PiontA != null && PiontB != null)
            Pos = PiontA;
    }

    void Update()
    {
        if (PiontA == null || PiontB == null) return;
        if (Vector2.Distance(transform.position, PiontA.position) < 0.1f)
            Pos = PiontB;
        else if (Vector2.Distance(transform.position, PiontB.position) < 0.1f)
            Pos = PiontA;
        transform.position = Vector2.MoveTowards(transform.position, Pos.position, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (!collision.CompareTag("Player")) return;

        if (!isBeingDestroyed && gameObject != null && gameObject.activeInHierarchy)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (!collision.CompareTag("Player")) return;

        // 如果平台正在销毁或已失活，则放弃处理（场景即将卸载，玩家也会被销毁）
        if (isBeingDestroyed || gameObject == null || !gameObject.activeInHierarchy)
        {
            return; // 直接返回，不进行任何操作
        }

        // 正常情况：玩家离开平台，解除父子关系
        if (collision.transform.parent == transform)
            collision.transform.SetParent(null);
    }

    private void OnDestroy()
    {
        isBeingDestroyed = true;
        // 强制解除所有以本平台为父级的玩家（平台销毁时兜底）
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player != null && player.transform.parent == transform)
            {
                player.transform.SetParent(null);
                Debug.Log("平台销毁时强制解除玩家父子关系");
            }
        }
    }
}