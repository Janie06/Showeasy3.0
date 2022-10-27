﻿using System.Collections.Generic;

namespace SqlSugar
{
    public interface IConditionalModel {

    }
    public class ConditionalCollections : IConditionalModel
    {
         public List<KeyValuePair<WhereType, ConditionalModel>> ConditionalList { get; set; }
    }
 
    public class ConditionalModel: IConditionalModel
    {
        public ConditionalModel()
        {
            this.ConditionalType = ConditionalType.Equal;
        }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public ConditionalType ConditionalType { get; set; }
    }
}
