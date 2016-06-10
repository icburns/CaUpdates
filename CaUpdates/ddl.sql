CREATE TABLE dbo.CaStateResults (
	Id INT NOT NULL IDENTITY PRIMARY KEY
	,BernieVotes INT NOT NULL
	,ClintonVotes INT NOT NULL
	,UpdatedAt DATETIME NOT NULL
)

GO

CREATE VIEW dbo.LogReport
AS
SELECT 
	 Id
	,BernieVotes
	,ClintonVotes
	,BernieVotes - (LAG(BernieVotes, 1, BernieVotes) OVER (ORDER BY UpdatedAt ASC)) AS BernieChange
	,ClintonVotes - (LAG(ClintonVotes, 1, ClintonVotes) OVER (ORDER BY UpdatedAt ASC)) AS ClintonChange
	,CAST(BernieVotes AS DECIMAL)/(BernieVotes + ClintonVotes)*100 AS BerniePercent
	,CAST(ClintonVotes AS DECIMAL)/(BernieVotes + ClintonVotes)*100 AS ClintonPercent
	,(CAST(BernieVotes AS DECIMAL)/(BernieVotes + ClintonVotes) - (LAG(CAST(BernieVotes AS DECIMAL)/(BernieVotes + ClintonVotes), 1, CAST(BernieVotes AS DECIMAL)/(BernieVotes + ClintonVotes)) OVER (ORDER BY UpdatedAt ASC)))*100 AS BerniePercentChange
	,(CAST(ClintonVotes AS DECIMAL)/(BernieVotes + ClintonVotes) - (LAG(CAST(ClintonVotes AS DECIMAL)/(BernieVotes + ClintonVotes), 1, CAST(ClintonVotes AS DECIMAL)/(BernieVotes + ClintonVotes)) OVER (ORDER BY UpdatedAt ASC)))*100 AS ClintonPercentChange
	,UpdatedAt
FROM dbo.CaStateResults
