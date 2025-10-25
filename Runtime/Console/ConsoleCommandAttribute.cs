// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Marks a class as a console command for auto-registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ConsoleCommandAttribute : Attribute {
        public string Name { get; }
        public string[] Aliases { get; }

        /// <summary>
        /// Attribute to add to classes.
        /// </summary>
        public ConsoleCommandAttribute(string name, params string[] aliases) {
            Name = name;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }
}