using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Discone.Codegen {

/// generates atom helpers
[Generator]
public class AtomGen: ISourceGenerator {
    // -- constants --
    /// the assembly this generates code for
    const string k_Assembly = "Assembly-CSharp";

    /// the assembly containing base unity atoms
    const string k_AtomsAssembly = "UnityAtoms.UnityAtomsBaseAtoms.Runtime";

    // -- ISourceGenerator --
    public void Initialize(GeneratorInitializationContext context) {
        // this does all its discovery using context.Compilation because it needs to inspect referenced assemblies
    }

    public void Execute(GeneratorExecutionContext context) {
        var assemblyName = context.Compilation.AssemblyName;
        if (assemblyName != k_Assembly) {
            throw new Exception($"[Discone.Codegen] assembly named `{assemblyName}` is not `{k_Assembly}`");
        }

        var sourceModule = context
            .Compilation
            .SourceModule;

        // find all variables
        var allVariables = sourceModule
            .ReferencedAssemblySymbols
            .Where((a) => a.Name.Equals(k_AtomsAssembly))
            .Select((a) => a.GlobalNamespace)
            .Append(sourceModule.GlobalNamespace)
            .SelectMany((ns) => ns.FindTypes())
            .Where((t) => t != null && t.IsA("AtomVariable"))
            .ToArray();

        // generate each state type
        if (!allVariables.Any()) {
            throw new Exception($"[Discone.Codegen] no `AtomVariable` subclasses in this assembly");
        }

        // implement dispose bag subscribe fns
        var addImpl = IntoLines(
            allVariables,
            @"
            /// add a subscription for a {1} changed event
            public static DisposeBag Add(this DisposeBag bag, {0} variable, Action<{1}> a) {{
                return variable != null ? bag.Add(variable.Changed, a) : bag;
            }}
            ",
            (v) => v.Name,
            (v) => FindAtomValue(v).NameOrSpecialName()
        );

        // find variables with a value that supports `GetComponent`
        var componentBaseTypes = new []{ "GameObject", "Component" };
        var componentVariables = allVariables
            .Where((v) => FindAtomValue(v).IsOneOf(componentBaseTypes));

        var getComponentImpl = IntoLines(
            componentVariables,
            @"
            /// get a component from the inner {1}
            public static C GetComponent<C>(
                this {0} obj
            ) where C: Component {{
                return obj.Value.GetComponent<C>();
            }}
            ",
            (v) => v.Name,
            (v) => FindAtomValue(v).Name
        );

        // add a debug tag
        var debugImpl = $@"
            public static string Debug() {{
                return """";
            }}
        ";

        // the extension impl
        var atomExtImpl = $@"
            using System;
            using UnityAtoms;
            using UnityAtoms.BaseAtoms;
            using UnityEngine;

            namespace Discone {{

            static partial class AtomExt {{
                {addImpl}
                {getComponentImpl}
                {debugImpl}
            }}

            }}
        ";

        // produce the state/frame extensions
        var filename = $"AtomExt.Generated.cs";
        context.AddSource(filename, SourceText.From(atomExtImpl, Encoding.UTF8));

        Console.WriteLine($"[Discone.Codegen] generated: {filename}\n---\n{atomExtImpl}\n---");
    }

    // -- helpers --
    /// find the variable's value type
    static ITypeSymbol FindAtomValue(ITypeSymbol type) {
        return type.BaseType?.TypeArguments[0];
    }

    /// format nodes into lines
    string IntoLines<T>(IEnumerable<T> nodes, string format, params Func<T, string>[] args) {
        return IntoLines(nodes, format, "\n", args);
    }

    /// format nodes into lines
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

}