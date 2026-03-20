/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallcheck : MonoBehaviour
{
    public bool isWall;//贴墙状态
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall Detected");
            isWall = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall Left");
            isWall = false;
        }
    }
}
*/