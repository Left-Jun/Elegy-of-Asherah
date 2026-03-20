using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    [SerializeField]private Transform point;
    public Transform checkPoint;//当前检查点
    // Start is called before the first frame update
    void Awake()
    {
        checkPoint = point;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
