using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemPool : MonoBehaviour
{
    public static ParticleSystemPool Instance;

    [Header("PS Pool")]
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private int poolSize = 50;

    private List<ParticleSystem> particleSystems;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreatePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += OnGameReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnGameReset;
    }

    private void OnGameReset()
    {
        //clear particles
        foreach (ParticleSystem p in particleSystems)
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // fully reset
        }
    }

    private void CreatePool()
    {
        particleSystems = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem ps = Instantiate(destroyParticles, transform);
            // Force warm-up
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Simulate(0.01f, true, true);  // simulate a tiny step to initialize
            ps.Clear(); // ensure no stray particles
            ps.gameObject.SetActive(false); // optional: deactivate until used
            particleSystems.Add(ps);
        }
    }

    private ParticleSystem GetAvailableSource()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.particleCount < 1)
                return ps;
        }

        // Fallback: reuse the first one (or expand pool if needed)
        return particleSystems[0];
    }

    public void PlayParticleSystem(Vector3 position)
    {
        ParticleSystem ps = GetAvailableSource();
        ps.transform.position = position;
        ps.gameObject.SetActive (true);

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // fully reset
        ps.Play();
        StartCoroutine(DisableParticle(ps));
    }

    private IEnumerator DisableParticle(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.duration);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // fully reset
        ps.gameObject.SetActive(false);
    }
}
