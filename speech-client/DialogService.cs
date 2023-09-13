using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Dialog;

namespace Microsoft.Robots.Speech
{
    public static class DialogService
    {
        public static async Task<DialogServiceConnector> InitializeConnector(string speechSubscriptionKey, AudioConfig audioConfig, string region = "eastus")
        {
            var botConfig = BotFrameworkConfig.FromSubscription(speechSubscriptionKey, region);
            botConfig.SetProperty(PropertyId.SpeechServiceConnection_RecoLanguage, "en-US");
            var connector = new DialogServiceConnector(botConfig, audioConfig);

            // ActivityReceived is the main way your bot will communicate with the client and uses bot framework activities
            connector.ActivityReceived += (sender, activityReceivedEventArgs) =>
            {
                Console.WriteLine($"Activity received, hasAudio={activityReceivedEventArgs.HasAudio} activity={activityReceivedEventArgs.Activity}");

                if (activityReceivedEventArgs.HasAudio)
                {
                    Console.WriteLine("Activity has audio");
                    // TODO: play audio
                    //SynchronouslyPlayActivityAudio(activityReceivedEventArgs.Audio);
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
    }
}
