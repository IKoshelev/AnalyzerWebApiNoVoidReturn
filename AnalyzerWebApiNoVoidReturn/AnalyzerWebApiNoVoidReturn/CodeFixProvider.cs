using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AnalyzerWebApiNoVoidReturn
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnalyzerWebApiNoVoidReturnCodeFixProvider)), Shared]
    public class AnalyzerWebApiNoVoidReturnCodeFixProvider : CodeFixProvider
    {
        private const string title = "Return random int";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(AnalyzerWebApiNoVoidReturnAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                                    .GetSyntaxRootAsync(context.CancellationToken)
                                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start)
                                    .Parent.AncestorsAndSelf()
                                    .OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => SwitchReturnToRandomInt(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static ReturnStatementSyntax ReturnRandomIntSyntax =
                                             SF.ReturnStatement(
                                                SF.InvocationExpression(
                                                    SF.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SF.ObjectCreationExpression(
                                                            SF.IdentifierName("Random"))
                                                        .WithArgumentList(
                                                            SF.ArgumentList()),
                                                        SF.IdentifierName("Next"))));

        private async Task<Solution> SwitchReturnToRandomInt(
                                                    Document document,
                                                    MethodDeclarationSyntax methodDecl, 
                                                    CancellationToken cancellationToken)
        {
            var newBody = methodDecl.Body;

            var newBodyStatements = newBody.ChildNodes().ToArray();

            var endsWithReturn = newBodyStatements.LastOrDefault() is ReturnStatementSyntax;

            if (endsWithReturn == false)
            {
                newBodyStatements = newBody
                                        .ChildNodes()
                                        .Concat(new[] { SF.ReturnStatement() })
                                        .ToArray();

                newBody = newBody.WithStatements(SF.List(newBodyStatements));
            }

            var returns = newBody.DescendantNodes()
                                    .OfType<ReturnStatementSyntax>()
                                    .ToArray();

            newBody = newBody.ReplaceNodes(returns, (original, updated) =>
            {
                return ReturnRandomIntSyntax
                            .WithLeadingTrivia(original.GetLeadingTrivia())
                            .WithTrailingTrivia(original.GetTrailingTrivia());
            });

            var intTypeSyntax = SF.PredefinedType(
                                    SF.Token(SyntaxKind.IntKeyword));

            var newMethodDecl = methodDecl
                                    .WithReturnType(intTypeSyntax)
                                    .WithBody(newBody);

            var oldClass = (ClassDeclarationSyntax)methodDecl.Parent;

            var newClass = oldClass.ReplaceNode(methodDecl, newMethodDecl);

            var newRoot = (await document.GetSyntaxRootAsync())
                                            .ReplaceNode(oldClass, newClass);

            //var newDocument = document.WithSyntaxRoot(newRoot);

            //var newDocumentInfo = DocumentInfo.

            var newSolution = document.Project.Solution
                                    .RemoveDocument(document.Id)
                                    .AddDocument(document.Id, document.Name, newRoot);

            return newSolution;

            //// Compute new uppercase name.
            //var identifierToken = methodDecl.Identifier;
            //var newName = identifierToken.Text.ToUpperInvariant();

            //// Get the symbol representing the type to be renamed.
            //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            //var typeSymbol = semanticModel.GetDeclaredSymbol(methodDecl, cancellationToken);

            //// Produce a new solution that has all references to that type renamed, including the declaration.
            //var originalSolution = document.Project.Solution;
            //var optionSet = originalSolution.Workspace.Options;
            //var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            //// Return the new solution with the now-uppercase type name.
        }
    }
}