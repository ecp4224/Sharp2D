using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NVorbis;
using OpenTK.Audio.OpenAL;
using Sharp2D;

public static class MusicPlayer
{
    private static int _source;
    private static int[] _buffers;
    private const int NUM_BUFFERS = 8;
    private const int BUFFER_SAMPLES = 176400;
    
    private static VorbisReader _decoder;
    private static bool _streaming;
    private static Thread _streamThread;
    
    private static ALDevice _device;
    private static ALContext _context;

    private class ALException : Exception
    {
        public ALException(string message) : base(message)
        {
        }

        public ALException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    private static void InitializeOpenAL()
    {
        if (_device != ALDevice.Null && _context != ALContext.Null)
            return;
        
        // Open the default device
        _device = ALC.OpenDevice(null);
        if (_device == ALDevice.Null)
            throw new Exception("Failed to open the default audio device.");

        // Create an audio context with default attributes.
        _context = ALC.CreateContext(_device, (int[])null);
        if (_context == ALContext.Null)
            throw new Exception("Failed to create OpenAL context.");

        // Make the context current so that subsequent AL calls work.
        ALC.MakeContextCurrent(_context);
    }
    
    public static void Play(string filePath)
    {
        if (!Screen.IsInOpenGLThread())
        {
            Screen.Invoke(() => Play(filePath));
            return;
        }

        // Ensure we initialize
        InitializeOpenAL();
        
        // Free previous audio data
        Stop();

        // Load .ogg data using StbVorbisSharp (or any wrapper you prefer)
        using var stream = File.OpenRead(filePath);
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        
        _decoder = new VorbisReader(ms, false);
        _buffers = new int[NUM_BUFFERS];
        AL.GenBuffers(NUM_BUFFERS, _buffers);
        CheckError("GenBuffers");
        
        for (int i = 0; i < NUM_BUFFERS; i++)
        {
            if (!FillBuffer(_buffers[i]))
                break;
        }
        
        ALFormat format;
        if (_decoder.Channels == 1)
            format = ALFormat.Mono16;
        else if (_decoder.Channels == 2)
            format = ALFormat.Stereo16;
        else
            throw new NotSupportedException("Unsupported channel count: " + _decoder.Channels);

        // Create source and queue the buffers.
        AL.GenSource(out _source);
        CheckError("GenSource");
        AL.SourceQueueBuffers(_source, NUM_BUFFERS, _buffers);
        CheckError("QueueBuffers");
        
        // We manage looping ourselves.
        AL.Source(_source, ALSourceb.Looping, false);
        CheckError("Set Looping");
        
        AL.SourcePlay(_source);
        CheckError("SourcePlay");

        // Start the streaming update thread.
        _streaming = true;
        _streamThread = new Thread(StreamUpdateLoop);
        _streamThread.IsBackground = true;
        _streamThread.Start();
    }
    
    private static void StreamUpdateLoop()
    {
        while (_streaming)
        {
            int processed;
            AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out processed);
            CheckError("Get BuffersProcessed");

            while (processed-- > 0)
            {
                int buffer = 0;
                AL.SourceUnqueueBuffers(_source, 1, ref buffer);
                CheckError("UnqueueBuffer");

                if (!FillBuffer(buffer))
                {
                    // End of stream reached; reset the decoder to loop.
                    ResetDecoder();
                    // Try filling again after resetting.
                    if (!FillBuffer(buffer))
                    {
                        // If still no data, break out.
                        _streaming = false;
                        break;
                    }
                }
                AL.SourceQueueBuffers(_source, 1, ref buffer);
                CheckError("RequeueBuffer");
            }

            // If the source has stopped, restart it.
            int state;
            AL.GetSource(_source, ALGetSourcei.SourceState, out state);
            if ((ALSourceState)state != ALSourceState.Playing)
            {
                AL.SourcePlay(_source);
                CheckError("Restart SourcePlay");
            }

            Thread.Yield();
        }
    }
    
    private static void ResetDecoder()
    {
        _decoder.SeekTo(0);
    }
    
    // Fills a given AL buffer with up to BUFFER_SAMPLES samples.
    // Returns false if no samples were decoded (end of stream).
    private static bool FillBuffer(int buffer)
    {
        List<short> sampleList = new List<short>();
        int channels = _decoder.Channels;
        // We'll use a float buffer to hold the raw samples.
        float[] floatBuffer = new float[BUFFER_SAMPLES];

        while (sampleList.Count < BUFFER_SAMPLES)
        {
            // ReadSamples returns the number of floats read.
            int samplesRead = _decoder.ReadSamples(floatBuffer, 0, floatBuffer.Length);
            if (samplesRead == 0)
            {
                // End of stream reached; reset the decoder to loop.
                ResetDecoder();
                continue; // try again to fill the buffer.
            }

            // Convert the float samples to 16-bit PCM.
            for (int i = 0; i < samplesRead; i++)
            {
                // Clamp the sample value and convert.
                float sample = floatBuffer[i];
                short s = (short)Math.Clamp(sample * 32767f, short.MinValue, short.MaxValue);
                sampleList.Add(s);
            }
        }

        if (sampleList.Count == 0)
            return false;


        if (sampleList.Count == 0)
        {
            // End of song.
            return false;
        }

        short[] pcm = sampleList.ToArray();
        int sizeInBytes = pcm.Length * sizeof(short);

        // Determine the AL format.
        ALFormat format;
        if (_decoder.Channels == 1)
            format = ALFormat.Mono16;
        else if (_decoder.Channels == 2)
            format = ALFormat.Stereo16;
        else
            throw new NotSupportedException("Unsupported channel count: " + _decoder.Channels);

        // Pin the PCM array and fill the buffer.
        GCHandle handle = GCHandle.Alloc(pcm, GCHandleType.Pinned);
        try
        {
            AL.BufferData(buffer, format, handle.AddrOfPinnedObject(), sizeInBytes, _decoder.SampleRate);
            CheckError("BufferData");
        }
        finally
        {
            handle.Free();
        }
        return true;
    }

    private static void CheckError(string context)
    {
        var error = AL.GetError();
        if (error != ALError.NoError)
        {
            var errStr = AL.GetErrorString(error) ?? error.ToString();
            Logger.Error($"OpenAL Error: {errStr} when doing {context}");
            throw new ALException(errStr);
        }
    }

    public static void Stop()
    {
        _streaming = false;
        _streamThread?.Join();

        if (_source != 0)
        {
            AL.SourceStop(_source);
            CheckError("SourceStop");
            AL.DeleteSource(_source);
            CheckError("DeleteSource");
            _source = 0;
        }
        if (_buffers != null)
        {
            AL.DeleteBuffers(_buffers.Length, _buffers);
            CheckError("DeleteBuffers");
            _buffers = null;
        }
    }
}