using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving all game objects in the Unity hierarchy
    /// </summary>
    public class GetHierarchyResource : McpResourceBase
    {
        public GetHierarchyResource()
        {
            Name = "get_hierarchy";
            Description = "Retrieves all game objects in the Unity loaded scenes with their active state";
        }
        
        /// <summary>
        /// Fetch all game objects in the Unity loaded scenes
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject (not used)</param>
        /// <returns>A JObject containing the hierarchy of game objects</returns>
        public override JObject Fetch(JObject parameters)
        {
            // Get all game objects in the hierarchy
            JArray hierarchyArray = GetSceneHierarchy();
                
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved hierarchy with {hierarchyArray.Count} root objects",
                ["hierarchy"] = hierarchyArray
            };
        }
        
        /// <summary>
        /// Get all game objects in the Unity loaded scenes
        /// </summary>
        /// <returns>A JArray containing the hierarchy of game objects</returns>
        private JArray GetSceneHierarchy()
        {
            JArray rootObjectsArray = new JArray();
            
            // Get all loaded scenes
            int sceneCount = SceneManager.loadedSceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                
                // Create a scene object
                JObject sceneObject = new JObject
                {
                    ["name"] = scene.name,
                    ["path"] = scene.path,
                    ["buildIndex"] = scene.buildIndex,
                    ["isDirty"] = scene.isDirty,
                    ["rootObjects"] = new JArray()
                };
                
                // Get root game objects in the scene
                GameObject[] rootObjects = scene.GetRootGameObjects();
                JArray rootObjectsInScene = (JArray)sceneObject["rootObjects"];
                
                foreach (GameObject rootObject in rootObjects)
                {
                    // Add the root object and its children to the array
                    rootObjectsInScene.Add(GameObjectToJObject(rootObject));
                }
                
                // Add the scene to the root objects array
                rootObjectsArray.Add(sceneObject);
            }
            
            return rootObjectsArray;
        }
        
        /// <summary>
        /// Convert a GameObject to a JObject with its hierarchy
        /// </summary>
        /// <param name="gameObject">The GameObject to convert</param>
        /// <returns>A JObject representing the GameObject</returns>
        private JObject GameObjectToJObject(GameObject gameObject)
        {
            // Create a JObject for the game object
            JObject gameObjectJson = new JObject
            {
                ["name"] = gameObject.name,
                ["activeSelf"] = gameObject.activeSelf,
                ["activeInHierarchy"] = gameObject.activeInHierarchy,
                ["tag"] = gameObject.tag,
                ["layer"] = gameObject.layer,
                ["layerName"] = LayerMask.LayerToName(gameObject.layer),
                ["instanceId"] = gameObject.GetInstanceID(),
                ["components"] = GetComponentsInfo(gameObject),
                ["children"] = new JArray()
            };
            
            // Add children
            JArray childrenArray = (JArray)gameObjectJson["children"];
            foreach (Transform child in gameObject.transform)
            {
                childrenArray.Add(GameObjectToJObject(child.gameObject));
            }
            
            return gameObjectJson;
        }
        
        /// <summary>
        /// Get basic information about the components attached to a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to get components from</param>
        /// <returns>A JArray containing component information</returns>
        private JArray GetComponentsInfo(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            JArray componentsArray = new JArray();
            
            foreach (Component component in components)
            {
                if (component == null) continue;
                
                JObject componentJson = new JObject
                {
                    ["type"] = component.GetType().Name,
                    ["enabled"] = IsComponentEnabled(component)
                };
                    
                componentsArray.Add(componentJson);
            }
            
            return componentsArray;
        }
        
        /// <summary>
        /// Check if a component is enabled (if it has an enabled property)
        /// </summary>
        /// <param name="component">The component to check</param>
        /// <returns>True if the component is enabled, false otherwise</returns>
        private bool IsComponentEnabled(Component component)
        {
            // Check if the component is a Behaviour (has enabled property)
            if (component is Behaviour behaviour)
            {
                return behaviour.enabled;
            }
            
            // Check if the component is a Renderer
            if (component is Renderer renderer)
            {
                return renderer.enabled;
            }
            
            // Check if the component is a Collider
            if (component is Collider collider)
            {
                return collider.enabled;
            }
            
            // Default to true for components without an enabled property
            return true;
        }
    }
}
