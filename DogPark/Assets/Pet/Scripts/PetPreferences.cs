using System.Collections.Generic;
using System.Linq;

/// A randomized set of preferences for categories of desire.
public sealed class PetPreferences {
    // -- props --
    private readonly Dictionary<Desire, float> mDesireToScale;

    // -- lifetime --
    public PetPreferences() {
        mDesireToScale = SeedWeightedDesires();
    }

    // -- commands --
    /// Scale the proportion of the pie allotted to this preference
    public void ScaleBy(Desire desire, float scale) {
        // scale this desire
        mDesireToScale[desire] *= scale;

        // re-balance the pie based on the new scale
        var sum = mDesireToScale.Values.Sum();
        foreach (var key in mDesireToScale.Keys.ToList()) {
            mDesireToScale[key] /= sum;
        }
    }

    // -- queries --
    /// Get the pet's preference for this desire.
    public float GetScale(Desire desire) {
        return mDesireToScale[desire];
    }

    // -- factories --
    private static Dictionary<Desire, float> SeedWeightedDesires() {
        var n = Desires.Length;

        // build a map of desire to scale
        var desires = new Dictionary<Desire, float>(n);

        // for each desire in a shuffled list
        var i = 0;
        var sum = n * (n + 1.0f) / 2.0f;
        foreach (var desire in Desires.Shuffled()) {
            desires[desire] = (i + 1) / sum;
            i++;
        }

        return desires;
    }
}
