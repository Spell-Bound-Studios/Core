using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Helper = SpellBound.Core.SpellBoundStaticHelper;

namespace SpellBound.Core {
    public class SpellBoundEntityBridge : MonoBehaviour {
        public static SpellBoundEntityBridge Instance;
        private GameObject _lastInteractor;

        private void Awake() {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable() {
            Helper.OnEntityInteraction += HandleSwap;
        }

        void OnDisable() {
            Helper.OnEntityInteraction -= HandleSwap;
        }
        
        public GameObject GetLastInteractor()  => _lastInteractor;
        
        void HandleSwap(Entity entity, GameObject interactor) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.HasComponent<SpellBoundComponent>(entity)
                || !entityManager.HasComponent<LocalTransform>(entity)) {

                return;
            }
            _lastInteractor = interactor;
            var sbbEntityData = entityManager.GetComponentData<SpellBoundComponent>(entity);
            var sbbDataList = new List<SbbData>();
            var op = sbbEntityData.PresetUiD.Value.ResolvePreset();
            foreach (var module in op.modules) {
                var sbbData = module.GetData(op);
                if (sbbData == null)
                    continue;
                sbbDataList.Add(sbbData.Value);
            }
            var transformData =  entityManager.GetComponentData<LocalTransform>(entity);
            var chunk = Helper.chunkManager.GetObjectParentChunk(transformData.Position);
            chunk.SwapInPersistent(sbbEntityData.PresetUiD.Value, sbbEntityData.GenerationIndex, transformData.Position, transformData.Rotation, transformData.Scale, sbbDataList.ToArray());
        }
    
    }
}



