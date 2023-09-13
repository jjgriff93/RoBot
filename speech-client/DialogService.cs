using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Dialog;
using ManagedBass;

namespace Microsoft.Robots.Speech
{
    public static class DialogService
    {
        public static async Task<DialogServiceConnector> InitializeConnector(string speechSubscriptionKey, AudioConfig audioConfig, string region = "eastus")
        {
            var botConfig = BotFrameworkConfig.FromSubscription(speechSubscriptionKey, region);
            botConfig.SetProperty(PropertyId.SpeechServiceConnection_RecoLanguage, "en-US");
            var connector = new DialogServiceConnector(botConfig, audioConfig);
            Bass.Init();

            var speechConfig = SpeechConfig.FromSubscription(speechSubscriptionKey, region);
            var synthesizer = new SpeechSynthesizer(speechConfig);

            // ActivityReceived is the main way your bot will communicate with the client and uses bot framework activities
            connector.ActivityReceived += async (sender, activityReceivedEventArgs) =>
            {
                Console.WriteLine($"Activity received, hasAudio={activityReceivedEventArgs.HasAudio} activity={activityReceivedEventArgs.Activity}");

                if (activityReceivedEventArgs.HasAudio)
                {
                    Console.WriteLine("Activity has audio");
                    PlayActivityAudio(activityReceivedEventArgs.Audio);
                }
            };
            // Canceled will be signaled when a turn is aborted or experiences an error condition
            connector.Canceled += (sender, canceledEventArgs) =>
            {
                Console.WriteLine($"Canceled, reason={canceledEventArgs.Reason}");
                if (canceledEventArgs.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"Error: code={canceledEventArgs.ErrorCode}, details={canceledEventArgs.ErrorDetails}");
                }
            };
            // Recognizing (not 'Recognized') will provide the intermediate recognized text while an audio stream is being processed
            connector.Recognizing += (sender, recognitionEventArgs) =>
            {
                Console.WriteLine($"Recognizing! in-progress text={recognitionEventArgs.Result.Text}");
            };
            // Recognized (not 'Recognizing') will provide the final recognized text once audio capture is completed
            connector.Recognized += (sender, recognitionEventArgs) =>
            {
                Console.WriteLine($"Final speech-to-text result: '{recognitionEventArgs.Result.Text}'");
            };
            // SessionStarted will notify when audio begins flowing to the service for a turn
            connector.SessionStarted += (sender, sessionEventArgs) =>
            {
                Console.WriteLine($"Now Listening! Session started, id={sessionEventArgs.SessionId}");
            };
            // SessionStopped will notify when a turn is complete and it's safe to begin listening again
            connector.SessionStopped += (sender, sessionEventArgs) =>
            {
                Console.WriteLine($"Listening complete. Session ended, id={sessionEventArgs.SessionId}");
            };

            await connector.ConnectAsync();
            return connector;
        }

        private static void PlayActivityAudio(PullAudioOutputStream activityAudio)
        {
            var playbackStreamWithHeader = new MemoryStream();
            playbackStreamWithHeader.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4); // ChunkID
            playbackStreamWithHeader.Write(BitConverter.GetBytes(UInt32.MaxValue), 0, 4); // ChunkSize: max
            playbackStreamWithHeader.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4); // Format
            playbackStreamWithHeader.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4); // Subchunk1ID
            playbackStreamWithHeader.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size: PCM
            playbackStreamWithHeader.Write(BitConverter.GetBytes(1), 0, 2); // AudioFormat: PCM
            playbackStreamWithHeader.Write(BitConverter.GetBytes(1), 0, 2); // NumChannels: mono
            playbackStreamWithHeader.Write(BitConverter.GetBytes(16000), 0, 4); // SampleRate: 16kHz
            playbackStreamWithHeader.Write(BitConverter.GetBytes(32000), 0, 4); // ByteRate
            playbackStreamWithHeader.Write(BitConverter.GetBytes(2), 0, 2); // BlockAlign
            playbackStreamWithHeader.Write(BitConverter.GetBytes(16), 0, 2); // BitsPerSample: 16-bit
            playbackStreamWithHeader.Write(Encoding.ASCII.GetBytes("data"), 0, 4); // Subchunk2ID
            playbackStreamWithHeader.Write(BitConverter.GetBytes(UInt32.MaxValue), 0, 4); // Subchunk2Size

            byte[] pullBuffer = new byte[2056];

            uint lastRead = 0;
            do
            {
                lastRead = activityAudio.Read(pullBuffer);
                playbackStreamWithHeader.Write(pullBuffer, 0, (int)lastRead);
            }
            while (lastRead == pullBuffer.Length);

            // Play the Stream
            // Convert MemoryStream to byte array
            byte[] audioData = playbackStreamWithHeader.ToArray();

            // Create a new Bass Stream
            int stream = Bass.CreateStream(audioData, 0, audioData.Length, BassFlags.Default);

            if (stream != 0)
            {
                var result = Bass.ChannelPlay(stream); // Play the stream
                Console.WriteLine($"Bass.ChannelPlay result: {result}");
            }
            // Error creating the stream
            else
            {
                Console.WriteLine("Error creating the stream: {0}!", Bass.LastError);
            }
        }
    }
}
