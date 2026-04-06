using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidgetController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Widget组件控制器，挂载在需要淡入的UI物体上")]
    [Header("渐入时间")]
    [SerializeField]float fadeDuration = 0.5f;

    void OnEnable() 
    {
    var w = GetComponent<Widget>();
    w.RenderOpacity = 0;
    w.Fade(1, fadeDuration, null);
    }
}
