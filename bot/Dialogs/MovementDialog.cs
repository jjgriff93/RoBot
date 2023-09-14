// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class MovementDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        private IRobotService _robotService;
        private readonly RobotRecognizer _cluRecognizer;
        private readonly IStatePropertyAccessor<RobotState> _robotStateAccessor;

        public MovementDialog(UserState userState, RobotRecognizer cluRecognizer, ILogger<MainDialog> logger, IRobotService robotService)
            : base(nameof(MovementDialog))
        {
            _cluRecognizer = cluRecognizer;
            _robotStateAccessor = userState.CreateProperty<RobotState>("RobotState");
            _robotService = robotService;
            Logger = logger;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckObjectPositionStepAsync,
                SetObjectPositionStepAsync,
                MoveStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckObjectPositionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var robotState = await _robotStateAccessor.GetAsync(stepContext.Context, () => new RobotState(), cancellationToken);
            
            if (robotState.CurrentMovement == null)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            if (robotState.ObjectPosition == null)
            {
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Where is the object right now?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetObjectPositionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var robotState = await _robotStateAccessor.GetAsync(stepContext.Context, () => new RobotState(), cancellationToken);
            if (robotState.ObjectPosition == null)
            {
                var input = (string)stepContext.Result;
                var position = Movement.GetPosition(input);

                // Check that the input was a valid object position
                if (!Movement.IsValidPosition(position))
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Sorry, that's not a valid position. Say hot, mild or cold."),
                        cancellationToken
                    );
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, stepContext.Options, cancellationToken);
                }
                else
                {
                    if (position == "mild")
                    {
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text("Sorry, I don't know how to pick up objects from the mild zone yet."),
                            cancellationToken
                        );
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }

                    robotState.ObjectPosition = input;
                    await _robotStateAccessor.SetAsync(stepContext.Context, robotState, cancellationToken);
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"Position set to {robotState.ObjectPosition}."),
                        cancellationToken
                    );
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> MoveStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var robotState = await _robotStateAccessor.GetAsync(stepContext.Context, () => new RobotState(), cancellationToken);

            // Initialize Movement action with any entities we may have found in the response.
            var move = stepContext.Options as Movement;
            string moveCommand = "";

            if (!string.IsNullOrEmpty(move.Destination))
            {
                string destination = move.Destination;
                string origin = robotState.ObjectPosition;
                moveCommand = Movement.CalculateMoveCommand(origin, destination);
            }

            if (!string.IsNullOrEmpty(moveCommand))
            {
                var movingMessage = $"Okay, moving {move.Object} to {move.Destination}";
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(movingMessage, movingMessage, InputHints.IgnoringInput),
                    cancellationToken
                );
                bool isMoved = await _robotService.MultiMoveRobotAsync(robotState.Id, "1", moveCommand);
                if (!isMoved)
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Sorry, I couldn't move. Please try again."),
                        cancellationToken
                    );
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                // Update the robot state with the new object position
                robotState.ObjectPosition = move.Destination;
                await _robotStateAccessor.SetAsync(stepContext.Context, robotState, cancellationToken);
            }
            else {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Sorry, I don't know how to do that yet."),
                    cancellationToken
                );
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var messageText = $"I've finished moving {move.Object} to {move.Destination}";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
