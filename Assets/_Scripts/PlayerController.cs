using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 300f;

    [Header("Look")]
    public float lookSensitivityX = 80f;
    public float lookSensitivityY = 80f;
    public float smoothTime = 0.1f;
    public Camera playerCamera;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float currentXVelocity;
    private float currentYVelocity;

    private Rigidbody rb;
    private PlayerInputActions playerInputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputActions = new PlayerInputActions();

        //Подписываемся на события ввода
        playerInputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += context => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += context => lookInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled += context => lookInput = Vector2.zero;
        //Включаем Action Map
        playerInputActions.Player.Enable();

        // Прячем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() //Переносим Look в Update
    {
        Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        //Преобразуем ввод в мировые координаты
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize(); //Нормализуем, чтобы избежать ускорения по диагонали

        //Применяем силу к Rigidbody
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void Look()
    {
        // Получаем ввод с мыши (для обычного вращения)
        float mouseX = lookInput.x * lookSensitivityX * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivityY * Time.deltaTime;

        // Вращаем игрока по горизонтали с плавностью
        yRotation += mouseX;
        yRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, yRotation, ref currentYVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

        // Вращаем камеру по вертикали с плавностью
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ограничиваем угол обзора, чтобы не перевернуться
        float smoothXRotation = Mathf.SmoothDampAngle(playerCamera.transform.localEulerAngles.x, xRotation, ref currentXVelocity, smoothTime);

        // Применяем вращение к камере
        playerCamera.transform.localRotation = Quaternion.Euler(smoothXRotation, 0, 0);
    }
}