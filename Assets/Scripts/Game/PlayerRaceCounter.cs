using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject))]
public class PlayerRaceCounter : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private string lapTextObjectName = "LapCounterText";

    [Header("Tags / Settings")]
    [SerializeField] private string finishLineTag = "FinishLine";

    [Header("Visuals")]
    [SerializeField] private Color activeCheckpointColor = Color.green;
    [SerializeField] private Color inactiveCheckpointColor = Color.red;

    // сетевой счетчик, пишет только сервер
    private NetworkVariable<int> lapCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // серверный список пройденных чекпоинтов для этого игрока
    private NetworkList<bool> triggeredCheckpoints = new NetworkList<bool>();

    // локальные ссылки на объекты чекпоинтов (все клиенты)
    private Dictionary<int, GameObject> checkpointById = new Dictionary<int, GameObject>();
    private Dictionary<int, Renderer> checkpointRendererById = new Dictionary<int, Renderer>();

    private TMP_Text lapText;

    // Серверная защита от многократного финиша без прохода чекпоинтов
    private bool anyCheckpointSinceReset = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Подписка на изменение счетчика
        lapCount.OnValueChanged += OnLapCountChanged;

        // Попробуем найти UI и чекпоинты (на случай если сцена уже грузнута)
        FindLapTextAndCheckpoints();

        // Если этот объект уже находится на сервере (host или dedicated server на той же сцене),
        // нужно инициализировать список чекпоинтов на сервере (если они уже найдены).
        if (IsServer)
        {
            InitializeTriggeredCheckpointsOnServer();
        }

        // Регистрация в RaceManager (если есть)
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.RegisterPlayer(this);
        }

        // Обновим UI локально
        if (IsOwner)
        {
            UpdateLapDisplay(lapCount.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        lapCount.OnValueChanged -= OnLapCountChanged;

        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.UnregisterPlayer(this);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Вызывается у всех клиентов и сервера — повторно ищем UI и чекпоинты
        FindLapTextAndCheckpoints();

        // Если сервер — (повтор) инициализируем список в соответствии с найденными чекпоинтами
        if (IsServer)
        {
            InitializeTriggeredCheckpointsOnServer();
        }
    }

    private void FindLapTextAndCheckpoints()
    {
        // Найти UI (может появиться после загрузки трассы)
        var textObj = GameObject.Find(lapTextObjectName);
        lapText = textObj ? textObj.GetComponent<TMP_Text>() : null;
        if (lapText == null)
        {
            Debug.Log($"[Player {OwnerClientId}] lapText '{lapTextObjectName}' not found (yet).");
        }
        else
        {
            UpdateLapDisplay(lapCount.Value);
        }

        // Найти все Checkpoint компоненты и собрать по id (все клиенты)
        checkpointById.Clear();
        checkpointRendererById.Clear();

        var all = FindObjectsOfType<Checkpoint>();
        foreach (var cp in all)
        {
            int id = cp.checkpointId;
            checkpointById[id] = cp.gameObject;
            var rend = cp.GetComponent<Renderer>();
            if (rend != null)
            {
                checkpointRendererById[id] = rend;
                // установка начального цвета — чтобы новые клиенты видели корректно
                rend.material.color = inactiveCheckpointColor;
            }
        }

        Debug.Log($"[Player {OwnerClientId}] Found {checkpointById.Count} checkpoints in scene.");
    }

    private void InitializeTriggeredCheckpointsOnServer()
    {
        if (!IsServer) return;

        // Определим требуемый размер: max(id)+1 или 0 если чекпоинтов нет
        int maxId = -1;
        foreach (var kv in checkpointById)
        {
            if (kv.Key > maxId) maxId = kv.Key;
        }
        int size = Mathf.Max(0, maxId + 1);

        // Если уже инициализирован и нужный размер совпадает — ничего не делаем
        if (triggeredCheckpoints.Count == size)
        {
            // сброс логики состояния между сценами
            for (int i = 0; i < triggeredCheckpoints.Count; i++) triggeredCheckpoints[i] = false;
            anyCheckpointSinceReset = false;
            return;
        }

        triggeredCheckpoints.Clear();
        for (int i = 0; i < size; i++) triggeredCheckpoints.Add(false);

        anyCheckpointSinceReset = false;

        Debug.Log($"[Server] Initialized triggeredCheckpoints size={size} for player {OwnerClientId}");
    }

private void OnLapCountChanged(int oldVal, int newVal)
{
    if (!IsOwner) return;
    UpdateLapDisplay(newVal);
}

private void UpdateLapDisplay(int laps)
{
    if (!IsOwner) return;

    if (lapText != null)
    {
        lapText.text = $"Laps: {laps}";
    }
}

    // Обработка 2D триггеров (владелец)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        // Финишная линия
        if (collision.CompareTag(finishLineTag))
        {
            // попросим сервер проверить круг
            RequestLapCheckServerRpc();
            return;
        }

        // Если объект имеет компонент Checkpoint
        var cp = collision.GetComponent<Checkpoint>();
        if (cp != null)
        {
            int id = cp.checkpointId;

            // Локально изменим визуал для мгновенного фидбека
            if (checkpointRendererById.TryGetValue(id, out var rend))
            {
                rend.material.color = activeCheckpointColor;
            }

            // Сообщим серверу о прохождении чекпоинта
            ActivateCheckpointServerRpc(id);
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void ActivateCheckpointServerRpc(int checkpointId, ServerRpcParams rpcParams = default)
    {
        if (checkpointId < 0 || checkpointId >= triggeredCheckpoints.Count)
        {
            Debug.LogWarning($"[Server] Invalid checkpointId {checkpointId} from player {rpcParams.Receive.SenderClientId}");
            return;
        }

        triggeredCheckpoints[checkpointId] = true;
        anyCheckpointSinceReset = true; // важная защита

        // Обновляем визуально всех клиентов
        ActivateCheckpointVisualClientRpc(checkpointId);

        Debug.Log($"[Server] Player {rpcParams.Receive.SenderClientId} activated checkpoint {checkpointId}");
    }

    [ClientRpc]
    private void ActivateCheckpointVisualClientRpc(int checkpointId)
    {
        if (checkpointRendererById.TryGetValue(checkpointId, out var rend))
        {
            rend.material.color = activeCheckpointColor;
        }
    }

    // Владелец просит сервер проверить круг
    [ServerRpc(RequireOwnership = true)]
    private void RequestLapCheckServerRpc(ServerRpcParams rpcParams = default)
    {
        // Защита: если нет чекпоинтов вообще — не засчитываем
        if (triggeredCheckpoints.Count == 0)
        {
            Debug.Log($"[Server] Player {rpcParams.Receive.SenderClientId} tried to finish but no checkpoints exist (Count=0). Lap NOT counted.");
            return;
        }

        // Требуем, чтобы был пройден хотя бы один чекпоинт с момента последнего сброса
        if (!anyCheckpointSinceReset)
        {
            Debug.Log($"[Server] Player {rpcParams.Receive.SenderClientId} touched finish but hasn't hit any checkpoint since last reset. Lap NOT counted.");
            return;
        }

        bool all = true;
        for (int i = 0; i < triggeredCheckpoints.Count; i++)
        {
            if (!triggeredCheckpoints[i])
            {
                all = false;
                break;
            }
        }

        if (all)
        {
            lapCount.Value++;
            // Сбрасываем состояние чекпоинтов
            for (int i = 0; i < triggeredCheckpoints.Count; i++) triggeredCheckpoints[i] = false;
            anyCheckpointSinceReset = false;

            // Обновляем визуал на клиентах
            ResetCheckpointsVisualClientRpc();

            Debug.Log($"[Server] Player {rpcParams.Receive.SenderClientId} completed lap {lapCount.Value}");

            if (RaceManager.Instance != null)
                RaceManager.Instance.OnPlayerCompletedLap(OwnerClientId, lapCount.Value);
        }
        else
        {
            int passed = 0;
            for (int i = 0; i < triggeredCheckpoints.Count; i++) if (triggeredCheckpoints[i]) passed++;
            Debug.Log($"[Server] Player {rpcParams.Receive.SenderClientId} DID NOT complete lap ({passed}/{triggeredCheckpoints.Count}).");
        }
    }

    [ClientRpc]
    private void ResetCheckpointsVisualClientRpc()
    {
        foreach (var kv in checkpointRendererById)
        {
            if (kv.Value != null)
                kv.Value.material.color = inactiveCheckpointColor;
        }
    }
}
