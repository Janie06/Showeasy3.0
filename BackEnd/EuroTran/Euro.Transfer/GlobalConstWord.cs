namespace Euro.Transfer
{
    public class GlobalConstWord
    {
        public const string TASKALERTSQL = @" SELECT t.OrgID,[EventID],[EventName],[Owner],[StartDate],[EndDate],(CASE [Important] WHEN 'M' THEN '普通' else '重要' end) as [Important],m.[MemberName] as [CreateUser],[TaskDescription],SourceFrom,Params
                                              FROM OTB_SYS_Task t inner join OTB_SYS_Members m on t.CreateUser=m.MemberID and t.OrgID=m.OrgID
	                                          where ([Status]='U' AND AlertTime IS NULL) or ([Status]<>'O' AND [Status]<>'D' AND AlertTime>=GETDATE()) ";
    }
}