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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Web.UI.HtmlControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Content Component" )]
    [Category( "CMS" )]
    [Description( "Block to manage and display content." )]
    public partial class ContentComponent : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="T:Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="T:System.Boolean" /> flag that indicates if the user can configure the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="T:System.Boolean" /> flag that indicates if the user can edit the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> containing all the icon <see cref="T:System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canEdit )
            {
                LinkButton lbConfigure = new LinkButton();
                lbConfigure.ID = "lbConfigure";
                lbConfigure.CssClass = "edit";
                lbConfigure.ToolTip = "Configure";
                lbConfigure.Click += lbConfigure_Click;
                configControls.Add( lbConfigure );
                HtmlGenericControl iConfigure = new HtmlGenericControl( "i" );
                iConfigure.Attributes.Add( "class", "fa fa fa-cog" );

                lbConfigure.Controls.Add( iConfigure );
                lbConfigure.CausesValidation = false;

                // will toggle the block config so they are no longer showing
                lbConfigure.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbConfigure );

                LinkButton lbEditContent = new LinkButton();
                lbEditContent.ID = "lbEditContent";
                lbEditContent.CssClass = "edit";
                lbEditContent.ToolTip = "Edit Content";
                lbEditContent.Click += lbEditContent_Click;
                configControls.Add( lbEditContent );

                HtmlGenericControl iEditContent = new HtmlGenericControl( "i" );
                iEditContent.Attributes.Add( "class", "fa fa-pencil-square-o" );

                lbEditContent.Controls.Add( iEditContent );
                lbEditContent.CausesValidation = false;


                // will toggle the block config so they are no longer showing
                lbEditContent.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEditContent );
            }

            var baseAdministrateControls = base.GetAdministrateControls( canConfig, canEdit );

            // remove the "aBlockProperties" control since we'll be taking care of all that with our "lbConfigure" button
            var aBlockProperties = baseAdministrateControls.FirstOrDefault( a => a.ID == "aBlockProperties" );
            if ( aBlockProperties != null )
            {
                baseAdministrateControls.Remove( aBlockProperties );
            }

            configControls.AddRange( baseAdministrateControls );

            return configControls;
        }

        /// <summary>
        /// Handles the Click event of the lbEditContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbEditContent_Click( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the Click event of the lbConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbConfigure_Click( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}