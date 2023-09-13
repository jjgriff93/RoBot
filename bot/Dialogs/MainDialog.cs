// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBotCLU;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Robots.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly RobotRecognizer _cluRecognizer;
        protected readonly ILogger Logger;
        private IRobotService _robotService;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(RobotRecognizer cluRecognizer, ILogger<MainDialog> logger, IRobotService robotService)
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;
            _robotService = robotService;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_cluRecognizer.IsConfigured)
            {
                throw new Exception("CLU is not configured correctly. Exiting.");
            }

            var messageText = stepContext.Options?.ToString() ?? $"Hello! What would you like me to do?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call CLU
            var cluResult = await _cluRecognizer.RecognizeAsync<RobotActions>(stepContext.Context, cancellationToken);
            switch (cluResult.GetTopIntent().intent)
            {
                case RobotActions.Intent.TurnOn: // Turn on the robot
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Turning on...", null, InputHints.IgnoringInput),
                        cancellationToken
                    );
                    //get the RobotId based on the available robots, For now it is hardcoded to available RobotId 1
                    bool isTurnedOn = await _robotService.StartSessionAsync(1);                    

                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Powered on and ready.", null, InputHints.IgnoringInput),
                        cancellationToken
                    );

                    break;
                
                case RobotActions.Intent.TurnOff: // Turn off the robot
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Turning off...", null, InputHints.IgnoringInput),
                        cancellationToken
                    );

                    bool isTurnedOff = await _robotService.StopSessionAsync(1);  

                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Robot is powered off.", null, InputHints.IgnoringInput),
                        cancellationToken
                    );

                    break;

                case RobotActions.Intent.Move:
                    // Initialize Movement action with any entities we may have found in the response.
                    var move = new Movement()
                    {
                        Object = cluResult.Entities.GetObject(),
                        //Origin = cluResult.Entities.GetOrigin(),
                        Destination = cluResult.Entities.GetDestination(),
                    };
                    // Call the robot service to move the robot, as of now the destination is not used by API
                    bool isMoved = await _robotService.MoveRobotAsync(move.Object, move.Destination);

                    var messageText = $"Moving {move.Object} to {move.Destination} with the isMoved status {isMoved}";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    break;

                case RobotActions.Intent.Help: // Provide help
                    var helpText = "I can turn on or off, or move something to a destination.";
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text(helpText, null, InputHints.IgnoringInput),
                        cancellationToken
                    );
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, did not compute. Please try asking in a different way (intent was {cluResult.GetTopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompts = new List<string> { "What else can I do?", "Anything else?", "What's your next command?", "I'm ready for my next task." };
            string promptMessage = prompts[new Random().Next(prompts.Count)];
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
