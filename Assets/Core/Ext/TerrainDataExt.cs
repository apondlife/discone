using System;
using UnityEngine;

/// extensions on UnityEngine.TerrainData
public static class TerrainDataExt {
    /// deep copies the terrain data
    public static TerrainData Clone(this TerrainData original) {
        // create a clone
        var dup = new TerrainData();

        dup.alphamapResolution = original.alphamapResolution;
        dup.baseMapResolution = original.baseMapResolution;


        // clone detail prototypes
        var srcDetails = original.detailPrototypes;
        var dstDetails = new DetailPrototype[srcDetails.Length];

        for (int n = 0; n < srcDetails.Length; n++) {
            dstDetails[n] = new DetailPrototype {
                dryColor = srcDetails[n].dryColor,
                healthyColor = srcDetails[n].healthyColor,
                maxHeight = srcDetails[n].maxHeight,
                maxWidth = srcDetails[n].maxWidth,
                minHeight = srcDetails[n].minHeight,
                minWidth = srcDetails[n].minWidth,
                noiseSpread = srcDetails[n].noiseSpread,
                prototype = srcDetails[n].prototype,
                prototypeTexture = srcDetails[n].prototypeTexture,
                renderMode = srcDetails[n].renderMode,
                usePrototypeMesh = srcDetails[n].usePrototypeMesh,
            };
        }

        dup.detailPrototypes = dstDetails;

        // The resolutionPerPatch is not publicly accessible so
        // it can not be cloned properly, thus the recommendet default
        // number of 16
        dup.SetDetailResolution(original.detailResolution, 16);

        dup.heightmapResolution = original.heightmapResolution;
        dup.size = original.size;


        dup.terrainLayers = original.terrainLayers;

        dup.wavingGrassAmount = original.wavingGrassAmount;
        dup.wavingGrassSpeed = original.wavingGrassSpeed;
        dup.wavingGrassStrength = original.wavingGrassStrength;
        dup.wavingGrassTint = original.wavingGrassTint;

        dup.SetAlphamaps(0, 0, original.GetAlphamaps(0, 0, original.alphamapResolution, original.alphamapResolution));
        dup.SetHeights(0, 0, original.GetHeights(0, 0, original.heightmapResolution, original.heightmapResolution));

        for (int n = 0; n < original.detailPrototypes.Length; n++) {
            dup.SetDetailLayer(0, 0, n, original.GetDetailLayer(0, 0, original.detailResolution, original.detailResolution, n));
        }

        // clone tree prototypes
        var srcTrees = original.treePrototypes;
        var dstTrees = new TreePrototype[srcTrees.Length];

        for (int n = 0; n < srcTrees.Length; n++) {
            dstTrees[n] = new TreePrototype {
                bendFactor = srcTrees[n].bendFactor,
                prefab = srcTrees[n].prefab,
            };
        }

        dup.treePrototypes = dstTrees;

        // clone tree instances
        var srcTreeInsts = original.treeInstances;
        var dstTreeInsts = new TreeInstance[srcTreeInsts.Length];
        Array.Copy(srcTreeInsts, dstTreeInsts, srcTreeInsts.Length);
        dup.treeInstances = dstTreeInsts;

        return dup;
    }
}