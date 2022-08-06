using UnityAtoms.BaseAtoms;
using UnityAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public static class AtomReferenceExtensions
  {
    public static E1 GetChanged<T, P, C, V, E1, E2, F, VI>(this AtomReference<T, P, C, V, E1, E2, F, VI> reference)
      where P : struct, IPair<T>
      where C : AtomBaseVariable<T>
      where V : AtomVariable<T, P, E1, E2, F>
      where E1 : AtomEvent<T>
      where E2 : AtomEvent<P>
      where F : AtomFunction<T, T>
      where VI : AtomVariableInstancer<V, P, T, E1, E2, F>
    {
      if (reference.Usage != AtomReferenceUsage.VARIABLE && reference.Usage != AtomReferenceUsage.VARIABLE_INSTANCER)
      {
        return null;
      }

      return reference.GetEvent<E1>();
    }
  }
}