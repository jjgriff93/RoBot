// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Robots
{
    public class Movement
    {
        public string Object { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public static List<string> Positions = new List<string> { "hot", "mild", "cold" };

        public static string GetPosition(string position)
        {
            if (position.ToLower().Contains("hot"))
            {
                return "hot";
            }
            else if (position.ToLower().Contains("mild"))
            {
                return "mild";
            }
            else if (position.ToLower().Contains("cold"))
            {
                return "cold";
            }
            else
            {
                return null;
            }
        }

        public static bool IsValidPosition(string position)
        {
            return Positions.Contains(position);
        }

        public static string CalculateMoveCommand(string origin, string destination)
        {
            var originPosition = GetPosition(origin);
            var destinationPosition = GetPosition(destination);

            if (originPosition == "hot" && destinationPosition == "cold")
            {
                return "HotToCold";
            }
            else if (originPosition == "cold" &&  destinationPosition == "hot")
            {
                return "ColdToHot";
            }
            else
            {
                return null;
            }
        }
    }
}
