// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Reflection;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Utility command metadata.
    /// </summary>
    public class UtilityCommandInfo {
        public MethodInfo Method;
        public Type DeclaringClass;
        public string Description;
    }
}