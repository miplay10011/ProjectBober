using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WalkController : MonoBehaviour
{
    [Header("Зависимости")]
    [Tooltip("Ссылка на PlayerController, управляющий движением персонажа")]
    public PlayerController playerController;

    [Tooltip("Аудиоклип для шагов по умолчанию")]
    public AudioClip defaultSound;

    [Header("Аудиоклипы для разных поверхностей (List)")]
    public List<AudioClip> stoneSounds;
    public List<AudioClip> woodSounds;
    public List<AudioClip> grassSounds;
    public List<AudioClip> metalSounds;
    public List<AudioClip> concreteSounds;

    [Header("Настройки ходьбы")]
    [Tooltip("Интервал между шагами в секундах (обычный шаг)")]
    public float stepInterval = 0.5f;

    [Tooltip("Интервал между шагами в секундах (бег)")]
    public float runStepInterval = 0.3f;

    [Tooltip("Максимальная дистанция рейкаста вниз для определения поверхности")]
    public float raycastDistance = 1.0f;

    [Tooltip("Смещение рейкаста от центра (чтобы не застревал в земле)")]
    public float raycastOffset = 0.1f;

    [Tooltip("Слой, который будет проверять рейкаст (пол, землю и т.д.)")]
    public LayerMask groundLayer;

    [Header("Настройки звука")]
    [Tooltip("Изменение высоты тона в случайном диапазоне +/- pitchOffset")]
    public float pitchOffset = 0.05f;

    [Tooltip("Громкость звука шагов")]
    [Range(0f, 1f)]
    public float volume = 0.5f; // Начальная громкость

    [Tooltip("AudioSource для воспроизведения звуков шагов.  Перетащите сюда AudioSource из Inspector.")]
    public AudioSource audioSource; // Сделали AudioSource публичным

    private float timeSinceLastStep = 0f;
    private bool isWalking = false;
    private float originalPitch;

    private Rigidbody rb;
    private PlayerInputActions playerInputActions; // Добавлено для работы с Input System

    void Awake()
    {
        playerInputActions = new PlayerInputActions(); // Инициализация Input Actions
        playerInputActions.Player.Enable(); // Включаем Action Map
    }

    void Start()
    {
        // Теперь AudioSource должен быть назначен в Inspector.
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned! Please assign an AudioSource in the Inspector.");
            return; // Прекращаем выполнение, если AudioSource не назначен
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("No Rigidbody found on this GameObject or its parent!");
                return; // Прекращаем выполнение, если Rigidbody не найден
            }
        }

        originalPitch = audioSource.pitch;
    }

    void Update()
    {
        // Проверяем, назначены ли audioSource и rb
        if (audioSource == null || rb == null) return;

        // Определяем, есть ли ввод с клавиш WASD
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
        isWalking = moveInput.magnitude > 0.01f;

        if (isWalking)
        {
            timeSinceLastStep += Time.deltaTime;

            // Определяем интервал между шагами
            float currentStepInterval = stepInterval;

            if (timeSinceLastStep > currentStepInterval)
            {
                PlayFootstepSound();
                timeSinceLastStep = 0f;
            }
        }
    }

    void PlayFootstepSound()
    {
        // Проверяем, назначен ли audioSource
        if (audioSource == null) return;

        // Выполняем рейкаст для определения материала поверхности
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * raycastOffset;

        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            // Получаем компонент AudioMaterial, если он есть
            AudioMaterial audioMaterial = hit.collider.GetComponent<AudioMaterial>();

            if (audioMaterial != null)
            {
                // Получаем материал поверхности
                string material = audioMaterial.material.ToLower(); // Приводим к нижнему регистру

                // Выбираем случайный звук из соответствующего списка
                AudioClip clipToPlay = null;

                switch (material)
                {
                    case "stone":
                        clipToPlay = GetRandomClip(stoneSounds);
                        break;
                    case "wood":
                        clipToPlay = GetRandomClip(woodSounds);
                        break;
                    case "grass":
                        clipToPlay = GetRandomClip(grassSounds);
                        break;
                    case "metal":
                        clipToPlay = GetRandomClip(metalSounds);
                        break;
                    case "concrete":
                        clipToPlay = GetRandomClip(concreteSounds);
                        break;
                    default:
                        clipToPlay = defaultSound;
                        break;
                }

                // Воспроизводим звук
                PlaySound(clipToPlay);
            }
            else
            {
                // Если AudioMaterial отсутствует, воспроизводим звук по умолчанию
                PlaySound(defaultSound);
            }
        }
        else
        {
            // Если рейкаст не попал ни в какую поверхность, воспроизводим звук по умолчанию
            PlaySound(defaultSound);
        }
    }

    // Вспомогательный метод для получения случайного аудиоклипа из списка
    private AudioClip GetRandomClip(List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0)
        {
            return clips[Random.Range(0, clips.Count)];
        }
        return null;
    }

    // Метод для воспроизведения звука
    private void PlaySound(AudioClip clip)
    {
        // Проверяем, назначен ли audioSource
        if (audioSource == null) return;

        if (clip != null)
        {
            // Случайное изменение высоты тона
            audioSource.pitch = originalPitch + Random.Range(-pitchOffset, pitchOffset);
            audioSource.volume = volume; // Устанавливаем громкость перед воспроизведением

            audioSource.PlayOneShot(clip);

            // Возвращаем высоту тона к оригинальному значению после воспроизведения (опционально)
            audioSource.pitch = originalPitch;
        }
    }
}