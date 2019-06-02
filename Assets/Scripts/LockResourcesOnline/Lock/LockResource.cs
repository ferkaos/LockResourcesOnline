using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockResourcesOnline {
    [Serializable]
    public enum ResourceType {
        GameObject,
        Scene
    }
    [Serializable]
    public class LockResource {
        public string name;
        public ResourceType resourceType;
        [JsonIgnore]
        public GameObject resource;

        public LockResource(string name, ResourceType resourceType, GameObject resource) {
            this.name = name;
            this.resourceType = resourceType;
            this.resource = resource;
        }
    }
}
