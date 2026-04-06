using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("玩家标签")]
    public string playerTag = "Player";

    [Header("靠近时自动启用，按 E 不禁用（离开时禁用）")]
    public GameObject[] onEnter_KeepOnInteract;

    [Header("靠近时自动启用，按 E 禁用（离开时禁用）")]
    public GameObject[] onEnter_DisableOnInteract;

    [Header("靠近时不启用，按 E 才启用（离开时禁用）")]
    public GameObject[] onInteractOnly;

    [Header("交互按键")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            SetActiveAll(onEnter_KeepOnInteract, true);
            SetActiveAll(onEnter_DisableOnInteract, true);
            // onInteractOnly 不自动激活
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            SetActiveAll(onEnter_KeepOnInteract, false);
            SetActiveAll(onEnter_DisableOnInteract, false);
            SetActiveAll(onInteractOnly, false);
        }
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(interactKey))
        {
            // 禁用 onEnter_DisableOnInteract 中的物体
            SetActiveAll(onEnter_DisableOnInteract, false);

            // 激活 onInteractOnly 中的物体（按E才启用）
            SetActiveAll(onInteractOnly, true);
        }
    }

    private void SetActiveAll(GameObject[] objects, bool active)
    {
        foreach (var obj in objects)
            if (obj != null) obj.SetActive(active);
    }
}