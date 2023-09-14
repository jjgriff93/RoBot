// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Robots
{
   public class Robot
{
    public int Id { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public Guid Key { get; set; }
    public string Status { get; set; }
    public string RobotMake { get; set; }
    public string RobotModel { get; set; }
}
}
