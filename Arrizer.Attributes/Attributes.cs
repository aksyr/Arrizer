using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace Arrizer.Attributes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    [CodeGenerationAttribute("Arrizer.Generators.SOAGenerator, Arrizer.Generators")]
    [Conditional("CodeGeneration")]
    public class SOAAttribute : Attribute
    {
        public SOAAttribute(string suffix = "SOA")
        {
            Suffix = suffix;
        }

        public string Suffix { get; }
    }
}
