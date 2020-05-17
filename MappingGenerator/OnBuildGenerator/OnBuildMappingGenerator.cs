using MappingGenerator.Mappings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SmartCodeGenerator.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MappingGenerator.OnBuildGenerator
{
    [CodeGenerator(typeof(MappingInterface))]
    public class OnBuildMappingGenerator : ICodeGenerator
    {
        private const string GeneratorName = "MappingGenerator.OnBuildMappingGenerator";
        private readonly MappingImplementorEngine ImplementorEngine = new MappingImplementorEngine();

        public Task<GenerationResult> GenerateAsync(CSharpSyntaxNode processedNode, AttributeData markerAttribute, TransformationContext context, CancellationToken cancellationToken)
        {
            var usings = new List<string>
            {
                "System",
                "System.Linq"
            };

            var typeMappers = new List<INamedTypeSymbol>();
            if (!markerAttribute.ConstructorArguments.IsEmpty)
            {
                foreach (var typeMappingClass in markerAttribute.ConstructorArguments[0].Values)
                {
                    var typeMappingTypeSymbol = (INamedTypeSymbol)typeMappingClass.Value;

                    if (!typeMappingTypeSymbol.IsStatic)
                    {
                        continue;
                    }

                    usings.Add(typeMappingTypeSymbol.ContainingNamespace.ToDisplayString());
                    typeMappers.Add(typeMappingTypeSymbol);
                }
            }

            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var mappingDeclaration = (InterfaceDeclarationSyntax)processedNode;

            var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(mappingDeclaration);
            usings.Add(interfaceSymbol.ContainingNamespace.ToDisplayString());

            var mappingClass = (ClassDeclarationSyntax)syntaxGenerator.ClassDeclaration(
                mappingDeclaration.Identifier.Text.Substring(1),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Partial,
                interfaceTypes: new List<SyntaxNode>()
                {
                    syntaxGenerator.TypeExpression(interfaceSymbol)
                },
                members: mappingDeclaration.Members.Select(x =>
                {
                    if (x is MethodDeclarationSyntax methodDeclaration)
                    {
                        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                        var statements = ImplementorEngine.CanProvideMappingImplementationFor(methodSymbol) ? ImplementorEngine.GenerateMappingStatements(methodSymbol, syntaxGenerator, context.SemanticModel, typeMappers) :
                                new List<StatementSyntax>()
                                {
                                    GenerateThrowNotSupportedException(context, syntaxGenerator, methodSymbol.Name)
                                };

                        var methodDeclarationSyntax = ((MethodDeclarationSyntax)syntaxGenerator.MethodDeclaration(
                            methodDeclaration.Identifier.Text,
                            parameters: methodDeclaration.ParameterList.Parameters,
                            accessibility: Accessibility.Public,
                            modifiers: DeclarationModifiers.Virtual,
                            typeParameters: methodDeclaration.TypeParameterList?.Parameters.Select(xx => xx.Identifier.Text),
                            returnType: methodDeclaration.ReturnType
                        )).WithBody(SyntaxFactory.Block(statements));
                        return methodDeclarationSyntax;
                    }
                    return x;
                }));

            var newRoot = WrapInTheSameNamespace(mappingClass, processedNode);
            return Task.FromResult(new GenerationResult()
            {
                Members = SyntaxFactory.SingletonList(newRoot),
                Usings = new SyntaxList<UsingDirectiveSyntax>(usings.Distinct().Select(x => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(x))))
            });
        }

        private static StatementSyntax GenerateThrowNotSupportedException(TransformationContext context, SyntaxGenerator syntaxGenerator, string methodName)
        {
            var notImplementedExceptionType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.NotSupportedException");
            var createNotImplementedException = syntaxGenerator.ObjectCreationExpression(notImplementedExceptionType,

                syntaxGenerator.LiteralExpression($"'{methodName}' method signature is not supported by {GeneratorName}"));
            return (StatementSyntax)syntaxGenerator.ThrowStatement(createNotImplementedException);
        }

        private static MemberDeclarationSyntax WrapInTheSameNamespace(ClassDeclarationSyntax generatedClass, SyntaxNode ancestor)
        {
            if (ancestor is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name.WithoutTrivia())
                    .AddMembers(generatedClass);
            }

            if (ancestor.Parent != null)
            {
                return WrapInTheSameNamespace(generatedClass, ancestor.Parent);
            }

            return generatedClass;
        }
    }
}