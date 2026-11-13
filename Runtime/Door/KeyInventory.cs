using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class KeyInventory : MonoBehaviour
    {
        private readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);

        public bool AddKey(string keyId)
        {
            return !string.IsNullOrWhiteSpace(keyId) && keys.Add(keyId.Trim());
        }

        public bool HasKey(string keyId)
        {
            return !string.IsNullOrWhiteSpace(keyId) && keys.Contains(keyId.Trim());
        }

        public bool RemoveKey(string keyId)
        {
            return !string.IsNullOrWhiteSpace(keyId) && keys.Remove(keyId.Trim());
        }
    }
}
