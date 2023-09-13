using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Dialog;

namespace Microsoft.Robots.Speech {
    class Program
    {
        private static DialogServiceConnector dialogConnector;

        async static Task Listen()
        {
            Console.WriteLine("Listening...");
            await dialogConnector.ListenOnceAsync();
        }

        async static Task Main(string[] args)
        {
            var speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
            if (string.IsNullOrEmpty(speechKey))
            {
                Console.WriteLine("Missing SPEECH_KEY environment variable.");
                return;
            }

            // Use default microphone for audio input
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            // Initialise the dialog service connector to connect to the bot
            dialogConnector = await DialogService.InitializeConnector(speechKey, audioConfig);

            Console.WriteLine("Press Enter then say your command (or Esc to quit)...");

            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
            {

                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        await Listen();
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            }
        }
    }
}
