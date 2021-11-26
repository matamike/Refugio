using UnityEngine;

namespace Utilities {
    public class MeshImportTools : MonoBehaviour {
    
        private Vector3 _currentOffset = Vector3.zero;
    
        public Transform centerOn = null;
        public bool centerMesh = false;

        private void OnValidate() {
            if (centerMesh) {
                if (centerOn != null && this.transform.childCount > 0) {
                    Vector3 tmpOffset = -centerOn.position;

                    foreach (Transform child in this.transform) {
                        child.position += tmpOffset;
                    }

                    _currentOffset += tmpOffset;
                }
            } else {
                if (this.transform.childCount > 0) {
                
                    if (this.transform.childCount > 0) {
                        foreach (Transform child in this.transform) {
                            child.position -= _currentOffset;
                        }
                    }

                    this.transform.GetChild(1).position -= _currentOffset;
                }
                _currentOffset = Vector3.zero;
            }
        }
    }
}
