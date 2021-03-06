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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column to display a boolean value.
    /// </summary>
    [ToolboxData( "<{0}:RockBoundField runat=server></{0}:RockBoundField>" )]
    public class RockBoundField : BoundField, IPriorityColumn, IRockGridField
    {
        /// <summary>
        /// Gets or sets the length of the truncate.
        /// </summary>
        /// <value>
        /// The length of the truncate.
        /// </value>
        public int TruncateLength
        {
            get { return ViewState["TruncateLength"] as int? ?? 0; }
            set { ViewState["TruncateLength"] = value; }
        }

        /// <summary>
        /// Gets or sets the column priority.
        /// </summary>
        /// <value>
        /// The priority of the column.
        /// </value>
        public ColumnPriority ColumnPriority
        {
            get
            {
                var columnPriority = ViewState["ColumnPriority"] as ColumnPriority?;
                return columnPriority ?? ColumnPriority.AlwaysVisible;
            }

            set
            {
                ViewState["ColumnPriority"] = value;
            }
        }


        /// <summary>
        /// When exporting a grid to Excel, this property controls whether a column is included
        /// in the export. See <seealso cref="ExcelExportBehavior" />.
        /// </summary>
        public virtual ExcelExportBehavior ExcelExportBehavior
        {
            get
            {
                var excelExportBehavior = ViewState["ExcelExportBehavior"] as ExcelExportBehavior?;

                // default to IncludeIfVisible
                return excelExportBehavior ?? ExcelExportBehavior.IncludeIfVisible;
            }

            set
            {
                ViewState["ExcelExportBehavior"] = value;
            }
        }

        /// <summary>
        /// Formats the data value.
        /// </summary>
        /// <param name="dataValue">The data value.</param>
        /// <returns></returns>
        public string FormatDataValue( object dataValue )
        {
            return this.FormatDataValue( dataValue, false );
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            if ( dataValue is string && TruncateLength > 0 )
            {
                return base.FormatDataValue( ( ( string ) dataValue ).Truncate( TruncateLength ), encode );
            }

            /*
             * [2020-07-04] DL - If dataValue.ToString() returns null, avoid calling
             * System.Web.UI.WebControls.BoundField.FormatDataValue because it will throw an Exception.
             */
            if ( dataValue != null
                 && string.IsNullOrEmpty( dataValue.ToString() ) )
            {
                return string.Empty;
            }
            else
            {
                return base.FormatDataValue( dataValue, encode );
            }
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public virtual object GetExportValue( GridViewRow row )
        {
            if ( row.DataItem is System.Data.DataRowView )
            {
                var dataRow = ( ( System.Data.DataRowView ) row.DataItem ).Row;
                return dataRow[this.DataField];
            }

            return row.DataItem.GetPropertyValue( this.DataField );
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {

            return base.Initialize( sortingEnabled, control );
        }

    }
}