using UnityEngine;

public class ClearFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _fxs;

    public void Play()
    {
        foreach (ParticleSystem fx in _fxs)
        {
            fx.gameObject.SetActive(true);
            fx.Stop();
            fx.Play();
        }
    }





}//class
