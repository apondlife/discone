using UnityEngine;
using UnityAtoms.BaseAtoms;
using Discone;

namespace UnityAtoms
{
    /// <summary>
    /// Adds Variable Instancer's Variable of type Player to a Collection or List on OnEnable and removes it on OnDestroy. 
    /// </summary>
    [AddComponentMenu("Unity Atoms/Sync Variable Instancer to Collection/Sync Player Variable Instancer to Collection")]
    [EditorIcon("atom-icon-delicate")]
    public class SyncPlayerVariableInstancerToCollection : SyncVariableInstancerToCollection<Player, PlayerVariable, PlayerVariableInstancer> { }
}
