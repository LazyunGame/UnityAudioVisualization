using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    public AudioSource audioSource;

    public AnimationCurve FrequencyDistributionCurve;
    float[] frequencyDistribution = new float[512];

    [HideInInspector] public float[] AudioBand, AudioBandBuffer, stereoBandSpread, stereoBandBufferSpread;
    [HideInInspector] public float Amplitude, AmplitudeBuffer;

    private float[] samplesLeft = new float[512];
    private float[] samplesRight = new float[512];

    private float[] frequencyBand;
    private float[] bandBuffer;
    private float[] bufferDecrease;
    private float[] freqBandHighest;

    private float amplitudeHighest;

    [Range(0, 512)] public int FrequencyBandCount = 8;
    public float AudioProfile;

    public enum Channel
    {
        Stereo,
        Left,
        Right
    }

    public enum FFTW
    {
        Rectangular,
        Triangle,
        Hamming,
        Hanning,
        Blackman,
        BlackmanHarris
    }

    public Channel channel = new Channel();
    public FFTW FrequencyReadMethod = new FFTW();
    private FFTWindow fftw;

    void Start()
    {
        frequencyBand = new float[512];
        bandBuffer = new float[512];
        bufferDecrease = new float[512];
        freqBandHighest = new float[512];
        AudioBand = new float[512];
        AudioBandBuffer = new float[512];
        stereoBandSpread = new float[512];

        switch (FrequencyReadMethod)
        {
            case FFTW.Blackman:
                fftw = FFTWindow.Blackman;
                break;
            case FFTW.BlackmanHarris:
                fftw = FFTWindow.BlackmanHarris;
                break;
            case FFTW.Hamming:
                fftw = FFTWindow.Hamming;
                break;
            case FFTW.Hanning:
                fftw = FFTWindow.Hanning;
                break;
            case FFTW.Rectangular:
                fftw = FFTWindow.Rectangular;
                break;
            case FFTW.Triangle:
                fftw = FFTWindow.Triangle;
                break;
            default:
                fftw = FFTWindow.Blackman;
                break;
        }

        GetFrequencyDistribution();
        MakeAudioProfile(AudioProfile);
    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            GetSpectrumAudioSource();
            MakeFrequencyBands();
            BandBuffer();
            CreateAudioBands();
            GetAmplitude();
            
            Shader.SetGlobalFloat("_BandCount", FrequencyBandCount);
            Shader.SetGlobalFloatArray("_FrequencyBand", frequencyBand);

        }
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samplesLeft, 0, fftw);
        audioSource.GetSpectrumData(samplesRight, 1, fftw);
    }

    void GetFrequencyDistribution()
    {
        for (int i = 0; i < FrequencyBandCount; i++)
        {
            var eval = Mathf.RoundToInt(((float) i) / FrequencyBandCount);
            frequencyDistribution[i] = FrequencyDistributionCurve.Evaluate(eval);
        }
    }

    void MakeFrequencyBands()
    {
        int band = 0;
        float average = 0;
        float stereoLeft = 0;
        float stereoRight = 0;

        for (int i = 0; i < FrequencyBandCount; i++)
        {
            var sample = (float) i;
            var current = FrequencyDistributionCurve.Evaluate(Mathf.RoundToInt(sample / 512));

            stereoLeft += (samplesLeft[i]) * (i + 1);
            stereoRight += (samplesRight[i]) * (i + 1);

            if (channel == Channel.Stereo)
                average += stereoLeft + stereoRight;
            else if (channel == Channel.Left)
                average += stereoLeft;
            else if (channel == Channel.Right)
                average += stereoRight;

            if (current == frequencyDistribution[band])
            {
                if (i != 0)
                {
                    average /= i;
                    stereoLeft /= i;
                    stereoRight /= i;
                }

                frequencyBand[band] = average * 10;
                stereoBandSpread[band] = stereoLeft - stereoRight;
                band++;
            }
        }
    }

    void BandBuffer()
    {
        for (int i = 0; i < FrequencyBandCount; i++)
        {
            if (frequencyBand[i] > bandBuffer[i])
            {
                bandBuffer[i] = frequencyBand[i];
                bufferDecrease[i] = 0.005f;
            }

            if (frequencyBand[i] < bandBuffer[i])
            {
                bandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.1f;
            }
        }
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < FrequencyBandCount; i++)
        {
            if (frequencyBand[i] > freqBandHighest[i])
            {
                freqBandHighest[i] = frequencyBand[i];
            }

            AudioBand[i] = Mathf.Clamp01(frequencyBand[i] / freqBandHighest[i]);
            AudioBandBuffer[i] = Mathf.Clamp01(bandBuffer[i] / freqBandHighest[i]);
        }
    }

    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;

        for (int i = 0; i < FrequencyBandCount; i++)
        {
            currentAmplitude += AudioBand[i];
            currentAmplitudeBuffer += AudioBandBuffer[i];
        }

        if (currentAmplitude > amplitudeHighest)
        {
            amplitudeHighest = currentAmplitude;
        }

        Amplitude = currentAmplitude / amplitudeHighest;
        AmplitudeBuffer = currentAmplitudeBuffer / amplitudeHighest;
    }

    void MakeAudioProfile(float audioProfile)
    {
        for (int i = 0; i < FrequencyBandCount; i++)
        {
            freqBandHighest[i] = audioProfile;
        }
    }
}