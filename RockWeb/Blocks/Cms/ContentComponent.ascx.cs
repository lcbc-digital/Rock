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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Content Component" )]
    [Category( "CMS" )]
    [Description( "Block to manage and display content." )]

    [ContentChannelField( "Content Channel", category: "CustomSetting" )]

    [IntegerField( "Item Cache Duration", "Number of seconds to cache the content item specified by the parameter.", false, 3600, "CustomSetting", 0, "ItemCacheDuration" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Content Component Template" )]
    [BooleanField( "Allow Multiple Content Items", category: "CustomSetting" )]
    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", required: false, key: "OutputCacheDuration", category: "CustomSetting" )]
    [CustomCheckboxListField( "Cache Tags", "Cached tags are used to link cached content so that it can be expired as a group", listSource: "", required: false, key: "CacheTags", category: "CustomSetting" )]

    [IntegerField( "Filter Id", "The data filter that is used to filter items", false, 0, "CustomSetting" )]
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
                // TODO
            }

            CreateDynamicControls();
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

        #endregion overrides

        #region Shared Methods

        /// <summary>
        /// Gets the content channel.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public ContentChannel GetContentChannel( RockContext rockContext )
        {
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            Guid? contentChannelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            ContentChannel contentChannel = null;
            if ( contentChannelGuid.HasValue )
            {
                contentChannel = contentChannelService.Get( contentChannelGuid.Value );
            }

            return contentChannel;
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        private void CreateDynamicControls()
        {
            if ( pnlContentComponentConfig.Visible )
            {
                // temporarily create so we can get the Attribute UI configured
                var contentChannel = new ContentChannel();
                contentChannel.ContentChannelTypeId = new ContentChannelTypeService( new RockContext() ).GetId( Rock.SystemGuid.ContentChannelType.CONTENT_COMPONENT.AsGuid() ) ?? 0;
                contentChannel.LoadAttributes();
                phContentChannelAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentChannel, phContentChannelAttributes, false, mdContentComponentConfig.ValidationGroup );
            }

            if ( pnlContentComponentEditContentChannelItems.Visible )
            {
                // temporarily create so we can get the Attribute UI configured
                ContentChannelItem contentChannelItem = new ContentChannelItem();

                var rockContext = new RockContext();
                var contentChannel = this.GetContentChannel( rockContext );
                if ( contentChannel != null )
                {
                    contentChannelItem.ContentChannelId = contentChannel.Id;
                }

                contentChannelItem.ContentChannelTypeId = new ContentChannelTypeService( rockContext ).GetId( Rock.SystemGuid.ContentChannelType.CONTENT_COMPONENT.AsGuid() ) ?? 0;
                contentChannelItem.LoadAttributes();
                phContentChannelItemsAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentChannelItem, phContentChannelItemsAttributes, false, mdContentComponentEditContentChannelItems.ValidationGroup );
            }
        }

        #endregion Shared Methods

        #region Content Component - Config

        /// <summary>
        /// Handles the Click event of the lbConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbConfigure_Click( object sender, EventArgs e )
        {
            pnlContentComponentConfig.Visible = true;
            mdContentComponentConfig.Show();

            // Component Name (Content Channel Name)
            var rockContext = new RockContext();
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            Guid? contentChannelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            ContentChannel contentChannel = null;
            if ( contentChannelGuid.HasValue )
            {
                contentChannel = contentChannelService.Get( contentChannelGuid.Value );
                tbComponentName.Text = contentChannel.Name;
                contentChannel.LoadAttributes();
                phContentChannelAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentChannel, phContentChannelAttributes, true, mdContentComponentConfig.ValidationGroup );
            }
            else
            {
                tbComponentName.Text = string.Empty;
            }

            nbItemCacheDuration.Text = this.GetAttributeValue( "ItemCacheDuration" );

            DefinedTypeCache contentComponentTemplateType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE.AsGuid() );
            if ( contentComponentTemplateType != null )
            {
                dvpContentComponentTemplate.DefinedTypeId = contentComponentTemplateType.Id;
            }

            DefinedValueCache contentComponentTemplate = null;
            var contentComponentTemplateValueGuid = this.GetAttributeValue( "ContentComponentTemplate" ).AsGuidOrNull();
            if ( contentComponentTemplateValueGuid.HasValue )
            {
                contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueGuid.Value );
            }

            dvpContentComponentTemplate.SetValue( contentComponentTemplate );

            cbAllowMultipleContentItems.Checked = this.GetAttributeValue( "AllowMultipleContentItems" ).AsBoolean();

            nbOutputCacheDuration.Text = this.GetAttributeValue( "OutputCacheDuration" );

            // Cache Tags
            cblCacheTags.DataSource = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).DefinedValues.Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            string[] selectedCacheTags = this.GetAttributeValue( "CacheTags" ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            cePreHtml.Text = this.BlockCache.PreHtml;
            cePostHtml.Text = this.BlockCache.PostHtml;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdContentComponentConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdContentComponentConfig_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            Guid? contentChannelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            ContentChannel contentChannel = null;

            if ( contentChannelGuid.HasValue )
            {
                contentChannel = contentChannelService.Get( contentChannelGuid.Value );
            }

            if ( contentChannel == null )
            {
                contentChannel = new ContentChannel();
                contentChannel.ContentChannelTypeId = new ContentChannelTypeService( rockContext ).GetId( Rock.SystemGuid.ContentChannelType.CONTENT_COMPONENT.AsGuid() ) ?? 0;
                contentChannelService.Add( contentChannel );
            }

            contentChannel.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phContentChannelAttributes, contentChannel );

            contentChannel.Name = tbComponentName.Text;
            rockContext.SaveChanges();
            contentChannel.SaveAttributeValues( rockContext );

            this.SetAttributeValue( "ContentChannel", contentChannel.Guid.ToString() );

            this.SetAttributeValue( "ItemCacheDuration", nbItemCacheDuration.Text );

            int? contentComponentTemplateValueId = dvpContentComponentTemplate.SelectedValue.AsInteger();
            Guid? contentComponentTemplateValueGuid = null;
            if ( contentComponentTemplateValueId.HasValue )
            {
                var contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueId.Value );
                if ( contentComponentTemplate != null )
                {
                    contentComponentTemplateValueGuid = contentComponentTemplate.Guid;
                }
            }

            this.SetAttributeValue( "ContentComponentTemplate", contentComponentTemplateValueGuid.ToString() );
            this.SetAttributeValue( "AllowMultipleContentItems", cbAllowMultipleContentItems.Checked.ToString() );
            this.SetAttributeValue( "OutputCacheDuration", nbOutputCacheDuration.Text );
            this.SetAttributeValue( "CacheTags", cblCacheTags.SelectedValues.AsDelimited( "," ) );

            this.SaveAttributeValues();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.PreHtml = cePreHtml.Text;
            block.PostHtml = cePostHtml.Text;
            rockContext.SaveChanges();

            mdContentComponentConfig.Hide();
            pnlContentComponentConfig.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        #endregion Content Component - Config

        #region Content Component - Edit Content

        /// <summary>
        /// Handles the Click event of the lbEditContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbEditContent_Click( object sender, EventArgs e )
        {
            pnlContentComponentEditContentChannelItems.Visible = true;
            mdContentComponentEditContentChannelItems.Show();


            /*contentChannelItem.LoadAttributes();
            phContentChannelItemsAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( contentChannelItem, phContentChannelItemsAttributes, false, mdContentComponentEditContentChannelItems.ValidationGroup );*/
        }

        /// <summary>
        /// Handles the SaveClick event of the mdContentComponentEditContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdContentComponentEditContentChannelItems_SaveClick( object sender, EventArgs e )
        {
            // TODO

            mdContentComponentEditContentChannelItems.Hide();
            pnlContentComponentEditContentChannelItems.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        protected void gContentChannelItems_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            // TODO
        }

        protected void gContentChannelItems_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            // TODO
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion


    }
}