using UnityEngine;

namespace Mirror.Examples.Additive
{
    public class RandomActiveChild : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            activeChild = Random.Range(0, children.Length);
        }

        // Color32 packs to 4 bytes
        [SyncVar(hook = nameof(SetActiveChild))]
        public int activeChild = 0;
        public GameObject[] children;

        // Unity clones the material when GetComponent<Renderer>().material is called
        // Cache it here and destroy it in OnDestroy to prevent a memory leak
        Material cachedMaterial;

        void SetActiveChild(int oldColor, int newColor)
        {
            for(int i = 0; i < children.Length; i++)
            {
                children[i].SetActive(i == newColor);
            }
        }
    }
}

