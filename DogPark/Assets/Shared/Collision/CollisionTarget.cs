using UnityEngine;

namespace CollisionTarget {
    public interface Any {
    }

    // -- collisions --
    public interface Enter: Any {
        void OnCollisionEnter(Collision collision);
    }

    public interface Stay: Any {
        void OnCollisionStay(Collision collision);
    }

    public interface Exit: Any {
        void OnCollisionExit(Collision collision);
    }

    // -- triggers --
    public interface TriggerEnter: Any {
        void OnTriggerSourceEnter(Collider source, Collider other);
    }

    public interface TriggerStay: Any {
        void OnTriggerSourceStay(Collider source, Collider other);
    }

    public interface TriggerExit: Any {
        void OnTriggerSourceExit(Collider source, Collider other);
    }

    // -- triggers/compound
    public interface TriggerEnterAndExit: TriggerEnter, TriggerExit {
    }
}


