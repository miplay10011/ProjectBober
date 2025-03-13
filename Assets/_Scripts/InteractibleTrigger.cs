using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour
{
    [Tooltip("События, которые будут вызваны при обычном взаимодействии (без учета ItemData).")]
    public UnityEvent OnInteract;

    [Tooltip("ItemData, необходимый для выполнения особого взаимодействия. Оставьте пустым, если обычного взаимодействия достаточно.")]
    public ItemData RequiredItem;
    [Tooltip("Удалить предмет из инвентаря после использования?")]
    public bool DeleteAfterUse = false;

    [Tooltip("События, которые будут вызваны, если в руке у игрока ItemData == RequiredItem")]
    public UnityEvent OnFinalInteract;

    [Tooltip("События, которые будут вызваны, если RequiredItem указан, но в руке у игрока его нет.")]
    public UnityEvent OnFailedInteract;


    public virtual void Interact(Inventory inventory)
    {
        // Проверяем, включен ли компонент.
        if (enabled)
        {
            if (RequiredItem != null)
            {
                // Проверяем, есть ли у игрока нужный предмет.
                if (inventory.HasItem(RequiredItem))
                {
                    OnFinalInteract?.Invoke();

                    // Удаляем предмет из инвентаря, если DeleteAfterUse == true
                    if (DeleteAfterUse)
                    {
                        inventory.RemoveItem(RequiredItem);
                    }
                }
                else
                {
                    OnFailedInteract?.Invoke();
                }
            }
            else
            {
                // Если RequiredItem не указан, вызываем обычные события.
                OnInteract?.Invoke();
            }
        }
        else
        {
            // Если компонент выключен, ничего не делаем. Можно добавить сюда, например, звук ошибки.
            Debug.Log("InteractableTrigger is disabled on " + gameObject.name);
        }
    }
}