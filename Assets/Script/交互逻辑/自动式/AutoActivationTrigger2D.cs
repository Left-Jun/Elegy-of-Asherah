using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AutoActivationTrigger2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool acceptPlayerChildColliders = true;

    [Header("Activation")]
    [SerializeField] private GameObject[] enableOnEnter;
    [SerializeField] private GameObject[] deactivateOnEnter;
    [SerializeField] private GameObject[] deactivateOnExit;

    [Header("Messages")]
    [SerializeField] private string activateMessage = "ActivateByTrigger";
    [SerializeField] private string deactivateMessage = "DeactivateByTrigger";
    [SerializeField] private bool fallbackSetInactiveWhenNoReceiver = true;

    [Header("Collider")]
    [SerializeField] private bool forceTriggerCollider = true;

    private readonly HashSet<Collider2D> playerCollidersInside = new HashSet<Collider2D>();

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        if (forceTriggerCollider)
            EnsureTriggerCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerCollidersInside.Add(other);
        ActivateTargets(enableOnEnter);
        DeactivateTargets(deactivateOnEnter);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerCollidersInside.Remove(other);
        if (playerCollidersInside.Count > 0)
            return;

        DeactivateTargets(deactivateOnExit);
    }

    public void ActivateTargets(GameObject[] targets)
    {
        if (targets == null)
            return;

        foreach (GameObject target in targets)
        {
            if (target == null)
                continue;

            if (!target.activeSelf)
                target.SetActive(true);

            InvokeMessageIfPresent(target, activateMessage);
        }
    }

    public void DeactivateTargets(GameObject[] targets)
    {
        if (targets == null)
            return;

        foreach (GameObject target in targets)
        {
            if (target == null)
                continue;

            bool handled = InvokeMessageIfPresent(target, deactivateMessage);
            if (!handled && fallbackSetInactiveWhenNoReceiver)
                target.SetActive(false);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null || string.IsNullOrEmpty(playerTag))
            return false;

        if (other.CompareTag(playerTag))
            return true;

        if (!acceptPlayerChildColliders)
            return false;

        Transform current = other.transform.parent;
        while (current != null)
        {
            if (current.CompareTag(playerTag))
                return true;

            current = current.parent;
        }

        return false;
    }

    private bool InvokeMessageIfPresent(GameObject target, string messageName)
    {
        if (target == null || string.IsNullOrWhiteSpace(messageName))
            return false;

        bool handled = false;
        MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            MethodInfo method = behaviour.GetType().GetMethod(
                messageName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                System.Type.EmptyTypes,
                null);

            if (method == null)
                continue;

            method.Invoke(behaviour, null);
            handled = true;
        }

        return handled;
    }

    private void EnsureTriggerCollider()
    {
        if (!forceTriggerCollider)
            return;

        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
}
