using System.Collections.Generic;
using UnityEngine;

public class FXController : Singleton<FXController>
{
    [SerializeField] private GameObject _clearFXPrefab;
    [SerializeField] private List<ClearFX> _clearFXs;

    public void PlayClearFXAt(Vector2 position, int index)
    {
        _clearFXs[index].transform.position = position;
        _clearFXs[index].Play();
    }

    public void PoolClearFX(int piecesToClear)
    {
        if (_clearFXs.Count == piecesToClear)
        {
            return;
        }

        int difference = piecesToClear - _clearFXs.Count;

        for (int i = 0; i < difference; i++)
        {
            _clearFXs.Add(Instantiate(_clearFXPrefab).GetComponent<ClearFX>());
        }
    }








}//class
