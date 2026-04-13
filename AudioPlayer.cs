// Basic system namespaces for console I/O and audio playback
using System;
using System.Media;
using System.Threading.Tasks;

// AudioPlayer: small utility for playing WAV files and creating a simple
// test greeting WAV. Comments explain each function for beginners.
public static class AudioPlayer
{
    // PlaySync: play a WAV file and block until it's finished.
    // If anything goes wrong we fail silently so callers don't crash.
    public static void PlaySync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return; // nothing to play

            if (!System.IO.File.Exists(path))
                return; // file is missing

            using (var player = new SoundPlayer(path))
            {
                player.PlaySync(); // synchronous playback
            }
        }
        catch
        {
            // Ignore audio errors on purpose
        }
    }

    // PlayAsync: start playing a WAV file in the background and return immediately.
    // This is useful when you don't want to block the app while audio plays.
    public static void PlayAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!System.IO.File.Exists(path))
                return;

            var player = new SoundPlayer(path);
            player.Play(); // non-blocking playback
            // Note: do not dispose the player immediately; the GC will clean it later.
        }
        catch
        {
            // Ignore playback errors
        }
    }

    // EnsureTestGreeting: create a tiny WAV file with a short beep if the file
    // doesn't already exist. Returns true when the file exists or was created.
    public static bool EnsureTestGreeting(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (System.IO.File.Exists(path))
                return true; // already there

            // Parameters for a 16-bit mono WAV sound (short beep)
            int sampleRate = 22050;
            short bitsPerSample = 16;
            short channels = 1;
            double durationSeconds = 0.8;
            double frequency = 540.0;

            int samples = (int)(sampleRate * durationSeconds);
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int subchunk2Size = samples * channels * bitsPerSample / 8;
            int chunkSize = 36 + subchunk2Size;

            using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            using (var bw = new System.IO.BinaryWriter(fs))
            {
                // Write the RIFF/WAVE header
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(chunkSize);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((short)1);
                bw.Write(channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((short)blockAlign);
                bw.Write(bitsPerSample);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(subchunk2Size);

                // Generate samples for a sine wave with short fade-in/out to avoid clicks.
                double amplitude = 0.25 * short.MaxValue;
                for (int i = 0; i < samples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = 1.0;
                    double fadeSamples = Math.Min(0.02 * sampleRate, samples / 10.0);
                    if (i < fadeSamples) env = i / fadeSamples;
                    else if (i > samples - fadeSamples) env = (samples - i) / fadeSamples;

                    double sample = amplitude * env * Math.Sin(2.0 * Math.PI * frequency * t);
                    short s = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, (int)sample));
                    bw.Write(s);
                }

                bw.Flush();
            }

            return true;
        }
        catch
        {
            return false; // creation failed
        }
    }
}
