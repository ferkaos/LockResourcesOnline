using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockResourcesOnline {

    [Serializable]
    public class LockResourcesUser {
        private string userId;
        private string machineId;

        public string UserId {
            get { return userId; }
        }

        public string MachineId {
            get { return machineId; }
        }

        public List<LockResource> lockedResources = new List<LockResource>();

        public LockResourcesUser(string userId, string machineId) {
            this.userId = userId;
            this.machineId = machineId;
        }

        public List<LockResource> GetLockedResources() {
            return lockedResources;
        }

        public void AddResource(LockResource resource) {
            lockedResources.Add(new LockResource(resource.resource.name,ResourceType.GameObject,resource.resource) );
        }

        public void RemoveResource(LockResource resource) {            
            lockedResources.Remove(lockedResources.Find(item => item.name == resource.name));
        }
    }
}
