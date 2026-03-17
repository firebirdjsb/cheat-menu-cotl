using Lamb.UI;
using System;
using System.Collections;
using src.Extensions;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using Spine.Unity;

namespace CheatMenu;

// ============================================================================
// PARTIAL FILE: CultUtils_Farming.cs
// Contains: Farming, ranching, animals, growth, ascension, halos
// ============================================================================

internal static partial class CultUtils {
    // -- RGB Halo Animation State ------------------------------------------------
    
    private static float s_hueOffset = 0f;
    // -- Animals & Ranching ----------------------------------------------------

    public static void ForceGrowAllAnimals(){
        try {
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                if(animal.Age < 2){
                    animal.Age = 2;
                }
                animal.GrowthStage = 0;
                animal.WorkedReady = true;
                animal.WorkedToday = false;
                animal.Satiation = Interaction_Ranchable.MAX_HUNGER;
                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                if(ranchable != null){
                    ranchable.UpdateSkin();
                }
                count++;
            }
            PlayNotification(count > 0 ? $"Force grew {count} animal(s)!" : "No animals to grow!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] ForceGrowAllAnimals error: {e.Message}");
            PlayNotification("Failed to force grow animals!");
        }
    }

    public static void AscendAllAnimals(){
        try {
            int count = 0;
            var animals = new List<StructuresData.Ranchable_Animal>(AnimalData.GetAnimals());
            foreach(var animal in animals){
                if(animal == null) continue;

                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);

                if(ranchable != null && ranchable.gameObject != null && ranchable.gameObject.activeInHierarchy){
                    try {
                        ranchable.StartCoroutine(AscendAnimalCoroutine(ranchable, animal));
                    } catch(Exception e){
                        UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to start ascend coroutine: {e.Message}");
                        RemoveAnimalImmediate(ranchable, animal);
                    }
                } else {
                    CollectAnimalResources(animal, Vector3.zero, false);
                    RemoveAnimalData(ranchable, animal);
                }

                count++;
            }
            PlayNotification(count > 0 ? $"Ascended {count} animal(s)!" : "No animals to ascend!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AscendAllAnimals error: {e.Message}");
            PlayNotification("Failed to ascend animals!");
        }
    }

    private static System.Collections.IEnumerator AscendAnimalCoroutine(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        ranchable.BeingAscended = true;
        ranchable.ReservedByPlayer = true;
        animal.State = Interaction_Ranchable.State.Animating;

        string animalName = GetAnimationAnimalName(animal.Type);
        string ascendAnim = "ascend-" + animalName;
        var spine = Traverse.Create(ranchable).Field("spine").GetValue();
        if(spine != null){
            var animState = Traverse.Create(spine).Property("AnimationState").GetValue();
            if(animState != null){
                try {
                    Traverse.Create(animState).Method("SetAnimation", new Type[]{typeof(int), typeof(string), typeof(bool)}).GetValue(0, ascendAnim, false);
                } catch {
                    Traverse.Create(animState).Method("SetAnimation", new Type[]{typeof(int), typeof(string), typeof(bool)}).GetValue(0, "idle", false);
                }
            }
        }

        try {
            AudioManager.Instance.PlayOneShot("event:/dlc/animal/shared/ascend", ranchable.transform.position);
        } catch { }

        try {
            BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
        } catch { }

        yield return new WaitForSeconds(1.5f);

        Vector3 spawnPos = ranchable.transform.position + new Vector3(0f, 0f, -2f);
        CollectAnimalResources(animal, spawnPos, true);

        yield return new WaitForSeconds(2.8333f);

        try {
            BiomeConstants.Instance.ChromaticAbberationTween(0.5f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
        } catch { }

        yield return new WaitForSeconds(1f);

        try {
            var spineObj = Traverse.Create(ranchable).Field("spine").GetValue();
            if(spineObj != null){
                Traverse.Create(spineObj).Property("gameObject").GetValue<GameObject>()?.SetActive(false);
            }
        } catch { }

        try {
            AudioManager.Instance.PlayOneShot("event:/dlc/animal/shared/cleanup_dead", ranchable.transform.position);
        } catch { }

        yield return new WaitForSeconds(0.5f);

        RemoveAnimalData(ranchable, animal);
        if(ranchable != null && ranchable.gameObject != null){
            UnityEngine.Object.Destroy(ranchable.gameObject);
        }
    }

    private static void CollectAnimalResources(StructuresData.Ranchable_Animal animal, Vector3 spawnPos, bool spawnVisual){
        List<InventoryItem> meatLoot = Interaction_Ranchable.GetMeatLoot(animal);
        foreach(var item in meatLoot){
            item.quantity = Mathf.RoundToInt((float)item.quantity);
        }

        if(spawnVisual && spawnPos != Vector3.zero){
            int spawnMeat = Mathf.Min(meatLoot.Count > 0 ? meatLoot[0].quantity : 0, 10);
            if(spawnMeat > 0){
                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, spawnMeat, spawnPos, 4f, null);
            }
            foreach(var item in meatLoot){
                Inventory.AddItem(item.type, Mathf.Max(0, item.quantity - spawnMeat), false);
            }
        } else {
            foreach(var item in meatLoot){
                AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, item.quantity);
            }
        }

        List<InventoryItem> workLoot = Interaction_Ranchable.GetWorkLoot(animal);
        foreach(var item in workLoot){
            int qty = item.quantity * 3;
            if(spawnVisual && spawnPos != Vector3.zero){
                InventoryItem.Spawn((InventoryItem.ITEM_TYPE)item.type, qty, spawnPos, 4f, null);
            } else {
                AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, qty);
            }
        }

        if(animal.Type == InventoryItem.ITEM_TYPE.ANIMAL_COW){
            if(spawnVisual && spawnPos != Vector3.zero){
                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MILK, 5, spawnPos, 4f, null);
            } else {
                AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 5);
            }
        }
    }

    private static void RemoveAnimalImmediate(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        CollectAnimalResources(animal, Vector3.zero, false);
        RemoveAnimalData(ranchable, animal);
        if(ranchable != null && ranchable.gameObject != null){
            UnityEngine.Object.Destroy(ranchable.gameObject);
        }
    }

    private static void RemoveAnimalData(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        if(ranchable != null && ranchable.ranch != null){
            ranchable.ranch.Brain.RemoveAnimal(animal);
        }
        DataManager.Instance.BreakingOutAnimals.Remove(animal);
        DataManager.Instance.DeadAnimalsTemporaryList.Add(animal);
    }

    private static string GetAnimationAnimalName(InventoryItem.ITEM_TYPE type){
        switch(type){
            case InventoryItem.ITEM_TYPE.ANIMAL_GOAT: return "goat";
            case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE: return "turtle";
            case InventoryItem.ITEM_TYPE.ANIMAL_CRAB: return "crab";
            case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER: return "spider";
            case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL: return "snail";
            case InventoryItem.ITEM_TYPE.ANIMAL_COW: return "cow";
            case InventoryItem.ITEM_TYPE.ANIMAL_LLAMA: return "llama";
            default: return "goat";
        }
    }

    // -- Animal Halos ----------------------------------------------------------

    private static List<GameObject> s_activeHalos = new List<GameObject>();

    private static float GetAnimalHaloHeight(InventoryItem.ITEM_TYPE type){
        // Heights adjusted to place halos on top of animal heads
        // These are roughly 1.0-1.5 units above animal base position
        switch(type){
            case InventoryItem.ITEM_TYPE.ANIMAL_GOAT:   return 1.2f;
            case InventoryItem.ITEM_TYPE.ANIMAL_COW:    return 1.5f;
            case InventoryItem.ITEM_TYPE.ANIMAL_LLAMA:   return 1.4f;
            case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE: return 0.8f;
            case InventoryItem.ITEM_TYPE.ANIMAL_CRAB:   return 0.5f;
            case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER: return 0.7f;
            case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL:  return 0.4f;
            default:                                    return 1.2f;
        }
    }

    public static void AddHalosToAnimals(){
        try {
            RemoveAnimalHalos();
            
            // Create animation helper object
            GameObject animHelper = new GameObject("HaloAnimationHelper");
            UnityEngine.Object.DontDestroyOnLoad(animHelper);
            HaloAnimator animator = animHelper.AddComponent<HaloAnimator>();
            s_hueOffset = 0f;
            
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                if(animal == null) continue;

                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                if(ranchable == null || ranchable.gameObject == null || !ranchable.gameObject.activeInHierarchy) continue;

                try {
                    float haloY = GetAnimalHaloHeight(animal.Type);

                    GameObject haloObj = new GameObject("CheatMenu_AnimalHalo");
                    haloObj.transform.SetParent(ranchable.transform);
                    haloObj.transform.localPosition = new Vector3(0f, haloY, -1f);

                    SpriteRenderer sr = haloObj.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateHaloSprite();
                    sr.color = new Color(1f, 1f, 1f, 0.9f); // Will be overridden by animation
                    sr.sortingLayerName = "Above";
                    sr.sortingOrder = 1000;
                    sr.material = new Material(Shader.Find("Sprites/Default"));
                    sr.material.SetFloat("_PixelSnap", 0f);
                    haloObj.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                    GameObject glowObj = new GameObject("HaloGlow");
                    glowObj.transform.SetParent(haloObj.transform);
                    glowObj.transform.localPosition = Vector3.zero;
                    glowObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
                    SpriteRenderer glowSr = glowObj.AddComponent<SpriteRenderer>();
                    glowSr.sprite = CreateGlowSprite();
                    glowSr.color = new Color(1f, 1f, 1f, 0.5f); // Will be overridden by animation
                    glowSr.sortingLayerName = "Above";
                    glowSr.sortingOrder = 999;
                    Shader addShader = Shader.Find("Particles/Additive");
                    if(addShader == null) addShader = Shader.Find("Legacy Shaders/Particles/Additive");
                    if(addShader == null) addShader = Shader.Find("Sprites/Default");
                    glowSr.material = new Material(addShader);

                    GameObject lightObj = new GameObject("HaloLight");
                    lightObj.transform.SetParent(haloObj.transform);
                    lightObj.transform.localPosition = Vector3.zero;
                    Light haloLight = lightObj.AddComponent<Light>();
                    haloLight.type = LightType.Point;
                    haloLight.color = Color.white; // Will be overridden by animation
                    haloLight.intensity = 3f;
                    haloLight.range = 2.5f;
                    haloLight.renderMode = LightRenderMode.Auto;

                    s_activeHalos.Add(haloObj);
                    count++;
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to add halo to animal: {e.Message}");
                }
            }
            PlayNotification(count > 0 ? $"RGB glowing halos added to {count} animal(s)!" : "No animals to add halos to!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AddHalosToAnimals error: {e.Message}");
            PlayNotification("Failed to add halos!");
        }
    }
    
    // MonoBehaviour to handle halo animation update
    private class HaloAnimator : MonoBehaviour {
        private void Update() {
            AnimateRGBHalos();
        }
    }

    private static Sprite s_haloSprite;
    private static Sprite s_glowSprite;

    private static Sprite CreateHaloSprite(){
        if(s_haloSprite != null) return s_haloSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float halfSize = size / 2f;

        float outerRadiusX = 0.85f;
        float outerRadiusY = 0.4f;
        float innerRadiusX = 0.55f;
        float innerRadiusY = 0.2f;
        float edgeSoftness = 0.12f;

        for(int y = 0; y < size; y++){
            for(int x = 0; x < size; x++){
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;

                float outerDist = (nx * nx) / (outerRadiusX * outerRadiusX) + (ny * ny) / (outerRadiusY * outerRadiusY);
                float innerDist = (nx * nx) / (innerRadiusX * innerRadiusX) + (ny * ny) / (innerRadiusY * innerRadiusY);

                float outerAlpha = Mathf.Clamp01((1f - outerDist) / edgeSoftness);
                float innerAlpha = Mathf.Clamp01((innerDist - 1f) / edgeSoftness);
                float ringAlpha = outerAlpha * innerAlpha;

                float glowDist = (nx * nx) / ((outerRadiusX + 0.2f) * (outerRadiusX + 0.2f)) + (ny * ny) / ((outerRadiusY + 0.15f) * (outerRadiusY + 0.15f));
                float glowAlpha = Mathf.Clamp01(1f - glowDist) * 0.25f;

                float alpha = Mathf.Max(ringAlpha, glowAlpha);

                if(alpha > 0.01f){
                    float r = Mathf.Lerp(1f, 1f, ringAlpha);
                    float g = Mathf.Lerp(0.4f, 0.2f, ringAlpha);
                    float b = Mathf.Lerp(0.8f, 0.6f, ringAlpha);
                    tex.SetPixel(x, y, new Color(r, g, b, alpha));
                } else {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        s_haloSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        return s_haloSprite;
    }

    private static Sprite CreateGlowSprite(){
        if(s_glowSprite != null) return s_glowSprite;

        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float halfSize = size / 2f;

        for(int y = 0; y < size; y++){
            for(int x = 0; x < size; x++){
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        s_glowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        return s_glowSprite;
    }

    private static void RemoveAnimalHalos(){
        foreach(var halo in s_activeHalos){
            if(halo != null){
                UnityEngine.Object.Destroy(halo);
            }
        }
        s_activeHalos.Clear();
        
        // Stop and destroy the animation helper
        var animHelpers = UnityEngine.Object.FindObjectsOfType<HaloAnimator>();
        foreach(var helper in animHelpers){
            if(helper != null && helper.gameObject != null){
                UnityEngine.Object.Destroy(helper.gameObject);
            }
        }
        s_hueOffset = 0f;
    }
    
    public static void RemoveAllAnimalHalos(){
        RemoveAnimalHalos();
        PlayNotification("All animal halos removed!");
    }
    
    private static Color HSVtoRGB(float h, float s, float v){
        Color color = Color.white;
        float c = v * s;
        float x = c * (1 - Mathf.Abs((h * 6f) % 2f - 1));
        float m = v - c;
        
        if(h < 1f/6f){ color = new Color(c, x, 0); }
        else if(h < 2f/6f){ color = new Color(x, c, 0); }
        else if(h < 3f/6f){ color = new Color(0, c, x); }
        else if(h < 4f/6f){ color = new Color(0, x, c); }
        else if(h < 5f/6f){ color = new Color(x, 0, c); }
        else { color = new Color(c, 0, x); }
        
        color.r += m;
        color.g += m;
        color.b += m;
        return color;
    }
    
    private static void AnimateRGBHalos(){
        s_hueOffset += Time.deltaTime * 0.5f; // Fast rotation
        if(s_hueOffset > 1f) s_hueOffset -= 1f;
        
        float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * 4f); // Fast pulse
        
        foreach(var haloParent in s_activeHalos){
            if(haloParent == null) continue;
            
            float hue = (s_hueOffset + UnityEngine.Random.Range(0f, 0.1f)) % 1f;
            Color rgbColor = HSVtoRGB(hue, 1f, 1f);
            
            // Update main halo
            SpriteRenderer sr = haloParent.GetComponent<SpriteRenderer>();
            if(sr != null) sr.color = new Color(rgbColor.r, rgbColor.g, rgbColor.b, 0.9f);
            
            // Update glow
            Transform glowTransform = haloParent.transform.Find("HaloGlow");
            if(glowTransform != null){
                SpriteRenderer glowSr = glowTransform.GetComponent<SpriteRenderer>();
                if(glowSr != null) glowSr.color = new Color(rgbColor.r, rgbColor.g, rgbColor.b, 0.5f * pulse);
            }
            
            // Update light
            Transform lightTransform = haloParent.transform.Find("HaloLight");
            if(lightTransform != null){
                Light haloLight = lightTransform.GetComponent<Light>();
                if(haloLight != null){
                    haloLight.color = rgbColor;
                    haloLight.intensity = 2f + 1f * Mathf.Sin(Time.time * 4f);
                }
            }
        }
    }
}
