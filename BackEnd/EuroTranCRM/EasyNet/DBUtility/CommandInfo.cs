using System;
using System.Data.SqlClient;

namespace EasyNet.DBUtility
{
    public enum EffentNextType
    {
        /// <summary>
        /// 對其它語句无任何影響
        /// </summary>
        None,

        /// <summary>
        /// 當前語句必須为"select count(1) from .."格式，如果存在則繼續執行，不存在回滾事務
        /// </summary>
        WhenHaveContine,

        /// <summary>
        /// 當前語句必須为"select count(1) from .."格式，如果不存在則繼續執行，存在回滾事務
        /// </summary>
        WhenNoHaveContine,

        /// <summary>
        /// 當前語句影響到的行數必須大于0，否則回滾事務
        /// </summary>
        ExcuteEffectRows,

        /// <summary>
        /// 引發事件-當前語句必須为"select count(1) from .."格式，如果不存在則繼續執行，存在回滾事務
        /// </summary>
        SolicitationEvent
    }

    public class CommandInfo
    {
        public object ShareObject = null;
        public object OriginalData;

        private event EventHandler _solicitationEvent;

        public event EventHandler SolicitationEvent
        {
            add
            {
                _solicitationEvent += value;
            }
            remove
            {
                _solicitationEvent -= value;
            }
        }

        public void OnSolicitationEvent()
        {
            _solicitationEvent?.Invoke(this, new EventArgs());
        }

        public string CommandText;
        public System.Data.Common.DbParameter[] Parameters;
        public EffentNextType EffentNextType = EffentNextType.None;

        public CommandInfo()
        {
        }

        public CommandInfo(string sqlText, SqlParameter[] para)
        {
            this.CommandText = sqlText;
            this.Parameters = para;
        }

        public CommandInfo(string sqlText, SqlParameter[] para, EffentNextType type)
        {
            this.CommandText = sqlText;
            this.Parameters = para;
            this.EffentNextType = type;
        }
    }
}