using System.Collections; // ��� ������ � Coroutine
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // ��� ������ � ���������� UI

public class Inventory : MonoBehaviour
{
    public ItemData[] startingItems; // ��������, ������� ��������� �������� ��� ������ ����
    public int inventorySize = 5;
    public ItemData[] items; // ������ ��������� � ���������.
    public GameObject[] itemPrefabs; // ��������������� ������� ��� �������� � ���������

    public GameObject itemHolder; // ������, � �������� ����� ������������� ������ �������� (��, ��� ������ ����� � �����).

    public Image[] itemSlots; // ������ �� Image ���������� ������ ��������� � Canvas.
    public Color highlightColor = Color.yellow; // ���� ��������� ���������� �����.
    private Color defaultColor;

    public Sprite emptySlotSprite; // ������, ������� ����� ������������ � ������ ������.

    private int selectedSlot = 0;
    private GameObject currentItemPrefab; // ������� ������ ��������, ������� ������ ����� � �����.
    private GameObject droppedItem; // ������ �� ����������� �������.

    [Header("Throwing")]
    public float throwForce = 10f; // ���� ������.
    public float pickupDistance = 2f; // ���������� ��� ������� ��������.
    public float ignoreCollisionDuration = 1f; // ������������ ������������� ������������ (� ��������).

    [Header("Audio")]
    public AudioSource dropAudioSource; // ������ �� AudioSource ��� ����� ������������.

    void Start()
    {
        items = new ItemData[inventorySize];
        itemPrefabs = new GameObject[inventorySize];
        defaultColor = itemSlots[0].color;
        InitializeInventory();
        SelectSlot(0); // �������� � ������� �����.
    }

    void Update()
    {
        HandleInput();
        CheckForPickups();
    }

    void InitializeInventory()
    {
        for (int i = 0; i < startingItems.Length; i++)
        {
            AddItem(startingItems[i]);
        }
        UpdateUI();
    }

    void HandleInput()
    {
        // ������������ ������ ��������� ����.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            SelectSlot(selectedSlot - 1 < 0 ? inventorySize - 1 : selectedSlot - 1);
        }
        else if (scroll < 0)
        {
            SelectSlot(selectedSlot + 1 >= inventorySize ? 0 : selectedSlot + 1);
        }

        // ������������ ������ ���������.
        for (int i = 1; i <= inventorySize; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                SelectSlot(i - 1);
            }
        }

        // ������������ ������� (E).
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }

        // ��������� ������� (���).
        if (Input.GetMouseButtonDown(1))
        {
            ThrowItem();
        }

        // ��������� ������� (G)
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }
    }

    void SelectSlot(int slotIndex)
    {
        // Deselect old slot
        itemSlots[selectedSlot].color = defaultColor;

        selectedSlot = slotIndex;

        // Highlight new slot
        itemSlots[selectedSlot].color = highlightColor;

        UpdateHeldItem();
    }

    void UpdateHeldItem()
    {
        // Destroy the current item prefab
        if (currentItemPrefab != null)
        {
            // �������� ��������� � ����������� �������� (���� �� ���)
            ToggleCollider(currentItemPrefab, true);
            Destroy(currentItemPrefab);
            currentItemPrefab = null;
        }

        // Instantiate and parent the new item prefab, if available
        if (items[selectedSlot] != null && items[selectedSlot].Prefab != null)
        {
            currentItemPrefab = Instantiate(items[selectedSlot].Prefab);
            currentItemPrefab.transform.SetParent(itemHolder.transform, false);

            // ��������� �������� ������� � ��������
            currentItemPrefab.transform.localPosition = items[selectedSlot].HandPositionOffset;
            currentItemPrefab.transform.localRotation = Quaternion.Euler(items[selectedSlot].HandRotationOffset);

            // ��������� ��������� � ������ ��������
            ToggleCollider(currentItemPrefab, false);
        }
    }

    // ��������� ������� � ���������
    public void AddItem(ItemData item)
    {
        // ����� ���������� �����
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                UpdateUI();
                return;
            }
        }
        Debug.LogWarning("Inventory is full!");
    }

    // ������� ������� �� ���������
    public void RemoveItem(int slotIndex)
    {
        items[slotIndex] = null;
        UpdateUI();
    }


    void UseSelectedItem()
    {
        if (items[selectedSlot] != null)
        {
            items[selectedSlot].UseItem(gameObject);
        }
    }

    void ThrowItem()
    {
        DropItem();
    }

    void DropItem()
    {
        if (currentItemPrefab != null)
        {
            // ������������� ���� ������������
            if (dropAudioSource != null && items[selectedSlot].DropAudioClip != null)
            {
                dropAudioSource.PlayOneShot(items[selectedSlot].DropAudioClip);
            }

            // �������� ��������� � �������������� ��������
            ToggleCollider(currentItemPrefab, true);

            // ������� �� ��������� (itemHolder)
            currentItemPrefab.transform.SetParent(null);

            // ��������� ������ � ����
            Rigidbody rb = currentItemPrefab.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentItemPrefab.AddComponent<Rigidbody>();
            }
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

            // �������� ����������� �������
            PickableItem pickableItem = currentItemPrefab.GetComponent<PickableItem>();
            if (pickableItem != null)
            {
                pickableItem.CanBePickedUp = true;
            }

            // ���������� ������������ � �������
            StartCoroutine(IgnoreCollisionForSeconds(currentItemPrefab, transform, ignoreCollisionDuration));

            // ������� ���� ���������
            items[selectedSlot] = null;
            itemPrefabs[selectedSlot] = null;
            currentItemPrefab = null;
            UpdateHeldItem(); // ��������� ����������� �������� � ����.
            UpdateUI();
        }
    }

    // Coroutine ��� ���������� ������������� ������������
    private IEnumerator IgnoreCollisionForSeconds(GameObject thrownObject, Transform playerTransform, float duration)
    {
        if (thrownObject != null && playerTransform != null)
        {
            Collider[] itemColliders = thrownObject.GetComponentsInChildren<Collider>();
            Collider[] playerColliders = playerTransform.GetComponentsInChildren<Collider>();

            foreach (Collider itemCollider in itemColliders)
            {
                foreach (Collider playerCollider in playerColliders)
                {
                    Physics.IgnoreCollision(playerCollider, itemCollider, true);
                }
            }

            yield return new WaitForSeconds(duration);

            if (thrownObject != null && playerTransform != null)
            {
                foreach (Collider itemCollider in itemColliders)
                {
                    foreach (Collider playerCollider in playerColliders)
                    {
                        Physics.IgnoreCollision(playerCollider, itemCollider, false);
                    }
                }
            }
        }
    }

    // ������ ���������
    void CheckForPickups()
    {
        // Raycast ��� ��������, ���� �� ������� ����� �������
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupDistance))
        {
            // ���� ��� ����� � �������
            if (hit.collider.gameObject == droppedItem) // ���������, ��� ��� ������ ����������� �������.
            {
                // TODO: �������� �� ��, ��� � ��������� ���� �����.
                AddItem(droppedItem.GetComponent<PickableItem>().itemData); // ��������� ItemData � ���������.
                Destroy(droppedItem); // ���������� ������ ������������ ��������
                droppedItem = null;
            }
        }
    }

    public bool HasItem(ItemData item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                return;
            }
        }
    }
    void UpdateUI()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (items[i] != null)
            {
                itemSlots[i].sprite = items[i].InventoryIcon;
            }
            else
            {
                itemSlots[i].sprite = emptySlotSprite;
            }
        }
    }

    // ������� ��� ���������/���������� ���������� � �������
    void ToggleCollider(GameObject item, bool enable)
    {
        if (item != null)
        {
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = enable;
            }
        }
    }
}