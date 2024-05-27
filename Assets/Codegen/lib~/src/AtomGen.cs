using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Discone.Codegen {

/// generates state code from the nodes discovered by `FrameSyntaxReceiver`
[Generator]
public class AtomGen: ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
    }

    IEnumerable<ITypeSymbol> GetTypes(INamespaceOrTypeSymbol member) {
        switch (member) {
            case INamespaceSymbol ns:
                return ns.GetMembers().SelectMany(GetTypes);
            case ITypeSymbol t:
                return Enumerable.Repeat(t, 1);
            default:
                return null;
        }
    }

    bool HasBaseType(ITypeSymbol type, string name) {
        var baseType = type.BaseType;
        while (baseType != null) {
            if (baseType.Name == name) {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    string GetTypeName(ITypeSymbol type) {
        switch (type.SpecialType) {
            case SpecialType.System_Boolean:
                return "bool";
            case SpecialType.System_String:
                return "string";
            case SpecialType.System_Int32:
                return "int";
            case SpecialType.System_Single:
                return "float";
            case SpecialType.System_Double:
                return "double";
            case SpecialType.None:
                return type.Name;
            default:
                throw new Exception($"[Discone.Codegen] unhandled special type {type.SpecialType}");
        }
    }

    public void Execute(GeneratorExecutionContext context) {
        var sourceModule = context
            .Compilation
            .SourceModule;

        var variables = sourceModule
            .ReferencedAssemblySymbols
            .Where((a) => a.Name.Equals("UnityAtoms.UnityAtomsBaseAtoms.Runtime"))
            .Select((a) => a.GlobalNamespace)
            .Append(sourceModule.GlobalNamespace)
            .SelectMany(GetTypes)
            .Where((t) => t != null && HasBaseType(t, "AtomVariable"));

        // generate each state type
        if (!variables.Any()) {
            throw new Exception($"[Discone.Codegen] no `AtomVariable` subclasses in this assembly");
        }

        // implement dispose bag subscribe fns
        var addImpl = IntoLines(
            variables,
            @"public static DisposeBag Add(this DisposeBag, {0} variable, Action<{1}> a) {{
                return variable != null ? Add(variable.Changed, a) : this;
            }}",
            (v) => v.Name,
            (v) => GetTypeName(v.BaseType?.TypeArguments[0])
        );

        // the extension impl
        var atomExtImpl = $@"
            using System;
            using UnityAtoms;
            using UnityAtoms.BaseAtoms;
            using UnityEngine;

            static class AtomExt {{
                {addImpl}
            }}
        ";

        // produce the state/frame extensions
        var filename = $"AtomExt.Generated.cs";
        context.AddSource(filename, SourceText.From(atomExtImpl, Encoding.UTF8));

        Console.WriteLine($"[Discone.Codegen] generated: {filename}\n---\n{atomExtImpl}\n---");
    }

    // -- helpers --
    string IntoLines<T>(IEnumerable<T> nodes, string format, params Func<T, string>[] args) {
        return IntoLines(nodes, format, "\n", args);
    }

    string IntoLines<T>(IEnumerable<T> nodes, string format, string separator, params Func<T, string>[] args) {
        return string.Join(
            separator,
            nodes.Select((n) => String.Format(
                format,
                args.Select((f) => f.Invoke(n)).ToArray()
            ))
        );
    }
}

/// extensions for dictionaries and related types
static class DictionaryExt {
    /// deconstruct a dictionary's key-value pair
    public static void Deconstruct<K, V>(this KeyValuePair<K, V> p, out K k, out V v) {
        k = p.Key;
        v = p.Value;
    }
}

}