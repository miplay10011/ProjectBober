using System.Collections; // Для работы с Coroutine
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для работы с элементами UI

public class Inventory : MonoBehaviour
{
    public ItemData[] startingItems; // Предметы, которые инвентарь содержит при старте игры
    public int inventorySize = 5;
    public ItemData[] items; // Массив предметов в инвентаре.
    public GameObject[] itemPrefabs; // Соответствующие префабы для хранения в инвентаре

    public GameObject itemHolder; // Объект, к которому будет прикрепляться префаб предмета (то, что держит игрок в руках).

    public Image[] itemSlots; // Ссылки на Image компоненты слотов инвентаря в Canvas.
    public Color highlightColor = Color.yellow; // Цвет выделения выбранного слота.
    private Color defaultColor;

    public Sprite emptySlotSprite; // Спрайт, который будет отображаться в пустых слотах.

    private int selectedSlot = 0;
    private GameObject currentItemPrefab; // Текущий префаб предмета, который держит игрок в руках.
    private GameObject droppedItem; // Ссылка на выброшенный предмет.

    [Header("Throwing")]
    public float throwForce = 10f; // Сила броска.
    public float pickupDistance = 2f; // Расстояние для подбора предмета.
    public float ignoreCollisionDuration = 1f; // Длительность игнорирования столкновений (в секундах).

    [Header("Audio")]
    public AudioSource dropAudioSource; // Ссылка на AudioSource для звука выбрасывания.

    void Start()
    {
        items = new ItemData[inventorySize];
        itemPrefabs = new GameObject[inventorySize];
        defaultColor = itemSlots[0].color;
        InitializeInventory();
        SelectSlot(0); // Начинаем с первого слота.
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
        // Переключение слотов колесиком мыши.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            SelectSlot(selectedSlot - 1 < 0 ? inventorySize - 1 : selectedSlot - 1);
        }
        else if (scroll < 0)
        {
            SelectSlot(selectedSlot + 1 >= inventorySize ? 0 : selectedSlot + 1);
        }

        // Переключение слотов клавишами.
        for (int i = 1; i <= inventorySize; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                SelectSlot(i - 1);
            }
        }

        // Использовать предмет (E).
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }

        // Выбросить предмет (ПКМ).
        if (Input.GetMouseButtonDown(1))
        {
            ThrowItem();
        }

        // Выбросить предмет (G)
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
            // Включаем коллайдер у предыдущего предмета (если он был)
            ToggleCollider(currentItemPrefab, true);
            Destroy(currentItemPrefab);
            currentItemPrefab = null;
        }

        // Instantiate and parent the new item prefab, if available
        if (items[selectedSlot] != null && items[selectedSlot].Prefab != null)
        {
            currentItemPrefab = Instantiate(items[selectedSlot].Prefab);
            currentItemPrefab.transform.SetParent(itemHolder.transform, false);

            // Применяем смещение позиции и поворота
            currentItemPrefab.transform.localPosition = items[selectedSlot].HandPositionOffset;
            currentItemPrefab.transform.localRotation = Quaternion.Euler(items[selectedSlot].HandRotationOffset);

            // Отключаем коллайдер у нового предмета
            ToggleCollider(currentItemPrefab, false);
        }
    }

    // Добавляем предмет в инвентарь
    public void AddItem(ItemData item)
    {
        // Поиск свободного слота
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

    // Удаляем предмет из инвентаря
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
            // Воспроизводим звук выбрасывания
            if (dropAudioSource != null && items[selectedSlot].DropAudioClip != null)
            {
                dropAudioSource.PlayOneShot(items[selectedSlot].DropAudioClip);
            }

            // Включаем коллайдер у выбрасываемого предмета
            ToggleCollider(currentItemPrefab, true);

            // Убираем из родителей (itemHolder)
            currentItemPrefab.transform.SetParent(null);

            // Добавляем физику и силу
            Rigidbody rb = currentItemPrefab.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentItemPrefab.AddComponent<Rigidbody>();
            }
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

            // Включаем возможность подбора
            PickableItem pickableItem = currentItemPrefab.GetComponent<PickableItem>();
            if (pickableItem != null)
            {
                pickableItem.CanBePickedUp = true;
            }

            // Игнорируем столкновения с игроком
            StartCoroutine(IgnoreCollisionForSeconds(currentItemPrefab, transform, ignoreCollisionDuration));

            // Очищаем слот инвентаря
            items[selectedSlot] = null;
            itemPrefabs[selectedSlot] = null;
            currentItemPrefab = null;
            UpdateHeldItem(); // Обновляем отображение предмета в руке.
            UpdateUI();
        }
    }

    // Coroutine для временного игнорирования столкновений
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

    // Подбор предметов
    void CheckForPickups()
    {
        // Raycast для проверки, есть ли предмет перед игроком
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupDistance))
        {
            // Если луч попал в предмет
            if (hit.collider.gameObject == droppedItem) // Проверяем, что это именно выброшенный предмет.
            {
                // TODO: Проверка на то, что в инвентаре есть место.
                AddItem(droppedItem.GetComponent<PickableItem>().itemData); // Добавляем ItemData в инвентарь.
                Destroy(droppedItem); // Уничтожаем объект выброшенного предмета
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

    // Функция для включения/выключения коллайдера у объекта
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