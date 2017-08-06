using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using AnalyzerWebApiNoVoidReturn;

namespace AnalyzerWebApiNoVoidReturn.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void OnEmptyFileNoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void OnEmptyClassNoDiagnostics()
        {
            var test = @"
namespace Test
{
    public class FooBar
    {
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void RightClassNutNonViolatingMehtodsNoDiag()
        {
            var test = @"
using System;
using System.Web.Http;

namespace Test
{
    public class FooBar: ApiController
    {
        public int Method1()
        {
            return 1;
        }
        private void Method2()
        {
            return;
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void OnClassInheritingWebWithViolationMethodGivesDiag()
        {
            var test = @"
using System;
using System.Web.Http;

namespace Test
{
    public class FooBar: ApiController
    {
        public void ViolatingMethod()
        {
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "AnalyzerWebApiNoVoidReturn",
                Message = String.Format(
                                AnalyzerWebApiNoVoidReturnAnalyzer.MessageFormat,
                                "FooBar",
                                "ViolatingMethod"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Web.Http;

namespace Test
{
    public class FooBar: ApiController
    {
        public int ViolatingMethod()
        {
            return new Random().Next();
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void WhenFixingUpdatesExistingReturns()
        {
            var test = @"
using System;
using System.Web.Http;

namespace Test
{
    public class FooBar: ApiController
    {
        public void ViolatingMethod(int a)
        {
            if(a > 0)
            {
                return;
            }
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "AnalyzerWebApiNoVoidReturn",
                Message = String.Format(
                                AnalyzerWebApiNoVoidReturnAnalyzer.MessageFormat,
                                "FooBar",
                                "ViolatingMethod"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Web.Http;

namespace Test
{
    public class FooBar: ApiController
    {
        public int ViolatingMethod(int a)
        {
            if(a > 0)
            {
                return new Random().Next();
            }

            return new Random().Next();
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AnalyzerWebApiNoVoidReturnCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AnalyzerWebApiNoVoidReturnAnalyzer();
        }
    }
}