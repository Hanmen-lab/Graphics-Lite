using HooahUtility.Model;
using System.Collections.Generic;
using UnityEngine;
using Graphics;
using MessagePack;
using Studio;
using UnityEngine.Rendering;

namespace Knife.DeferredDecals
{
    public class DecalProxy : MonoBehaviour, IFormData
    {
        public Decal SourceDecal;
        [HooahUtility.Model.Attribute.FieldName("Sorting Order")]
        [Key(0)]
        public int SortingOrderInterface
        {
            get
            {
                return SourceDecal.SortingOrder;
            }
            set
            {
                SourceDecal.UpdateSortingOrder(value);
            }
        }
        [HooahUtility.Model.Attribute.FieldName("Color")]
        [Key(1)]
        public Color InstancedColor
        {
            get
            {
                return SourceDecal.InstancedColor;
            }
            set
            {
                SourceDecal.InstancedColor = value;
            }
        }
        [HooahUtility.Model.Attribute.FieldName("Fade")]
        [Key(2)]
        [HooahUtility.Model.Attribute.PropertyRangeAttribute(0f, 1f)]
        public float Fade
        {
            get
            {
                return SourceDecal.Fade;
            }
            set
            {
                SourceDecal.Fade = value;
            }
        }

        [HooahUtility.Model.Attribute.FieldName("UV Tiling")]
        [Key(3)]
        public Vector3 UVTilingInterface
        {
            get
            {
                return new Vector3(SourceDecal.UVTiling.x, SourceDecal.UVTiling.y, 0);
            }
            set
            {
                SourceDecal.UVTiling.x = value.x;
                SourceDecal.UVTiling.y = value.y;
            }
        }
        [HooahUtility.Model.Attribute.FieldName("UV Offset")]
        [Key(4)]
        public Vector3 UVOffsetInterface
        {
            get
            {
                return new Vector3(SourceDecal.UVOffset.x, SourceDecal.UVOffset.y, 0);
            }
            set
            {
                SourceDecal.UVOffset.x = value.x;
                SourceDecal.UVOffset.y = value.y;
            }
        }
        [HooahUtility.Model.Attribute.FieldName("Show Gizmo")]
        [Key(5)]
        public bool ShowGizmo
        {
            get
            {
                return SourceDecal.ShowGizmo;
            }
            set
            {
                SourceDecal.ShowGizmo = value;
            }
        }
    }
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour
    {

        [SerializeField]
        private Material m_Material;
        public Material DecalMaterial
        {
            get
            {
                if (m_Material == null)
                    return DeferredDecalsSystem.DefaultDecalMaterial;

                return m_Material;
            }
            set
            {
                m_Material = value;
            }
        }
        public int SortingOrder;
        public Color InstancedColor = Color.white;
        public float Fade = 1f;

        public Vector2 UVTiling = new Vector2(1, 1);
        public Vector2 UVOffset = new Vector2(0, 0);
        public Vector4 UV
        {
            get
            {
                return new Vector4(UVTiling.x, UVTiling.y, UVOffset.x, UVOffset.y);
            }
            set
            {
                UVTiling.x = value.x;
                UVTiling.y = value.y;
                UVOffset.x = value.z;
                UVOffset.y = value.w;
            }
        }

        [HideInInspector]
        public float DistanceFade;

        public bool ShowGizmo = true; // Setting to control if it ever shows
        public bool NeedDrawGizmo = false; // Used internally to show when selected
        public bool ShouldShowGizmo()
        {
            return ShowGizmo && NeedDrawGizmo && Singleton<Studio.Studio>.Instance.workInfo.visibleAxis;
        }

        public MaterialPropertyBlock PropertyBlock;

        private Canvas guideInputCanvas;
        private Mesh cubeMesh;
        private Material cubeMaterial;
        private Material wireMaterial;
        private Vector3[] vertices;
        private int[][] edges;
        private static Color gizmocolor = new Color(0.0f, 0.7f, 1f, 1.0f);
        private CommandBuffer gizmoCommandBuffer;

        public Bounds Bounds
        {
            get
            {
                return decalBounds;
            }
            set
            {
                decalBounds = value;
            }
        }

        public Transform CachedTransform
        {
            get
            {
                return cachedTransform;
            }
        }

        Bounds decalBounds;

        Vector3 cachedPosition;
        Vector3 cachedSize;
        Quaternion cachedRotation;

        bool needRebuildBounds = false;

        Transform cachedTransform;

        GameObject decalGameObject;

        public GameObject DecalGameObject
        {
            get
            {
                if (decalGameObject == null)
                    decalGameObject = gameObject;

                return decalGameObject;
            }

            private set
            {
                decalGameObject = value;
            }
        }

        bool IsEnabledAndActive
        {
            get
            {
                return enabled && DecalGameObject.activeInHierarchy;
            }
        }

        public void OnEnable()
        {
            SetupGizmoCommandBuffer();

            cachedTransform = transform;
            if (IsEnabledAndActive)
                DeferredDecalsManager.instance.AddOrUpdateDecal(this);

#if UNITY_EDITOR
            var egu = typeof(UnityEditor.EditorGUIUtility);
            var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            var args = new object[] { gameObject, Icon };
            var setIcon = egu.GetMethod("SetIconForObject", flags, null, new System.Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
            setIcon.Invoke(null, args);
#endif
            PropertyBlock = new MaterialPropertyBlock();
        }

        private void SetupGizmoCommandBuffer()
        {
            if (gizmoCommandBuffer != null)
                return;

            gizmoCommandBuffer = new CommandBuffer();
            gizmoCommandBuffer.name = "Gizmo Overlay";

            // Добавляем команды после всех post-processing эффектов
            Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, gizmoCommandBuffer);
        }

        LineRenderer MaterialProxy;
        DecalProxy SettingsProxy;
        public void Awake()
        {
            MaterialProxy = gameObject.AddComponent<LineRenderer>();
            MaterialProxy.positionCount = 0;

            if (DecalMaterial != null) // This shouldn't ever end up null, but why not check.
            {
                MaterialProxy.material = DecalMaterial;
                DecalMaterial = MaterialProxy.material;
            }
            else
            {
                Debug.LogWarning("DecalMaterial is null. Assign a material to avoid issues with the LineRenderer.");
            }

            SettingsProxy = transform.root.gameObject.AddComponent<DecalProxy>();
            SettingsProxy.SourceDecal = this;
        }
        public void Start()
        {
            if (IsEnabledAndActive)
                DeferredDecalsManager.instance.AddOrUpdateDecal(this);

            if (cubeMaterial == null || wireMaterial == null)
                {
                    cubeMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                    wireMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                }

            InitializeCubeData();
            cubeMesh = CreateCubeMesh();

            guideInputCanvas = GameObject.Find("Canvas Guide Input")?.GetComponent<Canvas>();
        }

        private void OnRenderObject()
        {
            if (Camera.current != Camera.main)
                return;

            if (!IsVisibleFromCamera(Camera.current))
                return;

            if (guideInputCanvas == null || guideInputCanvas.gameObject.activeInHierarchy)
            {
                UpdateGizmoCommandBuffer();
            }
        }

        private bool IsVisibleFromCamera(Camera cam)
        {
            if (cam == null) return false;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, Bounds);
        }

        public void OnDisable()
        {
            if (gizmoCommandBuffer != null && Camera.main != null)
            {
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, gizmoCommandBuffer);
                gizmoCommandBuffer.Release();
                gizmoCommandBuffer = null;
            }

            DeferredDecalsManager.instance.RemoveDecal(this);
        }

        // Добавляем поля для кеширования
        private Mesh cachedGizmoMesh;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private Vector3 lastScale;
        private bool meshCacheValid = false;

        private void UpdateGizmoCommandBuffer()
        {
            if (gizmoCommandBuffer == null)
                return;

            gizmoCommandBuffer.Clear();
            gizmoCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            // Создаем единый mesh для всех элементов гизмо с кешированием
            Mesh gizmoMesh = CreateCombinedGizmoMesh();

            if (gizmoMesh != null)
            {
                Material gizmoMat = GetOrCreateGizmoMaterial();
                gizmoCommandBuffer.DrawMesh(gizmoMesh, Matrix4x4.identity, gizmoMat, 0, 0);
            }
        }

        private Mesh CreateCombinedGizmoMesh()
        {
            if (!ShouldShowGizmo())
                return null;

            // Проверяем, нужно ли пересоздавать mesh
            if (cachedGizmoMesh != null && IsMeshCacheValid())
                return cachedGizmoMesh;

            // Уничтожаем старый mesh
            if (cachedGizmoMesh != null)
            {
                DestroyImmediate(cachedGizmoMesh);
                cachedGizmoMesh = null;
            }

            // Создаем новый mesh
            cachedGizmoMesh = CreateGizmoMeshInternal();

            // Обновляем кеш трансформации
            UpdateTransformCache();
            meshCacheValid = true;

            return cachedGizmoMesh;
        }

        private bool IsMeshCacheValid()
        {
            if (!meshCacheValid)
                return false;

            Transform t = CachedTransform;
            return lastPosition == t.position &&
                   lastRotation == t.rotation &&
                   lastScale == t.localScale;
        }

        private void UpdateTransformCache()
        {
            Transform t = CachedTransform;
            lastPosition = t.position;
            lastRotation = t.rotation;
            lastScale = t.localScale;
        }

        private Mesh CreateGizmoMeshInternal()
        {
            // Предварительно вычисляем размер массивов для оптимизации
            int estimatedVertexCount = edges.Length * 2 + 2 + 12 * 4; // куб + стрелка + конус
            List<Vector3> allVertices = new List<Vector3>(estimatedVertexCount);
            List<int> allIndices = new List<int>(estimatedVertexCount);
            List<Color> allColors = new List<Color>(estimatedVertexCount);

            int currentVertexIndex = 0;

            // Добавляем данные wireframe куба
            AddWireCubeToMesh(allVertices, allIndices, allColors, ref currentVertexIndex);

            // Добавляем данные стрелки
            AddArrowToMesh(allVertices, allIndices, allColors, ref currentVertexIndex);

            if (allVertices.Count == 0)
                return null;

            Mesh mesh = new Mesh();
            mesh.name = "CombinedGizmoMesh";
            mesh.vertices = allVertices.ToArray();
            mesh.colors = allColors.ToArray();
            mesh.SetIndices(allIndices.ToArray(), MeshTopology.Lines, 0);

            return mesh;
        }

        // Метод для принудительного обновления кеша (вызывать при изменении настроек)
        public void InvalidateMeshCache()
        {
            meshCacheValid = false;
        }

        // Обновляем метод OnDestroy
        private void OnDestroy()
        {
            if (cachedGizmoMesh != null)
            {
                DestroyImmediate(cachedGizmoMesh);
                cachedGizmoMesh = null;
            }

            if (gizmoMaterial != null)
            {
                DestroyImmediate(gizmoMaterial);
                gizmoMaterial = null;
            }
        }

        // Дополнительно: метод для отслеживания изменений в инспекторе
        private void OnValidate()
        {
            // Сбрасываем кеш при изменении настроек в инспекторе
            InvalidateMeshCache();
        }

        private void AddWireCubeToMesh(List<Vector3> vertices, List<int> indices, List<Color> colors, ref int currentVertexIndex)
        {
            Matrix4x4 matrix = CachedTransform.localToWorldMatrix;
            Vector3 offset = Vector3.down * 0.5f;

            foreach (var edge in edges)
            {
                Vector3 start = matrix.MultiplyPoint3x4(this.vertices[edge[0]] + offset);
                Vector3 end = matrix.MultiplyPoint3x4(this.vertices[edge[1]] + offset);

                if (IsValidVertex(start) && IsValidVertex(end))
                {
                    vertices.Add(start);
                    vertices.Add(end);

                    colors.Add(gizmocolor);
                    colors.Add(gizmocolor);

                    indices.Add(currentVertexIndex);
                    indices.Add(currentVertexIndex + 1);

                    currentVertexIndex += 2;
                }
            }
        }

        private void AddArrowToMesh(List<Vector3> vertices, List<int> indices, List<Color> colors, ref int currentVertexIndex)
        {
            Vector3 start = CachedTransform.position;
            Vector3 direction = -CachedTransform.up;
            float arrowLength = CachedTransform.localScale.y;
            Vector3 end = start + direction * arrowLength;

            // Основная линия стрелки
            vertices.Add(start);
            vertices.Add(end);
            colors.Add(gizmocolor);
            colors.Add(gizmocolor);
            indices.Add(currentVertexIndex);
            indices.Add(currentVertexIndex + 1);
            currentVertexIndex += 2;

            // Добавляем линии для головки стрелки
            AddArrowHeadToMesh(vertices, indices, colors, ref currentVertexIndex, end, direction, CachedTransform.localScale);
        }

        private void AddArrowHeadToMesh(List<Vector3> vertices, List<int> indices, List<Color> colors, ref int currentVertexIndex,
            Vector3 position, Vector3 direction, Vector3 scale)
        {
            float coneHeight = 0.2f * scale.y;
            float coneRadius = 0.1f * scale.x;
            int segments = 12; // Уменьшено для оптимизации

            // Определяем базисные векторы для конуса
            Vector3 up = (Mathf.Abs(Vector3.Dot(direction.normalized, Vector3.up)) > 0.99f) ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(direction.normalized, up).normalized;
            up = Vector3.Cross(right, direction.normalized).normalized;

            Vector3 coneBase = position - direction.normalized * coneHeight;
            Vector3 coneTip = position;

            // Создаем вершины основания конуса
            Vector3[] baseVertices = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                Vector3 offset = right * Mathf.Cos(angle) * coneRadius + up * Mathf.Sin(angle) * coneRadius;
                baseVertices[i] = coneBase + offset;
            }

            // Добавляем линии от кончика к основанию (создаем wireframe конуса)
            for (int i = 0; i < segments; i++)
            {
                // Линия от кончика к текущей вершине основания
                vertices.Add(coneTip);
                vertices.Add(baseVertices[i]);
                colors.Add(gizmocolor);
                colors.Add(gizmocolor);
                indices.Add(currentVertexIndex);
                indices.Add(currentVertexIndex + 1);
                currentVertexIndex += 2;

                // Линия по периметру основания
                int nextIndex = (i + 1) % segments;
                vertices.Add(baseVertices[i]);
                vertices.Add(baseVertices[nextIndex]);
                colors.Add(gizmocolor);
                colors.Add(gizmocolor);
                indices.Add(currentVertexIndex);
                indices.Add(currentVertexIndex + 1);
                currentVertexIndex += 2;
            }
        }

        private Material gizmoMaterial;

        private Material GetOrCreateGizmoMaterial()
        {
            if (gizmoMaterial == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                if (shader == null)
                    shader = Shader.Find("Sprites/Default");

                gizmoMaterial = new Material(shader);
                gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
                gizmoMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                gizmoMaterial.SetInt("_ZWrite", 0);
                gizmoMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                gizmoMaterial.color = gizmocolor;
            }
            return gizmoMaterial;
        }

        private bool IsValidVertex(Vector3 vertex)
        {
            return !float.IsNaN(vertex.x) && !float.IsNaN(vertex.y) && !float.IsNaN(vertex.z) &&
                   !float.IsInfinity(vertex.x) && !float.IsInfinity(vertex.y) && !float.IsInfinity(vertex.z);
        }

        public void UpdateSortingOrder(int order)
        {
            SortingOrder = order;
            DeferredDecalsManager.instance.AddOrUpdateDecal(this);
        }

        public void SetupBounds()
        {
            if (!needRebuildBounds)
                return;

            float sideX = 0.5f;
            float sideY = 0.5f;
            float sideZ = 0.5f;

            Vector3 p1 = new Vector3(-sideX, -sideY * 2, -sideZ);
            Vector3 p2 = new Vector3(-sideX, -sideY * 2, sideZ);
            Vector3 p3 = new Vector3(sideX, -sideY * 2, sideZ);
            Vector3 p4 = new Vector3(sideX, -sideY * 2, -sideZ);
            Vector3 p5 = new Vector3(-sideX, 0, -sideZ);
            Vector3 p6 = new Vector3(-sideX, 0, sideZ);
            Vector3 p7 = new Vector3(sideX, 0, sideZ);
            Vector3 p8 = new Vector3(sideX, 0, -sideZ); // Исправлено: было sideZ, должно быть -sideZ

            p1 = CachedTransform.TransformPoint(p1);
            p2 = CachedTransform.TransformPoint(p2);
            p3 = CachedTransform.TransformPoint(p3);
            p4 = CachedTransform.TransformPoint(p4);
            p5 = CachedTransform.TransformPoint(p5);
            p6 = CachedTransform.TransformPoint(p6);
            p7 = CachedTransform.TransformPoint(p7);
            p8 = CachedTransform.TransformPoint(p8);

            float minX = Mathf.Min(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
            float minY = Mathf.Min(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);
            float minZ = Mathf.Min(p1.z, p2.z, p3.z, p4.z, p5.z, p6.z, p7.z, p8.z);

            float maxX = Mathf.Max(p1.x, p2.x, p3.x, p4.x, p5.x, p6.x, p7.x, p8.x);
            float maxY = Mathf.Max(p1.y, p2.y, p3.y, p4.y, p5.y, p6.y, p7.y, p8.y);
            float maxZ = Mathf.Max(p1.z, p2.z, p3.z, p4.z, p5.z, p6.z, p7.z, p8.z);

            Vector3 min = new Vector3(minX, minY, minZ);
            Vector3 max = new Vector3(maxX, maxY, maxZ);

            var bounds = Bounds;
            bounds.SetMinMax(min, max);
            Bounds = bounds;

            needRebuildBounds = false;
        }

        public void UpdateBoundsCenter()
        {
            var bounds = Bounds;
            bounds.center = CachedTransform.position - CachedTransform.up * 0.5f * cachedSize.y;
            Bounds = bounds;
        }

        //private void DrawGizmo(bool selected)
        //{
        //    // for bounds rendering
        //    var oldMatrix = Gizmos.matrix;

        //    if (ShouldShowGizmo() && selected)
        //    {
        //        var col = new Color(0.0f, 0.7f, 1f, 1.0f);
        //        col.a = selected ? 0.1f : 0.05f;
        //        if (!selected)
        //        {
        //            col.a = 0;
        //        }
        //        Gizmos.color = col;
        //        Gizmos.matrix = CachedTransform.localToWorldMatrix;
        //        Gizmos.DrawCube(Vector3.zero - Vector3.up * 0.5f, Vector3.one);

        //        col.a = selected ? 0.5f : 0.25f;
        //        if (!DeferredDecalsSystem.DrawDecalGizmos && !selected)
        //        {
        //            col.a = 0;
        //        }
        //        Gizmos.color = col;
        //        Gizmos.DrawWireCube(Vector3.zero - Vector3.up * 0.5f, Vector3.one);
        //    }
        //    // bounds rendering
        //    /*Gizmos.matrix = oldMatrix;
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireCube(Bounds.center, Bounds.size);*/
        //}

        private void Update()
        {
            if (Vector3.Distance(cachedPosition, CachedTransform.position) >= 0.001f)
            {
                cachedPosition = CachedTransform.position;
                UpdateBoundsCenter();
            }

            if (Vector3.Distance(cachedSize, CachedTransform.localScale) >= 0.001f)
            {
                needRebuildBounds = true;
                cachedSize = CachedTransform.localScale;
            }

            if (Quaternion.Angle(cachedRotation, CachedTransform.rotation) >= 0.1f)
            {
                needRebuildBounds = true;
                cachedRotation = CachedTransform.rotation;
            }
            if (ShouldShowGizmo())
            {
                DrawCustomGizmo(true);
            }
            // Проверяем изменения трансформации каждый кадр
            if (meshCacheValid && !IsMeshCacheValid())
            {
                meshCacheValid = false;
            }
        }

        private void InitializeCubeData()
        {
            vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f)
            };

            edges = new int[][]
            {
                new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 0 },
                new int[] { 4, 5 }, new int[] { 5, 6 }, new int[] { 6, 7 }, new int[] { 7, 4 },
                new int[] { 0, 4 }, new int[] { 1, 5 }, new int[] { 2, 6 }, new int[] { 3, 7 }
            };
        }

        private void DrawCustomGizmo(bool selected)
        {
            //if ()

            if (guideInputCanvas == null || guideInputCanvas.gameObject.activeInHierarchy && ShouldShowGizmo() || !selected)
            {
                var col = gizmocolor;
                col.a = selected ? 0.1f : 0.05f;
                if (!DeferredDecalsSystem.DrawDecalGizmos && !selected)
                {
                    col.a = 0;
                }
                cubeMaterial.color = col;

                UnityEngine.Graphics.DrawMesh(cubeMesh, CachedTransform.localToWorldMatrix * Matrix4x4.Translate(Vector3.down * 0.5f), cubeMaterial, 0);

            }
        }

        private Mesh CreateCubeMesh()
        {
            Mesh cube = new Mesh
            {
                vertices = vertices,
                triangles = new int[]
                {
                    0, 2, 1, 0, 3, 2,
                    4, 5, 6, 4, 6, 7,
                    0, 1, 5, 0, 5, 4,
                    2, 3, 7, 2, 7, 6,
                    1, 2, 6, 1, 6, 5,
                    0, 4, 7, 0, 7, 3
                }
            };
            cube.RecalculateNormals();
            return cube;
        }

        //public void OnDrawGizmos()
        //{
        //    DrawGizmo(false);
        //}
        //public void OnDrawGizmosSelected()
        //{
        //    DrawGizmo(true);
        //}

#if UNITY_EDITOR
        static Texture2D icon;
        public static Texture2D Icon
        {
            get
            {
                if (icon == null)
                    icon = Resources.Load<Texture2D>("Knife.DecalIcon");

                return icon;
            }
        }
#endif
    }
}