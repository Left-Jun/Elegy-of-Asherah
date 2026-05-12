using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneObjectActivatorTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool makeColliderTrigger = true;

    [Header("Targets")]
    [SerializeField] private GameObject[] targets;
    [SerializeField] private bool deactivateTargetsOnAwake = false;
    [SerializeField] private bool deactivateOnExit = false;

    [Header("State")]
    [SerializeField] private bool triggerOnlyOnce = false;
    [SerializeField] private bool saveActivatedState = false;
    [SerializeField] private string saveKey;

    [Header("Events")]
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;

    private readonly HashSet<Collider2D> playerCollidersInside = new HashSet<Collider2D>();
    private bool hasActivated;

    private string ResolvedSaveKey
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(saveKey))
                return saveKey;

            string sceneName = SceneManager.GetActiveScene().name;
            return $"{sceneName}/{gameObject.name}/SceneObjectActivatorTrigger";
        }
    }

    private void Awake()
    {
        EnsureTriggerCollider();

        bool savedActive = saveActivatedState && PlayerPrefs.GetInt(ResolvedSaveKey, 0) == 1;
        if (savedActive)
        {
            ActivateTargets(false);
            hasActivated = true;
        }
        else if (deactivateTargetsOnAwake)
        {
            SetTargetsActive(false);
        }
    }

    private void OnValidate()
    {
        if (makeColliderTrigger)
            EnsureTriggerCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerCollidersInside.Add(other);
        ActivateTargets(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerCollidersInside.Remove(other);
        if (playerCollidersInside.Count > 0)
            return;

        if (deactivateOnExit && !saveActivatedState)
            DeactivateTargets();
    }

    public void ActivateTargets()
    {
        ActivateTargets(true);
    }

    public void DeactivateTargets()
    {
        if (saveActivatedState)
            return;

        SetTargetsActive(false);
        onDeactivated?.Invoke();
    }

    public void ResetSavedState()
    {
        if (saveActivatedState)
            PlayerPrefs.DeleteKey(ResolvedSaveKey);

        hasActivated = false;
    }

    private void ActivateTargets(bool invokeEvents)
    {
        if (triggerOnlyOnce && hasActivated)
            return;

        hasActivated = true;
        SetTargetsActive(true);

        if (saveActivatedState)
        {
            PlayerPrefs.SetInt(ResolvedSaveKey, 1);
            PlayerPrefs.Save();
        }

        if (invokeEvents)
            onActivated?.Invoke();
    }

    private void SetTargetsActive(bool active)
    {
        if (targets == null)
            return;

        foreach (GameObject target in targets)
        {
            if (target != null)
                target.SetActive(active);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other != null && other.CompareTag(playerTag);
    }

    private void EnsureTriggerCollider()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
}
