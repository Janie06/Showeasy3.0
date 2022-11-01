using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EasyBL.DllDynamicCall
{
    public class DllCall
    {
        private Assembly _assembly;

        public Dictionary<string, object> Objects { get; set; }

        public Dictionary<string, Dictionary<string, MethodInfo>> Methods { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="dllPath">DLL绝对路径</param>
        public DllCall(string dllPath)
        {
            LoadDll(dllPath);

            Objects = new Dictionary<string, object>();
            Methods = new Dictionary<string, Dictionary<string, MethodInfo>>();
        }

        #region 公开方法

        /// <summary>
        /// 注册要目标类
        /// </summary>
        /// <param name="classFullName">类的完全限定名</param>
        /// <param name="objectName">指定的对象名</param>
        /// <param name="constructorParaTypes">构造器参数类型</param>
        /// <param name="constructorParas">构造器参数</param>
        public void RegisterObject(string classFullName, string objectName, Type[] constructorParaTypes, object[] constructorParas)
        {
            var t = _assembly.GetType(classFullName, true, false);

            var constructorInfo = t.GetConstructor(constructorParaTypes);

            if (constructorInfo != null)
            {
                var targetObject = constructorInfo.Invoke(constructorParas);

                if (!Objects.ContainsKey(objectName))
                {
                    Objects.Add(objectName, targetObject);
                }
                if (!Methods.ContainsKey(objectName))
                {
                    Methods.Add(objectName, new Dictionary<string, MethodInfo>());
                }
            }
        }

        /// <summary>
        /// 注册函数
        /// </summary>
        /// <param name="classFullName">类完全限定名</param>
        /// <param name="objectName">制定函数所在对象名</param>
        /// <param name="funcName">函数名</param>
        public void RegisterFunc(string classFullName, string objectName, string funcName)
        {
            var t = _assembly.GetType(classFullName, true, false);
            var method = t.GetMethod(funcName);
            if (Methods.ContainsKey(objectName))
            {
                if (!Methods[objectName].ContainsKey(funcName))
                {
                    Methods[objectName].Add(funcName, method);
                }
            }
        }

        /// <summary>
        /// 调用函数
        /// </summary>
        /// <param name="objectName">目标对象名</param>
        /// <param name="funcName">函数名</param>
        /// <param name="paras">参数表,没有参数则用null</param>
        /// <returns></returns>
        public object CallFunc(string objectName, string funcName, object[] paras)
        {
            var targetObjec = Methods[objectName];
            var targetFunc = targetObjec[funcName];
            const BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
            var result = targetFunc.Invoke(Objects[objectName], flag, Type.DefaultBinder, paras, null);
            return result;
        }

        #endregion 公开方法

        #region 私有方法

        /// <summary>
        /// 加载DLL
        /// </summary>
        /// <param name="dllPath">DLL绝对路径</param>
        private void LoadDll(string dllPath)
        {
            if (File.Exists(dllPath))
            {
                _assembly = Assembly.LoadFrom(dllPath);
            }
            else
            {
                throw new FileNotFoundException($"{dllPath} isn't exist!");
            }
        }

        #endregion 私有方法
    }
}