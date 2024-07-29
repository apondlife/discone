function getParamName(param) {
    if (param.preset) {
        return param.preset.presetOwner.name;
    } else {
        return undefined;
    }
}

function paramSearch(paramName) {
    function matches(param) {
        return getParamName(param) == paramName
    }
    return matches;
}

// explode currently selected multi-instrument into multiple single instruments along the 'Index' parameter sheet
// Event must be currently selected in Events tab as well
function scratch() {
    // var path = "event:/Character/icecream/Step";
    // var ev = studio.project.lookup(path);
    var event = studio.window.browserCurrent()
    var track0 = event.groupTracks[0];
    var paramName = "Index"
    var param = event.parameters.find(paramSearch(paramName));

    var multi = studio.window.editorCurrent();

    for (var i = 0; i < multi.sounds.length; i++) {
        var s = track0.addSound(param, 'SingleSound', i, 1);
        s.audioFile = multi.sounds[i].audioFile;
    }
}

studio.menu.addMenuItem({
    name: "scratch",
    isEnabled: function() { return true; },
    execute: scratch,
});
