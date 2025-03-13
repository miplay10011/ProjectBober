using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractController : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactionDistance = 2f;
    public Camera playerCamera;
    public LayerMask interactionLayer;

    [Header("UI")]
    public Text interactionText;
    public string defaultInteractionMessage = "Press F to Interact";
    public float fadeDuration = 0.25f;

    private GameObject _currentInteractable;
    private CanvasRenderer _textCanvasRenderer;
    public Inventory inventory;

    [Header("Audio")]
    public AudioSource pickupAudioSource;


    void Start()
    {
        if (interactionText != null)
        {
            _textCanvasRenderer = interactionText.GetComponent<CanvasRenderer>();
            if (_textCanvasRenderer == null)
            {
                _textCanvasRenderer = interactionText.gameObject.AddComponent<CanvasRenderer>();
            }
            _textCanvasRenderer.SetAlpha(0f);
            interactionText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        PerformRaycast();

        if (Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }
    }

    void PerformRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionDistance, interactionLayer))
        {
            if (hit.collider.gameObject.GetComponent<InteractableTrigger>() != null)
            {
                if (_currentInteractable != hit.collider.gameObject)
                {
                    _currentInteractable = hit.collider.gameObject;
                    StopAllCoroutines();
                    StartCoroutine(FadeText(1f));
                    ShowInteractionText(defaultInteractionMessage);
                }
            }
            else if (hit.collider.gameObject.GetComponent<PickableItem>() != null)
            {
                if (_currentInteractable != hit.collider.gameObject)
                {
                    _currentInteractable = hit.collider.gameObject;
                    StopAllCoroutines();
                    StartCoroutine(FadeText(1f));
                    ShowInteractionText(defaultInteractionMessage);
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FadeText(0f));
                _currentInteractable = null;
            }
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FadeText(0f));
            _currentInteractable = null;
        }
    }

    void Interact()
    {
        if (_currentInteractable != null)
        {
            InteractableTrigger interactableTrigger = _currentInteractable.GetComponent<InteractableTrigger>();
            if (interactableTrigger != null)
            {
                interactableTrigger.Interact(inventory); // Pass the inventory
                return;
            }

            PickableItem pickableItem = _currentInteractable.GetComponent<PickableItem>();
            if (pickableItem != null && pickableItem.CanBePickedUp)
            {
                inventory.AddItem(pickableItem.itemData);

                if (pickupAudioSource != null && pickableItem.itemData.PickupAudioClip != null)
                {
                    pickupAudioSource.PlayOneShot(pickableItem.itemData.PickupAudioClip);
                }

                Destroy(_currentInteractable.gameObject);
                StopAllCoroutines();
                StartCoroutine(FadeText(0f));
                _currentInteractable = null;
            }
        }
    }

    private IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = _textCanvasRenderer.GetAlpha();
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            _textCanvasRenderer.SetAlpha(alpha);
            yield return null;
        }

        _textCanvasRenderer.SetAlpha(targetAlpha);
    }

    private void ShowInteractionText(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
        }
    }
}