using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerControllerScript : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 5f;
    public float horizontalSpeed = 10f;
    public float smoothTime = 0.1f;

    [Header("Number Settings")]
    public Text headNumberText;
    public GameObject numberPrefab;
    public Transform numbersContainer;

    [Header("Collision Effects")]
    public Color collisionFlashColor = Color.red;
    public float collisionFlashDuration = 0.3f;

    // Public variables
    public int currentNumber = 1;
    public float targetX = 0f;
    public float velocityX = 0f;
    public List<GameObject> chainMembers = new List<GameObject>();
    public List<Vector3> positionHistory = new List<Vector3>();
    public int historyMaxSize = 500;
    public float positionRecordInterval = 0.05f;
    public float timeSinceLastRecord = 0f;
    public static PlayerControllerScript instance;

    // Private variables
    private float baseForwardSpeed;
    private float currentSpeedMultiplier = 1f;
    private Coroutine speedReductionCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        headNumberText.text = currentNumber.ToString();
        chainMembers.Add(gameObject);
        baseForwardSpeed = forwardSpeed;
    }

    void Update()
    {
        HandleInput();
        MoveForward();

        timeSinceLastRecord += Time.deltaTime;
        if (timeSinceLastRecord >= positionRecordInterval)
        {
            RecordPosition();
            timeSinceLastRecord = 0f;
        }

        UpdateChainPositions();
    }

    void RecordPosition()
    {
        positionHistory.Insert(0, transform.position);
        if (positionHistory.Count > historyMaxSize)
            positionHistory.RemoveAt(positionHistory.Count - 1);
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.mousePosition.x;
            float normalizedX = (mouseX / Screen.width) * 2 - 1;
            targetX = Mathf.Clamp(normalizedX * 3f, -4f, 4f);
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.SmoothDamp(pos.x, targetX, ref velocityX, smoothTime, horizontalSpeed);
        transform.position = pos;
    }

    void MoveForward()
    {
        transform.Translate(Vector3.forward * baseForwardSpeed * currentSpeedMultiplier * Time.deltaTime);
    }

    void UpdateChainPositions()
    {
        for (int i = 1; i < chainMembers.Count; i++)
        {
            if (chainMembers[i] == null) continue;

            int historyIndex = Mathf.Min(i * 8, positionHistory.Count - 1);
            if (historyIndex >= 0 && historyIndex < positionHistory.Count)
                chainMembers[i].transform.position = positionHistory[historyIndex];
        }
    }

    public void CollectNumber(int val)
    {
        currentNumber = Mathf.Max(1, currentNumber + val);
        headNumberText.text = currentNumber.ToString();

        UpdateChainSize();
        UpdateChainNumbers();
    }

    void UpdateChainSize()
    {
        int requiredCount = currentNumber;
        int currentCount = chainMembers.Count;

        // Add missing members
        for (int j = 0; j < requiredCount - currentCount; j++)
            CreateChainMember();

        // Remove extra members
        while (chainMembers.Count > requiredCount)
        {
            Destroy(chainMembers[chainMembers.Count - 1]);
            chainMembers.RemoveAt(chainMembers.Count - 1);
        }
    }

    void CreateChainMember()
    {
        if (numberPrefab == null) return;

        GameObject newMember = Instantiate(numberPrefab, transform.position, Quaternion.identity, numbersContainer);

        // Disable all pickup scripts
        foreach (MonoBehaviour script in newMember.GetComponents<MonoBehaviour>())
        {
            if (script != null && script.GetType().Name.Contains("NumberPickup"))
                script.enabled = false;
        }

        chainMembers.Add(newMember);
        UpdateNewChainMember(newMember, chainMembers.Count - 1);
    }

    void UpdateNewChainMember(GameObject member, int memberIndex)
    {
        if (memberIndex <= 0) return;

        Text numberText = member.GetComponentInChildren<Text>(true);
        if (numberText != null)
        {
            numberText.text = (currentNumber - memberIndex).ToString();
            ForceTextUpdate(numberText);
        }

        StartCoroutine(UpdateChainMemberTextDelayed(member, memberIndex));
    }

    IEnumerator UpdateChainMemberTextDelayed(GameObject member, int memberIndex)
    {
        yield return new WaitForEndOfFrame();
        if (member == null || memberIndex <= 0) yield break;

        Text numberText = member.GetComponentInChildren<Text>(true);
        if (numberText != null)
        {
            string expectedValue = (currentNumber - memberIndex).ToString();
            if (numberText.text != expectedValue)
            {
                numberText.text = expectedValue;
                ForceTextUpdate(numberText);
            }
        }
    }

    void UpdateChainNumbers()
    {
        if (headNumberText == null) return;

        headNumberText.text = currentNumber.ToString();

        for (int i = 1; i < chainMembers.Count; i++)
            UpdateChainMemberNumber(chainMembers[i], i);

        Canvas.ForceUpdateCanvases();
    }

    void UpdateChainMemberNumber(GameObject member, int index)
    {
        if (member == null) return;

        // Disable pickup script if enabled
        NumberPickUpScript pickupScript = member.GetComponent<NumberPickUpScript>();
        if (pickupScript != null && pickupScript.enabled)
            pickupScript.enabled = false;

        Text numberText = member.GetComponentInChildren<Text>(true);
        if (numberText == null) return;

        numberText.text = (currentNumber - index).ToString();
        ForceTextUpdate(numberText);
    }

    void ForceTextUpdate(Text text)
    {
        if (text is Graphic graphic)
            graphic.SetAllDirty();
    }

    public void RemoveNumbers(int count)
    {
        if (count <= 0) return;

        currentNumber = Mathf.Max(1, currentNumber - count);
        headNumberText.text = currentNumber.ToString();

        while (chainMembers.Count > currentNumber)
        {
            Destroy(chainMembers[chainMembers.Count - 1]);
            chainMembers.RemoveAt(chainMembers.Count - 1);
        }

        UpdateChainNumbers();
        StartCoroutine(FlashHeadColor(collisionFlashColor));
    }

    public void ReduceSpeed(float multiplier, float duration)
    {
        if (speedReductionCoroutine != null)
            StopCoroutine(speedReductionCoroutine);

        speedReductionCoroutine = StartCoroutine(SpeedReductionCoroutine(multiplier, duration));
    }

    IEnumerator SpeedReductionCoroutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeedMultiplier = 1f;
        speedReductionCoroutine = null;
    }

    IEnumerator FlashHeadColor(Color flashColor)
    {
        if (headNumberText == null) yield break;

        Color originalColor = headNumberText.color;
        float timer = 0f;

        while (timer < collisionFlashDuration)
        {
            timer += Time.deltaTime;
            headNumberText.color = Color.Lerp(originalColor, flashColor, timer / collisionFlashDuration);
            yield return null;
        }

        timer = 0f;
        while (timer < collisionFlashDuration)
        {
            timer += Time.deltaTime;
            headNumberText.color = Color.Lerp(flashColor, originalColor, timer / collisionFlashDuration);
            yield return null;
        }

        headNumberText.color = originalColor;
    }

    public void OnLevelComplete()
    {
        currentSpeedMultiplier = 0f;
    }


    public void LoadNextLevel()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

}