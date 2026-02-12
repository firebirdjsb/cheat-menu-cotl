using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CheatMenu;

public class UnityAnnotationHelper {
    private readonly List<MethodInfo> _initMethods = new();
    private readonly List<MethodInfo> _unloadMethods = new();
    private readonly List<MethodInfo> _onGuiMethods = new();
    private readonly List<MethodInfo> _updateMethods = new();

    public UnityAnnotationHelper(){
        var assembly = Assembly.GetExecutingAssembly();
        Type[] types;
        try {
            types = assembly.GetTypes();
        } catch(ReflectionTypeLoadException e){
            types = e.Types.Where(t => t != null).ToArray();
        }

        List<(MethodInfo method, int order)> enforceFirstMethods = new();
        List<MethodInfo> normalInitMethods = new();
        List<MethodInfo> enforceLastMethods = new();

        foreach(var type in types){
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach(var method in methods){
                if(method.GetCustomAttribute<Init>() != null){
                    var enforceFirst = method.GetCustomAttribute<EnforceOrderFirst>();
                    var enforceLast = method.GetCustomAttribute<EnforceOrderLast>();

                    if(enforceFirst != null){
                        enforceFirstMethods.Add((method, enforceFirst.Order));
                    } else if(enforceLast != null){
                        enforceLastMethods.Add(method);
                    } else {
                        normalInitMethods.Add(method);
                    }
                }

                if(method.GetCustomAttribute<Unload>() != null){
                    _unloadMethods.Add(method);
                }

                if(method.GetCustomAttribute<OnGui>() != null){
                    _onGuiMethods.Add(method);
                }

                if(method.GetCustomAttribute<Update>() != null){
                    _updateMethods.Add(method);
                }
            }
        }

        enforceFirstMethods.Sort((a, b) => a.order.CompareTo(b.order));
        _initMethods.AddRange(enforceFirstMethods.Select(x => x.method));
        _initMethods.AddRange(normalInitMethods);
        _initMethods.AddRange(enforceLastMethods);
    }

    public void RunAllInit(){
        foreach(var method in _initMethods){
            try {
                object instance = null;
                if(!method.IsStatic){
                    var prop = method.DeclaringType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if(prop != null){
                        instance = prop.GetValue(null);
                    }
                }
                method.Invoke(instance, null);
            } catch(Exception e){
                UnityEngine.Debug.LogError($"[UnityAnnotationHelper] Failed to run Init on {method.DeclaringType?.Name}.{method.Name}: {e}");
            }
        }
    }

    public void RunAllUnload(){
        foreach(var method in _unloadMethods){
            try {
                object instance = null;
                if(!method.IsStatic){
                    var prop = method.DeclaringType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if(prop != null){
                        instance = prop.GetValue(null);
                    }
                }
                method.Invoke(instance, null);
            } catch(Exception e){
                UnityEngine.Debug.LogError($"[UnityAnnotationHelper] Failed to run Unload on {method.DeclaringType?.Name}.{method.Name}: {e}");
            }
        }
    }

    public Action BuildRunAllOnGuiDelegate(){
        return () => {
            foreach(var method in _onGuiMethods){
                try {
                    method.Invoke(null, null);
                } catch(Exception e){
                    UnityEngine.Debug.LogError($"[UnityAnnotationHelper] OnGui error in {method.DeclaringType?.Name}.{method.Name}: {e}");
                }
            }
        };
    }

    public Action BuildRunAllUpdateDelegate(){
        return () => {
            foreach(var method in _updateMethods){
                try {
                    method.Invoke(null, null);
                } catch(Exception e){
                    UnityEngine.Debug.LogError($"[UnityAnnotationHelper] Update error in {method.DeclaringType?.Name}.{method.Name}: {e}");
                }
            }
        };
    }
}
