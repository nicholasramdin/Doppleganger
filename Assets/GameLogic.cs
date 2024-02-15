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

    private bool ComputerSequenceIsPlaying, ComputerSequenceHasCompleted;
    private bool PlayerWaitingForInput, PlayerInputComplete;

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

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerText();
        //StartCoroutine(PlayerTurn());
        if (acceptingInput)
        {
            PlayerTurn();
        }
    }

    void StartGame()
    {
        score = 0;
        lives = 3;
        sequence = new List<int>();
        playerInput = new List<int>();
        currentIndex = 0;
        PlayerInputComplete = true;
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

            if (PlayerInputComplete)
            {
                PlayerInputComplete = false;

                playerInput.Clear();
                acceptingInput = false;
                currentIndex = 0;


                yield return StartCoroutine(PlaySequence());
            }
            //Check that the player has completed first
            acceptingInput = true;
            timer = timerDuration;

            turnText.text = "Player's Turn";



            // Wait for the sequence to be played
            //yield return new WaitForSeconds(sequence.Count * 1.5f);

            // Player's turn


            // Wait for the player to input or timeout
            //while (playerInput.Count < sequence.Count && timer > 0)
            //{
            //    timer -= Time.deltaTime;
            //    yield return null;
            //}

            // Check correctness and update score/lives
            //OnPlayerTurn(sequence);

            // Reset variables for the next round

            // Restart the game loop after a delay
            yield return new WaitForSeconds(.03f);
        }
    }

    IEnumerator PlaySequence()
    {
        int steps = Random.Range(3, 5); // Randomly choose 3 or 4 steps
        List<int> computerSequence = new List<int>();
        ComputerSequenceIsPlaying = true;
        for (int i = 0; i < steps; i++)
        {
            int index = Random.Range(0, squares.Length);
            computerSequence.Add(index);

            // Highlight square
            Image squareImage = squares[index].GetComponent<Image>();
            Color originalColor = squareImage.color; // Save the original color

            squareImage.color = Color.white;

            // Play xylophone sound
            squareAudioSources[index].Play();

            // Wait for a short duration (adjust as needed)
            yield return new WaitForSeconds(0.5f);

            // Reset square color after playing
            squareImage.color = originalColor;

            // Wait before the next square in the sequence
            yield return new WaitForSeconds(0.7f);
        }

        // Wait for a delay before player's turn
        yield return new WaitForSeconds(.5f);
        ComputerSequenceIsPlaying = false;
        ComputerSequenceHasCompleted = true;
        // Notify the player's turn
        OnPlayerTurn(computerSequence);


    }
    /// <summary>
    /// THIS FUNCTION HANDLES THE TRANSITION INTO THE PLAYERS PLAYING THE GAME.  NO C ROUTINES ETC JUST SHOW THE BOARD, SET YOUR STATE WAIT FOR INPUT.
    /// </summary>
    /// <param name="computerSequence"></param>
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
            //// If the sequence is correct or there are more steps to input
            //if (sequenceCorrect || currentIndex < computerSequence.Count)
            //{
            //    // Increment currentIndex for the next step in the sequence
            //    currentIndex++;

            //    // If there are more steps, wait for a short duration before the next input
            //    if (currentIndex < computerSequence.Count)
            //    {
            //        StartCoroutine(WaitForPlayerInput());
            //    }
            //    else
            //    {
            //        // If the entire sequence is correct, reset variables for the next round
            //        currentIndex = 0;
            //        acceptingInput = false;
            //        StartCoroutine(RestartGameAfterDelay(1f));
            //    }
            //}
            acceptingInput = true;
        }
    }

    IEnumerator WaitForPlayerInput()
    {
        // Wait for a short duration before allowing the next input
        yield return new WaitForSeconds(0.5f);
    }

    void PlayerTurn()
    {
        ComputerSequenceHasCompleted = false;
        PlayerWaitingForInput = true;
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

                        // Check correctness immediately
                        CheckPlayerInput();

                        // If the player completed the input, exit the loop
                        if (playerInput.Count == sequence.Count)
                        {
                            // Wait for a short duration before restarting the game loop
                            //yield return new WaitForSeconds(1f);
                            PlayerWaitingForInput = false;
                            PlayerInputComplete = true;
                            ResetGame();
                            
                        }
                    }
                }
            }
            // Return null to satisfy IEnumerator
        }
    }

    void CheckPlayerInput()
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
        }
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
        // You can add additional logic for game over, such as showing a game over screen.
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
