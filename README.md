# Arrizer <sup><h6>(based on .NET Roslyn SDK)</h6></sup>

This is just a little experiment in using Roslyn for code generation. I was looking for something that could easily turn structures into structure of arrays. Couldn't find it.

If you're interested in writing something similar I recommend skimming through [Interesting parts](#interesting-parts), reading [Roslyn SDK Guide](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/) and most importantly **checking out the [Roslyn SDK Code Generation](https://github.com/AArnott/CodeGeneration.Roslyn)** repository.

## Interesting parts
Excerpt from `Arrizer.Generators/Generators.cs` which defines the specific code generation.
```C#
...
public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
{
    var ogStruct = (StructDeclarationSyntax)context.ProcessingNode;
    var ogMembers = ogStruct.Members;

    var newStruct = SyntaxFactory.StructDeclaration(ogStruct.Identifier.ValueText + suffix)
        .WithAttributeLists(ogStruct.AttributeLists)
        .WithTriviaFrom(ogStruct);
    List<MemberDeclarationSyntax> newMembers = new List<MemberDeclarationSyntax>();

    foreach(var ogMember in ogMembers)
    {
        if(ogMember.RawKind == (int)SyntaxKind.FieldDeclaration)
        {
            FieldDeclarationSyntax ogFieldSyntax = (FieldDeclarationSyntax)ogMember;
            VariableDeclarationSyntax ogVariableSyntax = ogFieldSyntax.Declaration;
            TypeSyntax ogType = ogVariableSyntax.Type;
            VariableDeclarationSyntax newVariableSyntax = ogVariableSyntax.WithType(ArrayType(ogType, SingletonList(ArrayRankSpecifier())));
            FieldDeclarationSyntax newFieldSyntax = ogFieldSyntax.ReplaceNode(ogVariableSyntax, newVariableSyntax);
            newMembers.Add(newFieldSyntax);
        }
        else
        {
            newMembers.Add(ogMember);
        }
    }

    newStruct = newStruct.AddMembers(newMembers.ToArray());

    var result = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(newStruct);

    return Task.FromResult(result);
}
...
```

Excerpt from `Arrizer.Attributes/Attributes.cs` which contains attribute that you can slap on your structs.
```C#
...
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
...
```

## Example usage
```C#
using System;
using System.Diagnostics;
using System.Reflection;
using CodeGeneration.Roslyn;
using Arrizer.Attributes;

namespace Arrizer
{

    [SOA] // default suffix for new type - "SOA"
    [SOA("DifferentSuffix")]
    struct Particle
    {
        public float PositionX;
        public float PositionY;
        public float Rotation;

        public int Status;
    }

    class Program
    {

        static void Main(string[] args)
        {
            foreach (var type in typeof(Program).Assembly.GetTypes())
            {
                Console.WriteLine(type.FullName);
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach(var field in fields)
                {
                    Console.WriteLine("  - " + (field.IsPublic?"public":"private") + " " + field.FieldType.Name + " " + field.Name);
                }
            }

            Particle particle = new Particle();
            ParticleSOA particleSOA = new ParticleSOA();
            ParticleDifferentSuffix particleDifferrentSuffix = new ParticleDifferentSuffix();
        }
    }
}

```
