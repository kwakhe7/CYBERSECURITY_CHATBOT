// Basic libraries for file and runtime helpers
using System;
using System.IO;
using System.Runtime.CompilerServices;

// This static helper runs when the program assembly is loaded.
// Its job is to ensure a small test "greeting.wav" exists in the
// program folder so the main app can play a short sound at startup.
static class GreetingInitializer
{
    // The ModuleInitializer attribute means this method runs automatically
    // before the application's Main method. It is used here to create the
    // WAV file early so the runtime folder contains the file when needed.
    [ModuleInitializer]
    public static void Initialize()
    {
        try
        {
            // Determine the folder where the app is running.
            string exeDir = AppContext.BaseDirectory;
            string greetingPath = Path.Combine(exeDir, "greeting.wav");

            // If the file already exists, do nothing.
            if (File.Exists(greetingPath))
                return;

            // WAV parameters for a short tone: sample rate, bit depth, channels, duration, frequency.
            int sampleRate = 22050; // how many audio samples per second
            short bitsPerSample = 16; // 16-bit audio
            short channels = 1; // mono
            double durationSeconds = 0.8; // how long the beep lasts
            double frequency = 540.0; // tone frequency in Hz

            // Compute sizes used in the WAV header
            int samples = (int)(sampleRate * durationSeconds);
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int subchunk2Size = samples * channels * bitsPerSample / 8;
            int chunkSize = 36 + subchunk2Size;

            // Create the WAV file and write headers+audio samples.
            using (var fs = new FileStream(greetingPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fs))
            {
                // RIFF header identifies the file type.
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(chunkSize);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // 'fmt ' subchunk describes audio format (PCM, channels, sample rate etc.).
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16); // PCM header size
                bw.Write((short)1); // PCM format code
                bw.Write(channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((short)blockAlign);
                bw.Write(bitsPerSample);

                // 'data' subchunk: raw audio sample bytes follow.
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(subchunk2Size);

                // Generate a sine-wave with a small fade to avoid clicks at the start/end.
                double amplitude = 0.25 * short.MaxValue;
                for (int i = 0; i < samples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = 1.0;
                    double fadeSamples = Math.Min(0.02 * sampleRate, samples / 10.0);
                    if (i < fadeSamples) env = i / fadeSamples; // fade-in
                    else if (i > samples - fadeSamples) env = (samples - i) / fadeSamples; // fade-out

                    double sample = amplitude * env * Math.Sin(2.0 * Math.PI * frequency * t);
                    short s = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, (int)sample));
                    bw.Write(s); // write 16-bit sample
                }

                bw.Flush();
            }
        }
        catch
        {
            // If anything fails we intentionally swallow errors so the app still starts.
        }
    }
}
