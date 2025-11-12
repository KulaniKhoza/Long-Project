using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCTutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [Header("Step Settings")]
        public string stepName;
        [TextArea(3, 5)]
        public string instructionText;

        [Header("NPC Position")]
        public Vector2 npcScreenPosition = new Vector2(0, 0);
        public TutorialAction requiredAction;

        [Header("Target Object (Assign in Inspector)")]
        public GameObject targetObject; // Direct object reference
        public Vector2 pointerOffset = new Vector2(0, 100f);

        [Header("Dynamic Target Settings")]
        public string dynamicTargetTag = ""; // For finding plowed tiles dynamically
        public bool useDynamicTarget = false;
    }

    public enum TutorialAction
    {
        None,
        ClickEmptyTile,
        ClickPlowButton,
        ClickPlowedTile,
        ClickPlantButton,
        SelectPlantType,
        ClickPlantToWater,
        WaterPlant,
        PlantSecondPlant,
        WaterSecondPlant,
        FirstEnemyAppears
    }

    [Header("NPC Tutorial Settings")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    public int currentStepIndex = 0;

    [Header("NPC UI References")]
    public GameObject npcPanel;
    public Image npcSprite;
    public TextMeshProUGUI instructionText;
    public RectTransform pointerArrow;
    public CanvasGroup npcCanvasGroup;

    [Header("NPC Animation")]
    public float fadeDuration = 0.5f;
    public float slideDuration = 0.8f;
    public float textTypeSpeed = 0.05f;

    [Header("NPC Visual Settings")]
    public Sprite npcIdleSprite;
    public Sprite npcTalkingSprite;
    public Vector2 npcDefaultSize = new Vector2(150, 150);

    [Header("Debug")]
    public bool enableDebug = true;

    [SerializeField] private bool isTutorialActive = false;
    [SerializeField] private bool waitingForAction = false;
    [SerializeField] private int plantsPlanted = 0;
    [SerializeField] private int plantsWatered = 0;
    private Coroutine currentTextCoroutine;
    private GraphicRaycaster npcRaycaster;

    // Reference to game systems
    private FarmGrid farmGrid;
    private GameManager gameManager;

    // Track the plowed tile position for the tutorial
    private Vector3 plowedTilePosition;
    private bool hasPlowedTile = false;

    private void Start()
    {
        farmGrid = FindFirstObjectByType<FarmGrid>();
        gameManager = FindFirstObjectByType<GameManager>();

        // Get and disable the raycaster if it exists
        if (npcPanel != null)
        {
            npcRaycaster = npcPanel.GetComponent<GraphicRaycaster>();
            if (npcRaycaster != null)
            {
                npcRaycaster.enabled = false;
                DebugLog("Disabled GraphicRaycaster on NPC panel");
            }
        }

        // Hide NPC initially and ensure it doesn't block raycasts
        if (npcCanvasGroup != null)
        {
            npcCanvasGroup.alpha = 0;
            npcCanvasGroup.interactable = false;
            npcCanvasGroup.blocksRaycasts = false;
            DebugLog("NPC CanvasGroup initialized - blocksRaycasts: false");
        }

        if (pointerArrow != null)
            pointerArrow.gameObject.SetActive(false);

        // Start tutorial after a brief delay to let objects instantiate
        Invoke("StartTutorial", 2f);
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Count == 0)
        {
            Debug.LogWarning("No tutorial steps defined!");
            return;
        }

        isTutorialActive = true;
        currentStepIndex = 0;
        plantsPlanted = 0;
        plantsWatered = 0;
        hasPlowedTile = false;

        ShowStep(currentStepIndex);
    }

    private void ShowStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialSteps.Count)
        {
            DebugLog($"Invalid step index: {stepIndex}. Total steps: {tutorialSteps.Count}");
            EndTutorial();
            return;
        }

        TutorialStep currentStep = tutorialSteps[stepIndex];

        DebugLog($"Showing step {stepIndex}: {currentStep.stepName}, Action: {currentStep.requiredAction}");

        // Ensure NPC doesn't block raycasts
        UpdateNPCRaycastBlocking();

        // Move NPC to new position with animation
        StartCoroutine(MoveNPCToPosition(currentStep.npcScreenPosition));

        // Show instruction text with typewriter effect
        if (currentTextCoroutine != null)
            StopCoroutine(currentTextCoroutine);

        currentTextCoroutine = StartCoroutine(TypeText(currentStep.instructionText));

        // Setup pointer arrow if there's a target to point at
        SetupPointerArrow(currentStep);

        // Change to talking sprite
        if (npcSprite != null && npcTalkingSprite != null)
            npcSprite.sprite = npcTalkingSprite;

        // Wait for player action if required
        if (currentStep.requiredAction != TutorialAction.None)
        {
            waitingForAction = true;
            DebugLog($"NPC Tutorial: Waiting for action - {currentStep.requiredAction}");

            // Special handling for plowed tile step
            if (currentStep.requiredAction == TutorialAction.ClickPlowedTile && hasPlowedTile)
            {
                // If we already know where the plowed tile is, point to it
                StartCoroutine(PointToPlowedTileAfterDelay());
            }
        }
        else
        {
            // If no action required, automatically advance after a delay
            Invoke("AdvanceToNextStep", 3f);
        }
    }

    private IEnumerator PointToPlowedTileAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // Find the plowed tile dynamically
        GameObject plowedTile = FindPlowedTile();
        if (plowedTile != null && pointerArrow != null)
        {
            SetupPointerForObject(plowedTile, new Vector2(0, 100f));
            DebugLog($"Pointer arrow now pointing at plowed tile at position: {plowedTile.transform.position}");
        }
    }

    private GameObject FindPlowedTile()
    {
        // Look for objects with the "field" tag
        GameObject[] fieldObjects = GameObject.FindGameObjectsWithTag("field");
        if (fieldObjects.Length > 0)
        {
            // If we have a specific position, find the closest one
            if (hasPlowedTile)
            {
                GameObject closest = null;
                float closestDistance = float.MaxValue;

                foreach (GameObject field in fieldObjects)
                {
                    float distance = Vector3.Distance(field.transform.position, plowedTilePosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = field;
                    }
                }
                return closest;
            }
            else
            {
                // Just return the first field we find
                return fieldObjects[0];
            }
        }

        return null;
    }

    private void UpdateNPCRaycastBlocking()
    {
        if (npcCanvasGroup != null)
        {
            npcCanvasGroup.blocksRaycasts = false;
            npcCanvasGroup.interactable = false;
        }

        if (npcRaycaster != null)
        {
            npcRaycaster.enabled = false;
        }
    }

    private IEnumerator MoveNPCToPosition(Vector2 targetPosition)
    {
        if (npcPanel == null || npcCanvasGroup == null) yield break;

        RectTransform npcRect = npcPanel.GetComponent<RectTransform>();
        if (npcRect == null) yield break;

        if (npcCanvasGroup.alpha < 0.1f)
        {
            float fadeTime = 0f;
            while (fadeTime < fadeDuration)
            {
                fadeTime += Time.deltaTime;
                npcCanvasGroup.alpha = Mathf.Lerp(0, 1, fadeTime / fadeDuration);
                yield return null;
            }
            npcCanvasGroup.alpha = 1;
            npcCanvasGroup.interactable = false;
            npcCanvasGroup.blocksRaycasts = false;
        }

        Vector2 startPosition = npcRect.anchoredPosition;
        float slideTime = 0f;

        while (slideTime < slideDuration)
        {
            slideTime += Time.deltaTime;
            npcRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, slideTime / slideDuration);
            yield return null;
        }

        npcRect.anchoredPosition = targetPosition;
    }

    private IEnumerator TypeText(string text)
    {
        if (instructionText == null) yield break;

        instructionText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            instructionText.text += letter;
            yield return new WaitForSeconds(textTypeSpeed);
        }
    }

    private void SetupPointerArrow(TutorialStep step)
    {
        if (pointerArrow == null) return;

        GameObject targetObject = step.targetObject;

        // For plowed tile step, try to find the tile dynamically
        if (step.requiredAction == TutorialAction.ClickPlowedTile && step.useDynamicTarget)
        {
            targetObject = FindPlowedTile();
            if (targetObject != null)
            {
                DebugLog($"Found plowed tile dynamically: {targetObject.name} at {targetObject.transform.position}");
            }
        }

        if (targetObject == null)
        {
            pointerArrow.gameObject.SetActive(false);
            Debug.LogWarning($"No target object found for pointer arrow in step: {step.stepName}");
            return;
        }

        SetupPointerForObject(targetObject, step.pointerOffset);
        DebugLog($"Pointer arrow pointing at: {targetObject.name}");
    }

    private void SetupPointerForObject(GameObject targetObject, Vector2 offset)
    {
        if (pointerArrow == null || targetObject == null) return;

        Vector3 worldPosition = targetObject.transform.position;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pointerArrow.parent as RectTransform,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        pointerArrow.anchoredPosition = localPoint + offset;

        Vector2 direction = (localPoint - pointerArrow.anchoredPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pointerArrow.rotation = Quaternion.Euler(0, 0, angle - 90f);

        pointerArrow.gameObject.SetActive(true);
    }

    // === TUTORIAL ACTION HANDLERS ===
    public void OnTileClicked(GameObject tile)
    {
        if (!waitingForAction || !isTutorialActive) return;

        var currentStep = tutorialSteps[currentStepIndex];

        DebugLog($"Tile clicked: {tile.name}. Current step requires: {currentStep.requiredAction}");

        if (currentStep.requiredAction == TutorialAction.ClickEmptyTile)
        {
            DebugLog("Empty tile clicked - completing ClickEmptyTile step");
            CompleteCurrentStep();
        }
    }

    // NEW METHOD: Specifically for handling plowed tile clicks
    public void OnPlowedTileClicked(GameObject plowedTile)
    {
        if (!waitingForAction || !isTutorialActive) return;

        var currentStep = tutorialSteps[currentStepIndex];

        DebugLog($"Plowed tile clicked: {plowedTile.name}. Current step requires: {currentStep.requiredAction}");

        if (currentStep.requiredAction == TutorialAction.ClickPlowedTile)
        {
            DebugLog("Plowed tile clicked - completing ClickPlowedTile step");

            // Store the plowed tile position for future reference
            plowedTilePosition = plowedTile.transform.position;
            hasPlowedTile = true;

            CompleteCurrentStep();
        }
    }

    // NEW METHOD: Called when a field is created (plowed)
    public void OnFieldCreated(GameObject field, Vector3 position)
    {
        DebugLog($"Field created at: {position}");

        // Store the position where the field was created
        plowedTilePosition = position;
        hasPlowedTile = true;

        // If we're waiting for a plowed tile click, update the pointer
        if (isTutorialActive && waitingForAction &&
            tutorialSteps[currentStepIndex].requiredAction == TutorialAction.ClickPlowedTile)
        {
            StartCoroutine(PointToPlowedTileAfterDelay());
        }
    }

    public void OnPlowButtonClicked()
    {
        if (!isTutorialActive) return;

        DebugLog($"Plow button clicked. Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction && tutorialSteps[currentStepIndex].requiredAction == TutorialAction.ClickPlowButton)
        {
            DebugLog("Plow button clicked - completing ClickPlowButton step");
            CompleteCurrentStep();
        }
    }

    public void OnPlantButtonClicked()
    {
        if (!isTutorialActive) return;

        DebugLog($"Plant button clicked. Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction && tutorialSteps[currentStepIndex].requiredAction == TutorialAction.ClickPlantButton)
        {
            DebugLog("Plant button clicked - completing ClickPlantButton step");
            CompleteCurrentStep();
        }
    }

    public void OnPlantSelected()
    {
        if (!isTutorialActive) return;

        DebugLog($"Plant selected. Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction && tutorialSteps[currentStepIndex].requiredAction == TutorialAction.SelectPlantType)
        {
            DebugLog("Plant selected - completing SelectPlantType step");
            CompleteCurrentStep();
        }
    }

    public void OnPlantClickedToWater(GameObject plant)
    {
        if (!isTutorialActive) return;

        DebugLog($"Plant clicked to water. Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction && tutorialSteps[currentStepIndex].requiredAction == TutorialAction.ClickPlantToWater)
        {
            DebugLog("Plant clicked to water - completing ClickPlantToWater step");
            CompleteCurrentStep();
        }
    }

    public void OnPlantWatered()
    {
        if (!isTutorialActive) return;

        plantsWatered++;
        DebugLog($"Plant watered - count: {plantsWatered}, Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction)
        {
            var currentStep = tutorialSteps[currentStepIndex];

            if (currentStep.requiredAction == TutorialAction.WaterPlant)
            {
                DebugLog("First plant watered - completing WaterPlant step");
                CompleteCurrentStep();
            }
            else if (currentStep.requiredAction == TutorialAction.WaterSecondPlant && plantsWatered >= 2)
            {
                DebugLog("Second plant watered - completing WaterSecondPlant step");
                CompleteCurrentStep();
            }
        }
    }

    public void OnPlantPlanted()
    {
        if (!isTutorialActive) return;

        plantsPlanted++;
        DebugLog($"Plant planted - count: {plantsPlanted}, Waiting: {waitingForAction}, Current action: {tutorialSteps[currentStepIndex].requiredAction}");

        if (waitingForAction)
        {
            var currentStep = tutorialSteps[currentStepIndex];

            if (currentStep.requiredAction == TutorialAction.PlantSecondPlant && plantsPlanted >= 2)
            {
                DebugLog("Second plant planted - completing PlantSecondPlant step");
                CompleteCurrentStep();
            }
        }
    }

    private void CompleteCurrentStep()
    {
        if (!isTutorialActive || !waitingForAction)
        {
            DebugLog($"Cannot complete step - Active: {isTutorialActive}, Waiting: {waitingForAction}");
            return;
        }

        DebugLog($"Completing step {currentStepIndex}: {tutorialSteps[currentStepIndex].stepName}");

        waitingForAction = false;

        if (npcSprite != null && npcIdleSprite != null)
            npcSprite.sprite = npcIdleSprite;

        CancelInvoke("AdvanceToNextStep");
        Invoke("AdvanceToNextStep", 1f);
    }

    private void AdvanceToNextStep()
    {
        if (!isTutorialActive) return;

        currentStepIndex++;
        DebugLog($"Advancing to step {currentStepIndex} of {tutorialSteps.Count}");

        if (currentStepIndex < tutorialSteps.Count)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            DebugLog("No more steps - ending tutorial");
            EndTutorial();
        }
    }

    public void SkipTutorial()
    {
        DebugLog("Tutorial skipped by player");
        EndTutorial();
    }

    private void EndTutorial()
    {
        DebugLog("Ending tutorial");
        isTutorialActive = false;
        waitingForAction = false;

        StartCoroutine(FadeOutNPC());

        if (farmGrid != null)
        {
            farmGrid.OnTutorialComplete();
        }

        if (gameManager != null)
        {
            gameManager.OnTutorialComplete();
        }
    }

    private IEnumerator FadeOutNPC()
    {
        if (npcCanvasGroup == null) yield break;

        if (pointerArrow != null)
            pointerArrow.gameObject.SetActive(false);

        float fadeTime = 0f;
        float startAlpha = npcCanvasGroup.alpha;

        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            npcCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, fadeTime / fadeDuration);
            yield return null;
        }

        npcCanvasGroup.alpha = 0;
        npcCanvasGroup.interactable = false;
        npcCanvasGroup.blocksRaycasts = false;

        DebugLog("NPC Tutorial completed! Game starting...");
    }

    private void DebugLog(string message)
    {
        if (enableDebug)
        {
            Debug.Log($"[TUTORIAL] {message}");
        }
    }

    public bool IsTutorialActive() => isTutorialActive;
    public bool IsWaitingForAction() => waitingForAction;
    public int GetCurrentStepIndex() => currentStepIndex;
    public string GetCurrentStepName() => currentStepIndex < tutorialSteps.Count ? tutorialSteps[currentStepIndex].stepName : "Completed";
}