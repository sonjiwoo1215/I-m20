using System;
using System.IO;
using UnityEngine;

public static class WavUtility_
{
    // WAV ������ AudioClip���� ��ȯ
    public static AudioClip ToAudioClip(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("������ ã�� �� �����ϴ�: " + filePath);
            return null;
        }

        byte[] wavFile = File.ReadAllBytes(filePath);
        return FromWavData(wavFile);
    }

    // WAV �����Ϳ��� AudioClip ����
    public static AudioClip FromWavData(byte[] wavFile)
    {
        // WAV ��� ���� ����
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int subchunk2 = BitConverter.ToInt32(wavFile, 40);

        // ����� ������ ����
        int startIndex = 44; // ����� ������ ���� ��ġ
        int samples = subchunk2 / 2; // 16-bit ����� ������
        float[] audioData = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, startIndex + i * 2);
            audioData[i] = sample / 32768.0f; // 16-bit �����͸� float���� ��ȯ
        }

        // AudioClip ����
        AudioClip audioClip = AudioClip.Create("wavClip", samples, channels, sampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }

    // AudioClip�� WAV �����ͷ� ��ȯ (���� �����)
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        // WAV ���� ��� ����
        const int headerSize = 44;
        int fileSize = clip.samples * clip.channels * 2 + headerSize;
        WriteHeader(stream, clip, fileSize);

        // ����� �����͸� ����Ʈ �迭�� ��ȯ
        float[] audioData = new float[clip.samples * clip.channels];
        clip.GetData(audioData, 0);
        short[] intData = new short[audioData.Length];

        // float ([-1.0f, 1.0f])�� short ([-32767, 32767])�� ��ȯ
        byte[] bytesData = new byte[audioData.Length * 2];
        int rescaleFactor = 32767; // float -> short ��ȯ ����

        for (int i = 0; i < audioData.Length; i++)
        {
            intData[i] = (short)(audioData[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        stream.Write(bytesData, 0, bytesData.Length);
        return stream.ToArray();
    }

    // WAV ��� ����
    private static void WriteHeader(Stream stream, AudioClip clip, int fileSize)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        // RIFF ���
        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        byte[] chunkSize = BitConverter.GetBytes(fileSize - 8);
        stream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        // fmt ���� ûũ
        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        ushort audioFormat = 1;
        byte[] audioFormatBytes = BitConverter.GetBytes(audioFormat);
        stream.Write(audioFormatBytes, 0, 2);

        byte[] numChannels = BitConverter.GetBytes(channels);
        stream.Write(numChannels, 0, 2);

        byte[] sampleRate = BitConverter.GetBytes(hz);
        stream.Write(sampleRate, 0, 4);

        byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        stream.Write(byteRate, 0, 4);

        ushort blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        ushort bps = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(bps);
        stream.Write(bitsPerSample, 0, 2);

        // data ���� ûũ
        byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(dataString, 0, 4);

        byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        stream.Write(subChunk2, 0, 4);
    }
}
