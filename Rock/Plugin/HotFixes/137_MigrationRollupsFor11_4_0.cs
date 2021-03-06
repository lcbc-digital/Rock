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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 137, "1.11.0" )]
    public class MigrationRollupsFor11_4_0 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //----------------------------------------------------------------------------------
            // <auto-generated>
            //     This Up() migration method was generated by the Rock.CodeGeneration project.
            //     The purpose is to prevent hotfix migrations from running when they are not
            //     needed. The migrations in this file are run by an EF migration instead.
            // </auto-generated>
            //----------------------------------------------------------------------------------
        }

        private void OldUp()
        {
            UpdateForgotUserNameTemplateUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: Update Forgot User Names Communication Template
        /// </summary>
        private void UpdateForgotUserNameTemplateUp()
        {
            string oldValue = @"<table class=""tiny-button button reset-password"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100px; overflow: hidden; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; display: block; width: auto !important; color: #ffffff; background: #2795b6; padding: 5px 0 4px; border: 1px solid #2284a1;"" align=""center"" bgcolor=""#00acee"" valign=""top"">
                                  <a href=""{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset"" style=""color: #ffffff; text-decoration: none; font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 12px;"">Reset Password</a>
                                </td>
                              </tr>
		 </table>".Replace( "'", "''" );
            string newValue = @"{% assign isChangeable =  SupportsChangePassword | Contains: User.UserName %}
{% if isChangeable %}
                     <table class=""tiny-button button reset-password"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100px; overflow: hidden; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; display: block; width: auto !important; color: #ffffff; background: #2795b6; padding: 5px 0 4px; border: 1px solid #2284a1;"" align=""center"" bgcolor=""#00acee"" valign=""top"">
                                  <a href=""{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset"" style=""color: #fffffe; text-decoration: none; font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 12px;"">Reset Password</a>
                                </td>
                              </tr>
		 </table>{% endif %}".Replace( "'", "''" );
            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE 
                        {targetColumn} LIKE '%{oldValue}%'
                        AND [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'" );
        }
    }
}
