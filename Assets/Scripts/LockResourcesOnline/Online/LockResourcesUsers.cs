using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockResourcesOnline {
    [Serializable]
    public class LockResourcesUsers {
        [SerializeField] public Dictionary<string, LockResourcesUser> users = new Dictionary<string, LockResourcesUser>();
    }
}
