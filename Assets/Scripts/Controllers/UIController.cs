using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [SerializeField] private MoveUI _stackThree;
    [SerializeField] private MoveUI _clickToPlay;
    [SerializeField] private GameObject _firstPlayButton;
    [SerializeField] private GameObject _restartMenu;
    [SerializeField] private Text _currentScore;
    [SerializeField] private Text _bestScore;

    public IEnumerator ShowStartMenu()
    {
        SetScoreText();

        _stackThree.MoveIn();
        _clickToPlay.MoveIn();

        while (!_stackThree.IsReached && !_clickToPlay.IsReached)
        {
            yield return null;
        }

        EnableScoreText(true);
        EnablePlayButton(true);
    }

    public void HideStartMenu()
    {
        _stackThree.MoveOut();
        _clickToPlay.MoveOut();
    }

    public void ShowRestartMenu()
    {
        GameLoopController.Instance.GameHasStarted = false;
        _bestScore.text = _currentScore.text;
        EnableRestartMenu(true);
    }

    public void SetScoreText()
    {
        _currentScore.text = BoardController.Instance.Score.ToString();
    }

    private void EnableScoreText(bool value)
    {
        _currentScore.gameObject.SetActive(value);
    }

    private void EnablePlayButton(bool value)
    {
        _firstPlayButton.SetActive(value);
    }

    private void EnableRestartMenu(bool value)
    {
        _restartMenu.SetActive(value);
    }

    public void FirstPlayButton()
    {
        GameLoopController.Instance.GameHasStarted = true;
        EnablePlayButton(false);
    }

    public void RestartButton()
    {
        SetScoreText();
        EnableRestartMenu(false);
        GameLoopController.Instance.RestartGame();
    }





}//class
