using Marketplace.Modules.NPC;
using UnityEngine.PostProcessing;

namespace Marketplace;

public static class PhotoManager
{
    public static readonly ScreenshotStuff __instance = new();
    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int Value = Shader.PropertyToID("_Value");

    public class ScreenshotStuff
    {
        private readonly GameObject INACTIVE;
        private readonly Camera rendererCamera;
        private readonly Light Light;
        private readonly Dictionary<long, Sprite> CachedSprites = new();
        private static readonly Vector3 SpawnPoint = new(10000f, 10000f, 10000f);


        public Sprite GetSprite(string prefab, Sprite defaultValue, int level)
        {
            int initHashcode = prefab.GetStableHashCode();
            if (CachedSprites.TryGetValue(initHashcode, out Sprite sprite))
                return sprite;
            level = Mathf.Clamp(level, 1, 3);
            int hashcode = prefab.GetStableHashCode() + level;
            if (CachedSprites.TryGetValue(hashcode, out Sprite sprite1))
                return sprite1;
            return defaultValue;
        }

        private class RenderObject
        {
            public readonly GameObject Spawn;
            public readonly Vector3 Size;
            public RenderRequest Request;

            public RenderObject(GameObject spawn, Vector3 size)
            {
                Spawn = spawn;
                Size = size;
            }
        }

        private class RenderRequest
        {
            public readonly GameObject Target;
            public int Level = 1;
            public int Width { get; set; } = 128;
            public int Height { get; set; } = 128;
            public Quaternion Rotation { get; set; } = Quaternion.Euler(0f, -24f, 0); //25.8f);
            public float FieldOfView { get; set; } = 0.5f;
            public float offset = 0.25f;

            public float DistanceMultiplier { get; set; } = 1f;

            public RenderRequest(GameObject target)
            {
                Target = target;
            }
        }

        private const int MAINLAYER = 29;

        public ScreenshotStuff()
        {
            INACTIVE = new GameObject("INACTIVEscreenshotHelper")
            {
                layer = MAINLAYER,
                transform =
                {
                    localScale = Vector3.one
                }
            };
            INACTIVE.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(INACTIVE);
            rendererCamera = new GameObject("Render Camera", typeof(Camera)).GetComponent<Camera>();
            rendererCamera.backgroundColor = new Color(0, 0, 0, 0);
            rendererCamera.clearFlags = CameraClearFlags.SolidColor;
            rendererCamera.transform.position = SpawnPoint;
            rendererCamera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rendererCamera.fieldOfView = 0.5f;
            rendererCamera.farClipPlane = 100000;
            rendererCamera.cullingMask = 1 << MAINLAYER;
            UnityEngine.Object.DontDestroyOnLoad(rendererCamera);

            Light = new GameObject("Render Light", typeof(Light)).GetComponent<Light>();
            Light.transform.position = SpawnPoint;
            Light.transform.rotation = Quaternion.Euler(5f, 180f, 5f);
            Light.type = LightType.Directional;
            Light.intensity = 0.5f;
            Light.cullingMask = 1 << MAINLAYER;
            UnityEngine.Object.DontDestroyOnLoad(Light);

            rendererCamera.gameObject.SetActive(false);
            Light.gameObject.SetActive(false);
        }

        private void ClearRendering()
        {
            rendererCamera.gameObject.SetActive(false);
            Light.gameObject.SetActive(false);
        }

        private static bool IsVisualComponent(Component component)
        {
            return component is Renderer or MeshFilter or Transform or Animator or LevelEffects;
        }


        private GameObject SpawnAndRemoveComponents(RenderRequest obj)
        {
            GameObject tempObj = UnityEngine.Object.Instantiate(obj.Target, INACTIVE.transform);
            List<Component> components = tempObj.GetComponents<Component>().ToList();
            components.AddRange(tempObj.GetComponentsInChildren<Component>(true));
            List<Component> ToRemove = new List<Component>();
            foreach (Component comp in components)
            {
                if (!IsVisualComponent(comp))
                {
                    ToRemove.Add(comp);
                }
            }

            ToRemove.Reverse();
            ToRemove.ForEach(UnityEngine.Object.DestroyImmediate);
            GameObject retObj = UnityEngine.Object.Instantiate(tempObj);
            retObj.layer = MAINLAYER;
            foreach (Transform VARIABLE in retObj.GetComponentsInChildren<Transform>())
            {
                VARIABLE.gameObject.layer = MAINLAYER;
            }

            Animator animator = retObj.GetComponentInChildren<Animator>();
            if (animator)
            {
                if (animator.HasState(0, Movement))
                    animator.Play(Movement);
                animator.Update(0f);
            }

            LevelEffects LevelEffects = retObj.GetComponentInChildren<LevelEffects>();
            if (LevelEffects)
            {
                obj.Level = Mathf.Clamp(obj.Level, 1, 3);
                if (obj.Level > 1)
                {
                    SETLEVEL(LevelEffects, obj.Level, obj.Target.name);
                }
            }
            else
            {
                obj.Level = 0;
            }

            retObj.SetActive(true);
            retObj.name = obj.Target.name;
            UnityEngine.Object.Destroy(tempObj);
            return retObj;
        }

        private void SETLEVEL(LevelEffects effects, int level, string prefab)
        {
            if (effects.m_levelSetups.Count >= level - 1)
            {
                LevelEffects.LevelSetup levelSetup = effects.m_levelSetups[level - 2];
                if (effects.m_mainRender)
                {
                    string key = prefab + level;
                    if (LevelEffects.m_materials.TryGetValue(key, out Material material))
                    {
                        Material[] sharedMaterials = effects.m_mainRender.sharedMaterials;
                        sharedMaterials[0] = material;
                        effects.m_mainRender.sharedMaterials = sharedMaterials;
                    }
                    else
                    {
                        Material[] sharedMaterials2 = effects.m_mainRender.sharedMaterials;
                        sharedMaterials2[0] = new Material(sharedMaterials2[0]);
                        sharedMaterials2[0].SetFloat(Hue, levelSetup.m_hue);
                        sharedMaterials2[0].SetFloat(Saturation, levelSetup.m_saturation);
                        sharedMaterials2[0].SetFloat(Value, levelSetup.m_value);
                        effects.m_mainRender.sharedMaterials = sharedMaterials2;
                        LevelEffects.m_materials[key] = sharedMaterials2[0];
                    }
                }

                if (effects.m_baseEnableObject)
                {
                    effects.m_baseEnableObject.SetActive(false);
                }

                if (levelSetup.m_enableObject)
                {
                    levelSetup.m_enableObject.SetActive(true);
                }
            }
        }

        private static readonly int Movement = Animator.StringToHash("Movement");
        

        public void MakeSprite(GameObject prefabArg, float scale, float offset, int level = 1)
        {
            level = Mathf.Clamp(level, 1, 3);
            try
            {
                int hashcode = prefabArg.name.GetStableHashCode();
                if (CachedSprites.ContainsKey(hashcode) || CachedSprites.ContainsKey(hashcode + level)) return;
                rendererCamera.gameObject.SetActive(true);
                Light.gameObject.SetActive(true);
                RenderRequest request = new(prefabArg) { Level = level, DistanceMultiplier = scale, offset = offset };
                GameObject spawn = SpawnAndRemoveComponents(request);
                spawn.transform.position = Vector3.zero;
                spawn.transform.rotation = request.Rotation;

                Vector3 min = new Vector3(1000f, 1000f, 1000f);
                Vector3 max = new Vector3(-1000f, -1000f, -1000f);
                foreach (Renderer meshRenderer in spawn.GetComponentsInChildren<Renderer>())
                {
                    if (meshRenderer is ParticleSystemRenderer) continue;
                    min = Vector3.Min(min, meshRenderer.bounds.min);
                    max = Vector3.Max(max, meshRenderer.bounds.max);
                }

                spawn.transform.position = SpawnPoint - (min + max) / 2f;
                Vector3 size = new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y),
                    Mathf.Abs(min.z) + Mathf.Abs(max.z));
                TimedDestruction timedDestruction = spawn.AddComponent<TimedDestruction>();
                timedDestruction.Trigger(1f);

                RenderObject go = new RenderObject(spawn, size)
                {
                    Request = request
                };
                RenderSprite(go);
                ClearRendering();
            }
            catch (Exception ex)
            {
                ClearRendering();
                Utils.print(ex, ConsoleColor.Red);
                CachedSprites[prefabArg.name.GetStableHashCode()] = AssetStorage.AssetStorage.PlaceholderMonsterIcon;
            }
        }

        

        private void RenderSprite(RenderObject renderObject)
        {
            int width = renderObject.Request.Width;
            int height = renderObject.Request.Height;

            RenderTexture oldRenderTexture = RenderTexture.active;
            RenderTexture temp = RenderTexture.GetTemporary(width, height, 32);
            rendererCamera.targetTexture = temp;
            rendererCamera.fieldOfView = renderObject.Request.FieldOfView;
            RenderTexture.active = rendererCamera.targetTexture;

            renderObject.Spawn.SetActive(true);
            float maxMeshSize = Mathf.Max(renderObject.Size.x, renderObject.Size.y) + 0.1f;
            float distance = maxMeshSize / Mathf.Tan(rendererCamera.fieldOfView * Mathf.Deg2Rad) *
                             renderObject.Request.DistanceMultiplier;
            rendererCamera.transform.position = SpawnPoint + new Vector3(0, renderObject.Request.offset, distance);
            rendererCamera.Render();
            renderObject.Spawn.SetActive(false);
            UnityEngine.Object.Destroy(renderObject.Spawn);
            Texture2D previewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
            previewImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            previewImage.Apply();
            RenderTexture.active = oldRenderTexture;
            rendererCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(temp);
            rendererCamera.gameObject.SetActive(false);
            Light.gameObject.SetActive(false);
            Sprite newSprite = Sprite.Create(previewImage, new Rect(0, 0, width, height), Vector2.one / 2f);
            CachedSprites[renderObject.Spawn.name.GetStableHashCode() + renderObject.Request.Level] = newSprite;
        }
        
        
         public string NPC_Photo(Market_NPC.NPCcomponent npc)
        {
            Light.gameObject.SetActive(true);
            Vector3 prevPos = npc.transform.position;
            Quaternion prevRot = npc.transform.rotation;
            npc.gameObject.layer = MAINLAYER;
            foreach (Transform VARIABLE in npc.GetComponentsInChildren<Transform>())
            {
                VARIABLE.gameObject.layer = MAINLAYER;
            }

            npc.transform.position = Vector3.zero;
            npc.transform.Find("TMP").gameObject.SetActive(false);
            npc.transform.rotation = Quaternion.Euler(0f, -24f, 0);
            Vector3 min = new Vector3(1000f, 1000f, 1000f);
            Vector3 max = new Vector3(-1000f, -1000f, -1000f);
            foreach (Renderer meshRenderer in npc.GetComponentsInChildren<Renderer>())
            {
                if (meshRenderer is ParticleSystemRenderer) continue;
                min = Vector3.Min(min, meshRenderer.bounds.min);
                max = Vector3.Max(max, meshRenderer.bounds.max);
            }

            npc.transform.position = SpawnPoint - (min + max) / 2f;

            Vector3 size = new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y),
                Mathf.Abs(min.z) + Mathf.Abs(max.z));

            const int width = 256;
            const int height = 256;

            RenderTexture oldRenderTexture = RenderTexture.active;
            RenderTexture temp = RenderTexture.GetTemporary(width, height, 32);
            Color backgroundColor = GameCamera.instance.m_camera.backgroundColor;
            CameraClearFlags prevFlag = GameCamera.instance.m_camera.clearFlags;
            float prevFov = GameCamera.instance.m_camera.fieldOfView;
            float prevClip = GameCamera.instance.m_camera.farClipPlane;
            int prevCulling = GameCamera.instance.m_camera.cullingMask;
            GameCamera.instance.m_camera.targetTexture = temp;
            GameCamera.instance.m_camera.backgroundColor = new Color(0, 0, 0, 0);
            GameCamera.instance.m_camera.clearFlags = CameraClearFlags.SolidColor;
            GameCamera.instance.m_camera.fieldOfView = 0.5f;
            GameCamera.instance.m_camera.farClipPlane = 100000;
            GameCamera.instance.m_camera.cullingMask = 1 << MAINLAYER;
            RenderTexture.active = GameCamera.instance.m_camera.targetTexture;
            float maxMeshSize = Mathf.Max(size.x, size.y) + 0.1f;
            float distance = maxMeshSize / Mathf.Tan(rendererCamera.fieldOfView * Mathf.Deg2Rad) * 1f;
            GameCamera.instance.m_camera.transform.position = SpawnPoint + new Vector3(0, 0, distance);
            GameCamera.instance.m_camera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            GameCamera.instance.GetComponent<PostProcessingBehaviour>().enabled = false;
            GameCamera.instance.m_camera.Render();
            Texture2D previewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
            previewImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            previewImage.Apply();
            GameCamera.instance.GetComponent<PostProcessingBehaviour>().enabled = true;
            RenderTexture.active = oldRenderTexture;
            GameCamera.instance.m_camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(temp);
            Light.gameObject.SetActive(false);
            GameCamera.instance.m_camera.backgroundColor = backgroundColor;
            GameCamera.instance.m_camera.clearFlags = prevFlag;
            GameCamera.instance.m_camera.fieldOfView = prevFov;
            GameCamera.instance.m_camera.farClipPlane = prevClip;
            GameCamera.instance.m_camera.cullingMask = prevCulling;
            npc.transform.position = prevPos;
            npc.transform.rotation = prevRot;
            npc.transform.Find("TMP").gameObject.SetActive(true);
            npc.gameObject.layer = LayerMask.NameToLayer("character");
            foreach (Transform VARIABLE in npc.GetComponentsInChildren<Transform>())
            {
                VARIABLE.gameObject.layer = LayerMask.NameToLayer("character");
            }

            return Convert.ToBase64String(previewImage.EncodeToPNG());
        }
        
    }
}