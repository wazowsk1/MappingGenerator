﻿using System.Collections.Generic;
using MappingGenerator.Mappings.MappingMatchers;
using MappingGenerator.Mappings.SourceFinders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace MappingGenerator.Mappings.MappingImplementors
{
    class MultiParameterPureMappingMethodImplementor: IMappingMethodImplementor
    {
        public bool CanImplement(IMethodSymbol methodSymbol)
        {
            return methodSymbol.Parameters.Length > 1 && methodSymbol.ReturnsVoid == false;
        }

        public IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel)
        {
            var mappingEngine = new MappingEngine(semanticModel, generator, methodSymbol.ContainingAssembly);
            var targetType = methodSymbol.ReturnType;
            var sourceFinder = new LocalScopeMappingSourceFinder(semanticModel, methodSymbol);
            var newExpression = mappingEngine.AddInitializerWithMapping(
                (ObjectCreationExpressionSyntax)generator.ObjectCreationExpression(targetType), new SingleSourceMatcher(sourceFinder), targetType);
            return new[] { generator.ReturnStatement(newExpression).WithAdditionalAnnotations(Formatter.Annotation) };
        }
    }
}