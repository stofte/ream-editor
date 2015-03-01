using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class CustomDocumentationProvider : DocumentationProvider
    {
        Assembly _assembly;
        XDocument _documentation;
        const string _versionRegex = @"(v\d\.\d(\.\d)?)";

        public CustomDocumentationProvider(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            
            _assembly = assembly;
            _documentation = GetAssemblyDocumentation(assembly);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is CustomDocumentationProvider)
            {
                return GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        protected override string GetDocumentationForSymbol(string documentationMemberID, System.Globalization.CultureInfo preferredCulture, System.Threading.CancellationToken cancellationToken)
        {
            if (_documentation != null)
            {
                var member = _documentation.Descendants("member").Where(x => x.Attribute("name").Value == documentationMemberID).FirstOrDefault();
                return member == null ? string.Empty : member.ToString();
            }

            return string.Empty;
        }

        public string GetDocumentation(string documentationMemberID)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return GetDocumentationForSymbol(documentationMemberID, culture, CancellationToken.None);
        }

        protected XDocument GetAssemblyDocumentation(Assembly assembly)
        {
            var path = GetAssemblyDocmentationPath(assembly);
            return string.IsNullOrWhiteSpace(path) ? null : XDocument.Load(path);
        }

        public static string GetAssemblyDocmentationPath(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var commonDocFolders = PathUtility.DocumentationPaths;
            var frameworkName = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
            var match = new Regex("Version=" + _versionRegex).Match(frameworkName);
            var tag = match.Groups[1].Value;

            string locatedDoc = null;
            foreach (var rootPath in commonDocFolders)
            {
                locatedDoc = GetDocPath(rootPath, assemblyName, tag);
                if (!string.IsNullOrWhiteSpace(locatedDoc))
                {
                    break;
                }
            }
            return locatedDoc;
        }

        static string GetDocPath(string folder, string assemblyName, string versionTag)
        {
            // this folder walk assumes that the initial root folder points
            var rootInfo = new DirectoryInfo(folder);
            if (rootInfo.Exists)
            {
                var versionRegex = new Regex(_versionRegex);
                var match = versionRegex.Match(rootInfo.FullName);

                if (match.Success && match.Groups[0].Value != versionTag)
                {
                    // incorrect framework version folder
                    return null;
                }

                foreach (var file in rootInfo.EnumerateFiles("*.xml"))
                {
                    if (Path.GetFileNameWithoutExtension(file.Name) == assemblyName)
                    {
                        Debug.Assert(match.Groups[0].Value == versionTag);
                        return file.FullName;
                    }
                }
                foreach (var subFolder in rootInfo.EnumerateDirectories())
                {
                    var subResult = GetDocPath(subFolder.FullName, assemblyName, versionTag);
                    if (subResult != null) return subResult;
                }
            }
            return null;
        }

        public override int GetHashCode()
        {
            // from DocumentationProvider's doc:

            /// DocumentationProviders are compared when determining whether an AssemblySymbol can be reused.
            ///             Hence, if multiple instances can represent the same documentation, it is imperative that
            ///             Equals (and GetHashCode) be overridden to capture this fact.  Otherwise, it is possible to end
            ///             up with multiple AssemblySymbols for the same assembly, which plays havoc with the type hierarchy.

            // since we create all references by Assembly ref, this should honour the requirement
            return _assembly.GetHashCode();
        }
    }
}
