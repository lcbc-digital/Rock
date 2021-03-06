DECLARE @StartDate DATE = '2020-1-1'
	,@EndDate DATE = '2020-12-31'
	,@PledgeCount int = 999

INSERT INTO [dbo].[FinancialPledge] (
	[AccountId]
	,[TotalAmount]
	,[PledgeFrequencyValueId]
	,[StartDate]
	,[EndDate]
	,[Guid]
	,[PersonAliasId]
	,[GroupId]
	)
SELECT TOP (@PledgeCount) fa.Id [AccountId]
	,1234.56 [TotalAmount]
	,pfdv.Id [PledgeFrequencyValueId]
	,@StartDate [StartDate]
	,@EndDate [EndDate]
	,newid() [Guid]
	,pa.Id [PersonAliasId]
	,NULL [GroupId]
FROM FinancialAccount fa
	,DefinedValue pfdv
	,PersonAlias pa
WHERE pfdv.DefinedTypeId = (
		SELECT TOP 1 Id
		FROM DefinedType
		WHERE [Guid] = '1F645CFB-5BBD-4465-B9CA-0D2104A1479B'
		) /* Recurring Transaction Frequency */
	AND pa.PersonId = pa.AliasPersonId
	AND pa.PersonId % 10 = 0