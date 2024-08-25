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
4. 打开启动器，进行全局配置。
5. 记得在场景中放置一个名为Game的Prefab，它在Assets/Resources/Managers/GameManager/Game.prefab
6. Asset/PengFramework/Prefab下若有预制体，请将其复制到应当出现的地方。例如，MainFreeLook预制体应当放在Resources/Cameras下。
7. 可以通过角色生成器生成角色，并且配置。之后，使用关卡生成器生成关卡，并且配置其逻辑。之后，将关卡预制体拖入场景中即可。
8. 说明：框架中的关卡，实际指代着关卡逻辑的控制器，它能够按照一个流程去等待某条件完成并执行某项操作。
9. 更多详细信息请参考：https://nicypeng.notion.site/ed948501a86940d389df1c71f7a9e058

# 后续开发计划
1. 配套AI插件
2. 更多关卡、角色节点（详情可见PengScript.cs和PengLevelRuntimeFunction.cs里的功能类型枚举。）

# 联系我

B站：@Nicy彭彭

个人博客：https://nicypeng.notion.site/54d5ce6ed8e2451fa36eb31b9c0e6efa?v=1167ad1ba4a24daaa0104f14f2d07e60&pvs=25

# 添加新节点
1. PengScript.cs里，添加新的脚本类别，并标注描述：(开发完毕？1：0),脚本中文名,类别,首字母,封装程度
2. EditorNodes文件夹下，根据脚本类别，在对应的脚本文件里写新的节点形式
3. PengAddNode里的两个方法里，补充新的脚本类别添加方法
4. RuntimeScripts文件夹下，根据脚本类别，在对应的脚本文件里写新的运行时功能
5. PengActorState.ConstructRunTimePengScript()里添加运行时构建新脚本的方法

关卡节点同理。
