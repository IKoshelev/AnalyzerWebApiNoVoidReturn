using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerWebApiNoVoidReturn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerWebApiNoVoidReturnAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyzerWebApiNoVoidReturn";

        public static readonly string Title = "Web Api public method return void.";
        public static readonly string MessageFormat = "Method {1} of Controller {0} return void.";
        public static readonly string Description = "Web Api public methods must not return void (Proxies choke on it).";
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = 
            new DiagnosticDescriptor(DiagnosticId, 
                                    Title, 
                                    MessageFormat, 
                                    Category, 
                                    DiagnosticSeverity.Error, 
                                    isEnabledByDefault: true, 
                                    description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
                                                        => ImmutableArray.Create(Rule);


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedMethodSymbol = (IMethodSymbol)context.Symbol;
            
            if (namedMethodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                return;
            }

            if (namedMethodSymbol.ReturnsVoid != true)
            {
                return;
            }

            var namedTypeSymbol = namedMethodSymbol.ContainingType;

            var constructors = namedTypeSymbol.Constructors;

            if (constructors.Contains(namedMethodSymbol))
            {
                return;
            }

            var webApiControllerTypeSymbol = 
                context.Compilation.GetTypeByMetadataName(
                                            "System.Web.Http.ApiController");

            var isInheritingWebApiController =
                    namedTypeSymbol.IsInheritingFrom(webApiControllerTypeSymbol);

            if (isInheritingWebApiController == false)
            {
                return;
            }

            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(
                                            Rule, 
                                            namedMethodSymbol.Locations[0],
                                            namedTypeSymbol.Name,
                                            namedMethodSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
