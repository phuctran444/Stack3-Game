using System.Collections;
using UnityEngine;

public class GameLoopController : Singleton<GameLoopController>
{
    private bool _firstPlay = true;
    public bool GameHasStarted { get; set; } = false;
    public bool GameIsOver { get; set; } = false;

    private void Start()
    {
        BoardController.Instance.CreateBoard();
        StartGameLoop();
    }

    private void StartGameLoop()
    {
        StartCoroutine(GameLoop());
    }

    #region Game Loop
    public IEnumerator GameLoop()
    {
        Debug.Log("Start gameloop");
        yield return StartCoroutine(StartGameCoroutine());
        yield return StartCoroutine(PlayGameCoroutine());
        yield return StartCoroutine(GameOverCoroutine());
        Debug.Log("End gameloop");
    }

    private IEnumerator StartGameCoroutine()
    {
        BoardController.Instance.FillBoard();

        if (_firstPlay)
        {
            yield return StartCoroutine(UIController.Instance.ShowStartMenu());
        }

        while (!GameHasStarted)
        {
            yield return null;
        }
    }

    private IEnumerator PlayGameCoroutine()
    {
        Debug.Log("GameHasStarted");
        if (_firstPlay)
        {
            yield return new WaitForSeconds(0.25f);
            UIController.Instance.HideStartMenu();
        }

        while (!GameIsOver)
        {
            BoardController.Instance.SpawnGamePieces();
            yield return null;
        }
    }

    private IEnumerator GameOverCoroutine()
    {
        Debug.Log("GameOver");
        yield return StartCoroutine(BoardController.Instance.GameOver());

        UIController.Instance.ShowRestartMenu();
    }
    #endregion

    public void RestartGame()
    {
        _firstPlay = false;
        GameIsOver = false;
        GameHasStarted = true;
        StartGameLoop();
    }




}//class
