namespace QueryEngine.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Text.RegularExpressions;
    using System.Text;

    public class FragmentService 
    {
        const string _wrapperTemplate = @"class Foo { public void wrapper() { ##INPUT## }}";
        public string Fix(string input) 
        {
            var wrappedInp = _wrapperTemplate
                .Replace("##INPUT##", input);
            var replacements = new Dictionary<SyntaxNode, SyntaxNode>();
            var opts = new CSharpParseOptions()
                .WithKind(SourceCodeKind.Script);
            var fragment = CSharpSyntaxTree.ParseText(wrappedInp);
            var methodBlock = fragment.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .ChildNodes()
                .OfType<BlockSyntax>()
                .Single()
                ;
            foreach (var node in methodBlock.ChildNodes())
            {
                var nodeStr = node.ToString();
                var suffix = nodeStr.Substring(nodeStr.TrimEnd().Length);
                string newStatement = null;
                var localNode = node as LocalDeclarationStatementSyntax;
                var exprNode = node as ExpressionStatementSyntax;
                if (localNode != null && localNode.SemicolonToken.IsMissing)
                {
                    newStatement = string.Format("{0};{1}", nodeStr.TrimEnd(), suffix);
                }
                else if (exprNode != null && exprNode.SemicolonToken.IsMissing)
                {
                    // todo pretty dump
                    var newNodeStr = nodeStr.TrimEnd();
                    var dumpRegex = new Regex(@"\.\W*Dump\W*(\W*)\W*$", RegexOptions.Multiline);
                    var m = dumpRegex.Match(newNodeStr);
                    if (!m.Success)
                    {
                        newNodeStr += ".Dump()";
                    }
                    newStatement = string.Format("{0};{1}", newNodeStr, suffix);
                }

                if (!string.IsNullOrWhiteSpace(newStatement))
                {
                    var newNode = SyntaxFactory.ParseStatement(newStatement);
                    replacements.Add(node, newNode);
                }
            }
            var newRoot = methodBlock.ReplaceNodes(replacements.Keys, (n1, n2) => replacements[n1]);
            var str = new StringBuilder();
            foreach(var stmt in newRoot.ChildNodes()) 
            {
                str.Append(stmt.ToString());
            }
            // todo test for errs
            return str.ToString();
        }
    }
}
