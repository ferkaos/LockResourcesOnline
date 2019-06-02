using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using WanzyeeStudio.Editrix.Toolkit;

namespace LockResourcesOnline {
    [InitializeOnLoad]
    public static class LockResourcesOnlineManager {
        static string jsonPath;
        static string json;
        static LockResourcesUsers jsonUsers = new LockResourcesUsers();
        static Dictionary<string, LockResourcesUser> users = new Dictionary<string, LockResourcesUser>();
        static LockResourcesUser myUser;
        static float updateTimeInterval = 2.0f;
        static double elapsedTime;
        static double timeMark;

        static LockResourcesOnlineManager() {
            EditorApplication.update += EditorUpdate;
            timeMark = EditorApplication.timeSinceStartup + updateTimeInterval;
        }

        static void EditorUpdate() {
            elapsedTime = EditorApplication.timeSinceStartup;
            if (elapsedTime > timeMark) {
                Debug.Log("Update Editor");
                timeMark = EditorApplication.timeSinceStartup + updateTimeInterval;
                BlockResources();
                GenerateJson();
            }
        }

        static void BlockResources() {
            List<LockResourcesUser> usersList = users.Select(kvp => kvp.Value).ToList();
            foreach (LockResourcesUser user in usersList) {
                foreach (LockResource lockedObject in user.GetLockedResources()) {
                    //if(user.UserId != myUser.UserId) {
                        ObjectLocker.SetLocked(true, lockedObject.resource);
                    //}
                }
            }
        }

        static string GetMyMachineId() {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static LockResourcesUser SearchMyUser() {
            if(myUser != null) {
                return myUser;
            }
            if (!users.ContainsKey(GetMyMachineId())) {
                return null;
            }
            myUser = users[GetMyMachineId()];
            return myUser;
        }

        public static Dictionary<string, LockResourcesUser> GetAllUsers() {
            return users;
        }

        public static void CreateMyUser(string userId) {
            if(myUser != null) {
                return;
            }
            string machineId = GetMyMachineId();
            LockResourcesUser newUser = new LockResourcesUser(userId, machineId);
            myUser = newUser;
            users.Add(myUser.MachineId, myUser);
            Debug.Log("MyUser created -> {User ID: " + myUser.UserId +",Machine ID: " + myUser.MachineId +"}");
        }

        public static void DeleteMyUser() {
            users.Remove(myUser.MachineId);
            myUser = null;
            Debug.Log("MyUser Deleted");
        }

        public static string GetMachineIdByUserId(string userId) {
            return users.FirstOrDefault(user => user.Value.UserId == userId).Value.MachineId;
        }

        public static LockResourcesUser SearchUser(string machineId) {
            return users[machineId];
        }

        public static void AddObjectToBlock(string userId, LockResource objectToBlock) {
            SearchUser(GetMachineIdByUserId(userId)).AddResource(objectToBlock);
        }

        public static void RemoveObjectToBlock(string userId, LockResource objectToBlock) {
            SearchUser(GetMachineIdByUserId(userId)).RemoveResource(objectToBlock);
            ObjectLocker.SetLocked(false,(GameObject) objectToBlock.resource);
        }

        public static int GetNumberOfResourcesBlocked() {
            int count = 0;
            foreach (LockResourcesUser user in users.Select(kvp => kvp.Value).ToList()) {
                foreach(LockResource resource in user.GetLockedResources()) {
                    count++;
                }
            }
            return count;
        }

        private static void UpdateResourcesFromJson() {
            users = JsonUtility.FromJson<LockResourcesUsers>(UpdateJson()).users;
        }

        private static string UpdateJson() {
            return "";
        }

        private static void GenerateJson() {
            jsonUsers.users = users;
            json = JsonConvert.SerializeObject(jsonUsers);
            Debug.Log(json);
            string filePath = Application.dataPath + "/Scripts/LockResourcesOnline/Json/LockResources.json";
            System.IO.File.WriteAllText(filePath, json);
        }
    }
}
