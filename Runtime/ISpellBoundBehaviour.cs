// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SpellBound.Core {
    public interface ISpellBoundBehaviour {
        public ObjectPreset GetObjectPreset();

        public void ReleaseIsClaimed();
        public byte[] PullSbbData(string key);

        public void PushAllSbbData(byte[] sbbDatas);

        public void PushOneSbbData(SbbData sbbData);

        public GoData ReadNgoData();

        public event Action SbbDataUpdated;

        public event Action InitializeFromSwap;

        public void InvokeInitializeFromSwap();

        public event Action InitializeFromSpawn;

        public void InvokeInitializeFromSpawn();

        public Task<bool> TryClaim();

        public bool IsSwapper();

        public void ClearIsSwapper();

        public void Register();

        public void CheckRegistryBounds(Bounds bounds);

        public GameObject GetGameObject();
        public void DestroySbb();
    }
}