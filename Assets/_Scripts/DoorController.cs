using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public enum OpenState { Closed, Opening, Open, Closing }
    public OpenState CurrentState = OpenState.Closed;

    [Header("Настройки")]
    public float openAngle = 90f; // Угол открытия двери (в градусах)
    public float openSpeed = 2f; // Скорость открытия/закрытия двери
    public float closeDelay = 2f; // Задержка перед автоматическим закрытием

    private Quaternion closedRotation;
    private Quaternion openedRotation;
    private float timer = 0f;

    void Start()
    {
        closedRotation = transform.rotation;
        openedRotation = Quaternion.Euler(closedRotation.eulerAngles.x, closedRotation.eulerAngles.y + openAngle, closedRotation.eulerAngles.z);
    }

    void Update()
    {
        switch (CurrentState)
        {
            case OpenState.Opening:
                transform.rotation = Quaternion.Slerp(transform.rotation, openedRotation, Time.deltaTime * openSpeed);
                if (Quaternion.Angle(transform.rotation, openedRotation) < 1f)
                {
                    CurrentState = OpenState.Open;
                }
                break;

            case OpenState.Closing:
                transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, Time.deltaTime * openSpeed);
                if (Quaternion.Angle(transform.rotation, closedRotation) < 1f)
                {
                    CurrentState = OpenState.Closed;
                }
                break;

            case OpenState.Open:
                timer += Time.deltaTime;
                if (closeDelay > 0 && timer >= closeDelay)
                {
                    SetDoorState(false); // Закрываем дверь автоматически
                    timer = 0f;
                }
                break;

            case OpenState.Closed:
                break;
        }
    }

    // Измененный метод для управления дверью через bool (true = открыть, false = закрыть)
    public void SetDoorState(bool open)
    {
        if (open)
        {
            if (CurrentState == OpenState.Closed)
            {
                CurrentState = OpenState.Opening;
                timer = 0f;
            }
        }
        else
        {
            if (CurrentState == OpenState.Open)
            {
                CurrentState = OpenState.Closing;
                timer = 0f;
            }
            else if (CurrentState == OpenState.Opening)
            {
                CurrentState = OpenState.Closing;
                timer = 0f;
            }
        }
    }
}