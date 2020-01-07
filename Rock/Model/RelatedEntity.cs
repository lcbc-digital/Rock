﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The Related Entity to allow linking entities (of the same or different types) to each other.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "RelatedEntity" )]
    [DataContract]
    public partial class RelatedEntity : Model<RelatedEntity>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of source entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the  source entity.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 1 )]
        public int SourceEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> of the source.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 2 )]
        public int SourceEntityId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of target entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the target entity.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 3 )]
        public int TargetEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityId of the <see cref="Rock.Model.EntityType"/> of the target.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 4 )]
        public int TargetEntityId { get; set; }

        /// <summary>
        /// Gets or sets the purpose key.
        /// </summary>
        /// <value>
        /// The purpose key.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 5 )]
        public string PurposeKey { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Site is part of the Rock core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value.
        /// </summary>
        /// <value>
        /// The qualifier value.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string QualifierValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the source entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType SourceEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the target entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType TargetEntityType { get; set; }

        #endregion
    }

    /// <summary>
    /// see <see cref="RelatedEntity.PurposeKey" />
    /// </summary>
    public static class RelatedEntityPurposeKey
    {
        /// <summary>
        /// The group placement
        /// </summary>
        public const string GroupPlacement = "PLACEMENT";

        /// <summary>
        /// The group placement template
        /// </summary>
        public const string GroupPlacementTemplate = "PLACEMENT-TEMPLATE";
    }

    #region Entity Configuration

    /// <summary>
    /// RelatedEntity Configuration class.
    /// </summary>
    public partial class RelatedEntityConfiguration : EntityTypeConfiguration<RelatedEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetItemConfiguration"/> class.
        /// </summary>
        public RelatedEntityConfiguration()
        {
            this.HasRequired( a => a.SourceEntityType ).WithMany().HasForeignKey( a => a.SourceEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.TargetEntityType ).WithMany().HasForeignKey( a => a.TargetEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}