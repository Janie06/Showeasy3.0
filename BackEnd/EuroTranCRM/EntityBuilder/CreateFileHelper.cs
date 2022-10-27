using SqlSugar;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EntityBuilder
{
    public class CreateFileHelper
    {
        /// <summary>
        /// 创建文件目录和文件
        /// </summary>
        /// <param name="tables">所有表</param>
        /// <param name="fileDir">文件目录</param>
        public static void CreateEntityHelper(List<DbTableInfo> tables, string fileDir)
        {
            CreateDirectory(fileDir);
            //實體類文件名
            var filePath = fileDir + "EntityHelper.cs";
            //文件是否存在
            var exists = File.Exists(filePath);
            if (exists)
            {
                File.Delete(filePath);
            }
            //創建文件
            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using (var sw = new StreamWriter(fs))
            {
                //生成代码
                var code = CreateFileHelper.BuilderEntityHelperCode(tables);

                //写入代码到文件
                sw.WriteLine(code);

                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 创建文件目录
        /// </summary>
        /// <param name="targetDir"></param>
        private static void CreateDirectory(string targetDir)
        {
            var dir = new DirectoryInfo(targetDir);
            if (!dir.Exists)
                dir.Create();
        }

        /// <summary>
        /// 根据表名，生成代码
        /// </summary>
        /// <param name="tables">todo: describe tables parameter on BuilderEntityHelperCode</param>
        /// <returns></returns>
        public static string BuilderEntityHelperCode(List<DbTableInfo> tables)
        {
            var sb = new StringBuilder();
            var EntityCase = new StringBuilder();
            sb.Append("using Entity.Sugar;").Append("\n\n");
            sb.Append("namespace Entity  ").Append("\n");
            sb.Append("{  ").Append("\n");
            sb.Append("\t public class EntityHelper").Append("\n");
            sb.Append("\t { ").Append("\n");

            foreach (DbTableInfo table in tables)
            {
                //實體類名稱
                var entityName = table.Name;//GenVarName(table.Name);
                var keyPre = "";
                var entitys = entityName.Split('_');
                if (entityName.StartsWith("OVW_"))
                {
                    keyPre = "ovw_";
                }
                sb.Append("\t\t").Append("public const string ").Append((keyPre + entitys[entitys.Length - 1]).ToUpper()).Append(" = ").Append("\"" + keyPre + (entitys[entitys.Length - 1]).ToLower() + "\";\n\n");

                EntityCase.Append("\t\t\t\t").Append("case " + (keyPre + entitys[entitys.Length - 1]).ToUpper() + ":\n\n");
                EntityCase.Append("\t\t\t\t\t\t").Append("entity = new " + entityName + "();\n\n");
                EntityCase.Append("\t\t\t\t\t\t").Append("break;\n");
            }

            sb.Append("\t\t").Append("/// <summary>").Append("\n");
            sb.Append("\t\t").Append("/// get the entity object").Append("\n");
            sb.Append("\t\t").Append("/// </summary>").Append("\n");
            sb.Append("\t\t").Append("/// <param name=\"type\"/>type{String}</param>").Append("\n");
            sb.Append("\t\t").Append("/// <returns>entity{Object}entity</returns>").Append("\n");
            sb.Append("\t\t").Append("public static object GetEntity(string type)").Append("\n");
            sb.Append("\t\t").Append("{").Append("\n");
            sb.Append("\t\t\t\t").Append("var entity = new object();").Append("\n");
            sb.Append("\t\t\t\t").Append("entity = \"\";").Append("\n");
            sb.Append("\t\t\t\t").Append("switch (type.ToLower())").Append("\n");
            sb.Append("\t\t\t\t").Append("{").Append("\n");
            sb.Append("\t\t").Append(EntityCase.ToString()).Append("\n");
            sb.Append("\t\t\t\t").Append("}").Append("\n");
            sb.Append("\t\t\t\t").Append("return entity;").Append("\n");
            sb.Append("\t\t").Append("}").Append("\n");
            sb.Append("\t").Append("}").Append("\n");
            sb.Append("}").Append("\n");

            return sb.ToString();
        }
    }
}