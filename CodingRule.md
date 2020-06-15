# 代码规范
    1. 每个cs文件中尽量只维护一个class对象<br>
    
    2. 私有属性使用 "_"(dashline下划线) 开头的驼峰命名规则, 如: _currSelectedObj;<br>
    
    3. 方法不公私有全部使用大写开头的驼峰命名规则, 如: ShowGird();<br>
    
    4. 常量使用全大写词与词之间使用 "_"(dashline下划线) 分割;<br>
    
    5. 单行的分支逻辑必须换行，如:<br>
        if( success ) <br>
        {<br>
            break;<br>
        } <br>
        