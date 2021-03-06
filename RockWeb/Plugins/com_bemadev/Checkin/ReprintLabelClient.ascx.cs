﻿// <copyright>
// Copyright by LCBC Church
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Utility;

namespace RockWeb.Plugins.com_bemaservices.CheckIn
{
    [DisplayName( "Reprint Label Client" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Used if the device prints from the client" )]
    public partial class ReprintLabelClient : CheckInBlockMultiPerson
    {
        #region Fields

        // used for private variables
        private const string USER_SETTING_LABELGUID = "PrintTest:Label";
        private const string USER_SETTING_DEVICEID = "PrintTest:Device";
        private const string USER_SETTING_PERSONID = "PrintTest:Person";

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !IsPostBack )
            {
                var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                RockContext rockContext = new RockContext();

                var groupMemberService = new GroupMemberService( rockContext );
                DeviceService deviceService = new DeviceService( rockContext );

                CheckInPerson checkinPerson = new CheckInPerson();
                if ( personId.HasValue )
                {
                    PersonService personService = new PersonService( rockContext );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        checkinPerson.Person = person;
                        var attendance = new AttendanceService( rockContext ).Queryable().Where( a => a.PersonAlias.PersonId == person.Id ).OrderByDescending( a => a.CreatedDateTime ).FirstOrDefault();
                        if ( attendance != null && attendance.AttendanceCode != null )
                        {
                            checkinPerson.SecurityCode = attendance.AttendanceCode.Code;
                            try
                            {
                                var PrinterIPs = new Dictionary<int, string>();

                                string detailMsg = GetAttributeValue( "DetailMessage" );

                                var personLabelsAdded = new List<Guid>();
                                var printFromClient = new List<CheckInLabel>();
                                var printFromServer = new List<CheckInLabel>();
                                var checkinLabels = new List<CheckInLabel>();
                                var methodHandler = new Rock.Workflow.Action.CheckIn.CreateLabels();

                                var mergeObjects = new Dictionary<string, object>();
                                foreach ( var keyValue in commonMergeFields )
                                {
                                    mergeObjects.Add( keyValue.Key, keyValue.Value );
                                }

                                var checkinSchedule = new CheckInSchedule();
                                checkinSchedule.Schedule = attendance.Occurrence.Schedule.Clone( false );
                                checkinSchedule.Selected = true;

                                var checkinLocation = new CheckInLocation();
                                checkinLocation.Location = attendance.Occurrence.Location.Clone( false );
                                checkinLocation.Location.CopyAttributesFrom( attendance.Occurrence.Location );
                                checkinLocation.Schedules.Add( checkinSchedule );
                                checkinLocation.Selected = true;

                                var checkinGroup = new CheckInGroup();
                                checkinGroup.Group = attendance.Occurrence.Group.Clone( false );
                                checkinGroup.Group.CopyAttributesFrom( attendance.Occurrence.Group );
                                checkinGroup.Locations.Add( checkinLocation );
                                checkinGroup.Selected = true;

                                var checkinGroupType = new CheckInGroupType();
                                checkinGroupType.GroupType = GroupTypeCache.Get( attendance.Occurrence.Group.GroupType );
                                checkinGroupType.Groups.Add( checkinGroup );
                                checkinGroupType.Selected = true;

                                var checkinPeople = new List<CheckInPerson>();
                                checkinPeople.Add( checkinPerson );

                                mergeObjects.Add( "Location", checkinLocation );
                                mergeObjects.Add( "Group", checkinGroup );
                                mergeObjects.Add( "Person", checkinPerson );
                                mergeObjects.Add( "People", checkinPeople );
                                mergeObjects.Add( "GroupType", checkinGroupType );

                                var groupMembers = groupMemberService.Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.PersonId == person.Id &&
                                        m.GroupId == attendance.Occurrence.Group.Id )
                                    .ToList();
                                mergeObjects.Add( "GroupMembers", groupMembers );

                                var personLabels = methodHandler.GetLabels( checkinPerson.Person, new List<KioskLabel>() );
                                var groupTypeLabels = methodHandler.GetLabels( attendance.Occurrence.Group.GroupType, personLabels );
                                var groupLabels = methodHandler.GetLabels( attendance.Occurrence.Group, groupTypeLabels );
                                var locationLabels = methodHandler.GetLabels( attendance.Occurrence.Location, groupLabels );

                                Device kioskDevice = null;
                                if ( LocalDeviceConfig.CurrentKioskId.HasValue )
                                {
                                    kioskDevice = new DeviceService( rockContext ).Get( LocalDeviceConfig.CurrentKioskId.Value );
                                }
                                else
                                {
                                    kioskDevice = attendance.Device;
                                }

                                if ( kioskDevice != null )
                                {
                                    var childLabelGuid = "56FBDF5C-8E4C-43F5-9B22-B30D7A0E1067".AsGuid();
                                    if ( childLabelGuid != null )
                                    {
                                        foreach ( var labelCache in locationLabels.OrderBy( l => l.LabelType ).ThenBy( l => l.Order ) )
                                        {
                                            if(labelCache.Guid == childLabelGuid )
                                            {
                                                checkinPerson.SetOptions( labelCache );

                                                if ( labelCache.LabelType == KioskLabelType.Person )
                                                {
                                                    if ( personLabelsAdded.Contains( labelCache.Guid ) )
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        personLabelsAdded.Add( labelCache.Guid );
                                                    }
                                                }


                                                var label = new CheckInLabel( labelCache, mergeObjects, person.Id );
                                                label.FileGuid = labelCache.Guid;

                                                label.PrintFrom = kioskDevice.PrintFrom;
                                                label.PrintTo = kioskDevice.PrintToOverride;

                                                if ( label.PrintTo == PrintTo.Default )
                                                {
                                                    label.PrintTo = attendance.Occurrence.Group.GroupType.AttendancePrintTo;
                                                }

                                                if ( label.PrintTo == PrintTo.Kiosk )
                                                {
                                                    var device = kioskDevice;
                                                    if ( device != null )
                                                    {
                                                        label.PrinterDeviceId = device.PrinterDeviceId;
                                                    }
                                                }
                                                else if ( label.PrintTo == PrintTo.Location )
                                                {
                                                    var deviceId = attendance.Occurrence.Location.PrinterDeviceId;
                                                    if ( deviceId != null )
                                                    {
                                                        label.PrinterDeviceId = deviceId;
                                                    }
                                                }

                                                if ( label.PrinterDeviceId.HasValue )
                                                {
                                                    if ( PrinterIPs.ContainsKey( label.PrinterDeviceId.Value ) )
                                                    {
                                                        label.PrinterAddress = PrinterIPs[label.PrinterDeviceId.Value];
                                                    }
                                                    else
                                                    {
                                                        var printerDevice = new DeviceService( rockContext ).Get( label.PrinterDeviceId.Value );
                                                        if ( printerDevice != null )
                                                        {
                                                            PrinterIPs.Add( printerDevice.Id, printerDevice.IPAddress );
                                                            label.PrinterAddress = printerDevice.IPAddress;
                                                        }
                                                    }
                                                }

                                                checkinLabels.Add( label );
                                            }
                                            
                                        }
                                    }
                                }

                                // Print the labels                   
                                if ( checkinLabels != null && checkinLabels.Any() )
                                {
                                    printFromClient.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                                    printFromServer.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                                }

                                if ( printFromClient.Any() )
                                {
                                    var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                    printFromClient
                                        .OrderBy( l => l.PersonId )
                                        .ThenBy( l => l.Order )
                                        .ToList()
                                        .ForEach( l => l.LabelFile = urlRoot + l.LabelFile );
                                    AddLabelScript( printFromClient.ToJson() );
                                }

                                if ( printFromServer.Any() )
                                {
                                    var messages = ZebraPrint.PrintLabels( printFromServer );
                                }

                                nbMessage.Text = String.Format( "Printed {0} Labels to Client and {0} Labels to Server.", printFromClient.Count, printFromServer.Count );
                                nbMessage.Visible = true;
                            }
                            catch ( Exception ex )
                            {
                                LogException( ex );
                            }
                        }
                    }
                }

            }
        }

        #endregion
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }
    }
}