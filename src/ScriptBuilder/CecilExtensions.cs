﻿namespace NServiceBus.Metrics.PerformanceCounters
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    static class CecilExtentions
    {
        public static CustomAttribute GetSingleAttribute(this FieldDefinition field, string attributeName)
        {
            return field.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == attributeName);
        }

        public static IEnumerable<TypeDefinition> AllClasses(this ModuleDefinition module)
        {
            return module.GetTypes()
                .Where(x => x.IsClass);
        }

        public static string GetFileName(this TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                var body = method.Body;
                if (body?.Instructions == null)
                {
                    continue;
                }
                foreach (var instruction in body.Instructions)
                {
                    var point = instruction.SequencePoint;
                    if (point?.Document?.Url == null)
                    {
                        continue;
                    }
                    return point.Document.Url;
                }
            }
            return null;
        }
    }
}