using UnityEngine;

namespace MutCommon {
    public static class LayerMaskExtensions
    {
    public static bool Contains(this LayerMask layerMask, int layer)
        => layerMask == (layerMask | (1 << layer));
    }
}