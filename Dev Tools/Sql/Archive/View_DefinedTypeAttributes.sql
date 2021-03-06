SELECT [Id]
      ,[Order]
      ,[Category]
      ,[Name]
      ,[Description]
      ,[Guid] [DefinedType.Guid]
      ,cast( 
        (select 
            [t].[Guid] [DefinedType.Guid],
            [ft].[Guid] [FieldType.Guid],
            [a].[Name] [Attribute.Name],
            [a].[Key] [Attribute.Key],
            [a].[Description] [Attribute.Description],
            [a].[Order] [Attribute.Order],
            [a].[DefaultValue] [Attribute.DefaultValue],
            [a].[Guid] [Attribute.Guid]
        FROM [Attribute] [a]
            left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
            join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
        where 
            e.Name = 'Rock.Model.DefinedValue' 
        and 
            a.EntityTypeQualifierColumn = 'DefinedTypeId'
        and
            a.EntityTypeQualifierValue = t.Id
        FOR XML PATH ('Attribute'), root ('root' ) ) as XML) [AttributeValues]
  FROM [DefinedType] [t]


