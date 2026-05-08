// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public class TooltipModule : PresetModule, IMouseoverHandler {
        [SerializeField] private string name;
        [SerializeField] private string description;

        public static event Action<string> OnMouseoverEvent;

        public void OnMouseover(IObjectParent parent, int instanceIndex, string presetUid, int eventSurfaceIndex) {
            Log.Debug($"OnMouseover running with eventSurfaceIndex {eventSurfaceIndex}");

            var tooltip = $"{name}";
 
                          
                          
                          
            if (TryGetCustomTooltipData(parent, instanceIndex, presetUid, eventSurfaceIndex, out var additionalTooltipData)){
                tooltip += additionalTooltipData;
                
            }
            
            OnMouseoverEvent?.Invoke(tooltip);
            
        }
        
        private bool TryGetCustomTooltipData(
            IObjectParent parent, int instanceIndex, string presetUid, int eventSurfaceIndex, out string tooltipData) {
            tooltipData = null;
            if (!parent.ObjectParent.TryReadDataAllData(instanceIndex, presetUid, eventSurfaceIndex, out var results)) {
                return false;
            }

            if (results.Count == 0) {
                return false;
            }

            foreach (var result in results) {
                tooltipData += result.ToString();
                
            }
            return true;
        }
    }
}