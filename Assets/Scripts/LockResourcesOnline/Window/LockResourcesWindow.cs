using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace LockResourcesOnline {
    public class LockResourcesWindow : EditorWindow {

        string myString = "Hello World";
        bool groupDeleteUser;
        bool myBool = true;
        float myFloat = 1.23f;
        static LockResourcesUser myUser;
        static LockResourcesUser MyUser {
            get {    if (myUser == null) {
                        LockResourcesOnlineManager.SearchMyUser();
                    } else {
                        return myUser;
                    }
                return null;
                }
            set { myUser = value; }
        }
        string userId;
        LockResource resourceToBlock = new LockResource("",ResourceType.GameObject,null);
        bool selectedResourceToggle;
        Vector2 scrollPos;
        bool orderByResource;
        bool orderAscending;
        Dictionary<LockResource, string> dictionaryResources = new Dictionary<LockResource, string>();

         [MenuItem("Window/Lock Resources Online")]
        public static void ShowWindow() {
            MyUser = LockResourcesOnlineManager.SearchMyUser();
            EditorWindow.GetWindow(typeof(LockResourcesWindow));
        }

        private void UserSettingsGUI() {
            GUILayout.Label("User Settings", EditorStyles.boldLabel);
            if(MyUser == null) {
                MyUser = LockResourcesOnlineManager.SearchMyUser();
            }
            if (MyUser != null) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UserId: " + MyUser.UserId, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("MachineId: " + MyUser.MachineId, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.BeginHorizontal();
                groupDeleteUser = EditorGUILayout.BeginToggleGroup("Delete User", groupDeleteUser);
                if (GUILayout.Button("Delete My User")) {
                    LockResourcesOnlineManager.DeleteMyUser();
                    myUser = null;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
            } else {
                EditorGUILayout.BeginHorizontal();
                userId = EditorGUILayout.TextField("UserId", userId);
                if (GUILayout.Button("Create User")) {
                    if (userId == "") {
                        Debug.Log("Introduce un id de usuario valido");
                        return;
                    }
                    groupDeleteUser = false;
                    LockResourcesOnlineManager.CreateMyUser(userId);
                    MyUser = LockResourcesOnlineManager.SearchMyUser();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ResourcesGUI() {
            GUILayout.Label("Lock Resources List", EditorStyles.boldLabel);
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            selectedResourceToggle = EditorGUILayout.Toggle("Resource From Selection", selectedResourceToggle);
            if (selectedResourceToggle && resourceToBlock.resource != null) {
                resourceToBlock.resource = Selection.activeGameObject;
                resourceToBlock.resourceType = ResourceType.GameObject;
                resourceToBlock.name = resourceToBlock.resource.name;
            }
            resourceToBlock.resource = (GameObject) EditorGUILayout.ObjectField((GameObject)resourceToBlock.resource, typeof(GameObject), true);
            if (GUILayout.Button("Add Object")) {

                if(resourceToBlock.resource == null || dictionaryResources.FirstOrDefault(o => o.Key.name == resourceToBlock.name).Key != null) {
                    return;
                }
                LockResourcesOnlineManager.AddObjectToBlock(MyUser.UserId, resourceToBlock);
            }
            EditorGUILayout.EndHorizontal();

            DrawTable();
        }

        void OnGUI() {
            UserSettingsGUI();
            GUILayout.Space(10);
            GuiLine(2);
            ResourcesGUI();
        }

        void GuiLine(int i_height = 1) {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        }

        private bool DrawSingleLine(int pos, string userId, LockResource resource) {
            float win = Screen.width * 0.6f;
            float w1 = win * 0.45f;
            float w2 = win * 0.15f;
            float w3 = win * 0.35f;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(userId, GUILayout.Width(w2));
            EditorGUILayout.LabelField(resource.name, GUILayout.Width(w1));
            if(myUser == null) {
                return false;
            }
            if(userId == myUser.UserId) {
                if (GUI.Button(new Rect(GUILayoutUtility.GetLastRect().position.x + 400, GUILayoutUtility.GetLastRect().position.y, 100, 20), "Remove Object")) {
                    if (resourceToBlock == null) {
                        return true;
                    }
                    LockResourcesOnlineManager.RemoveObjectToBlock(MyUser.UserId, resource);
                    return false;
                }
            }
            EditorGUILayout.EndHorizontal();
            return true;
        }

        private void DrawTable() {
            int i = 0;
            Dictionary<string, LockResourcesUser> users = LockResourcesOnlineManager.GetAllUsers();
            if(users.Count == 0) {
                return;
            }
            Rect tableRect = GUILayoutUtility.GetLastRect();
            tableRect.position = new Vector2(tableRect.position.x, tableRect.position.y + 40);
            GUI.Box(tableRect, "Locked Resources");
            tableRect.position = new Vector2(tableRect.position.x, tableRect.position.y + 20);
            tableRect.size = new Vector2(tableRect.size.x, Mathf.Clamp(LockResourcesOnlineManager.GetNumberOfResourcesBlocked() + 1,0,15) * 23);
            GUI.Box(tableRect, "");            
            GUILayout.Space(40);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 240));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UserId", EditorStyles.boldLabel,  GUILayout.Width(Screen.width * 0.6f * 0.15f));
            EditorGUILayout.LabelField("Resource Name", EditorStyles.boldLabel, GUILayout.Width(Screen.width * 0.6f * 0.45f));
            EditorGUILayout.EndHorizontal();
            GuiLine(2);
            List<LockResourcesUser> usersList = users.Select(kvp => kvp.Value).ToList();
            dictionaryResources = new Dictionary<LockResource, string>();
            foreach (LockResourcesUser user in usersList) {
                foreach (LockResource lockedObject in user.GetLockedResources()) {
                    dictionaryResources.Add(lockedObject, user.UserId);
                }
            }
            if (orderByResource) {
                foreach (KeyValuePair<LockResource, string> entry in dictionaryResources.OrderBy(o => o.Key.name)) {
                    if (!DrawSingleLine(i, entry.Value, entry.Key)) {
                        return;
                    }
                    i++;
                    GuiLine();
                }
            } else {
                foreach (KeyValuePair<LockResource, string> entry in dictionaryResources) {
                    if (!DrawSingleLine(i, entry.Value,entry.Key)) {
                        return;
                    }
                    i++;
                    GuiLine();
                }
            }
            EditorGUILayout.EndScrollView();
            orderByResource = EditorGUILayout.Toggle("Order by Resource Name", orderByResource);
        }

        private void OnSelectionChange() {
            Debug.Log("Selection change");
        }
    }
}
