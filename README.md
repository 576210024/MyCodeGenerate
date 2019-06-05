# LenovoCodeGenerate
公司内部项目专用代码模板

----------------关于------------------

此程序为基本版

----------------注意------------------

* 主要针对oracle优化模板
* sqlserver如果需要生成的话那么需要重新修改，基本不可用
* 表中应定义主键，至少应该包含一个逐渐
* 表集合过滤条件（防止过多）：LIKE(%HD_CONSUL%ITEMS%)
* 包含2个表名称LIKE 查找关键字，可以实现精准过滤和模糊查找

----------------生成模块说明-----------------

* 1、读取数据库中所有表及字段
* 2、遍历DBInfo，生成 Entities层代码
* 3、遍历DBInfo，生成 Rules层代码
* 4、遍历DBInfo，生成 IBusiness层代码
* 5、遍历DBInfo，生成 Business层代码
* 6、遍历DBInfo，生成 Proxy层代码
* 7、遍历DBInfo，生成 Presenters层代码
* 8、遍历DBInfo，生成 I--View层代码
* 9、遍历DBInfo，生成 ViewModel层代码
* 10、遍历DBInfo，生成 RuleFactory,BaseRule,Presenter,RuleFactory,DBHelper代码

----------------未完成功能点-----------------

* 1、前台界面结合公司使用UI框架生成基本的增删改查界面以及按钮等
* 2、cBusiness中的ViewModel类
* 3、前后端交互Lamda规则
* 4、Rules层中根据字段有无动态拼装字段或者加入Lamba后实现根据动态传输生成动态语句
* 5、实现第4点可以实现前后端数据传输量，减小带宽压力，交互更快、体验更好等优点
* 6、SqlServer（如果使用EntityFramework，那么仓储层这一块的内容可以不用生成）
