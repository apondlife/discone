using System;

namespace Soil {

/// a generic, type-erased disposable
public sealed class AnyDisposable: IDisposable {
    // -- props --
    /// a generic dispose action
    Action m_Dispose;

    // -- lifetime --
    /// create a disposable from this action
    public AnyDisposable(Action dispose) {
        m_Dispose = dispose;
    }

    // -- IDisposable --
    public void Dispose() {
        m_Dispose();
    }

    // -- operators --
    public static AnyDisposable operator +(AnyDisposable a, AnyDisposable b) {
      a.m_Dispose += b.m_Dispose;
      return a;
    }

    public static AnyDisposable operator -(AnyDisposable a, AnyDisposable b) {
      a.m_Dispose -= b.m_Dispose;
      return a;
    }
}

}