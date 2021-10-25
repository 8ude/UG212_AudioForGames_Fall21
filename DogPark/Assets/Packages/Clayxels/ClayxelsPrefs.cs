
using UnityEngine;

#if UNITY_EDITOR_OSX
    using UnityEditor;
#endif

public static class ClayxelsPrefs {
    // change the path to the retopology lib if Clayxels is not in the root of your Assets folder (winwdows only)
    public static string retopoLib = "Assets\\Clayxels\\Editor\\retopo\\retopoLib.dll";

    public static void apply(){
        Clayxels.ClayContainer.boundsColor = new Color(0.5f, 0.5f, 1.0f, 0.1f);
        
        Clayxels.ClayContainer.pickingKey = "p";
        Clayxels.ClayContainer.mirrorDuplicateKey = "m";

        // max number of points, only affects video memory
        Clayxels.ClayContainer.setPointCloudLimit(300000);

    	// max number of solids in a container
        // affects video memory
        Clayxels.ClayContainer.setMaxSolids(512);// 64, 128, 256, 512, 1024, 4096, 16384
        
        // how many solids you can have in a single voxel
        // lower values means faster evaluation and less video memory used
        Clayxels.ClayContainer.setMaxSolidsPerVoxel(128);// 32, 64, 128, 256, 512, 1024, 2048

        // skip a certain amount of frames before updating the clay
        Clayxels.ClayContainer.setUpdateFrameSkip(0);

        // Upon creating a container, clayxels will check if there is enough vram available.
        Clayxels.ClayContainer.enableVideoRamSafeLimit(true);

        #if UNITY_EDITOR_OSX
            // on mac disable warnings about missing bindings
            PlayerSettings.enableMetalAPIValidation = false;
        #endif
    }
}
