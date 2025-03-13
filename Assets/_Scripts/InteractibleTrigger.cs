using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour
{
    [Tooltip("�������, ������� ����� ������� ��� ������� �������������� (��� ����� ItemData).")]
    public UnityEvent OnInteract;

    [Tooltip("ItemData, ����������� ��� ���������� ������� ��������������. �������� ������, ���� �������� �������������� ����������.")]
    public ItemData RequiredItem;
    [Tooltip("������� ������� �� ��������� ����� �������������?")]
    public bool DeleteAfterUse = false;

    [Tooltip("�������, ������� ����� �������, ���� � ���� � ������ ItemData == RequiredItem")]
    public UnityEvent OnFinalInteract;

    [Tooltip("�������, ������� ����� �������, ���� RequiredItem ������, �� � ���� � ������ ��� ���.")]
    public UnityEvent OnFailedInteract;


    public virtual void Interact(Inventory inventory)
    {
        // ���������, ������� �� ���������.
        if (enabled)
        {
            if (RequiredItem != null)
            {
                // ���������, ���� �� � ������ ������ �������.
                if (inventory.HasItem(RequiredItem))
                {
                    OnFinalInteract?.Invoke();

                    // ������� ������� �� ���������, ���� DeleteAfterUse == true
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
                // ���� RequiredItem �� ������, �������� ������� �������.
                OnInteract?.Invoke();
            }
        }
        else
        {
            // ���� ��������� ��������, ������ �� ������. ����� �������� ����, ��������, ���� ������.
            Debug.Log("InteractableTrigger is disabled on " + gameObject.name);
        }
    }
}