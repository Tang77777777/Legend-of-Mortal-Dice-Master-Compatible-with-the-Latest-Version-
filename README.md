# 活侠传 DiceMaster 兼容修复

这是针对《活侠传 / Legend of Mortal》近期游戏更新的 DiceMaster 兼容修复包。

## 原始来源

- 原始骰子大师：<https://mod.3dmgame.com/mod/210536>
- 原始 `DiceMaster.dll` 保持原版本，原始作品的作者、版权和使用规则归原作者所有。

## 修复内容

1. 当前测试版本使用 64 位 Unity Mono 运行环境；原安装包中的 `winhttp.dll` 是 32 位，导致 BepInEx 无法注入。本包换成 64 位 BepInEx Unity Mono 加载器。
2. 原插件只在启动时按分辨率计算窗口缩放，导致部分高分辨率环境下局内修改器窗口和文字过小。本包加入 `DiceMasterUiFix.dll`，在绘制前设置最小 UI 缩放并限制窗口不超出屏幕。

## 普通玩家安装

1. 退出游戏。
2. 下载 `release/DiceMaster_64_fix_v1.1.zip`。
3. 把压缩包内的全部内容解压到能看到 `Mortal.exe` 的游戏根目录。
4. 覆盖同名文件后启动游戏。
5. 游戏主界面左上角出现 `Dice Log Output` 即表示成功。

首次启动后可以编辑：

```text
BepInEx/config/DiceMaster.UiFix.cfg
```

默认 `MinimumScale = 1.0`；如果仍觉得文字小，可改为 `1.25`，保存后重新启动游戏。

## 文件结构

- `package/`：可直接解压到游戏目录的修复包内容。
- `src/DiceMasterUiFix/`：窗口缩放补丁源码。
- `release/`：面向玩家的压缩包和安装说明。

## 构建窗口补丁

补丁使用当前游戏的 `BepInEx.Core.dll`、`BepInEx.Unity.Mono.dll`、`0Harmony.dll` 和 Unity 程序集编译。修改 `src/DiceMasterUiFix/DiceMasterUiFix.csproj` 中的程序集路径后执行：

```powershell
dotnet build src/DiceMasterUiFix/DiceMasterUiFix.csproj -c Release
```

本次已在 Unity `2020.3.49f1`、显示版本 `release_1.0.6000.0`、64 位 Windows 环境验证，日志确认 DiceMaster 和 DiceMaster UI Fix 均成功加载，且窗口放大可见。未来游戏更新后如果改动 BepInEx 或 DiceMaster 私有字段，可能需要重新适配。
