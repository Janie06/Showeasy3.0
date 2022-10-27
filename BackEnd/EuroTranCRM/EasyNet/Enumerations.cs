using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyNet
{
    public enum MaxNumberType
    {
        /// <summary>
        /// 依據年編號
        /// </summary>
        MinYear,

        /// <summary>
        /// 依據年編號
        /// </summary>
        Year,

        /// <summary>
        /// 依據年月編號
        /// </summary>
        Month,

        /// <summary>
        /// 依據年月編號
        /// </summary>
        Day,

        /// <summary>
        /// 依據年月日編號八位
        /// </summary>
        DayForSix,

        /// <summary>
        /// 依據年月日編號六位
        /// </summary>
        Other,

        /// <summary>
        /// 不需要依據年月日編號
        /// </summary>
        Empty
    }

    public class SerialNumber
    {
        public const string MaxNumberSQL = @"begin tran
               DECLARE @MaxNumberCatData INT;
                    SET @MaxNumberCatData = 0;
                    IF @Flag = 'Y' OR @Flag = 'Year'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
	                    END
                    ELSE IF  @Flag = 'MY' OR @Flag = 'MinYear'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = RIGHT(YEAR(GETDATE()),2) AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
	                    END
                    ELSE IF  @Flag = 'M' OR @Flag = 'Month'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND [CountMonth] = MONTH(GETDATE()) AND ([CountDay] IS NULL OR [CountDay]='')
	                    END
                    ELSE IF  @Flag = 'D' OR @Flag = 'Day'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND [CountMonth] = MONTH(GETDATE()) AND [CountDay] = DAY(GETDATE())
	                    END
                    ELSE IF  @Flag = 'DS' OR @Flag = 'DayForSix'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = RIGHT(YEAR(GETDATE()),2) AND [CountMonth] = MONTH(GETDATE()) AND [CountDay] = DAY(GETDATE())
	                    END
                    ELSE IF  @Flag = 'O' OR @Flag = 'Other'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND ([CountYear] IS NULL OR [CountYear]='') AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
	                    END
                    ELSE IF  @Flag = 'E' OR @Flag = 'Empty'
	                    BEGIN
		                    SELECT @MaxNumberCatData = ISNULL(CountMax,0)
		                    FROM OTB_SYS_MaxNumber WITH (TABLOCKX)
		                    WHERE OrgID = @OrgID AND  [Type] = @Type AND ([CountYear] IS NULL OR [CountYear]='') AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
	                    END

                    IF @MaxNumberCatData > 0
	                    BEGIN
                        set	@MaxNumberCatData=@MaxNumberCatData+1;
		                    IF @Flag = 'Y' OR @Flag = 'Year'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
			                    END
		                    ELSE IF  @Flag = 'MY' OR @Flag = 'MinYear'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = RIGHT(YEAR(GETDATE()),2) AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
			                    END
		                    ELSE IF  @Flag = 'M' OR @Flag = 'Month'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND [CountMonth] = MONTH(GETDATE()) AND ([CountDay] IS NULL OR [CountDay]='')
			                    END
		                    ELSE IF  @Flag = 'D' OR @Flag = 'Day'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = YEAR(GETDATE()) AND [CountMonth] = MONTH(GETDATE()) AND [CountDay] = DAY(GETDATE())
			                    END
		                    ELSE IF  @Flag = 'DS' OR @Flag = 'DayForSix'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND [CountYear] = RIGHT(YEAR(GETDATE()),2) AND [CountMonth] = MONTH(GETDATE()) AND [CountDay] = DAY(GETDATE())
			                    END
		                    ELSE IF  @Flag = 'O' OR @Flag = 'Other'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND ([CountYear] IS NULL OR [CountYear]='') AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
			                    END
		                    ELSE IF  @Flag = 'E' OR @Flag = 'Empty'
			                    BEGIN
				                    UPDATE OTB_SYS_MaxNumber SET CountMax = ISNULL(CountMax,0) + 1, ModifyUser=@ModifyUser,ModifyDate =GETDATE()
				                    WHERE OrgID = @OrgID AND  [Type] = @Type AND ([CountYear] IS NULL OR [CountYear]='') AND ([CountMonth] IS NULL OR [CountMonth]='') AND ([CountDay] IS NULL OR [CountDay]='')
			                    END
	                    END
                    ELSE
	                    BEGIN
	                    SET @MaxNumberCatData = 1;
		                    IF @Flag = 'Y' OR @Flag = 'Year'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,YEAR(GETDATE()),'','',1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'MY' OR @Flag = 'MinYear'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,RIGHT(YEAR(GETDATE()),2),'','',1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'M' OR @Flag = 'Month'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,YEAR(GETDATE()),MONTH(GETDATE()),'',1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'D' OR @Flag = 'Day'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,YEAR(GETDATE()),MONTH(GETDATE()),DAY(GETDATE()),1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'DS' OR @Flag = 'DayForSix'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,RIGHT(YEAR(GETDATE()),2),MONTH(GETDATE()),DAY(GETDATE()),1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'O' OR @Flag = 'Other'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,'','','',1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
		                    ELSE IF  @Flag = 'E' OR @Flag = 'Empty'
			                    BEGIN
				                    INSERT INTO OTB_SYS_MaxNumber(OrgID,[Type],CountYear,CountMonth,CountDay,CountMax,CreateUser,CreateDate,ModifyUser,ModifyDate)VALUES(@OrgID,@Type,'','','',1,@ModifyUser,GETDATE(),@ModifyUser,GETDATE())
			                    END
	                    END

                    SELECT @MaxNumberCatData
                  commit tran ";

        public static DateTime dteNowTime = DateTime.Now;         //當前系統DB時間

        #region GetMaxNumberByType

        /// <summary>
        /// 取得類型下資料的自動編號
        /// </summary>
        /// <param name="Type">類別代號</param>
        /// <param name="Flag">自動編號方式 Y:年；M:月；D:日；O:其他(不按照年月日編號)</param>
        /// <param name="OrgID">todo: describe OrgID parameter on GetMaxNumberByType</param>
        /// <param name="ModifyUser">todo: describe ModifyUser parameter on GetMaxNumberByType</param>
        /// <param name="iLen">todo: describe iLen parameter on GetMaxNumberByType</param>
        /// <param name="inSertStr">todo: describe inSertStr parameter on GetMaxNumberByType</param>
        /// <param name="AddType">todo: describe AddType parameter on GetMaxNumberByType</param>
        /// <returns>返回intLen位流水號不夠左邊補零並且添加Type為前綴</returns>
        public static string GetMaxNumberByType(string OrgID, string Type, MaxNumberType Flag, string ModifyUser, int iLen, string inSertStr = null, string AddType = "")
        {
            var sRetrun = string.Empty;
            switch (Flag)
            {
                case MaxNumberType.MinYear:
                    sRetrun += dteNowTime.ToString("yy");
                    break;

                case MaxNumberType.Year:
                    sRetrun += dteNowTime.ToString("yyyy");
                    break;

                case MaxNumberType.Month:
                    sRetrun += dteNowTime.ToString("yyyyMM");
                    break;

                case MaxNumberType.Day:
                    sRetrun += dteNowTime.ToString("yyyyMMdd");
                    break;

                case MaxNumberType.DayForSix:
                    sRetrun += dteNowTime.ToString("yyMMdd");
                    break;

                case MaxNumberType.Other:
                    sRetrun += "0";
                    break;

                default:
                    sRetrun = string.Empty;
                    break;
            }
            if (Type != "")
            {
                sRetrun = Type + sRetrun;
            }
            if (AddType != "")
            {
                Type = Type + AddType;
            }
            if (inSertStr != "")
            {
                sRetrun += inSertStr;
            }
            var dic_pm = new Dictionary<string, string>
            {
                { nameof(OrgID), OrgID },
                { nameof(Type), Type },
                { nameof(Flag), Flag.ToString() },
                { nameof(ModifyUser), ModifyUser }
            };

            var db = SugarBase.GetIntance();
            sRetrun += db.Ado.GetString(MaxNumberSQL, dic_pm).PadLeft(iLen, '0');
            return sRetrun;
        }

        #endregion GetMaxNumberByType

        public static MaxNumberType GetMaxNumberType(string sType)
        {
            var numtype = new MaxNumberType();
            switch (sType)
            {
                case "MinYear":
                    numtype = MaxNumberType.MinYear;
                    break;

                case "Year":
                    numtype = MaxNumberType.Year;
                    break;

                case "Month":
                    numtype = MaxNumberType.Month;
                    break;

                case "Day":
                    numtype = MaxNumberType.Day;
                    break;

                case "DayForSix":
                    numtype = MaxNumberType.DayForSix;
                    break;

                case "Other":
                    numtype = MaxNumberType.Other;
                    break;

                case "Empty":
                    numtype = MaxNumberType.Empty;
                    break;

                default:
                    break;
            }

            return numtype;
        }

        /// <summary>
        /// 計算檢核碼並返回
        /// </summary>
        /// <param name="str">傳入字串</param>
        /// <returns></returns>
        public static int Pcheck(string str)
        {
            var iRes = 0;
            var iEven = 0;
            var iOdd = 0;
            try
            {
                for (int idx = 0; idx < str.Length; idx++)
                {
                    var iRsl = 0;
                    var sChart = str[idx].ToString();
                    if (idx % 2 == 0)
                    {
                        if (int.TryParse(sChart, out iRsl)) //判断是否可以转换为整型
                        {
                            iEven += iRsl;
                        }
                    }
                    else
                    {
                        if (int.TryParse(sChart, out iRsl))
                        {
                            iOdd += iRsl;
                        }
                    }
                }
                var sSum = (iEven * 3 + iOdd).ToString();
                iRes = 10 - int.Parse(sSum.Substring(sSum.Length - 1));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return iRes == 10 ? 0 : iRes;
        }
    }
}