using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Audio; // Required for Mixer Groups

namespace EmberVFX
{
    // ---------------------------------------------------------
    // 1. METADATA ENUMS
    // ---------------------------------------------------------
    public enum EffectCategory { Spell, Character, Environment, Interaction, StatusEffect, UI, Weapon, Explosion, Misc }
    public enum EffectIntensity { Low, Medium, High, Extreme }
    public enum EffectDurationType { Instant, Loop, Sustained }
    public enum EffectCost { Cheap, Moderate, Expensive }

    [System.Flags]
    public enum EffectPlatform
    {
        None = 0,
        Mobile = 1 << 0,
        PC = 1 << 1,
        Console = 1 << 2,
        VR = 1 << 3,
        WebGL = 1 << 4
    }

    public enum VFXSystemType { Shuriken, VFXGraph }
    public enum EmberColliderType { None, Sphere, Box }

    // ---------------------------------------------------------
    // 2. DATA CLASSES
    // ---------------------------------------------------------

    [Serializable]
    public class EmberMetadata
    {
        public string author = "User";
        public string version = "1.0";
        public EffectCategory category;
        public string context = "None";
        public EffectIntensity intensity;
        public EffectDurationType durationType;
        public EffectCost performanceCost;
        public EffectPlatform supportedPlatforms = EffectPlatform.PC | EffectPlatform.Console;
    }

    [Serializable]
    public class EmberEffectItem
    {
        [Tooltip("Unique Identifier for code references.")]
        public string id;
        [Tooltip("The Key used to spawn this effect.")]
        public string effectName = "New Effect";

        public EmberMetadata metadata = new EmberMetadata();

        public VFXSystemType systemType;
        public ParticleSystem shurikenPrefab;
        public VisualEffect vfxGraphPrefab;

        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup; // NEW: Audio Mixer Support
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Randomizes pitch (0.9 - 1.1) to avoid repetition fatigue.")]
        public bool randomizePitch = true;

        public EmberColliderType colliderType;
        public bool isTrigger = true;
        public Vector3 colliderSize = Vector3.one;

        public bool useScaling;
        public float scaleDuration = 1f;
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("How many instances to create on Game Start to prevent lag.")]
        public int prewarmCount = 5;
        [Tooltip("Max instances allowed. Older ones will be recycled or spawning stops.")]
        public int maxPoolSize = 50;
        [Tooltip("Force a specific lifetime. Set to 0 to calculate automatically.")]
        public float durationOverride = 0f;
    }

    // ---------------------------------------------------------
    // 3. THE INSTANCE
    // ---------------------------------------------------------
    [RequireComponent(typeof(AudioSource))]
    public class EmberInstance : MonoBehaviour
    {
        private EmberEffectItem _data;
        private float _timer;
        private Collider _col;
        private AudioSource _audioSource;
        private Action<EmberInstance> _returnToPoolCallback;

        // NEW: Exposed properties for modification via code
        public ParticleSystem ParticleSystemComponent { get; private set; }
        public VisualEffect VFXGraphComponent { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void Initialize(EmberEffectItem data, Action<EmberInstance> returnCallback)
        {
            _data = data;
            _returnToPoolCallback = returnCallback;
            _timer = 0f;

            // Reset Transform logic handles parenting in Manager, but we ensure scale is 1
            if (transform.parent == null) transform.localScale = Vector3.one;
            else transform.localScale = Vector3.one; // Local scale relative to parent

            // Cache components for Property Overrides
            if (data.systemType == VFXSystemType.Shuriken && ParticleSystemComponent == null)
                ParticleSystemComponent = GetComponent<ParticleSystem>();
            else if (data.systemType == VFXSystemType.VFXGraph && VFXGraphComponent == null)
                VFXGraphComponent = GetComponent<VisualEffect>();

            // Audio
            if (data.clips != null && data.clips.Length > 0)
            {
                _audioSource.clip = data.clips[UnityEngine.Random.Range(0, data.clips.Length)];
                _audioSource.outputAudioMixerGroup = data.mixerGroup; // NEW: Apply Mixer
                _audioSource.volume = data.volume;
                _audioSource.pitch = data.randomizePitch ? UnityEngine.Random.Range(0.9f, 1.1f) : 1f;
                _audioSource.spatialBlend = 1f;
                _audioSource.Play();
            }

            // Collider
            SetupCollider(data);

            // Lifecycle
            float lifeTime = 5f;

            if (data.durationOverride > 0)
            {
                lifeTime = data.durationOverride;
            }
            else
            {
                if (data.systemType == VFXSystemType.Shuriken && data.shurikenPrefab != null)
                {
                    lifeTime = data.shurikenPrefab.main.duration;
                    if (!data.shurikenPrefab.main.loop) lifeTime += data.shurikenPrefab.main.startLifetime.constantMax;
                }
                else if (data.systemType == VFXSystemType.VFXGraph)
                {
                    lifeTime = 3f;
                }
            }

            StopAllCoroutines();
            StartCoroutine(ReturnToPoolRoutine(lifeTime));
        }

        private IEnumerator ReturnToPoolRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_returnToPoolCallback != null) _returnToPoolCallback(this);
        }

        void SetupCollider(EmberEffectItem data)
        {
            if (data.colliderType == EmberColliderType.None)
            {
                if (_col != null) _col.enabled = false;
                return;
            }

            if (_col == null)
            {
                if (data.colliderType == EmberColliderType.Box) _col = gameObject.AddComponent<BoxCollider>();
                else if (data.colliderType == EmberColliderType.Sphere) _col = gameObject.AddComponent<SphereCollider>();
            }

            if (_col is BoxCollider box)
            {
                box.isTrigger = data.isTrigger;
                box.size = data.colliderSize;
            }
            else if (_col is SphereCollider sphere)
            {
                sphere.isTrigger = data.isTrigger;
                sphere.radius = data.colliderSize.x / 2f;
            }
            _col.enabled = true;
        }

        void Update()
        {
            if (_data != null && _data.useScaling)
            {
                _timer += Time.deltaTime;
                float progress = Mathf.Clamp01(_timer / _data.scaleDuration);
                float scaleMult = _data.scaleCurve.Evaluate(progress);
                transform.localScale = Vector3.one * scaleMult;
            }
        }
    }

    // ---------------------------------------------------------
    // 4. THE MANAGER
    // ---------------------------------------------------------
    public class EmberCoreVFXManagement : MonoBehaviour
    {
        public static EmberCoreVFXManagement Instance;

        [Tooltip("The list of all registered effects.")]
        public List<EmberEffectItem> library = new List<EmberEffectItem>();

        [HideInInspector]
        public List<string> availableContexts = new List<string>() { "Mechanic", "Ambient", "Cutscene", "Menu" };

        private Dictionary<string, EmberEffectItem> _lookup;
        private Dictionary<string, Queue<EmberInstance>> _pools;
        private Transform _poolContainer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSystem()
        {
            _lookup = new Dictionary<string, EmberEffectItem>();
            _pools = new Dictionary<string, Queue<EmberInstance>>();

            GameObject container = new GameObject("Ember_Pool_Container");
            container.transform.SetParent(transform);
            _poolContainer = container.transform;

            foreach (var item in library)
            {
                if (string.IsNullOrEmpty(item.effectName)) continue;

                if (_lookup.ContainsKey(item.effectName))
                {
                    Debug.LogError($"[EmberCore] Duplicate Effect Name found: '{item.effectName}'. Skipping.");
                    continue;
                }

                _lookup.Add(item.effectName, item);
                _pools.Add(item.effectName, new Queue<EmberInstance>());

                for (int i = 0; i < item.prewarmCount; i++)
                {
                    EmberInstance obj = CreateNewInstance(item);
                    obj.gameObject.SetActive(false);
                    _pools[item.effectName].Enqueue(obj);
                }
            }
        }

        // --- PUBLIC API (Now includes Parenting & Callbacks) ---

        // 1. Standard Spawn
        public void Spawn(string effectName, Vector3 position, Quaternion rotation)
        {
            InternalSpawn(effectName, position, rotation, null, null);
        }

        public void Spawn(string effectName, Vector3 position) => InternalSpawn(effectName, position, Quaternion.identity, null, null);

        // 2. Spawn with Parent (Attached Effect)
        public void Spawn(string effectName, Transform parent)
        {
            InternalSpawn(effectName, Vector3.zero, Quaternion.identity, parent, null);
        }

        // 3. Spawn with Callback (For Property Overrides like SetColor)
        public void Spawn(string effectName, Vector3 position, Quaternion rotation, Action<EmberInstance> onSpawn)
        {
            InternalSpawn(effectName, position, rotation, null, onSpawn);
        }

        // --- INTERNAL SPAWN LOGIC ---
        private void InternalSpawn(string effectName, Vector3 posOrOffset, Quaternion rot, Transform parent, Action<EmberInstance> onSpawn)
        {
            if (_lookup == null || !_lookup.ContainsKey(effectName))
            {
                Debug.LogWarning($"[EmberCore] Effect '{effectName}' not found.");
                return;
            }

            EmberEffectItem data = _lookup[effectName];
            EmberInstance instance = GetFromPool(data);

            if (instance != null)
            {
                // Handle Parenting
                if (parent != null)
                {
                    instance.transform.SetParent(parent);
                    instance.transform.localPosition = posOrOffset; // usually 0,0,0
                    instance.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    instance.transform.SetParent(_poolContainer); // Ensure it's not stuck on an old dead parent
                    instance.transform.position = posOrOffset;
                    instance.transform.rotation = rot;
                }

                instance.gameObject.SetActive(true);

                // Initialize logic
                instance.Initialize(data, ReturnToPool);

                // Execute User Callback (Property Overrides)
                if (onSpawn != null) onSpawn.Invoke(instance);
            }
        }

        private EmberInstance GetFromPool(EmberEffectItem data)
        {
            Queue<EmberInstance> pool = _pools[data.effectName];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return CreateNewInstance(data);
            }
        }

        private EmberInstance CreateNewInstance(EmberEffectItem data)
        {
            GameObject prefabToSpawn = null;

            if (data.systemType == VFXSystemType.Shuriken && data.shurikenPrefab != null)
                prefabToSpawn = data.shurikenPrefab.gameObject;
            else if (data.systemType == VFXSystemType.VFXGraph && data.vfxGraphPrefab != null)
                prefabToSpawn = data.vfxGraphPrefab.gameObject;

            if (prefabToSpawn == null)
            {
                prefabToSpawn = new GameObject($"Empty_{data.effectName}");
            }

            GameObject obj = Instantiate(prefabToSpawn, _poolContainer);
            obj.name = $"{data.effectName}_Pooled";

            EmberInstance handler = obj.AddComponent<EmberInstance>();
            return handler;
        }

        private void ReturnToPool(EmberInstance instance)
        {
            instance.gameObject.SetActive(false);
            // Reset parent to container so it doesn't get destroyed if the parent entity dies
            instance.transform.SetParent(_poolContainer);

            string key = instance.gameObject.name.Replace("_Pooled", "");

            if (_pools.ContainsKey(key))
            {
                Queue<EmberInstance> pool = _pools[key];
                EmberEffectItem data = _lookup[key];

                if (pool.Count < data.maxPoolSize)
                {
                    pool.Enqueue(instance);
                }
                else
                {
                    Destroy(instance.gameObject);
                }
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }
    }
}