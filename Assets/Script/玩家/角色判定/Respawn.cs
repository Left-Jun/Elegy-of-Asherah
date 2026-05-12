using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRespawn : MonoBehaviour
{
    private Rigidbody2D rbody;
    private Vector2 respawnPosition;

    [SerializeField] private float respawnDelay = 0.3f; // 重生延迟时间（秒），可在Inspector面板中调整

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        respawnPosition = GetComponent<Checkpoints>().checkPoint.position;
    }

    void Update()
    {
        Checkpoints checkpoints = GetComponent<Checkpoints>();
        if (checkpoints != null && checkpoints.checkPoint != null)
            respawnPosition = checkpoints.checkPoint.position;
    }

    public void Respawn()
    {
        // 取消之前还未执行的延迟调用（避免重复重生时多个延迟叠加）
        CancelInvoke(nameof(PerformRespawn));
        // 延迟执行真正的重生逻辑
        Invoke(nameof(PerformRespawn), respawnDelay);
    }

    private void PerformRespawn()
    {
        transform.position = respawnPosition;
        // 可选：重置刚体速度，防止残留的动量影响重生后的状态
        if (rbody != null)
        {
            rbody.velocity = Vector2.zero;
            rbody.angularVelocity = 0f;
        }
    }
}
