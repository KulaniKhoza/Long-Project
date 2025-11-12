using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text UI")]
    public TextMeshProUGUI tutorialText;
    public float instructionDisplayTime = 5f;

    [Header("Tutorial Instructions")]
    [TextArea(3, 5)]
    public string[] tutorialInstructions = {
        "Click on an empty tile to select it",
        "Click the plow button to plow the selected tile",
        "Click on the plowed tile to select it",
        "Click the plant button to open plant selection",
        "Select a plant type to plant",
        "Click on the plant to water it"
    };

    [Header("Debug")]
    public bool enableDebug = true;

    private int currentInstructionIndex = 0;
    private Coroutine textChangeCoroutine;
    private bool isTutorialActive = false;

    private void Start()
    {
        // Hide text initially
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }

        // Start tutorial after a brief delay
        Invoke("StartTutorial", 2f);
    }

    private void Update()
    {
        // Allow clicking to advance to next instruction
        if (isTutorialActive && tutorialText != null && tutorialText.gameObject.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                SkipToNextInstruction();
            }
        }
    }

    public void StartTutorial()
    {
        if (tutorialText == null)
        {
            Debug.LogWarning("No tutorial text UI assigned!");
            return;
        }

        if (tutorialInstructions.Length == 0)
        {
            Debug.LogWarning("No tutorial instructions defined!");
            return;
        }

        tutorialText.gameObject.SetActive(true);
        currentInstructionIndex = 0;
        isTutorialActive = true;

        // Show first instruction
        ShowInstruction(currentInstructionIndex);

        DebugLog("Tutorial started with simple text display");
    }

    private void ShowInstruction(int index)
    {
        if (index < 0 || index >= tutorialInstructions.Length)
        {
            DebugLog("All tutorial instructions completed");
            EndTutorial();
            return;
        }

        tutorialText.text = tutorialInstructions[index];
        DebugLog($"Showing instruction {index}: {tutorialInstructions[index]}");

        // Schedule next instruction (auto-advance after time)
        if (textChangeCoroutine != null)
            StopCoroutine(textChangeCoroutine);

        textChangeCoroutine = StartCoroutine(ShowNextInstructionAfterDelay());
    }

    private IEnumerator ShowNextInstructionAfterDelay()
    {
        yield return new WaitForSeconds(instructionDisplayTime);

        currentInstructionIndex++;
        ShowInstruction(currentInstructionIndex);
    }

    private void SkipToNextInstruction()
    {
        if (!isTutorialActive) return;

        DebugLog("Player clicked - advancing to next instruction");

        if (textChangeCoroutine != null)
            StopCoroutine(textChangeCoroutine);

        currentInstructionIndex++;
        ShowInstruction(currentInstructionIndex);
    }

    public void SkipTutorial()
    {
        DebugLog("Tutorial skipped by player");
        EndTutorial();
    }

    private void EndTutorial()
    {
        isTutorialActive = false;

        if (textChangeCoroutine != null)
            StopCoroutine(textChangeCoroutine);

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }

        DebugLog("Tutorial completed!");

        // Notify other systems that tutorial is complete
        FarmGrid farmGrid = FindFirstObjectByType<FarmGrid>();
        if (farmGrid != null)
        {
            farmGrid.OnTutorialComplete();
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnTutorialComplete();
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebug)
        {
            Debug.Log($"[SIMPLE TUTORIAL] {message}");
        }
    }

    // Public method to check if tutorial is active
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
}