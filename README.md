# 彭框架 PengFramework
A framework for the development of action games.

动作游戏开发框架。

核心目标：
1. 人性化的交互。
2. 灵活，拓展性强。
3. 符合动作游戏的开发实践。
4. 让全中国的战斗策划都有Demo可做！
5. 让Unity再次伟大！

优秀特性：
1. 编辑器模式、运行时模式均能对角色逻辑进行编辑。
2. 能实时预览状态的角色动画、特效、攻击框与音效。
3. 常用节点高度集成。
4. 时间轴和蓝图的结合，符合直觉、交互清晰，逻辑的纵向先后与横向分支都能hold住。
5. 配套Buff编辑器、关卡编辑器、AI编辑器等工具，一套组件即可完成游戏的开发。
6. 调试方便，实现简单，潜规则少，开发效率高。
7. 与Cinemachine，InputSystem相配合，不依赖其他付费插件。
8. 信息集中于角色、关卡，免去配表烦恼。

特别信息：

1. 某位好友出于某种原因，希望我要求该框架的使用者不能用该框架来制作类魂Gameplay。当然，你可以拿来做任何想做的玩法，我只是帮他把这句话贴在这里。
2. 使用该框架开发个人作品Demo请务必标注出处！你可以用以下任意方式之一进行标注：

   a. 在暂停界面里标注；

   b. 在标题画面里标注；

   c. 在载入界面里放上该框架的Logo；

   d. 在实际Gameplay的画面边缘用透明小字标注。

# 使用方式
1. 创建一个URP的3D项目。
2. 在你的项目中引入Cinemachine, AI Navigation和Input System。
3. 将此项目克隆到Asset文件夹。例如：C:nicy/PengFrameworkSample/Asset/PengFramework
4. 打开启动器，进行全局配置；每次更新后，建议都使用启动器更新一遍，以补全可能需要的文件夹。
5. 记得在场景中放置一个名为Game的Prefab，它在Assets/Resources/Managers/GameManager/Game.prefab
6. Asset/PengFramework/Prefab下若有预制体，请将其复制到应当出现的地方。例如，MainFreeLook预制体应当放在Resources/Cameras下；PlayerHPBar、EnemyHPBar、BossHPBar应当放在Resources/UIs/Universal下。MainCanvas和EventSystem则不需要额外的移动，拖入场景中即可；AirWall一般直接放置在Level的子层级即可。请勿修改任何预制体的名称、Tag、层级等信息。
7. 可以通过角色生成器生成角色，并且配置。之后，使用关卡生成器生成关卡，并且配置其逻辑。之后，将关卡预制体拖入场景中即可。
8. 说明：框架中的关卡，实际指代着关卡逻辑的控制器，它能够按照一个流程去等待某条件完成并执行某项操作。
9. 关卡编辑完毕后，点击Game预制体中的NavMeshSurface组件的Bake按键，以生成AI可通过的区域。NavMesh的用法请自行搜索相关教程。
10. 更多详细信息请参考（持续更新中）：https://nicypeng.notion.site/ed948501a86940d389df1c71f7a9e058
11. 有新的需求欢迎提Issue

# 后续开发计划
1. 角色功能：伤害跳字、技能CD（CD、充能制等）、相机相关的节点
2. 关卡功能：跳转状态、增删Buff、跳转场景、生成对话、物件交互、显示UI、屏蔽UI
3. AI节点：修改变量、敌方状态事件、友方跟随
4. 存档系统、暂停等外围系统模板
5. 可能的：英文本地化

# 联系我

B站：@Nicy彭彭

个人博客：https://nicypeng.notion.site/54d5ce6ed8e2451fa36eb31b9c0e6efa?v=1167ad1ba4a24daaa0104f14f2d07e60&pvs=25

# 添加新节点
1. PengScript.cs里，添加新的脚本类别，并标注描述：(开发完毕？1：0),脚本中文名,类别,首字母,封装程度
2. EditorNodes文件夹下，根据脚本类别，在对应的脚本文件里写新的节点形式
3. PengAddNode里的两个方法里，补充新的脚本类别添加方法
4. RuntimeScripts文件夹下，根据脚本类别，在对应的脚本文件里写新的运行时功能
5. PengActorState.ConstructRunTimePengScript()里添加运行时构建新脚本的方法

关卡、AI节点同理。
