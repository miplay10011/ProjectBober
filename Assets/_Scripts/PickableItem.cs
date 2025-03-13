using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public ItemData itemData; // ������ �� Scriptable Object � ������� ��������.
    private bool _canBePickedUp = true;

    public bool CanBePickedUp
    {
        get { return _canBePickedUp; }
        set { _canBePickedUp = value; }
    }
}