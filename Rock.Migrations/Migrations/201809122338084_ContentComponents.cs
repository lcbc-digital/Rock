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
    public partial class ContentComponents : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201809122338084_ContentComponents_CreateContentComponentChannelType );

            RockMigrationHelper.AddDefinedType( "Content Channel", "Content Component Template", "Lava Template that can be used with a Content Component block.", Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, Rock.SystemGuid.FieldType.LAVA, "Display Lava", "DisplayLava", "The Lava Template to use when rendering the Content Component", 0, "", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorHeight", "400", "573D06B9-5BDE-4E64-B0B1-85E167FCB47A" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorTheme", "0", "94E49DA2-F796-4F83-A857-A9751F273CF6" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorMode", "3", "17B8ECCA-2F67-4130-A43D-4D76C71D11D0" );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Hero", "", "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809122338084_ContentComponents_Hero );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Ad Unit", "", "902D960C-0B7B-425E-9CEA-94CF215AABE4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "902D960C-0B7B-425E-9CEA-94CF215AABE4", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809122338084_ContentComponents_AdUnit );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Side By Side", "", "EC429625-767E-4F69-BB48-F55DA3C836A3" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EC429625-767E-4F69-BB48-F55DA3C836A3", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809122338084_ContentComponents_SideBySide );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Card", "", "54A6FE8C-B38F-46DB-81F7-A7648886B592" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "54A6FE8C-B38F-46DB-81F7-A7648886B592", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809122338084_ContentComponents_Card );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
