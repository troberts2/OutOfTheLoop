using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Transform tim;
    [SerializeField] private BasicEnemyCircle circleToDodge;
    public bool hasExited = false;

    [SerializeField] private GameObject heartsBack;
    [SerializeField] private GameObject heartsFront;

    [SerializeField] private BasicEnemyCircle[] showCircles;

    [SerializeField] private Multiplier[] multipliers;
    public int multipliersCollected = 0;

    private Tween currentTween;

    private void Start()
    {
        StartCoroutine(TutorialProcess());
    }

    private IEnumerator TutorialProcess()
    {
        switch(GameManager.Instance.movementType)
        {
            case MovementType.FingerFollow:
                FadeIn("This is Tim.\r\n\r\nMove Tim by holding the screen.");
                yield return StartCoroutine(WaitForClickOrTap());
                break;
            case MovementType.JoyStick:
                FadeIn("This is Tim.\r\n\r\nMove Tim by holding the screen.");
                yield return StartCoroutine(WaitForClickOrTap());
                break;
            case MovementType.Tilt:
                FadeIn("This is Tim.\r\n\r\nMove Tim by tilting the screen.");
                yield return StartCoroutine(WaitForTilt());
                break;
        }
        

        yield return new WaitForSeconds(3f);

        FadeOut();
        yield return new WaitForSeconds(fadeDuration + 1f);

        FadeIn("Nice work.");

        yield return new WaitForSeconds(2f);
        FadeOut();

        //reset Tim and spawn red circle
        tim.position = Vector2.zero;
        var playerMov = tim.GetComponent<PlayerMovement>();
        var playerCol = tim.GetComponent<PlayerCollision>();
        playerMov.canMove = false;

        yield return new WaitForSeconds(fadeDuration);

        FadeIn("Tim hates circles.\r\n\r\nKeep Tim Out of the Loop!");

        yield return new WaitForSeconds(fadeDuration);

        //make circle appear and almost explode
        circleToDodge.gameObject.SetActive(true);
        circleToDodge.ActivateDeathCircleTutorial();
        yield return new WaitForSeconds(2f);

        //enable Tim movement so he can get out
        playerMov.canMove = true;

        while(!hasExited)
        {
            yield return null; 
        }

        //when player exits finish circle pop
        circleToDodge.FinishDeathCircleTutorial();
        FadeOut();

        yield return new WaitForSeconds(fadeDuration);

        FadeIn("You must be a gamer.");

        yield return new WaitForSeconds(fadeDuration + 1f);

        playerMov.canMove = false;
        tim.position = Vector2.zero;

        FadeOut();

        yield return new WaitForSeconds(fadeDuration);

        FadeIn("Circles are usually dangerous.\r\n\r\nRed circles damage Tim.");
        heartsBack.SetActive(true);
        heartsFront.SetActive(true);
        yield return new WaitForSeconds(fadeDuration + 2f);
        circleToDodge.gameObject.SetActive(true);
        circleToDodge.ActivateDeathCircle(true);

        yield return new WaitForSeconds(fadeDuration + 3f);

        FadeOut();
        yield return new WaitForSeconds(fadeDuration);

        FadeIn("Some circles are good.\r\n\r\nGreen circles heal Tim.");
        yield return new WaitForSeconds(fadeDuration + 2f);
        circleToDodge.gameObject.SetActive(true);
        circleToDodge.ActivateDeathCircle(false, true);

        yield return new WaitForSeconds(3f);
        FadeOut();

        yield return new WaitForSeconds(fadeDuration);
        FadeIn("Each circle gives you points.\r\n\r\nThe goal is to survive.");
        yield return new WaitForSeconds(fadeDuration + 2f);
        GameManager.Instance.playerScore = 0;
        GameManager.Instance.scoreText.text = 0.ToString();
        GameManager.Instance.scoreText.enabled = true;

        //pop a few circles
        foreach(var circle in showCircles)
        {
            circle.gameObject.SetActive(true);
            circle.ActivateDeathCircle(true);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(4f);
        FadeOut();

        //make player collect multiplier
        yield return new WaitForSeconds(fadeDuration);
        FadeIn("You can collect multipliers.\r\n\r\nCircles will give more points.");
        GameManager.Instance.playerScore = 0;
        GameManager.Instance.scoreText.text = 0.ToString();
        yield return new WaitForSeconds(fadeDuration + 2f);

        //pop a few multipliers
        foreach (var multiplier in multipliers)
        {
            multiplier.gameObject.SetActive(true);
            multiplier.TurnOn(true);
            tim.GetComponentInChildren<MultiplierPointer>().target = multiplier.transform;
            yield return new WaitForSeconds(0.2f);
        }

        playerMov.canMove = true;

        //wait for player to collect multpliers
        while(multipliersCollected < 3)
        {
            yield return null;
        }

        //reset player and show how much score increases with multiplier
        playerMov.canMove = false;
        tim.position = Vector2.zero;

        yield return new WaitForSeconds(2f);

        //pop a few circles
        foreach (var circle in showCircles)
        {
            circle.gameObject.SetActive(true);
            circle.ActivateDeathCircle(true);
            yield return new WaitForSeconds(0.2f);
        }

        FadeOut();

        yield return new WaitForSeconds(fadeDuration + 1f);
        FadeIn("That's it, you know everything.\r\n\r\nYou should be ready now...");
        yield return new WaitForSeconds(fadeDuration + 5f);
        FadeOut();
        yield return new WaitForSeconds(fadeDuration);

        //go to game scene?
        EventSystem.current.enabled = false;
        SceneManager.LoadScene("GameScene");
        GameManager.Instance.ResetGame();
    }

    /// <summary>
    /// Sets the text and fades it in.
    /// </summary>
    public void FadeIn(string message)
    {
        currentTween?.Kill();
        tutorialText.text = message;
        tutorialText.alpha = 0f;

        currentTween = tutorialText.DOFade(1f, fadeDuration)
            .OnComplete(() => currentTween = null);
    }

    /// <summary>
    /// Fades the current text out.
    /// </summary>
    public void FadeOut()
    {
        currentTween?.Kill();

        currentTween = tutorialText.DOFade(0f, fadeDuration)
            .OnComplete(() => currentTween = null);
    }

    /// <summary>
    /// Instantly hide text (useful for resetting).
    /// </summary>
    public void HideInstant()
    {
        currentTween?.Kill();
        tutorialText.alpha = 0f;
    }

    private IEnumerator WaitForClickOrTap()
    {
        while (true)
        {
            // Mouse click
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                yield break;

            // Touch tap
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                yield break;

            yield return null; // wait a frame and check again
        }
    }

    private IEnumerator WaitForTilt()
    {
        while (true)
        {
            if (Accelerometer.current == null)
                yield break; // device doesn’t support accelerometer

            // Read acceleration (x,y,z)
            Vector3 tilt = Accelerometer.current.acceleration.ReadValue();

            // Subtract calibration so "rest" = neutral
            Vector3 moveInput = tilt - GameManager.Instance.calibrationOffset;

            // Optional: ignore Z tilt (forward/back angle)
            moveInput.z = 0f;

            // Apply threshold (dead zone)
            if (moveInput.magnitude > 0.1f)
            {
                yield break;
            }

            yield return null; // wait a frame and check again
        }
    }

}
