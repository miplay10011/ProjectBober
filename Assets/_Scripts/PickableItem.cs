using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public ItemData itemData; // —сылка на Scriptable Object с данными предмета.
    private bool _canBePickedUp = true;

    public bool CanBePickedUp
    {
        get { return _canBePickedUp; }
        set { _canBePickedUp = value; }
    }
}