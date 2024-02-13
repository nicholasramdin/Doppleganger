using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class GameLogic : MonoBehaviour
{
    public GameObject[] squares;
    public AudioSource[] squareAudioSources;
    public int score = 0;
    public int lives = 3;
    public Text scoreText; // UI text for displaying score
    public Text livesText; //  UI text for displaying lives

    private List<int> sequence;
    private List<int> playerInput;
    private bool acceptingInput;
    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
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
        CheckPlayerInput();
    }

    void StartGame()
    {
        score = 0;
        lives = 3;
        sequence = new List<int>();
        playerInput = new List<int>();

        // start game loop
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            // generate sequence
            GenerateSequence();
            yield return new WaitForSeconds(2f);

            // play sequence
            StartCoroutine(PlaySequence());

            // check player input
            currentIndex = 0;
            playerInput.Clear(); // clear previous player input
            acceptingInput = true;
            yield return new WaitUntil(() => currentIndex == sequence.Count);

            // check correctness and update score/lives
            CheckPlayerInput();
            UpdateScoreAndLives();
        }
    }

    IEnumerator PlaySequence()
    {
        foreach (int index in sequence)
        {
            // Highlight square and play xylophone sound attached to the square
            squares[index].GetComponent<Renderer>().material.color = Color.white;
            squareAudioSources[index].Play();

            yield return new WaitForSeconds(1f);

            // Reset square color after playing
            squares[index].GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    void CheckPlayerInput()
    {
        // check player input
        // compare input against the current sequence
        // if correct, increment currentIndex

        if (Input.GetMouseButtonDown(0) && acceptingInput)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // check if square is clicked
                GameObject clickedSquare = hit.collider.gameObject;
                int squareIndex = System.Array.IndexOf(squares, clickedSquare);

                if (squareIndex != -1)
                {
                    // Player clicked on a square
                    playerInput.Add(squareIndex);

                    // Check correctness
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
                        ResetGame();
                    }
                }
            }
        }
    }

    void UpdateScoreAndLives()
    {
        // update score and lives based on player input
        // increment score for correct input
        // decrement score for incorrect input

        scoreText.text = "Score: " + score;
        livesText.text = "Lives: " + lives;
    }

    void ResetGame()
    {
        // reset for the next round
        playerInput.Clear();
        acceptingInput = false;

        // restart game loop after delay
        StartCoroutine(RestartGameAfterDelay(2f));
    }

    IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        acceptingInput = true;
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
        for (int i = 0; i < 3; i++) // sequence length
        {
            int randomIndex = Random.Range(0, squares.Length);
            sequence.Add(randomIndex);
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
    }
}
