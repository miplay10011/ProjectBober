using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public GameObject Prefab; // ������, ������� ����� ������ � �����.
    public Sprite InventoryIcon; // ������ ��� ����������� � ���������.
    public bool IsStackable; // ����������� ���������� ��������� ��������� � ���� ���� (��������, ��� �����).
    public int MaxStackSize = 99; // ������������ ���������� ��������� � ����� (���� IsStackable == true).
    [TextArea(3, 10)]
    public string Description; // �������� �������� (��� ����������� � UI).

    // �������������� ���������, ��������� �� ���� ��������
    public bool isConsumable; // �������� �� ������� ��������� (��������, �����)
    public bool isWeapon; // �������� �� ������� �������
    public Vector3 HandPositionOffset; // �������� ������� �������� � ����.
    public Vector3 HandRotationOffset; // �������� �������� �������� � ����
    public AudioClip DropAudioClip; // ���� ������������.
    public AudioClip PickupAudioClip; // ���� �������.

    // ������ ��� ������������� �������� (��������, ����� ��������, ���������� �������)
    public virtual void UseItem(GameObject user)
    {
        Debug.Log("Used " + ItemName);
    }

}