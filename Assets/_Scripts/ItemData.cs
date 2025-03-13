using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public GameObject Prefab; // Префаб, который игрок держит в руках.
    public Sprite InventoryIcon; // Иконка для отображения в инвентаре.
    public bool IsStackable; // Возможность складывать несколько предметов в один слот (например, для стрел).
    public int MaxStackSize = 99; // Максимальное количество предметов в стаке (если IsStackable == true).
    [TextArea(3, 10)]
    public string Description; // Описание предмета (для отображения в UI).

    // Дополнительные параметры, зависящие от типа предмета
    public bool isConsumable; // Является ли предмет расходным (например, зелье)
    public bool isWeapon; // Является ли предмет оружием
    public Vector3 HandPositionOffset; // Смещение позиции предмета в руке.
    public Vector3 HandRotationOffset; // Смещение поворота предмета в руке
    public AudioClip DropAudioClip; // Звук выбрасывания.
    public AudioClip PickupAudioClip; // Звук подбора.

    // Методы для использования предмета (например, вызов анимации, применение эффекта)
    public virtual void UseItem(GameObject user)
    {
        Debug.Log("Used " + ItemName);
    }

}