using UnityEngine;

// Quits the player when the user hits escape
public class CloseApp : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey("escape")) // если нажата клавиша Esc (Escape)
        {
            Application.Quit(); // закрыть приложение
        }
    }

    public void Close()
    {
        Application.Quit();
    }
}