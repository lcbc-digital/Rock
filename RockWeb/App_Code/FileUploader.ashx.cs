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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class FileUploader : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="WebFaultException">Must be logged in</exception>
        public virtual void ProcessRequest( HttpContext context )
        {
            if ( !context.User.Identity.IsAuthenticated )
            {
                // If not, see if there's a valid token
                string authToken = context.Request.Headers["Authorization-Token"];
                if ( string.IsNullOrWhiteSpace( authToken ) )
                {
                    authToken = context.Request.Params["apikey"];
                }

                if ( !string.IsNullOrWhiteSpace( authToken ) )
                {
                    var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                    var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                    if ( userLogin != null )
                    {
                        var identity = new GenericIdentity( userLogin.UserName );
                        var principal = new GenericPrincipal( identity, null );
                        context.User = principal;
                    }
                }
            }

            var currentUser = UserLoginService.GetCurrentUser();
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            try
            {
                HttpFileCollection hfc = context.Request.Files;
                HttpPostedFile uploadedFile = hfc.AllKeys.Select( fk => hfc[fk] ).FirstOrDefault();

                // No file or no data?  No good.
                if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
                {
                    throw new Rock.Web.FileUploadException( "No File Specified", System.Net.HttpStatusCode.BadRequest );
                }

                // Check to see if this is a BinaryFileType/BinaryFile or just a plain content file
                bool isBinaryFile = context.Request.QueryString["isBinaryFile"].AsBoolean();

                if ( isBinaryFile )
                {
                    ProcessBinaryFile( context, uploadedFile, currentPerson );
                }
                else
                {
                    if ( !context.User.Identity.IsAuthenticated )
                    {
                        throw new Rock.Web.FileUploadException( "Must be logged in", System.Net.HttpStatusCode.Forbidden );
                    }
                    else
                    {
                        if ( context.Request.Form["IsAssetStorageProviderAsset"].AsBoolean() )
                        {
                            ProcessAssetStorageProviderAsset( context, uploadedFile );
                        }
                        else
                        {
                            ProcessContentFile( context, uploadedFile );
                        }
                    }
                }
            }
            catch ( Rock.Web.FileUploadException fex )
            {
                ExceptionLogService.LogException( fex, context );
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = ( int ) fex.StatusCode;
                context.Response.Write( fex.Detail );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = ( int ) System.Net.HttpStatusCode.InternalServerError;
                context.Response.Write( "error: " + ex.Message );
            }
        }

        private void ProcessAssetStorageProviderAsset( HttpContext context, HttpPostedFile uploadedFile )
        {
            int? assetStorageId = context.Request.Form["StorageId"].AsIntegerOrNull();
            string assetKey = context.Request.Form["Key"] + uploadedFile.FileName;

            if ( assetStorageId == null || assetKey.IsNullOrWhiteSpace() )
            {
                throw new Rock.Web.FileUploadException( "Insufficient info to upload a file of this type.", System.Net.HttpStatusCode.Forbidden );
            }

            var assetStorageService = new AssetStorageProviderService( new RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageService.Get( (int)assetStorageId );
            assetStorageProvider.LoadAttributes();
            var component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Rock.Storage.AssetStorage.Asset();
            asset.Key = assetKey;
            asset.Type = Rock.Storage.AssetStorage.AssetType.File;
            asset.AssetStream = uploadedFile.InputStream;

            if ( component.UploadObject( assetStorageProvider, asset ) )
            {
                context.Response.Write( new { Id = string.Empty, FileName = assetKey }.ToJson() );
            }
            else
            {
                throw new Rock.Web.FileUploadException( "Unable to upload file", System.Net.HttpStatusCode.BadRequest );
            }
        }

        /// <summary>
        /// Processes the content file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessContentFile( HttpContext context, HttpPostedFile uploadedFile )
        {
            // validate file type (child FileUploader classes, like ImageUploader, can do additional validation);
            this.ValidateFileType( context, uploadedFile );

            //
            // Get filename and scrub invalid characters.
            //
            var filename = Path.GetFileName( uploadedFile.FileName );
            filename = Regex.Replace( filename, @"[<>*%&:\\]", string.Empty, RegexOptions.CultureInvariant );

            // get folderPath and construct filePath
            string relativeFolderPath = context.Request.Form["folderPath"] ?? string.Empty;
            string relativeFilePath = Path.Combine( relativeFolderPath, filename );
            string rootFolderParam = context.Request.QueryString["rootFolder"];

            string rootFolder = string.Empty;

            if ( !string.IsNullOrWhiteSpace( rootFolderParam ) )
            {
                // if a rootFolder was specified in the URL, decrypt it (it is encrypted to help prevent direct access to filesystem)
                rootFolder = Rock.Security.Encryption.DecryptString( rootFolderParam );
            }

            if ( string.IsNullOrWhiteSpace( rootFolder ) )
            {
                // set to default rootFolder if not specified in the params
                rootFolder = "~/Content";
            }

            string physicalRootFolder = context.Request.MapPath( rootFolder );
            string physicalContentFolderName = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( new char[] { '/', '\\' } ) );

            // Make sure the physicalContentFolderName doesn't have any special directory navigation indicators
            if ( physicalContentFolderName != System.IO.Path.GetFullPath( physicalContentFolderName ) )
            {
                throw new Rock.Web.FileUploadException( "Unable to upload file", System.Net.HttpStatusCode.BadRequest );
            }

            string physicalFilePath = Path.Combine( physicalContentFolderName, filename );
            var fileContent = GetFileContentStream( context, uploadedFile );

            // store the content file in the specified physical content folder
            if ( !Directory.Exists( physicalContentFolderName ) )
            {
                Directory.CreateDirectory( physicalContentFolderName );
            }

            if ( File.Exists( physicalFilePath ) )
            {
                File.Delete( physicalFilePath );
            }

            using ( var writeStream = File.OpenWrite( physicalFilePath ) )
            {
                if ( fileContent.CanSeek )
                {
                    fileContent.Seek( 0, SeekOrigin.Begin );
                }

                fileContent.CopyTo( writeStream );
            }

            var response = new
            {
                Id = string.Empty,
                FileName = relativeFilePath
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Dictionary of deprecated or incorrect mimetypes and what they should be mapped to instead
        /// </summary>
        private Dictionary<string, string> _mimeTypeRemap = new Dictionary<string, string>
        {
            { "text/directory", "text/vcard" },
            { "text/directory; profile=vCard", "text/vcard" },
            { "text/x-vcard", "text/vcard" }
        };

        /// <summary>
        /// Processes the binary file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessBinaryFile( HttpContext context, HttpPostedFile uploadedFile, Person currentPerson )
        {
            // get BinaryFileType info
            Guid fileTypeGuid = context.Request.QueryString["fileTypeGuid"].AsGuid();

            RockContext rockContext = new RockContext();
            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid );

            if ( binaryFileType == null )
            {
                throw new Rock.Web.FileUploadException( "Binary file type must be specified", System.Net.HttpStatusCode.Forbidden );
            }
            else
            {
                if ( !binaryFileType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    throw new Rock.Web.FileUploadException( "Not authorized to upload this type of file", System.Net.HttpStatusCode.Forbidden );
                }
            }

            char[] illegalCharacters = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

            if ( uploadedFile.FileName.IndexOfAny( illegalCharacters ) >= 0 )
            {
                throw new Rock.Web.FileUploadException( "Invalid Filename.  Please remove any special characters (" + string.Join( " ", illegalCharacters ) + ").", System.Net.HttpStatusCode.UnsupportedMediaType );
            }

            // always create a new BinaryFile record of IsTemporary when a file is uploaded
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = new BinaryFile();
            binaryFileService.Add( binaryFile );

            // assume file is temporary unless specified otherwise so that files that don't end up getting used will get cleaned up
            binaryFile.IsTemporary = context.Request.QueryString["IsTemporary"].AsBooleanOrNull() ?? true;
            binaryFile.BinaryFileTypeId = binaryFileType.Id;
            binaryFile.MimeType = uploadedFile.ContentType;
            binaryFile.FileSize = uploadedFile.ContentLength;
            binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );

            if ( _mimeTypeRemap.ContainsKey( binaryFile.MimeType ) )
            {
                binaryFile.MimeType = _mimeTypeRemap[binaryFile.MimeType];
            }

            binaryFile.ContentStream = GetFileContentStream( context, uploadedFile );
            rockContext.SaveChanges();

            var response = new
            {
                Id = binaryFile.Id,
                FileName = binaryFile.FileName.UrlEncode()
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Gets the file bytes
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public virtual Stream GetFileContentStream( HttpContext context, HttpPostedFile uploadedFile )
        {
            // NOTE: GetFileBytes can get overridden by a child class (ImageUploader.ashx.cs for example)
            return uploadedFile.InputStream;
        }

        /// <summary>
        /// Validates the type of the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <exception cref="WebFaultException{System.String}">Filetype not allowed</exception>
        public virtual void ValidateFileType( HttpContext context, HttpPostedFile uploadedFile )
        {
            // validate file type (applies to all uploaded files)
            var globalAttributesCache = GlobalAttributesCache.Get();
            IEnumerable<string> contentFileTypeBlackList = ( globalAttributesCache.GetValue( "ContentFiletypeBlacklist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );

            // clean up list
            contentFileTypeBlackList = contentFileTypeBlackList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            // Get file extension and then trim any trailing spaces (to catch any nefarious stuff).
            string fileExtension = Path.GetExtension( uploadedFile.FileName ).ToLower().TrimStart( new char[] { '.' } ).Trim();
            if ( contentFileTypeBlackList.Contains( fileExtension ) )
            {
                throw new Rock.Web.FileUploadException( "Filetype not allowed", System.Net.HttpStatusCode.NotAcceptable );
            }
        }
    }
}