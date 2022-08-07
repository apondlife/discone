using System;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// Variable of type `Entities`. Inherits from `AtomVariable&lt;Entities, EntitiesPair, EntitiesEvent, EntitiesPairEvent, EntitiesEntitiesFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/Entities", fileName = "EntitiesVariable")]
    public sealed class EntitiesVariable : AtomVariable<Entities, EntitiesPair, EntitiesEvent, EntitiesPairEvent, EntitiesEntitiesFunction>
    {
        protected override bool ValueEquals(Entities other) {
            return System.Object.ReferenceEquals(other, Value);
        }
    }
}
