using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Arrizer.Generators
{
    public class SOAGenerator : ICodeGenerator
    {
        private readonly string suffix;

        public SOAGenerator(AttributeData attributeData)
        {
            suffix = (string)attributeData.ConstructorArguments.ElementAt(0).Value;
        }

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
    }
}
