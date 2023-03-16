using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : Singleton<BoardController>
{
    #region Board Properties
    public const int Width = 5;
    public const int Height = 7;

    public float MinBoundsX { get; } = 0;
    public float MaxBoundsX { get; } = Width - 1;
    [field: SerializeField] public float MaxBoundsY { get; private set; }

    private readonly Cell[,] _cells = new Cell[Width, Height];
    private readonly GamePiece[,] _gamePieces = new GamePiece[Width, Height];
    #endregion

    #region
    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _insidePieces;
    [SerializeField] private GameObject _outsidePieces;

    [Header("Prefabs")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject[] _gamePiecePrefabs;

    [Header("Spawner")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _triggerPoint;
    [SerializeField] private float _timeToSpawn;
    private float _timer = 0;

    [SerializeField] private float _timeToMove;
    [SerializeField] private float _snapSpeed = 50f;
    [SerializeField] private float _collapseSpeed = 10f;

    [Header("Score")]
    [SerializeField] private int _pointsPerPiece;
    public int Score { get; private set; } = 0;

    [Header("Colors")]
    [SerializeField] private Color _blueColor;
    [SerializeField] private Color _greenColor;
    [SerializeField] private Color _orangeColor;
    [SerializeField] private Color _purpleColor;
    [SerializeField] private Color _yellowColor;

    private int _lastColumn = -1;
    private bool _highlighted = false;
    #endregion

    #region Create Board
    public void CreateBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 position = new Vector2(x, y);
                Cell newCell = Instantiate(_cellPrefab, position, Quaternion.identity).GetComponent<Cell>();
                newCell.GetChildSpriteRenderer();
                newCell.transform.SetParent(transform);
                _cells[x, y] = newCell;
            }
        }
    }
    #endregion

    #region Fill Board
    public void FillBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 1; y >= Height - 2; y--)
            {
                FillBoardAt(x, y);
            }
        }
    }

    private void FillBoardAt(int x, int y)
    {
        Vector2 position = new Vector2(x, y);
        GamePiece newPiece = GenerateRandomPiece(position);
        _gamePieces[x, y] = newPiece;

        if (x >= 2)
        {
            while (HasMatchAt(x, y))
            {
                Destroy(newPiece.gameObject);
                newPiece = GenerateRandomPiece(position);
            }
        }

        newPiece.InitWhileFilling(x, y, _insidePieces.transform);
    }

    private GamePiece GenerateRandomPiece(Vector2 position)
    {
        int randomIndex = Random.Range(0, _gamePiecePrefabs.Length);
        GamePiece newPiece = Instantiate(_gamePiecePrefabs[randomIndex], position, Quaternion.identity).GetComponent<GamePiece>();
        newPiece.GetCollider();
        return newPiece;
    }

    private bool HasMatchAt(int x, int y, int piecesToMatch = 3)
    {
        List<GamePiece> matches = FindMatches(x, y, Vector2.left, piecesToMatch);
        return matches.Count >= piecesToMatch;
    }
    #endregion

    #region Spawn Every timeToSpawn Seconds
    public void SpawnGamePieces()
    {
        if (_timer <= 0f)
        {
            GamePiece newPiece = GenerateRandomPiece(_spawnPoint.position);
            newPiece.transform.SetParent(_outsidePieces.transform);
            newPiece.Move(_triggerPoint.position, _timeToMove);
            _timer = _timeToSpawn;
        }
        else
        {
            _timer -= Time.deltaTime;
        }
    }
    #endregion

    #region Snap To Grid
    public void SnapToGrid(GamePiece gamePiece)
    {
        StartCoroutine(SnapToGridCoroutine(gamePiece));
    }

    private IEnumerator SnapToGridCoroutine(GamePiece gamePiece)
    {
        Vector2 gamePiecePosition = gamePiece.transform.position;

        int column = GetColumn(gamePiecePosition.x);

        if (_gamePieces[column, 0] != null)
        {
            gamePiece.transform.position = new Vector2(column, MaxBoundsY);
            gamePiece.transform.SetParent(_insidePieces.transform);
            GameLoopController.Instance.GameIsOver = true;
            yield break;
        }

        float snapDuration = 0f;
        for (int i = Height - 1; i >= 0; i--)
        {
            if (_gamePieces[column, i] == null)
            {
                _gamePieces[column, i] = gamePiece;
                gamePiece.transform.SetParent(_insidePieces.transform);
                gamePiece.SetCoordinates(column, i);

                Vector2 destination = new Vector2(column, i);
                float distance = Vector2.Distance(gamePiecePosition, destination);
                snapDuration = distance / _snapSpeed;

                gamePiece.Move(destination, snapDuration);
                break;
            }
        }

        yield return new WaitForSeconds(snapDuration);

        List<GamePiece> finalMatches = FindFinalMatches(gamePiece.XCoordinate, gamePiece.YCoordinate);

        if (finalMatches.Count == 0)
        {
            yield break;
        }

        ClearAndMoveUpward(finalMatches);
    }
    #endregion

    private int GetColumn(float positionX)
    {
        int column = 0;

        if (positionX < 0.5f)
        {
            column = 0;
        }
        else if (positionX >= 0.5f && positionX < 1.5f)
        {
            column = 1;
        }
        else if (positionX >= 1.5f && positionX < 2.5f)
        {
            column = 2;
        }
        else if (positionX >= 2.5f && positionX < 3.5f)
        {
            column = 3;
        }
        else if (positionX >= 3.5f && positionX < 5f)
        {
            column = 4;
        }
        else if (positionX >= 5f)
        {
            column = Random.Range(0, Width);
        }

        return column;
    }

    #region Clear And Move Upward
    private void ClearAndMoveUpward(List<GamePiece> finalMatches)
    {
        StartCoroutine(ClearAndMoveUpwardCoroutine(finalMatches));
    }

    private IEnumerator ClearAndMoveUpwardCoroutine(List<GamePiece> finalMatches)
    {
        List<int> columnsToMove = GetColumnsToMove(finalMatches);
        ClearMatches(finalMatches);
        UpdateScore(finalMatches.Count);
        yield return null;

        List<GamePiece> movingPieces = MovePiecesUpward(columnsToMove);

        while (!IsMovingUpward(movingPieces))
        {
            yield return null;
        }

        List<GamePiece> movingMatches = new List<GamePiece>();
        foreach (GamePiece gamePiece in movingPieces)
        {
            List<GamePiece> matches = FindFinalMatches(gamePiece.XCoordinate, gamePiece.YCoordinate);
            movingMatches = movingMatches.Union(matches).ToList();
        }

        if (movingMatches.Count == 0)
        {
            yield break;
        }

        ClearAndMoveUpward(movingMatches);
    }
    #endregion

    #region Find Matches
    private List<GamePiece> FindFinalMatches(int x, int y)
    {
        List<GamePiece> combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count >= 2)
        {
            List<GamePiece> finalMatches = FindExtraPieces(combinedMatches);

            if (finalMatches.Count >= 3)
            {
                return finalMatches;
            }
        }

        return Enumerable.Empty<GamePiece>().ToList();
    }

    private List<GamePiece> FindMatchesAt(int x, int y)
    {
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y);
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y);

        if (verticalMatches.Count > 0 || horizontalMatches.Count > 0)
        {
            List<GamePiece> combinedMatches = verticalMatches.Union(horizontalMatches).ToList();
            return combinedMatches;
        }

        return Enumerable.Empty<GamePiece>().ToList();
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

        if (upwardMatches.Count > 0 || downwardMatches.Count > 0)
        {
            List<GamePiece> verticalMatches = upwardMatches.Union(downwardMatches).ToList();
            return verticalMatches;
        }

        return Enumerable.Empty<GamePiece>().ToList();
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY)
    {
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2);
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2);

        if (leftMatches.Count > 0 || rightMatches.Count > 0)
        {
            List<GamePiece> horizontalMatches = leftMatches.Union(rightMatches).ToList();
            return horizontalMatches;
        }

        return Enumerable.Empty<GamePiece>().ToList();
    }

    public List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int piecesToMatch)
    {
        List<GamePiece> matches = new List<GamePiece>();

        GamePiece startPiece = _gamePieces[startX, startY];
        matches.Add(startPiece);

        int nextX;
        int nextY;

        for (int i = 1; i < Height; i++)
        {
            nextX = startX + (int)searchDirection.x * i;
            nextY = startY + (int)searchDirection.y * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = _gamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break;
            }

            if (startPiece.MatchingColor == nextPiece.MatchingColor)
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        if (matches.Count >= piecesToMatch)
        {
            return matches;
        }

        return Enumerable.Empty<GamePiece>().ToList();
    }

    private List<GamePiece> FindExtraPieces(List<GamePiece> combinedMatches)
    {
        List<GamePiece> finalMatches = new List<GamePiece>();

        foreach (GamePiece gamePiece in combinedMatches)
        {
            List<GamePiece> extraPieces = FindMatchesAt(gamePiece.XCoordinate, gamePiece.YCoordinate);
            finalMatches = finalMatches.Union(extraPieces).ToList();
        }

        return finalMatches;
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < Width && y >= 0 && y < Height);
    }
    #endregion

    #region Clear Matches
    private void ClearMatches(List<GamePiece> matches)
    {
        int piecesToClear = matches.Count;
        FXController.Instance.PoolClearFX(piecesToClear);

        for (int i = 0; i < matches.Count; i++)
        {
            ClearMatchedPiece(matches[i]);
            FXController.Instance.PlayClearFXAt(matches[i].transform.position, i);
        }
    }

    private void ClearMatchedPiece(GamePiece gamePiece)
    {
        _gamePieces[gamePiece.XCoordinate, gamePiece.YCoordinate] = null;
        Destroy(gamePiece.gameObject);
    }
    #endregion

    #region Move Pieces Upward
    private List<int> GetColumnsToMove(List<GamePiece> matches)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece gamePiece in matches)
        {
            if (!columns.Contains(gamePiece.XCoordinate))
            {
                columns.Add(gamePiece.XCoordinate);
            }
        }

        return columns;
    }

    private List<GamePiece> MovePiecesUpward(List<int> columnsToMove)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        foreach (int column in columnsToMove)
        {
            movingPieces = movingPieces.Union(MoveUpwardPiecesAtColumn(column)).ToList();
        }

        return movingPieces;
    }

    private List<GamePiece> MoveUpwardPiecesAtColumn(int column)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = Height - 1; i > 0; i--)
        {
            if (_gamePieces[column, i] == null)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (_gamePieces[column, j] != null)
                    {
                        _gamePieces[column, i] = _gamePieces[column, j];
                        _gamePieces[column, i].SetCoordinates(column, i);
                        _gamePieces[column, j] = null;
                        _gamePieces[column, i].Move(new Vector2(column, i), (i - j) / _collapseSpeed);

                        if (!movingPieces.Contains(_gamePieces[column, i]))
                        {
                            movingPieces.Add(_gamePieces[column, i]);
                        }

                        break;
                    }
                }
            }
        }

        return movingPieces;
    }

    private bool IsMovingUpward(List<GamePiece> movingPieces)
    {
        int count = 0;

        foreach (GamePiece gamePiece in movingPieces)
        {
            if (gamePiece.IsReached)
            {
                count++;
            }
        }

        if (count == movingPieces.Count)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Handle Mouse Drag
    public void OnDrag(GamePiece gamePiece)
    {
        Vector2 mousePosition = GetMousePosition();
        HandleMouseDrag(mousePosition, gamePiece.transform);
        HighLightColumn(gamePiece.transform.position.x, gamePiece.MatchingColor);
    }

    private Vector2 GetMousePosition()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return mousePosition;
    }

    private void HandleMouseDrag(Vector2 mousePosition, Transform gamePieceTransform)
    {
        if (IsWithinBoundsX(mousePosition.x))
        {
            if (IsWithinBoundsY(mousePosition.y))
            {
                gamePieceTransform.position = mousePosition;
            }
            else
            {
                gamePieceTransform.position = new Vector2(mousePosition.x, MaxBoundsY);
            }
        }
        else //outOfBoundsX
        {
            if (IsWithinBoundsY(mousePosition.y))
            {
                if (mousePosition.x > MaxBoundsX)
                {
                    gamePieceTransform.position = new Vector2(MaxBoundsX, mousePosition.y);
                }
                else if (mousePosition.x < MinBoundsX)
                {
                    gamePieceTransform.position = new Vector2(MinBoundsX, mousePosition.y);
                }
            }
            else
            {
                if (mousePosition.x > MaxBoundsX)
                {
                    gamePieceTransform.position = new Vector2(MaxBoundsX, MaxBoundsY);
                }
                else if (mousePosition.x < MinBoundsX)
                {
                    gamePieceTransform.position = new Vector2(MinBoundsX, MaxBoundsY);
                }
            }
        }
    }

    private bool IsWithinBoundsX(float x)
    {
        return x >= MinBoundsX && x <= MaxBoundsX;
    }

    private bool IsWithinBoundsY(float y)
    {
        return y <= MaxBoundsY;
    }
    #endregion

    #region Highlight & Unhighlight Column
    private void HighLightColumn(float positionX, MatchingColor colorValue)
    {
        int currentColumn = GetColumn(positionX);

        if (currentColumn == _lastColumn)
        {
            return;
        }

        if (_lastColumn != -1)
        {
            UnhighlightColumn(_lastColumn);
        }

        _lastColumn = currentColumn;

        Color highlightColor = GetColor(colorValue);

        for (int i = 0; i < Height; i++)
        {
            _cells[currentColumn, i].Highlight(highlightColor);
        }

        _highlighted = true;
    }

    private Color GetColor(MatchingColor colorValue)
    {
        Color color = new Color();

        switch (colorValue)
        {
            case MatchingColor.Blue:
                color = _blueColor;
                break;
            case MatchingColor.Green:
                color = _greenColor;
                break;
            case MatchingColor.Orange:
                color = _orangeColor;
                break;
            case MatchingColor.Purple:
                color = _purpleColor;
                break;
            case MatchingColor.Yellow:
                color = _yellowColor;
                break;
        }

        return color;
    }

    private void UnhighlightColumn(int column)
    {
        for (int i = 0; i < Height; i++)
        {
            _cells[column, i].Unhighlight();
        }

        _highlighted = false;
    }

    private void UnhighlightColumn()
    {
        if (!_highlighted)
        {
            return;
        }

        UnhighlightColumn(_lastColumn);
    }
    #endregion

    #region Handle Mouse Released
    public void OnMouseReleased(GamePiece gamePiece)
    {
        UnhighlightColumn();
        SnapToGrid(gamePiece);
    }
    #endregion

    #region Score
    private void UpdateScore(int matchedPieces)
    {
        Score += matchedPieces * _pointsPerPiece;
        UIController.Instance.SetScoreText();
    }
    #endregion

    #region Gameover
    private void ClearRemainingPieces(GameObject parentPiece)
    {
        int children = parentPiece.transform.childCount;

        for (int i = 0; i < children; i++)
        {
            Transform childPiece = parentPiece.transform.GetChild(i);
            Destroy(childPiece.gameObject);
        }
    }

    public IEnumerator GameOver()
    {
        _timer = 0;
        Score = 0;
        UnhighlightColumn();
        ClearRemainingPieces(_outsidePieces);
        yield return new WaitForSeconds(1f);
        ClearRemainingPieces(_insidePieces);
    }
    #endregion






}//class
