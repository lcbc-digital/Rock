﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemadev.LogFix
{
    /// <summary>
    /// Block for displaying the history of changes to a particular entity.
    /// </summary>
    [DisplayName( "Temp History Log" )]
    [Category( "com_bemadev > Log Fix" )]
    [Description( "Block for displaying the history of changes to a particular entity." )]

    [ContextAware]
    [TextField( "Heading", "The Lava template to use for the heading. <span class='tip tip-lava'></span>", false, "{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "", 0 )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, order: 1 )]

    public partial class TempHistoryLog : RockBlock, ISecondaryBlock
    {

        #region Fields

        private IEntity _entity = null;

        #endregion

        #region Base Control Methods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gHistory.GridRebind += gHistory_GridRebind;
            gHistory.DataKeyNames = new string[] { "FirstHistoryId" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _entity = this.ContextEntity();
            if ( _entity != null )
            {
                if ( !Page.IsPostBack )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Entity", _entity );
                    lHeading.Text = GetAttributeValue( "Heading" ).ResolveMergeFields( mergeFields );

                    BindFilter();
                    BindGrid();

                    IModel model = _entity as IModel;
                    if ( model != null && model.CreatedDateTime.HasValue )
                    {
                        hlDateAdded.Text = String.Format( "Date Created: {0}", model.CreatedDateTime.Value.ToShortDateString() );
                    }
                    else
                    {
                        hlDateAdded.Visible = false;
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Entity", _entity );
            lHeading.Text = GetAttributeValue( "Heading" ).ResolveMergeFields( mergeFields );

            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            gfSettings.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : "" );

            gfSettings.SaveUserPreference( "Summary Contains", tbSummary.Text );

            int? personId = ppWhoFilter.PersonId;
            gfSettings.SaveUserPreference( "Who", personId.HasValue ? personId.ToString() : string.Empty );

            gfSettings.SaveUserPreference( "Date Range", drpDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        int? categoryId = e.Value.AsIntegerOrNull();
                        if ( categoryId.HasValue )
                        {
                            var category = CategoryCache.Get( categoryId.Value );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Summary Contains":
                    {
                        break;
                    }
                case "Who":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse( e.Value, out personId ) )
                        {
                            var person = new PersonService( new RockContext() ).GetNoTracking( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? categoryId = gfSettings.GetUserPreference( "Category" ).AsIntegerOrNull();
            cpCategory.SetValue( categoryId );

            tbSummary.Text = gfSettings.GetUserPreference( "Summary Contains" );
            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
            {
                var person = new PersonService( new RockContext() ).Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SaveUserPreference( "Who", string.Empty );
                }
            }

            drpDates.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );
        }

       

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _entity != null )
            {
                var entityTypeCache = EntityTypeCache.Get( _entity.GetType(), false );
                if ( entityTypeCache != null )
                {
                    var rockContext = new RockContext();
                    int? databaseTimeoutSeconds = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull();
                    if ( databaseTimeoutSeconds != null && databaseTimeoutSeconds.Value > 0 )
                    {
                        rockContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
                    }
                    var historyService = new HistoryService( rockContext );
                    IQueryable<History> qry;

                    if ( entityTypeCache.Id == EntityTypeCache.GetId<Rock.Model.Person>() )
                    {
                        // If this is History for a Person, also include any History for any of their Families
                        int? groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>();
                        List<int> familyIds = ( _entity as Person ).GetFamilies().Select( a => a.Id ).ToList();

                        qry = historyService.Queryable().Include( a => a.CreatedByPersonAlias.Person )
                        .Where( h =>
                            ( h.EntityTypeId == entityTypeCache.Id && h.EntityId == _entity.Id )
                            || ( h.EntityTypeId == groupEntityTypeId && familyIds.Contains( h.EntityId ) ) );

                        // as per issue #1594, if relatedEntityType is an Attribute then check View Authorization
                        var attributeEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE.AsGuid() );
                        var personAttributes = new AttributeService( rockContext ).GetByEntityTypeId( entityTypeCache.Id ).ToList().Select( a => AttributeCache.Get( a ) );
                        var allowedAttributeIds = personAttributes.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).Select( a => a.Id ).ToList();
                        qry = qry.Where( a => ( a.RelatedEntityTypeId == attributeEntity.Id ) ? allowedAttributeIds.Contains( a.RelatedEntityId.Value ) : true );                            
                    }
                    else
                    {
                        qry = historyService.Queryable().Include( a => a.CreatedByPersonAlias.Person )
                        .Where( h =>
                            ( h.EntityTypeId == entityTypeCache.Id && h.EntityId == _entity.Id ) );
                    }

                    var historyCategories = new CategoryService( rockContext ).GetByEntityTypeId( EntityTypeCache.GetId<Rock.Model.History>() ).ToList().Select( a => CategoryCache.Get( a ) );
                    var allowedCategoryIds = historyCategories.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).Select( a => a.Id ).ToList();

                    qry = qry.Where( a => allowedCategoryIds.Contains( a.CategoryId ) );

                    int? categoryId = gfSettings.GetUserPreference( "Category" ).AsIntegerOrNull();
                    if ( categoryId.HasValue )
                    {
                        qry = qry.Where( a => a.CategoryId == categoryId.Value );
                    }

                    int? personId = gfSettings.GetUserPreference( "Who" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        qry = qry.Where( h => h.CreatedByPersonAlias.PersonId == personId.Value );
                    }

                    var drp = new DateRangePicker();
                    drp.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );
                    if ( drp.LowerValue.HasValue )
                    {
                        qry = qry.Where( h => h.CreatedDateTime >= drp.LowerValue.Value );
                    }
                    if ( drp.UpperValue.HasValue )
                    {
                        DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                        qry = qry.Where( h => h.CreatedDateTime < upperDate );
                    }

                    // Combine history records that were saved at the same time
                    var historySummaryList = historyService.GetHistorySummary( qry );

                    string summary = gfSettings.GetUserPreference( "Summary Contains" );
                    if ( !string.IsNullOrWhiteSpace( summary ) )
                    {
                        historySummaryList = historySummaryList.Where( h => h.HistoryList.Any( x => x.SummaryHtml.IndexOf( summary, StringComparison.OrdinalIgnoreCase ) >= 0 ) ).ToList();
                    }

                    SortProperty sortProperty = gHistory.SortProperty;
                    if ( sortProperty != null )
                    {
                        historySummaryList = historySummaryList.AsQueryable().Sort( sortProperty ).ToList();
                    }
                    else
                    {
                        historySummaryList = historySummaryList.OrderByDescending( t => t.CreatedDateTime ).ToList();
                    }

                    gHistory.DataSource = historySummaryList;
                    gHistory.EntityTypeId = EntityTypeCache.Get<History>().Id;
                    gHistory.DataBind();
                }
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}