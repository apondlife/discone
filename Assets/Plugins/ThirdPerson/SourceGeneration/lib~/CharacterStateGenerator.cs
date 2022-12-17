using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace ThirdPerson.SourceGeneration {
    [Generator]
    public class CharacterStateGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new FrameClassReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            var receiver = (FrameClassReceiver)context.SyntaxReceiver;

            if (!receiver.FrameClasses.Any()) {
                throw new Exception("[state generator] no state class in this assembly");
            }

            // find all generatable fields
            var frameFields = receiver.FrameClasses
                .SelectMany((c) => c.Members)
                .Select((m) => m as FieldDeclarationSyntax)
                .Where((m) => !(m is null))
                .SelectMany((m) =>
                    m.Declaration.Variables.Select(v => ((
                        name: v.Identifier.ValueText,
                        type: m.Declaration.Type.ToString()
                    )))
                );

            // frame constructor
            var frameCtorImpl = IntoLines(
                frameFields,
                "{0} = f.{0};",
                (f) => f.name
            );

            // frame equality
            var frameEqualsImpl = frameFields.Count() == 0 ? "true" : IntoLines(
                frameFields,
                "{0} == o.{0}", " && ",
                (f) => f.name
            );

            // accessors for properties on the current frame
            var stateFieldsImpl = IntoLines(
                frameFields,
                @"public {1} {0} {{
                    get => m_Frames[0].{0};
                    set => m_Frames[0].{0} = value;
                }}",
                (f) => f.name,
                (f) => f.type
            );

            // the state
            var stateImpl = $@"
                using UnityEngine;

                namespace ThirdPerson {{

                public partial class CharacterState {{
                    {stateFieldsImpl}

                    public partial class Frame {{
                        /// create a copy of an existing frame
                        public Frame(CharacterState.Frame f) {{
                            {frameCtorImpl}
                        }}

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
            context.AddSource("CharacterState.Generated.cs",
                SourceText.From(stateImpl, Encoding.UTF8)
            );
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

    /// the syntax receiver to search for the frame class
    sealed class FrameClassReceiver: ISyntaxReceiver {
        // -- props --
        /// the list of classes; there could be multiple partial frame classes
        public List<ClassDeclarationSyntax> FrameClasses = new List<ClassDeclarationSyntax>();

        // -- ISyntaxReceiver --
        public void OnVisitSyntaxNode(SyntaxNode node) {
            // find all the partial frame classes
            if (node is ClassDeclarationSyntax c) {
                if (FindFullyQualifiedName(node) == "ThirdPerson.CharacterState.Frame") {
                    FrameClasses.Add(c);
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
}
