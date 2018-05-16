// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupStatusValue : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "StatusValueId", c => c.Int());
            AddColumn("dbo.GroupType", "GroupStatusDefinedTypeId", c => c.Int());
            CreateIndex("dbo.Group", "StatusValueId");
            CreateIndex("dbo.GroupType", "GroupStatusDefinedTypeId");
            AddForeignKey("dbo.GroupType", "GroupStatusDefinedTypeId", "dbo.DefinedType", "Id");
            AddForeignKey("dbo.Group", "StatusValueId", "dbo.DefinedValue", "Id");

            RockMigrationHelper.AddDefinedType( "Group", "Family Status", "Determines the level of connection (Participant , Unknown, etc.) the family has to the church.", "792C6979-0F40-47C5-BD0C-06FA7DF22837", @"" );
            RockMigrationHelper.UpdateDefinedValue( "792C6979-0F40-47C5-BD0C-06FA7DF22837", "eRA", "", "4B5776E9-0A2A-49F0-A04F-337DBC2A421F", false );
            RockMigrationHelper.UpdateDefinedValue( "792C6979-0F40-47C5-BD0C-06FA7DF22837", "Participant", "", "079E625F-AA51-41B8-885A-CA5A007185CF", false );
            RockMigrationHelper.UpdateDefinedValue( "792C6979-0F40-47C5-BD0C-06FA7DF22837", "Unknown", "", "99844B92-3D63-4246-BB22-B0DB7BDA8D01", false );

            Sql( @"
DECLARE @GROUPTYPE_FAMILY UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
    , @GroupStatusFamily INT = (
        SELECT TOP 1 Id

        FROM DefinedType

        WHERE[Guid] = '792C6979-0F40-47C5-BD0C-06FA7DF22837'
        )

UPDATE[GroupType]
SET GroupStatusDefinedTypeId = @GroupStatusFamily
WHERE[Guid] = @GROUPTYPE_FAMILY

    AND GroupStatusDefinedTypeId != @GroupStatusFamily" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue( "079E625F-AA51-41B8-885A-CA5A007185CF" ); // Participant
            RockMigrationHelper.DeleteDefinedValue( "4B5776E9-0A2A-49F0-A04F-337DBC2A421F" ); // eRA
            RockMigrationHelper.DeleteDefinedValue( "99844B92-3D63-4246-BB22-B0DB7BDA8D01" ); // Unknown
            RockMigrationHelper.DeleteDefinedType( "792C6979-0F40-47C5-BD0C-06FA7DF22837" ); // Family Status

            DropForeignKey("dbo.Group", "StatusValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.GroupType", "GroupStatusDefinedTypeId", "dbo.DefinedType");
            DropIndex("dbo.GroupType", new[] { "GroupStatusDefinedTypeId" });
            DropIndex("dbo.Group", new[] { "StatusValueId" });
            DropColumn("dbo.GroupType", "GroupStatusDefinedTypeId");
            DropColumn("dbo.Group", "StatusValueId");
        }
    }
}
