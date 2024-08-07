﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace ThirdPerson.Codegen {
    /// generates state code from the nodes discovered by `FrameSyntaxReceiver`
    [Generator]
    public class StateGenerator: ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new FrameClassReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            // find our syntax receiver
            var receiver = (FrameClassReceiver)context.SyntaxReceiver;

            // generate each state type
            foreach (var (className, classes) in receiver.FrameClasses) {
                if (!classes.Any()) {
                    throw new Exception($"[ThirdPerson.Codegen] no {className} class in this assembly");
                }

                // find all frame fields
                var frameFields = classes
                    .SelectMany((c) => c.Members)
                    .Select((m) => m as FieldDeclarationSyntax)
                    .Where((m) => m != null)
                    .SelectMany((m) =>
                        m.Declaration.Variables.Select(v => ((
                            name: v.Identifier.ValueText,
                            type: m.Declaration.Type.ToString()
                        )))
                    )
                    .ToArray();

                // frame assign
                var frameCopyImpl = IntoLines(
                    frameFields,
                    "{0} = o.{0};",
                    (f) => f.name
                );

                // frame equality
                var frameEqualsImpl = frameFields.Length == 0 ? "true" : IntoLines(
                    frameFields,
                    "{0} == o.{0}",
                    " && ",
                    (f) => f.name
                );

                // the state impl
                var stateImpl = $@"
                    using Soil;
                    using UnityEngine;

                    namespace ThirdPerson {{

                    public partial class {className} {{
                        public partial class Frame {{
                            // -- commands --
                            /// re-assign properties from an existing frame
                            public void Assign(Frame o) {{
                                {frameCopyImpl}
                            }}

                            // -- IEquatable --
                            public bool Equals(Frame o) {{
                                if (o == null) {{
                                    return false;
                                }}

                                return (
                                    {frameEqualsImpl}
                                );
                            }}
                        }}
                    }}

                    }}
                ";

                // produce the state/frame extensions
                var filename = $"{className}.Generated.cs";
                context.AddSource(filename, SourceText.From(stateImpl, Encoding.UTF8));
                Console.WriteLine($"[ThirdPerson.Codegen] generated: {filename}\n---\n{stateImpl}\n---");
            }
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

    /// finds syntax nodes for various <>State classes
    sealed class FrameClassReceiver: ISyntaxReceiver {
        // -- props --
        /// the list of classes; there could be multiple partial frame classes
        public Dictionary<string, List<ClassDeclarationSyntax>> FrameClasses = new Dictionary<string, List<ClassDeclarationSyntax>>() {
            {"CharacterState", new List<ClassDeclarationSyntax>()},
            {"CameraState", new List<ClassDeclarationSyntax>()},
        };

        // -- ISyntaxReceiver --
        public void OnVisitSyntaxNode(SyntaxNode node) {
            // find all the partial frame classes
            if (node is ClassDeclarationSyntax c) {
                foreach (var (className, classes) in FrameClasses) {
                    var name = FindFullyQualifiedName(node);
                    if (name == $"ThirdPerson.{className}.Frame") {
                        classes.Add(c);
                    }
                }
            }
        }

        // -- helpers --
        string FindFullyQualifiedName(SyntaxNode node) {
            var names = new List<string>();

            do {
                if (node is ClassDeclarationSyntax c) {
                    names.Add(c.Identifier.ToString());
                } else if (node is NamespaceDeclarationSyntax n) {
                    names.Add(n.Name.ToString());
                } else {
                    break;
                }

                node = node.Parent;
            } while(node != null);

            names.Reverse();
            return string.Join(".", names);
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