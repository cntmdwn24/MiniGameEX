using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Game Data
    [Header("Game Data")]
    public GameData gameData;
    #endregion

    [SerializeField] private AudioSource TouchSound;
    [SerializeField] private AudioSource ssss;

    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject selectGamePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Sprite defaultStageImage;
    [SerializeField] private GameObject StuffPanel;

    [Header("Selected Stage UI")]
    [SerializeField] private TMP_Text selectedStageText;
    [SerializeField] private Image selectedStageImage;

    [Header("Message Prefabs")]
    [SerializeField] private GameObject successMessagePrefab;
    [SerializeField] private GameObject failureMessagePrefab;
    [SerializeField] private GameObject lockedStageMessagePrefab;
    [SerializeField] private GameObject stageSelectedMessagePrefab;

    [Header("Message Parent")]
    [SerializeField] private Transform messageParent;
    #endregion

    #region Mini Game Settings
    [Header("Mini Game Settings")]
    [SerializeField] private GameObject[] miniGamePrefabs;
    [SerializeField] private Transform gameSpawnPoint;
    [SerializeField] private Sprite[] stageImages;
    [SerializeField] private string[] miniGameNames;

    private const int STAGE_COST = 10;
    private int selectedStageIndex = -1;
    private GameObject currentMiniGameInstance;
    private bool isTouchSoundEnabled = true;
    #endregion

    #region Reward System
    [Header("Reward System")]
    [SerializeField] private Reward[] rewards;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 게임 오브젝트를 파괴하지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 존재하는 인스턴스가 있다면 이 객체를 파괴
        }
    }

    private void Start()
    {
        int numberOfStages = miniGamePrefabs.Length;
        int numberOfRewards = rewards.Length;
        gameData = new GameData(numberOfStages, numberOfRewards);

        LoadGameDataFromJson(); // 게임 시작 시 데이터 로드

        if (selectedStageIndex == -1) // 첫 실행 시 선택된 스테이지를 초기화
        {
            selectedStageIndex = 1;
            gameData.games[selectedStageIndex] = true;
        }

        UpdateUI();
    }
    #endregion

    public void ToggleTouchSound()
    {
        isTouchSoundEnabled = !isTouchSoundEnabled;
        Debug.Log("Touch Sound is now " + (isTouchSoundEnabled ? "Enabled" : "Disabled"));
    }

    #region UI Handling
    public void GameStart()
    {
        menuPanel.SetActive(true);
        StuffPanel.SetActive(true);
    }

    public void SelectGame()
    {
        selectGamePanel.SetActive(true);
    }

    public void CloseSelect()
    {
        selectGamePanel.SetActive(false);
    }

    private void CloseSelectGamePanel()
    {
        selectGamePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void ShowMessage(GameObject messagePrefab)
    {
        if (messagePrefab != null && messageParent != null)
        {
            // 메시지 프리팹을 인스턴스화하여 메시지 부모 아래에 생성합니다.
            GameObject messageInstance = Instantiate(messagePrefab, messageParent);
    
            // 메시지 인스턴스를 활성화합니다.
            messageInstance.SetActive(true);
            ssss.Play();
        }
    }

    private void DeleteAllMessages()
    {
        if (messageParent != null)
        {
            // 메시지 부모의 모든 자식 오브젝트를 순회하며 삭제합니다.
            foreach (Transform child in messageParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void UpdateUI()
    {
        if (keyText != null) keyText.text = $"Keys: {gameData.key}";
        if (coinText != null) coinText.text = $"Gold: {gameData.coin}";
        UpdateSelectedStageDisplay();
        UpdateRewardsUI();

        // 데이터 저장
        SaveGameDataToJson();
    }

    private void UpdateSelectedStageDisplay()
    {
        if (selectedStageText != null)
        {
            if (IsValidStageIndex(selectedStageIndex))
            {
                selectedStageText.text = $"{miniGameNames[selectedStageIndex]} 선택됨";
                selectedStageImage.sprite = stageImages != null && stageImages.Length > selectedStageIndex
                    ? stageImages[selectedStageIndex]
                    : defaultStageImage;
            }
            else
            {
                selectedStageText.text = "선택된 미니게임이 없습니다";
                selectedStageImage.sprite = defaultStageImage;
            }
        }
    }

    private void UpdateRewardsUI()
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            if (rewards[i] != null)
            {
                Reward reward = rewards[i];

                if (reward.button != null)
                {
                    reward.button.interactable = !reward.GetReward && gameData.key >= reward.needKey;
                }

                if (reward.point != null)
                {
                    reward.point.color = !reward.GetReward && gameData.key >= reward.needKey ? Color.white : Color.red;
                }
            }
        }
    }
    #endregion

    #region Stage Management
    public void OnStageButtonClick(int stageIndex)
    {
        if (IsValidStageIndex(stageIndex) && gameData.games[stageIndex])
        {
            selectedStageIndex = stageIndex;
            UpdateSelectedStageDisplay();
            ShowMessage(stageSelectedMessagePrefab);
            CloseSelectGamePanel();
        }
        else
        {
            ShowMessage(lockedStageMessagePrefab);
        }
    }
    

    public void UnlockStage(int stageIndex)
    {
        if (IsValidStageIndex(stageIndex))
        {
            if (gameData.key >= STAGE_COST && !gameData.games[stageIndex])
            {
                gameData.key -= STAGE_COST;
                gameData.games[stageIndex] = true;
                UpdateUI();
                ShowMessage(successMessagePrefab);
            }
            else
            {
                ShowMessage(failureMessagePrefab);
            }
        }
        else
        {
            ShowMessage(failureMessagePrefab);
        }
    }

    private bool IsValidStageIndex(int stageIndex)
    {
        return stageIndex >= 0 && stageIndex < gameData.games.Length;
    }

    private bool IsValidRewardIndex(int rewardIndex)
    {
        return rewardIndex >= 0 && rewardIndex < rewards.Length;
    }
    #endregion

    #region Game Flow
    public void PlaySelectedStage()
    {
        DeleteAllMessages();
        if (IsValidStageIndex(selectedStageIndex) && gameData.games[selectedStageIndex])
        {
            if (currentMiniGameInstance != null)
            {
                Destroy(currentMiniGameInstance);
            }
            canvas.SetActive(false);
            currentMiniGameInstance = Instantiate(miniGamePrefabs[selectedStageIndex], gameSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            ShowMessage(lockedStageMessagePrefab);
        }
    }

    public void EndMiniGame(int score)
    {
        canvas.SetActive(true);

        // 점수를 열쇠와 코인으로 환산
        int keysEarned = score / 10;
        int coinsEarned = score / 5;
        
        gameData.key += keysEarned;
        gameData.coin += coinsEarned;
        gameData.gameScores[selectedStageIndex] = score;

        Debug.Log($"EndMiniGame called with score: {score}");
        Debug.Log($"Keys earned: {keysEarned}, Coins earned: {coinsEarned}");
        Debug.Log($"Total keys: {gameData.key}, Total coins: {gameData.coin}");

        UpdateUI();
    }

    public void ClaimReward(int rewardIndex)
    {
        if (IsValidRewardIndex(rewardIndex))
        {
            Reward reward = rewards[rewardIndex];
            if (!reward.GetReward && gameData.key >= reward.needKey)
            {
                // 열쇠를 소모하지 않음
                reward.GetReward = true;
                gameData.coin += reward.needKey;
                UpdateRewardsUI();
                ShowMessage(successMessagePrefab);
                SaveGameDataToJson();
                UpdateUI();
            }
            else
            {
                ShowMessage(failureMessagePrefab);
            }
        }
        else
        {
            ShowMessage(failureMessagePrefab);
        }
    }
    #endregion

    #region Data Management
    [ContextMenu("To Json Data")]
    public void SaveGameDataToJson()
    {
        if (gameData.rewards == null || gameData.rewards.Length != rewards.Length)
        {
            gameData.rewards = new Reward[rewards.Length];
        }

        for (int i = 0; i < rewards.Length; i++)
        {
            if (rewards[i] != null)
            {
                gameData.rewards[i] = new Reward
                {
                    needKey = rewards[i].needKey,
                    GetReward = rewards[i].GetReward
                };
            }
        }

        string jsonData = JsonUtility.ToJson(gameData, true);
        string path = Path.Combine(Application.persistentDataPath, "GameData.json");
        File.WriteAllText(path, jsonData);

        Debug.Log($"Game data saved: {jsonData}");
    }

    [ContextMenu("From Json Data")]
    public void LoadGameDataFromJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "GameData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(jsonData);

            Debug.Log($"Game data loaded: {jsonData}");

            // 게임 데이터가 불러와졌을 때 UI 업데이트
            UpdateUI();
        }
        else
        {
            Debug.Log("No saved game data found.");
        }
    }

    [ContextMenu("Reset Game Data")]
    public void ResetGameData()
    {
        int numberOfStages = miniGamePrefabs.Length;
        int numberOfRewards = rewards.Length;
        gameData = new GameData(numberOfStages, numberOfRewards);

        selectedStageIndex = 1;
        gameData.games[selectedStageIndex] = true;

        for (int i = 0; i < rewards.Length; i++)
        {
            gameData.rewards[i] = new Reward
            {
                needKey = rewards[i].needKey,
                GetReward = false
            };
        }

        gameData.key = 0;
        gameData.coin = 0;

        UpdateUI();
        SaveGameDataToJson();
    }
    
    public void PlayTouchSound()
    {
        if (isTouchSoundEnabled && TouchSound != null)
        {
            TouchSound.Play();
        }
    }
    #endregion

    #region Reward Class
    [System.Serializable]
    public class Reward
    {
        public int needKey;
        public bool GetReward;
        public Button button;
        public Image point;
    }
    #endregion

    #region Game Data Class
    [System.Serializable]
    public class GameData
    {
        public bool[] games;
        public int[] gameScores;
        public Reward[] rewards;
        public int key;
        public int coin;

        public GameData(int numberOfStages, int numberOfRewards)
        {
            games = new bool[numberOfStages];
            gameScores = new int[numberOfStages];
            rewards = new Reward[numberOfRewards];
        }
    }
    #endregion
}