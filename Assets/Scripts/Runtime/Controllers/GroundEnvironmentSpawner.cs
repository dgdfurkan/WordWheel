using System.Collections.Generic;
using UnityEngine;
using WordWheel.Runtime.Data;

namespace WordWheel.Runtime.Controllers
{
    public class GroundEnvironmentSpawner : MonoBehaviour
    {
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private float gizmoHeight = 0.2f;
        [SerializeField] private float overrideGroundLength = 0f;
        [SerializeField] private List<SpawnZone> zones = new List<SpawnZone>();

        public List<SpawnZone> Zones => zones;

        public float LocalGroundZLength
        {
            get
            {
                float scaleZ = transform.lossyScale.z;
                if (scaleZ <= 0.0001f) scaleZ = 1f;

                if (overrideGroundLength > 0f) return overrideGroundLength / scaleZ;

                var meshFilter = GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    float localMeshZ = meshFilter.sharedMesh.bounds.size.z;
                    if (localMeshZ > 0.01f) return localMeshZ;
                }

                var rend = GetComponentInChildren<Renderer>();
                if (rend != null && rend.bounds.size.z > 0.01f)
                    return rend.bounds.size.z / scaleZ;

                var col = GetComponentInChildren<Collider>();
                if (col != null && col.bounds.size.z > 0.01f)
                    return col.bounds.size.z / scaleZ;

                return 25f / scaleZ;
            }
        }

        private void Reset()
        {
            zones = new List<SpawnZone>
            {
                new SpawnZone
                {
                    zoneName = "Roadside Props",
                    zoneColor = new Color(1f, 0.9f, 0.2f, 0.4f),
                    xPosition = 4f,
                    width = 2f,
                    mirrorToLeft = true
                },
                new SpawnZone
                {
                    zoneName = "Trees Zone",
                    zoneColor = new Color(0.2f, 0.8f, 0.3f, 0.4f),
                    xPosition = 7.75f,
                    width = 4.5f,
                    mirrorToLeft = true
                },
                new SpawnZone
                {
                    zoneName = "Buildings Zone",
                    zoneColor = new Color(0.2f, 0.6f, 1f, 0.4f),
                    xPosition = 14.25f,
                    width = 7.5f,
                    mirrorToLeft = true
                }
            };
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos || zones == null) return;

            float length = LocalGroundZLength;
            for (int i = 0; i < zones.Count; i++)
            {
                var zone = zones[i];
                if (zone == null || !zone.isEnabled) continue;

                DrawZone(zone, length);
            }
        }

        private void DrawZone(SpawnZone zone, float length)
        {
            float width = zone.width;
            if (width <= 0f || length <= 0f) return;

            float centerX = zone.xPosition;
            float centerZ = 0f;

            Matrix4x4 prevMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 size = new Vector3(width, gizmoHeight, length);

            // Right side
            Vector3 rightCenter = new Vector3(centerX, gizmoHeight * 0.5f, centerZ);
            Gizmos.color = zone.zoneColor;
            Gizmos.DrawCube(rightCenter, size);

            Color wireColor = zone.zoneColor;
            wireColor.a = 1f;
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(rightCenter, size);

            // Left side (Mirrored)
            if (zone.mirrorToLeft)
            {
                Vector3 leftCenter = new Vector3(-centerX, gizmoHeight * 0.5f, centerZ);
                Gizmos.color = zone.zoneColor;
                Gizmos.DrawCube(leftCenter, size);

                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(leftCenter, size);
            }

            Gizmos.matrix = prevMatrix;
        }
        private const string PREVIEW_HOLDER_NAME = "_PreviewHolder";

        [ContextMenu("Generate Preview Spawns")]
        public void GeneratePreviewSpawns()
        {
            ClearPreviewSpawns();

            GameObject previewGo = new GameObject(PREVIEW_HOLDER_NAME);
            Transform previewHolder = previewGo.transform;
            previewHolder.SetParent(transform, false);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(previewGo, "Generate Preview Spawns");
            }
#endif

            float halfLength = LocalGroundZLength * 0.5f;

            for (int zIdx = 0; zIdx < zones.Count; zIdx++)
            {
                var zone = zones[zIdx];
                if (zone == null || !zone.isEnabled || zone.spawnCount <= 0) continue;

                SpawnZonePreviewObjects(zone, previewHolder, halfLength, isRightSide: true);
                if (zone.mirrorToLeft)
                {
                    SpawnZonePreviewObjects(zone, previewHolder, halfLength, isRightSide: false);
                }
            }
        }

        [ContextMenu("Clear Preview Spawns")]
        public void ClearPreviewSpawns()
        {
            Transform existingHolder = transform.Find(PREVIEW_HOLDER_NAME);
            if (existingHolder != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existingHolder.gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    var selectedObjects = UnityEditor.Selection.gameObjects;
                    if (selectedObjects != null)
                    {
                        foreach (var sel in selectedObjects)
                        {
                            if (sel != null && sel.transform.IsChildOf(existingHolder))
                            {
                                UnityEditor.Selection.activeGameObject = gameObject;
                                break;
                            }
                        }
                    }
                    UnityEditor.Undo.DestroyObjectImmediate(existingHolder.gameObject);
#else
                    DestroyImmediate(existingHolder.gameObject);
#endif
                }
            }
        }

        private void SpawnZonePreviewObjects(SpawnZone zone, Transform parent, float halfLength, bool isRightSide)
        {
            if (zone.prefabs == null || zone.prefabs.Length == 0) return;

            List<Vector3> spawnedLocalPositions = new List<Vector3>();
            int attempts = zone.spawnCount * 10;
            int spawnedCount = 0;

            for (int i = 0; i < attempts && spawnedCount < zone.spawnCount; i++)
            {
                float localX = Random.Range(zone.MinX, zone.MaxX);
                if (!isRightSide) localX = -localX;

                float localZ = Random.Range(-halfLength, halfLength);
                Vector3 candidateLocalPos = new Vector3(localX, 0f, localZ);

                bool valid = true;
                for (int p = 0; p < spawnedLocalPositions.Count; p++)
                {
                    if (Vector3.Distance(spawnedLocalPositions[p], candidateLocalPos) < zone.minSpacing)
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid) continue;

                spawnedLocalPositions.Add(candidateLocalPos);
                spawnedCount++;

                GameObject objToSpawn = zone.prefabs[Random.Range(0, zone.prefabs.Length)];
                if (objToSpawn == null) continue;

                Vector3 worldPos = transform.TransformPoint(candidateLocalPos);
                Quaternion worldRot = transform.rotation;
                if (zone.randomYRotation)
                {
                    worldRot *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                GameObject instance = Instantiate(objToSpawn, worldPos, worldRot);
                instance.transform.SetParent(parent, true);
            }
        }
    }
}
