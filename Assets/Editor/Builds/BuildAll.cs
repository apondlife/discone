using System;
using System.IO;
using UnityEditor;

namespace Builds {

/// builds all targets
public static class BuildAll {
    // -- constants --
    /// the name of the game binary
    static readonly string k_Name = "discone";

    /// the path to the scene
    static readonly string k_Scene = "Main.unity";

    /// the mac channel
    static readonly string k_Channel_Mac = "mac";

    /// the windows channel
    static readonly string k_Channel_Windows = "win";

    /// the windows-server channel
    static readonly string k_Channel_WindowsServer = "win-server";

    // -- command --
    /// run the builds
    public static void Call() {
        // get project dir
        var dir = Directory.GetCurrentDirectory();

        // get build name
        // TODO: build number, read/write from disk
        var buildName = $"discone-{DateTime.Now.ToString("yyyy.MM.dd")}";

        // the relavant dirs
        var dirBuilds = Path.Combine("Artifacts", "Builds", buildName);

        // build mac
        var mo = BuildOptions();
        mo.target = BuildTarget.StandaloneOSX;
        mo.targetGroup = BuildTargetGroup.Standalone;
        mo.locationPathName = Path.Combine(dirBuilds, k_Channel_Mac, k_Name);

        BuildPipeline.BuildPlayer(mo);

        // build win
        var wo = BuildOptions();
        wo.target = BuildTarget.StandaloneWindows64;
        wo.targetGroup = BuildTargetGroup.Standalone;
        wo.locationPathName = Path.Combine(dirBuilds, k_Channel_Windows, k_Name);

        BuildPipeline.BuildPlayer(wo);

        // build win-server
        var so = BuildOptions();
        so.target = BuildTarget.StandaloneWindows64;
        so.subtarget = (int)StandaloneBuildSubtarget.Server;
        so.targetGroup = BuildTargetGroup.Standalone;
        so.locationPathName = Path.Combine(dirBuilds, k_Channel_WindowsServer, k_Name);

        BuildPipeline.BuildPlayer(so);
    }

    // build player options w/ shared values
    public static BuildPlayerOptions BuildOptions() {
        // build options
        var o = new BuildPlayerOptions();

        // add src options
        o.scenes = new string[]{
            Path.Combine("Assets", k_Scene)
        };

        return o;
    }
}

}