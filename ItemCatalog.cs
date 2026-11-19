using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project.Effects;
using YukkuriMovieMaker.Settings;

namespace Searchable;

public static class ItemCatalog
{
    public static List<SearchResultItem> BuildCatalog()
    {
        var items = new List<SearchResultItem>();

        CollectWindows(items);
        CollectObjects(items);
        CollectEffects(items);
        CollectCharacters(items);
        CollectTemplates(items);
        CollectCommands(items);

        return items;
    }

    private static void CollectWindows(List<SearchResultItem> list)
    {
        // 1. Built-in windows from Commands
        var windowCommands = new (ICommandId Id, string Title, string Sub, string Keywords)[]
        {
            (Commands.OpenCharacterEditorWindow, "キャラクターの編集", "ウィンドウ", "character c kyara"),
            (Commands.OpenScenarioWindow, "台本編集", "ウィンドウ", "scenario daihon script"),
            (Commands.OpenResourceWindow, "使用素材一覧", "ウィンドウ", "resource sozai asset"),
            (Commands.OpenDictionaryWindow, "単語・発音辞書", "ウィンドウ", "dictionary jisho yomi"),
            (Commands.OpenVideoInfoEditorWindow, "動画の設定", "ウィンドウ", "video info config setting"),
            (Commands.OpenSettingWindow, "環境設定 (設定)", "ウィンドウ", "setting kankyo option preference"),
            (Commands.OpenSearchWindow, "アイテムの検索", "ウィンドウ", "search item kensaku"),
            (Commands.OpenReplaceWindow, "テキスト置換", "ウィンドウ", "replace chikan"),
            (Commands.OpenTemplateEditorWindow, "テンプレートの編集", "ウィンドウ", "template henshu"),
            (Commands.OpenLayerSettingEditorWindow, "レイヤーの設定", "ウィンドウ", "layer reiya"),
            (Commands.OpenScenePreviewWindow, "別ウィンドウプレビュー", "ウィンドウ", "scene preview window dokuritsu"),
            (Commands.OpenUpdaterWindow, "YMM4のバージョン変更", "ウィンドウ", "update version updater")
        };

        foreach (var w in windowCommands)
        {
            var cmd = CommandRegistry.GetCommand(w.Id);
            if (cmd != null)
            {
                list.Add(new SearchResultItem(
                    title: w.Title,
                    category: "ウィンドウ",
                    subText: w.Sub,
                    keywords: $"{w.Title} {w.Keywords} ウィンドウ window 画面",
                    kind: SearchItemKind.Window,
                    action: () =>
                    {
                        var target = Application.Current?.MainWindow;
                        if (cmd.CanExecute(null, target))
                        {
                            cmd.Execute(null, target);
                        }
                    }
                ));
            }
        }

        // 2. Tool plugins (dockable/floating windows)
        try
        {
            var mainVm = Ymm4HostBridge.GetMainViewModel();
            if (mainVm != null)
            {
                var anchorablesProp = mainVm.GetType().GetProperty("AnchorableAreaViewModels", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (anchorablesProp?.GetValue(mainVm) is IEnumerable anchorables)
                {
                    foreach (var area in anchorables)
                    {
                        if (area == null) continue;
                        var titleProp = area.GetType().GetProperty("DetailedTitle") ?? area.GetType().GetProperty("Title");
                        var idProp = area.GetType().GetProperty("Id");
                        var vmProp = area.GetType().GetProperty("ViewModel");

                        var title = titleProp?.GetValue(area) as string;
                        var id = idProp?.GetValue(area) as string;
                        var vm = vmProp?.GetValue(area);

                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(id))
                        {
                            if (list.Any(x => x.Title == title)) continue;

                            list.Add(new SearchResultItem(
                                title: title,
                                category: "ツールウィンドウ",
                                subText: "ツールパネル",
                                keywords: $"{title} {vm?.GetType().Name} tool window panel パネル ドック dock",
                                kind: SearchItemKind.Window,
                                action: () =>
                                {
                                    Ymm4HostBridge.OpenToolWindowByContentId(id);
                                }
                            ));
                        }
                    }
                }
            }
        }
        catch
        {
        }
    }

    private static void CollectObjects(List<SearchResultItem> list)
    {
        var objectDefs = new (string Title, string MethodName, string Sub, string Keywords)[]
        {
            ("テキストアイテム", "AddTextItem", "オブジェクト", "text jimaku moji テキスト 字幕"),
            ("図形アイテム", "AddShapeItem", "オブジェクト", "shape zukei 図形 シェイプ"),
            ("エフェクトアイテム", "AddEffectItem", "オブジェクト", "effect item エフェクト アイテム"),
            ("場面切り替えアイテム", "AddTransitionItem", "オブジェクト", "transition bamen kirikae シーンチェンジ"),
            ("画面の複製 (フレームバッファ)", "AddFrameBufferItem", "オブジェクト", "framebuffer fukusei キャプチャ コピー"),
            ("グループ制御", "AddGroupItem", "オブジェクト", "group seigyo グループ まとめる"),
            ("シーンアイテム", "AddSceneItem", "オブジェクト", "scene nest シーン 入れ子")
        };

        foreach (var def in objectDefs)
        {
            list.Add(new SearchResultItem(
                title: def.Title,
                category: "オブジェクト追加",
                subText: def.Sub,
                keywords: $"{def.Title} {def.Keywords} add create 新規 追加",
                kind: SearchItemKind.Item,
                action: () =>
                {
                    var (f, l) = Ymm4HostBridge.GetCurrentPlacementTarget();
                    if (def.MethodName == "AddTextItem")
                    {
                        Ymm4HostBridge.ExecuteMainModelMethod("AddTextItem", f, l, string.Empty);
                    }
                    else
                    {
                        Ymm4HostBridge.ExecuteMainModelMethod(def.MethodName, f, l);
                    }
                }
            ));
        }

        // Voice item placement
        list.Add(new SearchResultItem(
            title: "ボイスアイテム",
            category: "オブジェクト追加",
            subText: "オブジェクト",
            keywords: "voice serifu koe 音声 声 セリフ 台詞 ボイス",
            kind: SearchItemKind.Item,
            action: () =>
            {
                var (f, l) = Ymm4HostBridge.GetCurrentPlacementTarget();
                var chara = GetCurrentOrFirstCharacter();
                if (chara != null)
                {
                    Ymm4HostBridge.ExecuteMainModelMethod("AddVoiceItemAsync", f, l, chara, "ゆっくりしていってね！", null);
                }
            }
        ));
    }

    private static void CollectEffects(List<SearchResultItem> list)
    {
        var videoBaseType = typeof(VideoEffectBase);
        var audioBaseType = typeof(AudioEffectBase);

        var foundTypes = new HashSet<Type>();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (IsSystemAssembly(asm)) continue;

            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || !type.IsClass) continue;

                bool isVideo = videoBaseType.IsAssignableFrom(type);
                bool isAudio = audioBaseType.IsAssignableFrom(type);

                if (!isVideo && !isAudio) continue;
                if (!foundTypes.Add(type)) continue;

                var (name, categories, keywords) = ReadMetadata(type, isVideo);
                string cat = categories.Length > 0 ? categories[0] : (isVideo ? "映像エフェクト" : "音声エフェクト");
                string typeName = type.Name;

                list.Add(new SearchResultItem(
                    title: name,
                    category: isVideo ? $"映像エフェクト ({cat})" : $"音声エフェクト ({cat})",
                    subText: cat,
                    keywords: $"{name} {keywords} {string.Join(" ", categories)} {typeName} effect エフェクト",
                    kind: isVideo ? SearchItemKind.VideoEffect : SearchItemKind.AudioEffect,
                    action: () =>
                    {
                        bool success = Ymm4HostBridge.ApplyEffectToSelected(type, isVideo);
                        if (!success)
                        {
                            // If no selected item, add an EffectItem to timeline and apply to it
                            var (f, l) = Ymm4HostBridge.GetCurrentPlacementTarget();
                            Ymm4HostBridge.ExecuteMainModelMethod("AddEffectItem", f, l);
                            Application.Current?.Dispatcher.InvokeAsync(async () =>
                            {
                                await System.Threading.Tasks.Task.Delay(50);
                                Ymm4HostBridge.ApplyEffectToSelected(type, isVideo);
                            });
                        }
                    }
                ));
            }
        }
    }

    private static void CollectCharacters(List<SearchResultItem> list)
    {
        try
        {
            var charSettingsType = typeof(YukkuriMovieMaker.App).Assembly.GetType("YukkuriMovieMaker.Settings.CharacterSettings");
            if (charSettingsType == null) return;

            var sbType = typeof(SettingsBase<>).MakeGenericType(charSettingsType);
            var defProp = sbType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
            var charSettings = defProp?.GetValue(null);
            if (charSettings == null) return;

            var charsProp = charSettings.GetType().GetProperty("Characters");
            if (charsProp?.GetValue(charSettings) is IEnumerable chars)
            {
                foreach (var c in chars)
                {
                    if (c == null) continue;
                    var nameProp = c.GetType().GetProperty("Name");
                    var name = nameProp?.GetValue(c) as string;
                    if (string.IsNullOrEmpty(name)) continue;

                    list.Add(new SearchResultItem(
                        title: name,
                        category: "キャラクター",
                        subText: "キャラクター切替",
                        keywords: $"{name} character kyara キャラ キャラクター 切替",
                        kind: SearchItemKind.Character,
                        action: () =>
                        {
                            SetCurrentCharacter(c);
                        }
                    ));
                }
            }
        }
        catch
        {
        }
    }

    private static void CollectTemplates(List<SearchResultItem> list)
    {
        try
        {
            var itemSettingsType = typeof(YukkuriMovieMaker.App).Assembly.GetType("YukkuriMovieMaker.Settings.ItemSettings");
            if (itemSettingsType == null) return;

            var sbType = typeof(SettingsBase<>).MakeGenericType(itemSettingsType);
            var defProp = sbType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
            var itemSettings = defProp?.GetValue(null);
            if (itemSettings == null) return;

            var templatesProp = itemSettings.GetType().GetProperty("Templates");
            if (templatesProp?.GetValue(itemSettings) is IEnumerable templates)
            {
                foreach (var t in templates)
                {
                    if (t == null) continue;
                    var nameProp = t.GetType().GetProperty("Name");
                    var groupProp = t.GetType().GetProperty("Group");
                    var name = nameProp?.GetValue(t) as string;
                    var group = groupProp?.GetValue(t)?.ToString();

                    if (string.IsNullOrEmpty(name)) continue;

                    list.Add(new SearchResultItem(
                        title: name,
                        category: "テンプレート",
                        subText: group ?? "一般",
                        keywords: $"{name} {group} template テンプレート",
                        kind: SearchItemKind.Template,
                        action: () =>
                        {
                            var (f, l) = Ymm4HostBridge.GetCurrentPlacementTarget();
                            Ymm4HostBridge.ExecuteMainModelMethod("AddTemplateItemAsync", f, l, t);
                        }
                    ));
                }
            }
        }
        catch
        {
        }
    }

    private static void CollectCommands(List<SearchResultItem> list)
    {
        var cmds = new (ICommandId Id, string Title, string Sub, string Keywords)[]
        {
            (Commands.Cut, "切り取り", "操作", "cut kiritori カット"),
            (Commands.Copy, "コピー", "操作", "copy コピー"),
            (Commands.Paste, "貼り付け", "操作", "paste ペースト 貼り付け"),
            (Commands.Delete, "削除", "操作", "delete sakujo 削除 消去"),
            (Commands.Undo, "元に戻す", "操作", "undo modosu 元に戻す 取り消し"),
            (Commands.Redo, "やり直し", "操作", "redo yarinaoshi やり直し"),
            (Commands.SelectAll, "すべて選択", "操作", "select all subete 全選択"),
            (Commands.SplitCurrentPosition, "再生位置で分割", "操作", "split bunkatsu 分割 スプリット"),
            (Commands.PlayPause, "再生 / 一時停止", "操作", "play pause saisei 再生 停止 ポーズ"),
            (Commands.Stop, "停止", "操作", "stop teishi 停止"),
            (Commands.ReloadPlayer, "プレイヤーを再読み込み", "操作", "reload saiyomikomi 更新 リロード"),
            (Commands.SaveProject, "プロジェクトを保存", "操作", "save hozon 保存"),
            (Commands.SaveProjectAs, "プロジェクトを別名保存", "操作", "save as betsumei 別名保存")
        };

        foreach (var c in cmds)
        {
            var cmd = CommandRegistry.GetCommand(c.Id);
            if (cmd != null)
            {
                list.Add(new SearchResultItem(
                    title: c.Title,
                    category: "編集・操作コマンド",
                    subText: c.Sub,
                    keywords: $"{c.Title} {c.Keywords} command コマンド 操作",
                    kind: SearchItemKind.Command,
                    action: () =>
                    {
                        var target = Application.Current?.MainWindow;
                        if (cmd.CanExecute(null, target))
                        {
                            cmd.Execute(null, target);
                        }
                    }
                ));
            }
        }
    }

    private static (string Name, string[] Categories, string Keywords) ReadMetadata(Type type, bool isVideo)
    {
        var name = type.Name;
        var categories = Array.Empty<string>();
        var keywords = string.Empty;

        try
        {
            if (isVideo)
            {
                var attr = type.GetCustomAttribute<VideoEffectAttribute>();
                if (attr != null)
                {
                    name = attr.GetName();
                    categories = attr.GetCategories() ?? Array.Empty<string>();
                    keywords = string.Join(" ", attr.GetKeywords() ?? Array.Empty<string>());
                }
            }
            else
            {
                var attr = type.GetCustomAttribute<AudioEffectAttribute>();
                if (attr != null)
                {
                    name = attr.GetName();
                    categories = attr.GetCategories() ?? Array.Empty<string>();
                    keywords = string.Join(" ", attr.GetKeywords() ?? Array.Empty<string>());
                }
            }
        }
        catch
        {
        }

        return (name, categories, keywords);
    }

    private static object? GetCurrentOrFirstCharacter()
    {
        try
        {
            var charSettingsType = typeof(YukkuriMovieMaker.App).Assembly.GetType("YukkuriMovieMaker.Settings.CharacterSettings");
            if (charSettingsType == null) return null;
            var sbType = typeof(SettingsBase<>).MakeGenericType(charSettingsType);
            var defProp = sbType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
            var charSettings = defProp?.GetValue(null);
            if (charSettings == null) return null;

            var curProp = charSettings.GetType().GetProperty("CurrentCharacter");
            var cur = curProp?.GetValue(charSettings);
            if (cur != null) return cur;

            var charsProp = charSettings.GetType().GetProperty("Characters");
            if (charsProp?.GetValue(charSettings) is IEnumerable chars)
            {
                foreach (var c in chars) return c;
            }
        }
        catch
        {
        }
        return null;
    }

    private static void SetCurrentCharacter(object character)
    {
        try
        {
            var tlv = Ymm4HostBridge.GetActiveTimelineViewModel();
            if (tlv != null)
            {
                var curProp = tlv.GetType().GetProperty("CurrentCharacter");
                var rx = curProp?.GetValue(tlv);
                if (rx != null)
                {
                    var valProp = rx.GetType().GetProperty("Value");
                    valProp?.SetValue(rx, character);
                }
            }
        }
        catch
        {
        }
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.FullName;
        if (string.IsNullOrEmpty(name)) return true;
        return name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("Presentation", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("UIAutomation", StringComparison.OrdinalIgnoreCase);
    }
}
