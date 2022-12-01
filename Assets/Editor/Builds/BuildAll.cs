using System;
using System.IO;
using UnityEditor;

namespace Builds {

/// builds all targets
public class BuildAll {
    // -- constants --
    /// the name of the game binary
    const string k_Name = "discone";

    /// the name of the main scene
    const string k_Scene = "Main";

    /// the name of the playtest scene
    const string k_Scene_Playtest = "Main_Test";

    /// the name of the art room scene
    const string k_Scene_Art = "Main_Art";

    /// the release build dir
    const string k_Paths_Release = "release";

    /// the playtest build dir
    const string k_Paths_Playtest = "playtest";

    /// the art room build dir
    const string k_Paths_Art = "art";

    // -- main --
    /// run the builds
    public static void Main() {
        var optns = Options.Decode(Environment.GetCommandLineArgs());
        var build = new BuildAll(optns);
        build.Call();
    }

    // -- props --
    /// the options
    Options m_Options;

    // -- lifetime --
    /// create a new command
    BuildAll(Options options) {
        m_Options = options;
    }

    // -- commands --
    /// build all the targets
    void Call() {
        Console.WriteLine($"[build] init - variant: {m_Options.Variant}, target: {m_Options.Target}");

        // get build dir
        var buildDir = FindBuildDir();

        // get initial target to restore it later
        var initial = EditorUserBuildSettings.activeBuildTarget;

        // build mac
        if (m_Options.IncludeTarget(Target.Mac)) {
            Console.WriteLine($"[build] start - target: {Target.Mac}");

            var mo = DefaultPlayerOptions();
            mo.target = BuildTarget.StandaloneOSX;
            mo.targetGroup = BuildTargetGroup.Standalone;
            mo.locationPathName = Path.Combine(buildDir, Target.Mac, k_Name);

            BuildPlayer(mo);
        }

        // build win
        if (m_Options.IncludeTarget(Target.Windows)) {
            Console.WriteLine($"[build] start - target: {Target.Windows}");

            var wo = DefaultPlayerOptions();
            wo.target = BuildTarget.StandaloneWindows64;
            wo.targetGroup = BuildTargetGroup.Standalone;
            wo.locationPathName = Path.Combine(buildDir, Target.Windows, k_Name + ".exe");

            BuildPlayer(wo);
        }

        // build win-server
        if (m_Options.IncludeTarget(Target.WindowsServer)) {
            Console.WriteLine($"[build] start - target: {Target.WindowsServer}");

            var so = DefaultPlayerOptions();
            so.target = BuildTarget.StandaloneWindows64;
            so.subtarget = (int)StandaloneBuildSubtarget.Server;
            so.targetGroup = BuildTargetGroup.Standalone;
            so.locationPathName = Path.Combine(buildDir, Target.WindowsServer, k_Name + ".exe");
            so.extraScriptingDefines = new[] { "UNITY_SERVER" };

            BuildPlayer(so);
        }

        // restore the user's initial target
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildPipeline.GetBuildTargetGroup(initial),
            initial
        );
    }

    /// build a specific player
    void BuildPlayer(BuildPlayerOptions options) {
        // clean the dir
        if (Directory.Exists(options.locationPathName)) {
            Directory.Delete(options.locationPathName, true);
        }

        // switch target; not switching target may cause some build defines to
        // not be set directly
        // https://forum.unity.com/threads/issues-with-build-player-options-subtarget-setting-dedicated-server-option-for-buildpipeline.1306821/
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            options.targetGroup,
            options.target
        );

        // build player
        BuildPipeline.BuildPlayer(options);
    }

    // -- queries --
    /// the dir for all the builds
    string FindBuildDir() {
        // get the variant subdirectory
        var variant = m_Options.Variant switch {
            Variant.Playtest => k_Paths_Playtest,
            Variant.Art      => k_Paths_Art,
            _                => k_Paths_Release,
        };

        // get the build subdirectory
        // TODO: build number, read/write from disk
        var build = $"discone-{DateTime.Now.ToString("yyyy.MM.dd")}";

        // combine the full path
        return Path.Combine("Artifacts", "Builds", variant, build);
    }

    // build player options w/ shared values
    BuildPlayerOptions DefaultPlayerOptions() {
        // build options
        var o = new BuildPlayerOptions();

        // pick the right scene
        var scene = m_Options.Variant switch {
            Variant.Playtest => k_Scene_Playtest,
            Variant.Art      => k_Scene_Art,
            _ => k_Scene,
        };

        // add src options
        o.scenes = new string[]{
            Path.Combine("Assets", $"{scene}.unity")
        };

        return o;
    }
}

}