using UnityEngine;

// Quits the player when the user hits escape
public class CloseApp : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey("escape")) // ���� ������ ������� Esc (Escape)
        {
            Application.Quit(); // ������� ����������
        }
    }

    public void Close()
    {
        Application.Quit();
    }
}