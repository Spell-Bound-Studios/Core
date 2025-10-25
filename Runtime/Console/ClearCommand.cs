// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Core.Console {
    [ConsoleCommand("clear", "cls")]
    public class ClearCommand : ICommand {
        public string Name => "clear";
        public string Description => "Clears the console output";
        public string Usage => "clear";

        public CommandResult Execute(string[] args) {
            var console = Object.FindFirstObjectByType<ConsoleController>();

            if (console != null) {
                console.ClearOutput();

                return CommandResult.Ok();
            }

            return CommandResult.Fail("Console controller not found");
        }
    }
}