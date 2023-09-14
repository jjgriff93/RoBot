// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
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
        private readonly IStatePropertyAccessor<RobotState> _robotStateAccessor;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(UserState userState, RobotRecognizer cluRecognizer, ILogger<MainDialog> logger, IRobotService robotService)
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;
            _robotService = robotService;
            _robotStateAccessor = userState.CreateProperty<RobotState>("RobotState");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));
            AddDialog(new MovementDialog(userState, cluRecognizer, logger, robotService));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_cluRecognizer.IsConfigured)
            {
                throw new Exception("CLU is not configured correctly. Exiting.");
            }

            string errorMessage = string.Empty;
            var robotState = await _robotStateAccessor.GetAsync(stepContext.Context, () => new RobotState(), cancellationToken);

            // Check for available robots    
            var availableRobots = await _robotService.GetAvailableRobotsAsync();

            // Find the robot heartbeat
            var robotDetails = availableRobots
                .Where(robot => robot.IpAddress == "localhost")
                .Select(robot=>new {robot.Id, robot.Key})
                .FirstOrDefault();
            var isRobotActive = await _robotService.GetRobotHeartbeatAsync(robotId:robotDetails.Id.ToString(),key:robotDetails.Key.ToString());
            string robotId = robotDetails.Id.ToString();
    
            if (availableRobots.Count != 0 && isRobotActive)
            {
                robotState.Id = robotId;
                await _robotStateAccessor.SetAsync(stepContext.Context, robotState, cancellationToken);

                var messageText = stepContext.Options?.ToString() ?? $"Hey! What would you like me to do?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("No Robots Available.", null, InputHints.IgnoringInput),
                    cancellationToken
                );
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var robotState = await _robotStateAccessor.GetAsync(stepContext.Context, () => new RobotState(), cancellationToken);

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
                    bool isTurnedOn = await _robotService.StartSessionAsync(robotState.Id);

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

                    bool isTurnedOff = await _robotService.StopSessionAsync(robotState.Id);

                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Robot is powered off.", null, InputHints.IgnoringInput),
                        cancellationToken
                    );

                    break;

                case RobotActions.Intent.Move:
                    // Create a movement command from the clu result
                    var movement = new Movement()
                    {
                        Object = cluResult.Entities.GetObject(),
                        Destination = cluResult.Entities.GetDestination(),
                    };

                    // Persist it to the Robot State
                    robotState.CurrentMovement = movement;
                    await _robotStateAccessor.SetAsync(stepContext.Context, robotState, cancellationToken);

                    return await stepContext.BeginDialogAsync(nameof(MovementDialog), movement, cancellationToken);

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
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "Anything else?", cancellationToken);
        }
    }
}
