# RS-TID-SID-Frame-Finder
## 关于
本项目汉化由 白希洛/Hakuhiro 一人完成 包含修改、打包、测试等操作

本软件用于宝可梦gen3红蓝宝石的表里ID乱数操作
输入你想要的表ID以及里ID或者PID，然后通过一些算法计算出可以产生这些组合的Seed、帧数、日期、异色情况、TSV

本项目软件还进行了整体的新增、删改、修复，内容大致如下
- 去除DarkUI组件，改为WinForm自带组件（据了解，因为这个组件库的作者似乎已经去世）
- 所有控件的汉化
- 新增单选按钮，分为SID和PID，现在你可以尽情选择想要的表里ID组合啦！（据测试，部分表里ID组合无法算出结果）
- 修复表格底部总是多出空白的一行
- 新增表格中TSV闪值列
- 所有字体改为宋体

有问题请前往本人博客hakuhiro.xyz寻找联系方式，或者b站搜索相关教程联系到我

关于具体的使用教程的视频说明，可以在前往[本人b站首页](https://b23.tv/KTuljQ9) 搜索查看

# 以下是原说明文档
Input the **TID** you want and the **PID** you want shiny and this program will output the Seed, (Live Battery) Frame, and Date you need to get the correct TID/SID Combination.

![Screenshot](https://i.imgur.com/rB5ezm9.png)

The concepts in this code are taken from [PokeFinder,](https://github.com/Admiral-Fish/PokeFinder) so credit to *Admiral Fish*, *zaksabeast*, and the others who've worked on that tool.
