using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int maxMoves = 15;
    public int currentMoves;
    public bool isGameOver;
    public float moveDuration = 1.0f;
    public float jumpPower = 0.3f;

    [Header("Penguin Visuals")]
    [Range(0.1f, 2f)] public float penguinScale = 0.4f;
    [Range(0f, 1f)] public float verticalOffset = 0.3f;
    [Range(0.1f, 2f)] public float animationSpeed = 0.5f;

    [Header("UI References")]
    public Text moveText;
    public GameObject winPanel;
    public GameObject losePanel;

    // Player State
    private GameObject playerObj;
    private Vector2Int playerGridPos;
    private bool isPlayerMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (isGameOver || isPlayerMoving || playerObj == null) return;

        int x = 0;
        int y = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) y = 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) y = -1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) x = -1;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) x = 1;
        
        if (x != 0 || y != 0)
        {
            RequestMove(new Vector2Int(x, y));
        }
    }

    // Called by LevelManager
    public void InitializeLevel(GameObject newPlayerObj, Vector2Int startPos)
    {
        playerObj = newPlayerObj;
        currentMoves = maxMoves;
        playerGridPos = startPos;
        isGameOver = false;
        isPlayerMoving = false;
        
        // Setup Animator
        Animator anim = playerObj.GetComponent<Animator>();
        if (anim == null) anim = playerObj.AddComponent<Animator>();
        
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Animations/PenguinController");
        if (controller != null) anim.runtimeAnimatorController = controller;
        else Debug.LogWarning("Could not load Animations/PenguinController");
        
        // Kill existing tweens on the player to prevent conflicts
        if (playerObj != null) playerObj.transform.DOKill();

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        
        // Apply Visual Settings
        if (playerObj != null)
        {
            playerObj.transform.localScale = Vector3.one * penguinScale;
            Animator a = playerObj.GetComponent<Animator>();
            if (a != null) a.speed = animationSpeed;
        }

        UpdateUI();
    }

    private void LateUpdate()
    {
        // Allow runtime tweaking
        if (playerObj != null && !isGameOver)
        {
             // Optional: visual tweak in realtime
             if (playerObj.transform.localScale.x != penguinScale) 
                 playerObj.transform.localScale = Vector3.one * penguinScale;
             
             Animator a = playerObj.GetComponent<Animator>();
             if (a != null && a.speed != animationSpeed) a.speed = animationSpeed;
        }
    }

    public void ClearLevel()
    {
        if (playerObj != null)
        {
            playerObj.transform.DOKill();
            if (Application.isPlaying) Destroy(playerObj);
            else DestroyImmediate(playerObj);
            playerObj = null;
        }
    }

    public void RequestMove(Vector2Int direction)
    {
        if (isGameOver || playerObj == null || isPlayerMoving) return;

        Vector2Int nextPos = playerGridPos + direction;
        TileData nextTile = GridManager.Instance.GetTileAt(nextPos);

        if (nextTile == null || nextTile.type == TileType.Obstacle) return;

        // Cost Check
        int cost = nextTile.GetMoveCost();
        if (currentMoves >= cost)
        {
            currentMoves -= cost;
            UpdateUI();
            
            // Calculate Path (Handle Ice)
            List<TileData> path = new List<TileData>();
            path.Add(nextTile);
            
            TileData currentTile = nextTile;
            // Slide Loop
            while (currentTile.type == TileType.Ice)
            {
                Vector2Int slideNextPos = new Vector2Int(currentTile.x + direction.x, currentTile.y + direction.y);
                TileData slideNextTile = GridManager.Instance.GetTileAt(slideNextPos);

                if (slideNextTile != null && slideNextTile.type != TileType.Obstacle)
                {
                    path.Add(slideNextTile);
                    currentTile = slideNextTile;
                    if (currentTile.type == TileType.Home) break;
                }
                else
                {
                    break;
                }
            }

            StartCoroutine(MoveSequence(path));
        }
        else
        {
             // Out of moves
        }
    }

    IEnumerator MoveSequence(List<TileData> path)
    {
        isPlayerMoving = true;
        foreach (TileData t in path)
        {
            if (t.visualObject == null) continue;

            Vector3 endPos = t.visualObject.transform.position;
            // Maintain Z depth
            endPos.z = playerObj.transform.position.z;
            endPos.y += verticalOffset; // Visual center offset

            Animator anim = playerObj.GetComponent<Animator>();
            TileData startTile = GridManager.Instance.GetTileAt(playerGridPos);
            bool isSliding = (startTile != null && startTile.type == TileType.Ice);

            if (anim != null)
            {
                anim.SetTrigger(isSliding ? "Slide" : "Jump");
            }

            if (isSliding)
            {
                yield return playerObj.transform.DOMove(endPos, moveDuration).SetEase(Ease.Linear).WaitForCompletion();
            }
            else
            {
                yield return playerObj.transform.DOJump(endPos, jumpPower, 1, moveDuration).WaitForCompletion();
            }
            
            playerGridPos = new Vector2Int(t.x, t.y);
            CheckGameState(t);
            if (isGameOver) break;
        }
        isPlayerMoving = false;
        
        // Check Loss condition after move sequence ends (if not won)
        if (!isGameOver && currentMoves <= 0)
        {
           LoseGame();
        }
    }

    // MoveRoutine deleted (replaced by DOTween)

    void CheckGameState(TileData t)
    {
        if (t.type == TileType.Home)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if(isGameOver) return;
        isGameOver = true;
        Debug.Log("VICTORY!");
        if (winPanel) winPanel.SetActive(true);
        
        StartCoroutine(NextLevelRoutine());
    }
    
    IEnumerator NextLevelRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    void LoseGame()
    {
        if(isGameOver) return;
        isGameOver = true;
        Debug.Log("GAME OVER!");
        if (losePanel) losePanel.SetActive(true);
    }

    void UpdateUI()
    {
        if (moveText) moveText.text = $"Moves: {currentMoves}";
    }
}
