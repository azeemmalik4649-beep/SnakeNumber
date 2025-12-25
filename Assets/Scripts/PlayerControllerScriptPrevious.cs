// PlayerControllerScript.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PlayerControllerScriptPrevious : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sideSpeed = 10f;
    public float smoothTime = 0.1f;

    [Header("Number Settings")]
    public Text headNumberDisplay;
    public GameObject numberPrefab;
    public Transform chainContainer;

    [Header("Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.3f;

    // Player state
    public int playerNumber = 1;
    private float targetX = 0f;
    private float velocityX = 0f;
    private bool levelComplete = false;

    // Chain management
    public List<GameObject> numberChain = new List<GameObject>();
    public List<Vector3> positionHistory = new List<Vector3>();
    private float recordTimer = 0f;
    private float recordInterval = 0.05f;
    private int maxHistory = 5000;


    public static PlayerControllerScriptPrevious instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        // Initialize player
        headNumberDisplay.text = playerNumber.ToString();
        numberChain.Add(gameObject); // Player is first in chain
    }

    void Update()
    {
        if (levelComplete) return;

        HandleMovement();
        MoveForward();
        UpdatePositionHistory();
        UpdateChainFollow();
    }

    void HandleMovement()
    {
        if (Input.GetMouseButton(0))
        {
            float mousePercent = Input.mousePosition.x / Screen.width;
            float normalizedX = (mousePercent * 2f) - 1f;
            targetX = Mathf.Clamp(normalizedX * 3f, -4f, 4f);
        }

        // Smooth side movement
        Vector3 position = transform.position;
        position.x = Mathf.SmoothDamp(position.x, targetX, ref velocityX, smoothTime, sideSpeed);
        transform.position = position;
    }

    void MoveForward()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    void UpdatePositionHistory()
    {
        recordTimer += Time.deltaTime;

        if (recordTimer >= recordInterval)
        {
            positionHistory.Insert(0, transform.position);
            recordTimer = 0f;

            // Limit history size
            if (positionHistory.Count > maxHistory)
            {
                positionHistory.RemoveAt(positionHistory.Count - 1);
            }
        }
    }

    void UpdateChainFollow()
    {
        // Update all chain members except player (index 0)
        for (int i = 1; i < numberChain.Count; i++)
        {
            if (numberChain[i] != null)
            {
                // Each member follows with delay
                int historyIndex = Mathf.Min(i * 8, positionHistory.Count - 1);
                if (historyIndex < positionHistory.Count)
                {
                    numberChain[i].transform.position = positionHistory[historyIndex];
                }
            }
        }
    }

    public void AddNumber(int value)
    {
        playerNumber += value;
        playerNumber = Mathf.Max(1, playerNumber);
        headNumberDisplay.text = playerNumber.ToString();

        UpdateChainSize();
    }

    void UpdateChainSize()
    {
        // Add missing numbers
        while (numberChain.Count < playerNumber)
        {
            AddChainMember();
        }

        // Remove extra numbers
        while (numberChain.Count > playerNumber)
        {
            RemoveLastChainMember();
        }

        UpdateAllNumbers();
    }

    void AddChainMember()
    {
        GameObject newMember = Instantiate(numberPrefab, transform.position, Quaternion.identity, chainContainer);

        // Disable pickup script to prevent interference
        MonoBehaviour[] scripts = newMember.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name.Contains("NumberPickup"))
            {
                script.enabled = false;
            }
        }

        numberChain.Add(newMember);
    }

    void RemoveLastChainMember()
    {
        if (numberChain.Count <= 1) return;

        GameObject lastMember = numberChain[numberChain.Count - 1];
        numberChain.RemoveAt(numberChain.Count - 1);

        Destroy(lastMember);
    }

    void UpdateAllNumbers()
    {
        // Update head
        headNumberDisplay.text = playerNumber.ToString();

        // Update chain members
        for (int i = 1; i < numberChain.Count; i++)
        {
            Text memberText = numberChain[i].GetComponentInChildren<Text>();
            if (memberText != null)
            {
                int displayNumber = playerNumber - i;
                memberText.text = displayNumber.ToString();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        playerNumber = Mathf.Max(1, playerNumber - damage);
        headNumberDisplay.text = playerNumber.ToString();

        // Visual feedback
        StartCoroutine(FlashDamage());

        UpdateChainSize();
    }

    IEnumerator FlashDamage()
    {
        Color originalColor = headNumberDisplay.color;
        headNumberDisplay.color = damageColor;

        yield return new WaitForSeconds(flashDuration);

        headNumberDisplay.color = originalColor;
    }

    public void FinishLevel()
    {
        levelComplete = true;
        // Stop movement
        moveSpeed = 0f;

        // Celebration effect
        StartCoroutine(Celebrate());
    }

    IEnumerator Celebrate()
    {
        Vector3 startScale = transform.localScale;
        float duration = 0.5f;

        // Grow
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.3f, t / duration);
            yield return null;
        }

        // Shrink back
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(startScale * 1.3f, startScale, t / duration);
            yield return null;
        }

        transform.localScale = startScale;
    }
}