using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project.Effects;
using YukkuriMovieMaker.Settings;

namespace Searchable;

public static class Ymm4HostBridge
{
    private static readonly MethodInfo? OpenToolWindowMethod;
    private static readonly FieldInfo? ItemEditorAreaField;
    private static readonly MethodInfo? UpdateSelectItemsMethod;

    static Ymm4HostBridge()
    {
        try
        {
            var mainVmType = typeof(YukkuriMovieMaker.App).Assembly.GetType("YukkuriMovieMaker.ViewModels.MainViewModel");
            if (mainVmType != null)
            {
                OpenToolWindowMethod = mainVmType.GetMethod("<CreateCommandBindings>g__OpenToolWindowFromType|137_316", BindingFlags.NonPublic | BindingFlags.Instance);
                ItemEditorAreaField = mainVmType.GetField("itemEditorAreaViewModel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            var itemEditorVmType = typeof(YukkuriMovieMaker.App).Assembly.GetType("YukkuriMovieMaker.ViewModels.ItemEditorViewModel");
            if (itemEditorVmType != null)
            {
                UpdateSelectItemsMethod = itemEditorVmType.GetMethod("UpdateSelectItems", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
        catch
        {
        }
    }

    public static object? GetMainViewModel()
    {
        return Application.Current?.MainWindow?.DataContext;
    }

    public static object? GetActiveTimelineViewModel()
    {
        var mainVm = GetMainViewModel();
        if (mainVm == null) return null;
        var prop = mainVm.GetType().GetProperty("ActiveTimelineViewModel", BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(mainVm);
    }

    public static object? GetTimelineModel()
    {
        var tlv = GetActiveTimelineViewModel();
        if (tlv == null) return null;
        var field = tlv.GetType().GetField("timeline", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(tlv);
    }

    public static object? GetMainModel()
    {
        var mainVm = GetMainViewModel();
        if (mainVm == null) return null;
        var field = mainVm.GetType().GetField("model", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(mainVm);
    }

    public static (int Frame, int Layer) GetCurrentPlacementTarget()
    {
        int frame = 0;
        int layer = 0;

        try
        {
            var timeline = GetTimelineModel();
            if (timeline != null)
            {
                var curFrameProp = timeline.GetType().GetProperty("CurrentFrame");
                if (curFrameProp?.GetValue(timeline) is int cf)
                {
                    frame = cf;
                }

                var lsProp = timeline.GetType().GetProperty("LayerSelection");
                var ls = lsProp?.GetValue(timeline);
                if (ls != null)
                {
                    var maxLayerProp = ls.GetType().GetProperty("MaxSelectedLayer");
                    if (maxLayerProp?.GetValue(ls) is int ml && ml >= 0)
                    {
                        layer = ml;
                    }
                }
            }
        }
        catch
        {
        }

        return (frame, layer);
    }

    public static bool OpenToolWindowByViewModelType(Type viewModelType)
    {
        try
        {
            var mainVm = GetMainViewModel();
            if (mainVm != null && OpenToolWindowMethod != null)
            {
                OpenToolWindowMethod.Invoke(mainVm, new object[] { viewModelType });
                return true;
            }

            // Fallback via OpenToolWindow command
            var cmd = CommandRegistry.GetCommand(Commands.OpenToolWindow);
            if (cmd != null)
            {
                var args = new OpenToolWindowCommandArgs(viewModelType);
                var target = Application.Current?.MainWindow;
                if (cmd.CanExecute(args, target))
                {
                    cmd.Execute(args, target);
                    return true;
                }
            }
        }
        catch
        {
        }

        return false;
    }

    public static bool OpenToolWindowByContentId(string contentId)
    {
        try
        {
            var mainVm = GetMainViewModel();
            if (mainVm == null) return false;

            var lsProp = mainVm.GetType().GetProperty("LayoutService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var layoutService = lsProp?.GetValue(mainVm);
            if (layoutService != null)
            {
                var showMethod = layoutService.GetType().GetMethod("Show", new[] { typeof(string) });
                var activateMethod = layoutService.GetType().GetMethod("Activate", new[] { typeof(string) });

                showMethod?.Invoke(layoutService, new object[] { contentId });
                activateMethod?.Invoke(layoutService, new object[] { contentId });
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    public static void NotifyItemEditorUpdate()
    {
        try
        {
            var mainVm = GetMainViewModel();
            if (mainVm != null && ItemEditorAreaField != null)
            {
                var area = ItemEditorAreaField.GetValue(mainVm);
                if (area != null)
                {
                    var vmProp = area.GetType().GetProperty("ViewModel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var vm = vmProp?.GetValue(area);
                    if (vm != null)
                    {
                        UpdateSelectItemsMethod?.Invoke(vm, null);
                    }
                }
            }
        }
        catch
        {
        }
    }

    public static List<object> GetSelectedTimelineItems()
    {
        var list = new List<object>();
        try
        {
            var tlv = GetActiveTimelineViewModel();
            if (tlv == null) return list;

            var itemsProp = tlv.GetType().GetProperty("Items");
            if (itemsProp?.GetValue(tlv) is not IEnumerable items) return list;

            foreach (var itemVm in items)
            {
                if (itemVm == null) continue;
                var isSelectedProp = itemVm.GetType().GetProperty("IsSelected");
                if (isSelectedProp?.GetValue(itemVm) is not true) continue;

                var itemProp = itemVm.GetType().GetProperty("Item");
                var item = itemProp?.GetValue(itemVm);
                if (item != null)
                {
                    list.Add(item);
                }
            }
        }
        catch
        {
        }
        return list;
    }

    public static bool ApplyEffectToSelected(Type effectType, bool isVideo)
    {
        var selectedItems = GetSelectedTimelineItems();
        if (selectedItems.Count == 0)
        {
            return false;
        }

        int count = 0;
        string propName = isVideo ? "VideoEffects" : "AudioEffects";

        foreach (var item in selectedItems)
        {
            try
            {
                var newInstance = Activator.CreateInstance(effectType);
                if (newInstance == null) continue;

                var prop = item.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    var curList = prop.GetValue(item);
                    if (curList != null)
                    {
                        var addMethod = curList.GetType().GetMethod("Add", new[] { newInstance.GetType() })
                                      ?? curList.GetType().GetMethod("Add", new[] { typeof(object) });
                        if (addMethod != null)
                        {
                            var updatedList = addMethod.Invoke(curList, new[] { newInstance });
                            prop.SetValue(item, updatedList);
                            count++;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        if (count > 0)
        {
            NotifyItemEditorUpdate();
            return true;
        }

        return false;
    }

    public static bool ExecuteMainModelMethod(string methodName, params object?[] parameters)
    {
        try
        {
            var mm = GetMainModel();
            if (mm == null) return false;

            var types = parameters.Select(p => p?.GetType() ?? typeof(object)).ToArray();
            var method = mm.GetType().GetMethod(methodName, types)
                       ?? mm.GetType().GetMethod(methodName);

            if (method != null)
            {
                var result = method.Invoke(mm, parameters);
                if (result is Task)
                {
                    // For async calls, let it run
                }
                return true;
            }
        }
        catch
        {
        }
        return false;
    }
}
