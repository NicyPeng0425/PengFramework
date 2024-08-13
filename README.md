# 彭框架 PengFramework
A framework for the development of action games.

动作游戏开发框架。

核心目标：
1. 人性化的交互。
2. 灵活，拓展性强。
3. 符合动作游戏的开发实践。
4. 让全中国的战斗策划都有Demo可做！
5. 让Unity再次伟大！

# 使用方式
1. 创建一个URP的3D项目。
2. 在你的项目中引入Cinemachine和Input System。
3. 将此项目克隆到Asset文件夹。例如：C:nicy/PengFrameworkSample/Asset/PengFramework
4. 在Resources/InputData/Universal里右键，Create - InputActions，命名为PengActorInput，随后点击该资源，在Inspector里勾选Generate C# Class，然后点击Apply。等待编译完成后，双击该资源，在打开的窗口里，找到最左侧一栏，表头为Action Maps的，点击它右侧的加号，创建一个新的映射，将其命名为Basic；随后在Actions一栏中，双击New Action，将其重命名为Move。然后，在Action Properties里，将Action Type改为Value，Control Type改为Vector2。然后点击Move一栏右侧的加号，点击Add Up/Down/Left/Right Composite，并将Move下默认的<No Binding>删掉。在创建出的上下左右里，分别配置其Binding Properties里的Binding中的Path为WASD。配置完毕后，记得点上方的Save Asset，等待其完成自动编译。
5. 打开启动器，进行全局配置。
5. 记得在场景中放置一个名为Game的Prefab，它在Assets/Resources/Managers/GameManager/Game.prefab

# 后续开发计划
1. 配套AI插件
2. 黑板
3. 更多事件
4. 配套关卡编辑器
5. 断点
6. 运行时数据监控
7. 更多节点

# 联系我

B站：@Nicy彭彭

个人博客：https://nicypeng.notion.site/54d5ce6ed8e2451fa36eb31b9c0e6efa?v=1167ad1ba4a24daaa0104f14f2d07e60&pvs=25
