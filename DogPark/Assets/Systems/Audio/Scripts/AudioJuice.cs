using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioJuice : MonoBehaviour
{
    public float expansionFactor = 1.1f;
    private AudioSource _audioSource;

    private Vector3 _initialScale;
    private float _scaleFactor;
    private float[] _samples;
    private float _smoothVel;

    public bool useParentAudioSource = false;

    public float refValue = 0.001f;    // RMS value for 0 dB
    public int qSamples = 1024;

    public float smoothTime = 0.5f;

    public float minDb = -40f;

    public Gradient grad;

    private Renderer _renderer;

    // Start is called before the first frame update
    void Start()
    {
        if (useParentAudioSource) {
            _audioSource = GetComponentInParent<AudioSource>();
        } else {
            _audioSource = GetComponent<AudioSource>();
        }
        _initialScale = transform.localScale;
        _samples = new float[qSamples];
        _scaleFactor = 1f;
        _smoothVel = 0f;

        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float db = GetDecibelLevel();

        float f = Mathf.InverseLerp(minDb, 0f, db);
        f = Mathf.Log(1f + f)/Mathf.Log(2f);
        float target = Mathf.Lerp(1f, expansionFactor, f);
        _renderer.material.color = grad.Evaluate(f);


        _scaleFactor = Mathf.SmoothDamp(_scaleFactor, target, ref _smoothVel, smoothTime);

        transform.localScale = _initialScale*_scaleFactor;
    }

    float GetDecibelLevel()
    {
        _audioSource.GetOutputData(_samples, 0);    // fill array with samples
        float sum = 0;
        for (int i = 0; i < qSamples; i++) {
            sum += _samples[i]*_samples[i];    // sum squared samples
        }
        float rmsValue = Mathf.Sqrt(sum/qSamples);    // rms = square root of average
        float dbValue = 20 * Mathf.Log10(rmsValue/refValue);    // calculate dB
        if (dbValue < minDb)
        {
            dbValue = minDb;        // clamp it to -160 dB min
        }
        return dbValue;
     }
}
