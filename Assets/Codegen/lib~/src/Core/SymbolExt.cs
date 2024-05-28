using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Discone.Codegen {

static class SymbolExt {
    // -- ITypeSymbol --
    /// if this type inherits from a type of the given the name
    public static bool IsA(
        this ITypeSymbol type,
        string name
    ) {
        var baseType = type;
        while (baseType != null) {
            if (baseType.Name == name) {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// if this type inherits from a type of the given names
    public static bool IsOneOf(
        this ITypeSymbol type,
        ICollection<string> names
    ) {
        var baseType = type;
        while (baseType != null) {
            if (names.Contains(baseType.Name)) {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// the name of this type, or its primitive name if a special type
    public static string NameOrSpecialName(
        this ITypeSymbol type
    ) {
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

    // -- INamespaceOrTypeSymbol --
    /// recursively find all types in this namespace
    public static IEnumerable<ITypeSymbol> FindTypes(
        this INamespaceOrTypeSymbol namespaceOrType
    ) {
        switch (namespaceOrType) {
            case INamespaceSymbol ns:
                return ns.GetMembers().SelectMany((m) => m.FindTypes());
            case ITypeSymbol t:
                return Enumerable.Repeat(t, 1);
            default:
                return null;
        }
    }
}

}