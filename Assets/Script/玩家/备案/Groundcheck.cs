/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Groundcheck : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]private float groundCheckDistance;//地面检测距离
    [SerializeField]private LayerMask groundLayer;//地面图层
    public bool isGrounded;//地面状态
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
    }
    
    // 可视化地面检测射线
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,new Vector2(transform.position.x,transform.position.y - groundCheckDistance));
    }
    //地面检测函数
    private void GroundCheck()
    {
        isGrounded = Physics2D.Raycast(transform.position,Vector2.down,groundCheckDistance,groundLayer);
    }
}
*/