//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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


namespace Rock.Client
{
    /// <summary>
    /// Base client model for Metric that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class MetricEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public int? AdminPersonAliasId { get; set; }

        /// <summary />
        public int? DataViewId { get; set; }

        /// <summary />
        public string Description { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public string IconCssClass { get; set; }

        /// <summary />
        public bool IsCumulative { get; set; }

        /// <summary />
        public bool IsSystem { get; set; }

        /// <summary />
        public DateTime? LastRunDateTime { get; set; }

        /// <summary />
        public int? MetricChampionPersonAliasId { get; set; }

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public int? ScheduleId { get; set; }

        /// <summary />
        public string SourceSql { get; set; }

        /// <summary />
        public int? SourceValueTypeId { get; set; }

        /// <summary />
        public string Subtitle { get; set; }

        /// <summary />
        public string Title { get; set; }

        /// <summary />
        public string XAxisLabel { get; set; }

        /// <summary />
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// This does not need to be set or changed. Rock will always set this to the current date/time when saved to the database.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// If you need to set this manually, set ModifiedAuditValuesAlreadyUpdated=True to prevent Rock from setting it
        /// </summary>
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public int? ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source Metric object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( Metric source )
        {
            this.Id = source.Id;
            this.AdminPersonAliasId = source.AdminPersonAliasId;
            this.DataViewId = source.DataViewId;
            this.Description = source.Description;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IconCssClass = source.IconCssClass;
            this.IsCumulative = source.IsCumulative;
            this.IsSystem = source.IsSystem;
            this.LastRunDateTime = source.LastRunDateTime;
            this.MetricChampionPersonAliasId = source.MetricChampionPersonAliasId;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.ScheduleId = source.ScheduleId;
            this.SourceSql = source.SourceSql;
            this.SourceValueTypeId = source.SourceValueTypeId;
            this.Subtitle = source.Subtitle;
            this.Title = source.Title;
            this.XAxisLabel = source.XAxisLabel;
            this.YAxisLabel = source.YAxisLabel;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for Metric that includes all the fields that are available for GETs. Use this for GETs (use MetricEntity for POST/PUTs)
    /// </summary>
    public partial class Metric : MetricEntity
    {
        /// <summary />
        public ICollection<MetricCategory> MetricCategories { get; set; }

        /// <summary />
        public DefinedValue SourceValueType { get; set; }

        /// <summary>
        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }

        /// <summary>
        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }
    }
}
