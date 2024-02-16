using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    public GameObject[] squares;
    public AudioSource[] squareAudioSources;
    public int score = 0;
    public int lives = 3;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI timerText;

    private List<int> sequence;
    private List<int> playerInput;
    private bool acceptingInput;
    private int currentIndex;
    private float timerDuration = 10f;
    private float timer;
    private Color[] originalColors;

    // Start is called before the first frame update
    void Start()
    {
        // Assign UI Text components here
        Transform canvasTransform = GameObject.Find("Canvas").transform;
        scoreText = canvasTransform.Find("GameLogicObject/scoreText").GetComponent<TMPro.TextMeshProUGUI>();
        livesText = canvasTransform.Find("GameLogicObject/livesText").GetComponent<TMPro.TextMeshProUGUI>();
        turnText = canvasTransform.Find("GameLogicObject/turnText").GetComponent<TMPro.TextMeshProUGUI>();
        timerText = canvasTransform.Find("GameLogicObject/timerText").GetComponent<TMPro.TextMeshProUGUI>();

        playerInput = new List<int>();
        squares = GameObject.FindGameObjectsWithTag("Square");
        squareAudioSources = new AudioSource[squares.Length];

        for (int i = 0; i < squares.Length; i++)
        {
            squareAudioSources[i] = squares[i].GetComponent<AudioSource>();
        }
        originalColors = new Color[squares.Length];
        for (int i = 0; i < squares.Length; i++)
        {
            Image squareImage = squares[i].GetComponent<Image>();
            originalColors[i] = squareImage.color;
        }

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerText();
        StartCoroutine(PlayerTurn());
    }

    void StartGame()
    {
        score = 0;
        lives = 3;
        sequence = new List<int>();
        playerInput = new List<int>();
        currentIndex = 0;

        // start game loop
        StartCoroutine(GameLoop());

        // Update UI at the start of the game
        UpdateScoreAndLives();
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            // Computer's turn
            turnText.text = "Computer's Turn";
            GenerateSequence();
            yield return StartCoroutine(PlaySequence());

            // Wait for the sequence to be played
            yield return new WaitForSeconds(0.5f);

            // Player's turn
            turnText.text = "Player's Turn";
            acceptingInput = true;
            timer = timerDuration;

            // Wait for the player to input or timeout
            while (playerInput.Count < sequence.Count && timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            // Check correctness and update score/lives
            OnPlayerTurn(sequence);

            // Reset variables for the next round
            playerInput.Clear();
            acceptingInput = false;
            currentIndex = 0;

            // Restart the game loop after a delay
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator PlaySequence()
    {
        int steps = Random.Range(3, 5); // Randomly choose 3 or 4 steps
        List<int> computerSequence = new List<int>();

        for (int i = 0; i < steps; i++)
        {
            int index = Random.Range(0, squares.Length);
            computerSequence.Add(index);

            // Highlight square
            HighlightSquare(index, 0.1f);

            // Play xylophone sound
            squareAudioSources[index].Play();

            // Wait for a short duration (adjust as needed)
            yield return new WaitForSeconds(0.3f);

            // Reset square color after playing
            ResetSquareColorAfterDelay(index, 0.5f);

            // Wait before the next square in the sequence
            yield return new WaitForSeconds(0.5f);
        }

        // Wait for a delay before player's turn
        yield return new WaitForSeconds(0.5f);

        // Notify the player's turn
        OnPlayerTurn(computerSequence);
    }

    void OnPlayerTurn(List<int> computerSequence)
    {
        // This method is called when it's the player's turn
        // You should compare the player's input with the computer's sequence

        // Continue with the rest of the logic in OnPlayerTurn
        bool sequenceCorrect = true;

        // Check if the player's input matches the computer's sequence
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != computerSequence[i])
            {
                sequenceCorrect = false;
                break;
            }
        }

        // If the sequence is correct, award 5 points to the score
        if (sequenceCorrect)
        {
            score += 5;
        }
        else
        {
            // If the sequence is incorrect, decrement lives and update UI
            lives--;
            UpdateScoreAndLives();
        }

        // Check if game over
        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            // If the sequence is correct or there are more steps to input
            if (sequenceCorrect || currentIndex < computerSequence.Count)
            {
                // Increment currentIndex for the next step in the sequence
                currentIndex++;

                // If there are more steps, wait for a short duration before the next input
                if (currentIndex < computerSequence.Count)
                {
                    StartCoroutine(WaitForPlayerInput());
                }
                else
                {
                    // If the entire sequence is correct, reset variables for the next round
                    currentIndex = 0;
                    acceptingInput = false;
                    StartCoroutine(RestartGameAfterDelay(1f));
                }
            }
        }
    }

    IEnumerator WaitForPlayerInput()
    {
        // Wait for a short duration before allowing the next input
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator PlayerTurn()
    {
        // Enable player input only during the player's turn
        if (acceptingInput)
        {
            // Check for player input
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                RectTransform clickedTransform = GetClickedTransform(mousePosition);

                if (clickedTransform != null)
                {
                    // Check if the clicked object is one of the squares
                    GameObject clickedSquare = clickedTransform.gameObject;
                    int squareIndex = System.Array.IndexOf(squares, clickedSquare);

                    if (squareIndex != -1)
                    {
                        // Player clicked on a square
                        playerInput.Add(squareIndex);

                        // Highlight square
                        HighlightSquare(squareIndex, 0.1f);

                        // Check correctness immediately
                        CheckPlayerInput();

                        // If the player completed the input, exit the loop
                        if (playerInput.Count == sequence.Count)
                        {
                            // Wait for a short duration before restarting the game loop
                            yield return new WaitForSeconds(1f);
                            ResetGame();
                        }
                    }
                }
            }

            // Return null to satisfy IEnumerator
            yield return null;
        }
    }

    void CheckPlayerInput()
    {
        // Check correctness immediately
        if (!CheckCorrectness())
        {
            // Incorrect input, decrement lives
            lives--;
            UpdateScoreAndLives();

            // Check if game over
            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                // Incorrect input, reset sequence and restart the game loop
                ResetGame();
            }
        }
        else if (playerInput.Count == sequence.Count)
        { // correct input for the sequence
            score += 5; // gives 5 points
            UpdateScoreAndLives();
        }
    }

    void HighlightSquare(int index, float duration)
    {
        Image squareImage = squares[index].GetComponent<Image>();
        squareImage.color = Color.white;
        squareAudioSources[index].Play();
        StartCoroutine(ResetSquareColorAfterDelay(index, duration));
    }

    IEnumerator ResetSquareColorAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        Image squareImage = squares[index].GetComponent<Image>();

        // Set the square color back to its original color
        squareImage.color = originalColors[index];
    }


    RectTransform[] GetSquareTransforms()
    {
        return squares.Select(square => square.GetComponent<RectTransform>()).ToArray();
    }

    RectTransform GetClickedTransform(Vector2 screenPosition)
    {
        // Convert the screen position to a point in the UI space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            turnText.rectTransform, screenPosition, Camera.main, out Vector2 localPoint);

        // Check if any UI element was clicked
        RectTransform clickedTransform = null;
        foreach (RectTransform squareTransform in GetSquareTransforms())
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(squareTransform, screenPosition))
            {
                clickedTransform = squareTransform;
                break;
            }
        }

        return clickedTransform;
    }

    void UpdateScoreAndLives()
    {
        if (scoreText != null && livesText != null)
        {
            // Update score and lives based on player input
            // increment score for correct input
            // decrement score for incorrect input

            scoreText.text = "Score: " + score;
            livesText.text = "Lives: " + lives;
        }
        else
        {
            Debug.LogError("scoreText or livesText is not assigned in the Inspector.");
        }
    }

    void ResetGame()
    {
        // Reset variables for the next round
        playerInput.Clear();

        // Restart the game loop after a delay (replace 2f with your desired delay)
        StartCoroutine(RestartGameAfterDelay(2f));
    }

    IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(GameLoop());
    }

    bool CheckCorrectness()
    {
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != sequence[i])
            {
                return false; // incorrect input
            }
        }
        return true; // correct input
    }

    void GenerateSequence()
    {
        // generate a new random sequence
        sequence.Clear();
        for (int i = 0; i < Random.Range(3, 5); i++) // sequence length
        {
            int randomIndex = Random.Range(0, squares.Length);
            sequence.Add(randomIndex);
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        //  can add additional logic for game over, such as showing a game over screen.
    }

    void UpdateTimerText()
    {
        // Update timer text on the UI
        if (timerText != null)
        {
            timerText.text = "Timer: " + Mathf.Ceil(timer);
        }
        else
        {
            Debug.LogError("timerText is not assigned in the Inspector.");
        }
    }
}
