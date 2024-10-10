using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

public class SwipeFollow : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI swipeForceText;
    public TextMeshProUGUI endTextHint;

    public Transform carTransform; // The car object
    public SplineContainer spline; // Your spline path, assuming you have a spline component

    private int _swipeForce;
    private bool _swiped;

    private Vector2 _swipeStart;
    private Vector2 _swipeEnd;
    private bool _swipeDetected = false;
    private float _maxSwipeForce = 1.0f;
    private const int WinThresholdMin = 3;
    private const int WinThresholdMax = 5;
    private bool _gameEnded;

    private void Update()
    {
        if (_gameEnded) return;
        if (!_swiped)
        {
            DetectSwipe();
        }
    }

    private void DetectSwipe()
    {
        if (Input.touchCount <= 0) return;
        var touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                _swipeStart = touch.position;
                break;
            case TouchPhase.Ended:
                _swipeEnd = touch.position;
                CalculateSwipeForce();
                break;
            case TouchPhase.Moved:
                break;
            case TouchPhase.Stationary:
                break;
            case TouchPhase.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CalculateSwipeForce()
    {
        var swipeDelta = _swipeEnd - _swipeStart;
        var swipeDistance = swipeDelta.magnitude;

        var normalizedSwipe = swipeDistance / Screen.height;
        _swipeForce = Mathf.Clamp(Mathf.RoundToInt(normalizedSwipe * 10), 0, 10);

        swipeForceText.text = $"Swipe force: {_swipeForce}";

        EndGame(_swipeForce is >= WinThresholdMin and <= WinThresholdMax);

        _swiped = true;
    }

    private void EndGame(bool success)
    {
        _gameEnded = true;
        if (success)
        {
            MoveCarToEnd();
        }
        else
        {
            MoveCarToRandomLocation();
        }
    }

    private void MoveCarToEnd()
    {
        StartCoroutine(MoveCarOnSpline(1.0f, true));
    }

    private void MoveCarToRandomLocation()
    {
        var randomPosition = UnityEngine.Random.Range(0.1f, 0.9f);
        StartCoroutine(MoveCarOnSpline(randomPosition, false));
    }

    private System.Collections.IEnumerator MoveCarOnSpline(float targetPosition, bool success)
    {
        var currentTime = 0f;
        const float startPosition = 0f;
        var duration = UnityEngine.Random.Range(2f, 4f);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            var t = currentTime / duration;
            var newPosition = Mathf.Lerp(startPosition, targetPosition, t);

            carTransform.position = spline.EvaluatePosition(newPosition);

            carTransform.rotation = Quaternion.LookRotation(spline.EvaluateTangent(newPosition));
            carTransform.rotation *= Quaternion.Euler(0, 180, 0);

            yield return null;
        }

        resultText.text = success ? "You win!" : "You lost!";
        resultText.color = success ? Color.green : Color.red;

        if (success)
        {
            endTextHint.text = "Nice!";
        }
        else
            endTextHint.text = _swipeForce switch
            {
                < WinThresholdMin => "Too slow!",
                > WinThresholdMax => "Too fast!",
                _ => endTextHint.text
            };

        if (!success)
        {
            Invoke(nameof(ShowInterstitialAd), 1.0f);
        }
    }

    public void ShowInterstitialAd()
    {
        FindObjectOfType<InterstitialAdExample>().ShowAd();
    }

    public void RestartGame()
    {
        _gameEnded = false;
        _swiped = false;
        resultText.text = "";
        swipeForceText.text = "";
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
