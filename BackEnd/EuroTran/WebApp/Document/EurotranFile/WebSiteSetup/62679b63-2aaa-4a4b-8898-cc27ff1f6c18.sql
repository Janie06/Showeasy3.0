
select * from OTB_SYS_Announcement

--update OTB_SYS_Announcement set [Description]=REPLACE([Description],'http:','https:')



 --update OTB_OPM_BillsBak set ForeignCurrencyCode='NTD' where OrgID='sg' and ForeignCurrencyCode=''
 --update OTB_OPM_BillsBak set ForeignCurrencyCode='' where OrgID='sg' and ForeignCurrencyCode='RMB'



alter table OTB_EIP_AttendanceDiff add RelationId  varchar(36);
alter table OTB_EIP_BillChangeApply add RelationId  varchar(36);
alter table OTB_EIP_BusinessTravel add RelationId  varchar(36);
alter table OTB_EIP_InvoiceApplyInfo add RelationId  varchar(36);
alter table OTB_EIP_Leave add RelationId  varchar(36);
alter table OTB_EIP_OverTime add RelationId  varchar(36);
alter table OTB_EIP_TravelExpense add RelationId  varchar(36);

