/**
 * Created by DongJi_Ren on 2016/4/28.
 */
//数据库名
var DB_NAME='addr_list';
//对象存储空间名
var OBJ_SPASE_NAME='addrs';
//数据库实例对象
var mydb;



$(document).ready(function(){
/**********************一、数据库初始化的操作*************************************************************************************/
    //IndexedDB的规范尚未最终定稿，不同的浏览器厂商还是使用浏览器前缀实现IndexedDB API。
    // 基于Gecko内核的浏览器使用moz前缀，基于WebKit内核的浏览器使用webkit前缀。
    window.indexedDB=window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB;
    if(window.indexedDB){                                                       //当浏览器支持IndexedDB数据库
        var request;                                                            // request对象用于处理用户对数据库的操作请求
        //1、打开或创建数据库
        request=window.indexedDB.open(DB_NAME,'3.0');                           //参数为：数据库名和版本号；数据库名存在，则打开它；否则创建数据库。
        //2、指定操作成功的处理函数(可以获得对象存储空间信息)
        request.onsuccess=function(event){
            alert('数据库打开成功！');
            mydb=request.result;                                                   //数据库实例对象
            //var len = mydb.objectStoreNames.length;                             //对象存储空间名的个数
            //var  name=mydb.objectStoreNames[i];                                 //对象存储空间名
            /*******调用********************/
            byCursorGet(mydb);
        };
        //3、指定操作失败的处理函数
        request.onerror=function(event){
            alert("打开失败,错误号：" + event.target.errorCode);
        };
        //4、执行改变数据库结构的操作函数（包括创建对象存储空间）。onupgradeneeded事件在下列情况下被触发：数据库第一次被打开时；打开数据库时指定的版本号高于当前被持久化的数据库版本号。(可通过修改版本号触发该事件)
        request.onupgradeneeded = function(event) {
            mydb=request.result;//获得数据库实例对象
            if(!mydb.objectStoreNames.contains(OBJ_SPASE_NAME)) {                   //判断对象存储空间名称是否已经存在
                var objectStore = mydb.createObjectStore(OBJ_SPASE_NAME, {keyPath: "phone"});//创建students对象存储空间;指定keyPath选项为id（即主键为id）
                //对象存储空间students的列email上创建一个唯一索引email,可以创建多个索引。
                objectStore.createIndex("phone",                                //索引名
                                        "phone",                                //创建索引的列（即keyPath,索引属性字段名）
                                        { unique: true });                      //索引选项(索引属性值是否唯一:true or false)
            }
        }
    } else{
        alert("您的浏览器不支持IndexedDB数据库。");
    }
    /*setTimeout(function(){
        mydb.close();                                                       //关闭连接
        window.indexedDB.deleteDatabase(DB_NAME);                          //删除数据库
    },1500);*/



/*********************二、定义方法调用****************************************************************************/


    //插入数据，
    function insert(mydb){
        var data = {
            "name": $("#name").val(),
            "phone": $("#phone").val(),
            "company": $("#company").val(),
            "date":new Date().toLocaleTimeString()
        };
        //使用事务
        var transaction = mydb.transaction(OBJ_SPASE_NAME,                        //事务操作的对象存储空间名
                                        'readwrite');                         //事务模式:'readwrite'可读写模式;READ_ONLY只读模式;VERSION_CHANGE版本升级模式;
        //2.1、当事务中的所有操作请求都被处理完成时触发
        transaction.oncomplete = function(event) {};
        //2.2、当事务中出现错误时触发，默认的处理方式为回滚事务；
        transaction.onerror = function(event) {};
        //2.3、当事务被终止时触发
        transaction.onabort = function(event){};
        //2.4、从事务中获得相关的对象存储空间对象
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        //向students存储空间加入一个student对象，获得request对象用于处理用户对数据库的操作请求(同样拥有onerror，onupgradeneeded，onsuccess事件)
        var request = objStore.add(data);
        request.onsuccess = function(e) {
            alert("成功插入数据，id=" + e.target.result);
        };
    }
    //查询数据
    function get(mydb){
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var request = objStore.get(n);                 //按照id查询
        request.onsuccess=function(e){
            var tr="";
            var obj=e.target.result;
            tr=tr+"<tr><td>"+(1)+"</td><td>"+obj.name+"</td><td class='p'>"+obj.phone
                +"</td><td>"+obj.company+"</td><td>"+obj.date
                +"</td><td><input type='button' value='删除'></td></tr>";
            //叠加表格内容
            $("#check span").html(
                "<table width='100%'>"
                +"<tr><td>序号</td><td>姓名</td><td>手机</td><td>公司</td><td>添加时间</td><td>操作</td></tr>"
                +tr+"</table>"
            );
        }
    }
    //更新数据
    function update(mydb){
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var request = objStore.get("110");
        request.onsuccess=function(e){
            var student=e.target.result;
            student.name='wwww1';
            objStore.put(student);
        }
    }
    //删除数据
    function remove(mydb){
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var request = objStore.delete(myphone);
        request.onsuccess = function(e) {
            alert("成功删除数据");
        };
    }
    //利用索引查询
    function byIndexGet(mydb){
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var index = objStore.index('email');                //索引名
        var request=index.get('liming1@email.com');         //通过索引值获取数据
        request.onsuccess=function(e){
            var student=e.target.result;
            alert(student.name+"：索引查询");
        }
    }
    //游标遍历所有
    function byCursorGet(mydb){
        var tr='';var i=0;
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var request=objStore.openCursor();//打开游标
        request.onsuccess = function(e){
            var cursor = e.target.result;
            if(cursor){
                //alert(cursor.value.name);
                var obj=cursor.value;
                tr=tr+"<tr><td>"+(i+1)+"</td><td>"+obj.name+"</td><td class='p'>"+obj.phone
                    +"</td><td>"+obj.company+"</td><td>"+obj.date
                    +"</td><td><input type='button' value='删除'></td></tr>";i=i+1;
                cursor.continue();
            }else {
                alert('遍历完成');
                //叠加表格内容
                $("#result span").html(
                    "<table width='100%'>"
                    +"<tr><td>序号</td><td>姓名</td><td>手机</td><td>公司</td><td>添加时间</td><td>操作</td></tr>"
                    +tr+"</table>"
                );
            }
        }
    }
    //通过范围和排序条件，游标遍历符合条件的数据
    function byCursorGetForRangeAndSort(mydb){
        var transaction = mydb.transaction(OBJ_SPASE_NAME,'readwrite');
        transaction.oncomplete = function(event) {};
        transaction.onerror = function(event) {};
        transaction.onabort = function(event){};
        var objStore = transaction.objectStore(OBJ_SPASE_NAME);
        var range = IDBKeyRange.bound("110", "113", false, true);   //范围
        var request=objStore.openCursor(range,             //范围（可以为null或省略不写）
                                        IDBCursor.NEXT);    //游标顺序循环(可以省略不写)
        request.onsuccess = function(e){
            var cursor1 = e.target.result;
            if(cursor1){
                alert(cursor1.value.name);
                cursor1.continue();
            }else {
                alert('遍历完成');
            }
        }
        /**
         * 范围：
         *（1）匹配等于指定键值的记录：var range = IDBKeyRange.only(指定键值)
         *（2）匹配小于指定键值的记录：var range = IDBKeyRange.lowerBound(指定键值, 是否不包括指定键值)
         *（3）匹配大于指定键值的记录：var range = IDBKeyRange.upperBound(指定键值, 是否不包括指定键值)
         *（4）匹配指定范围内的记录：var range = IDBKeyRange.bound("110",//下限键值
         *                                 "113",//上限键值
         *                                false,//是否不包括下限键值
         *                                true);//是否不包括上限键值
         */
        /**
         * 顺序参数：
         * IDBCursor.NEXT，顺序循环；
         * IDBCursor.NEXT_NO_DUPLICATE，顺序循环且键值不重复；
         * IDBCursor.PREV，倒序循环；
         * IDBCursor.PREV _NO_DUPLICATE，倒序循环且键值不重复。

         */
    }






/***************************三、界面事件处理*****************************************************************/
    //插入数据
    $("#sm").on('click',function(){
        insert(mydb);
        byCursorGet(mydb);
    });
    //单击删除按钮
    var myphone;
    $(document).on('click',"table input",function(){
        myphone=$(this).parent().prev().prev().prev().text();
        $(this).parent().parent().remove();
        remove(mydb);
    });
    //查询
    var n;
    $("#check input[type='button']").on('click',function(){
        n=$("#check input:first").val();
        get(mydb);
    });





});
