// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core.Packing {
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class FromHandlerAttribute : Attribute {
        public Type HandlerType { get; }

        public FromHandlerAttribute(Type handlerType) {
            if (!typeof(IHandlerThreshold).IsAssignableFrom(handlerType))
                throw new ArgumentException($"{handlerType.Name} must implement IHandlerThreshold");

            HandlerType = handlerType;
        }
    }
}