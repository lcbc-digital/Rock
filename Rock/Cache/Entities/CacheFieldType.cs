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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheFieldType : ModelCache<CacheFieldType, FieldType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        [DataMember]
        public string Assembly { get; private set; }

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        [DataMember]
        public string Class { get; private set; }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        [DataMember]
        public Field.IFieldType Field { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var fieldType = entity as FieldType;
            if ( fieldType == null ) return;

            IsSystem = fieldType.IsSystem;
            Name = fieldType.Name;
            Description = fieldType.Description;
            Assembly = fieldType.Assembly;
            Class = fieldType.Class;

            Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <returns></returns>
        public new static List<CacheFieldType> All()
        {
            // use 'new' to override the base All since we want to sort field types
            return ModelCache<CacheFieldType, FieldType>.All().OrderBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public new static List<CacheFieldType> All( RockContext rockContext )
        {
            // use 'new' to override the base All since we want to sort field types
            return ModelCache<CacheFieldType, FieldType>.All( rockContext ).OrderBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CacheFieldType Get<T>() where T : Field.IFieldType
        {
            return All().FirstOrDefault( a => a.Class == typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int id )
        {
            var fieldType = Get( id );
            return fieldType?.Name;
        }

        #endregion
    }
}