var currentMsgIdSelected = null;
var currentUserNameSelected = null;
var $calendar = null;

function initializeSockets() {
    var dashboardHub = $.connection.dashboardHub;

    $.connection.hub.start().done(function () {
        console.log('Connection establised');
    });

    dashboardHub.client.updateUserMessages = function () {
        alert('message updated');
        loadMessagesView();
    }
}

//points the management action pointer to the selected action
function pointToSelectedAction(position) {
    $('.arrow-top').animate({
        left: position.left + 40
    });
}

//decides which view to load based on the action id
function loadView(actionId) {
    switch (actionId) {
        case 'messages':
            currentMsgIdSelected = null;//clear current message selected 
            currentUserNameSelected = null;
            loadMessagesView();
            break;
        case 'requisitions': loadRequisitionsView();
            break;
        case 'meetings': loadMeetingsView();
            break;
        case 'complaints': loadComplaintsView();
            break;
        case 'bills': loadBillsView();
            break;
        case 'tennants': loadTennantsView();
            break;
    }

    $('.management-action-container').removeClass('hide');
    $('.arrow-top').removeClass('hide');
}

//loads the message view
function loadMessagesView() {
    $.ajax({
        url: '/landlordmanagement/GetMessagesView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-action-content-holder').html(data);

            loadMsgList();

            if (currentMsgIdSelected != null) {
                getMsgThread();
            }
        },
        error: function () {
            alert('An error occurred while loading messages');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }

    });
}

//loads the requisitions view
function loadRequisitionsView() {
    $.ajax({
        url: '/landlordmanagement/GetRequisitionsView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-action-content-holder').html(data);
        },
        error: function () {
            alert('An error occurred while loading requisitions');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }

    });
}

function loadMeetingCalender() {
    $calendar = $('#calender').fullCalendar({
        header: {
            left: 'title,prev,next today',
            center: '',
            right: 'month,agendaWeek,agendaDay'
        },
        selectable: true,
        theme: true,
        aspectRatio: 2,
        themeSystem: 'standard',
        // This is the callback that will be triggered when a selection is made.
        // It gets start and end date/time as part of its arguments
        select: function (start, end, jsEvent, view) {

            orStartDate = new Date(start).toISOString();
            //todo correct time zone
            var startDate = moment(Date.parse(orStartDate)).format("YYYY/MM/DD");

            loadMeetingRequestView(startDate, null);
        }, // End select callback
        // Make events editable, globally
        editable: true,

        // Callback triggered when we click on an event
        eventClick: function (event, jsEvent, view) {
            loadMeetingRequestView(null, event);
        } // End callback eventClick
    });
}

function populateMeetingCalender() {
    $.ajax({
        url: '/landlordmanagement/getMeetingsForUser',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            if (data != null) {
                var date = data.MeetingDate;
                var time = data.MeetingTime;

                var dateTime = moment(date + ' ' + time, 'DD/MM/YYYY HH:mm');

                $.each(data, function (index, value) {
                    var event = {
                        id: value.ID,
                        title: value.MeetingTitle,
                        start: dateTime
                    };

                    $calendar.fullCalendar("updateEvent", event);//adds events to the calender
                });
            }
        },
        error: function () {
            alert('An error occurred while loading calender');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function loadMeetingsView() {
    $.ajax({
        url: '/landlordmanagement/GetMeetingsView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-action-content-holder')
                .html(data).promise().done(function () {
                    loadMeetingCalender();
                    populateMeetingCalender();
                });
        },
        error: function () {
            alert('An error occurred while loading calender');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function getInvitees() {
    $.ajax({
        url: '/landlordmanagement/getInvitees',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $.each(data, function (index, value) {
                $('#meetingMembers').append($('<option></option>')
                    .attr('id', value.UserID)
                    .attr('value', value.FullName)
                    .text(value.FullName));
            });
        },
        error: function () {
            alert('An error occurred while loading invitees');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function getMeeting(Id) {
    $.ajax({
        url: '/landlordmanagement/getMeeting',
        type: 'GET',
        data: { Id: Id },
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('#id').val(data.ID);
            $('#meetingTitle').val(data.MeetingTitle);
            $('#meetingDate').val(data.MeetingDate);
            $('#meetingTime').val(data.MeetingTime);
            $('#location').val(data.Location);
            $('#purpose').val(data.Purpose);
            $('#isEdit').val(true);

            //select the invitees which were originally selected within the meeting
            $("#meetingMembers option").each(function (i) {
                var inviteeUserID = $(this).attr('id');

                $.each(data.MeetingMemberUserIDs, function (index, value) {
                    if (inviteeUserID == value) {
                        $(this).prop('selected', true);
                    }
                });
            });
        },
        error: function () {
            alert('An error occurred while loading invitees');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function loadMeetingRequestView(meetingDate, event) {
    $.ajax({
        url: '/landlordmanagement/GetMeetingRequestView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.meeting-request').html(data).promise().done(function () {
                $('#meetingDate').val(meetingDate);

                if (event != null)
                    getMeeting(event.id);//used when editing meetings

                getInvitees();//loads the combo box with the invitees for each user
            });
        },
        error: function () {
            alert('An error occurred while loading meeting request view');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }

    });
}

//loads/reloads the message list
function loadMsgList() {
    $.ajax({
        url: '/landlordmanagement/getMessages',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $.each(data, function (index, value) {
                var seenVal = value.Seen ? "seen" : "not-seen";
                var activeVal = currentUserNameSelected != null
                                && value.From == currentUserNameSelected ? "active" : "";

                $('#dashboard-messages').empty();

                $('#dashboard-messages').append(
                    '<li class="msg">'
                    + '<a class="' + seenVal + ' ' + activeVal + '" href="' + value.ID + '" id="' + value.ID + '">'
                    + '<div>'
                    + '<span class="glyphicon glyphicon-user img-circle img-circle-sm" style="margin-right:25px; font-size:25px;"> </span><strong id="from-user-name" style="font-size:14px; margin-right:10px;">' + value.From + '</strong> <em><span class="glyphicon glyphicon-calendar"> </span> ' + value.DateTCreated + '</em>'
                    + '<div class="txt-holder txt">' + value.Msg + '</div></div></a></li>'
                );
            });
        },
        error: function () {
            alert('An error occurred while loading the messages');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });

}

//loads or reloads the msg-container with the message threads sent between the users
function getMsgThread() {
    if (currentMsgIdSelected != null) {
        $.ajax({
            url: '/landlordmanagement/getMsgThread',
            type: 'GET',
            data: { id: currentMsgIdSelected },
            success: function (data) {
                $(this).parent().removeClass('seen').addClass('seen');
                var userId = $('#userId').val();
                var msgBoxContainer = $('.management-action-section .msg-container');
                msgBoxContainer.empty();

                $.each(data, function (index, value) {
                    if (userId == value.From) {
                        msgBoxContainer.append('<div id="' + value.ID + '" class="single-msg user">' + value.Msg + ' <span class="img-circle img-circle-sm" style="font-size:14px;">You</span></div>');
                    } else {
                        msgBoxContainer.append('<div id="' + value.ID + '" class="single-msg not-user"><span class="glyphicon glyphicon-user img-circle img-circle-sm" style="font-size:25px;"> </span> ' + value.Msg + '</div>');
                    }
                });
                //TODO update seen message
                $(".management-action-section .msg-container").animate({ scrollTop: $('.management-action-section .msg-container').prop("scrollHeight") }, 1000);
                $('#send-msg-btn').prop('disabled', false);
            },
            error: function () {
                alert('An error occurred while loading the selected message');
            }
        });
    }
}

//loads properties in the dashboard page
$(document).ready(function () {
    initializeSockets();

    //initialization of bootstrap popover
    $('[data-toggle="popover"]').popover({
        trigger: 'hover click',
        container: 'body'
    });

    //sets configuration for the modal display
    var system = function () {

        this.modalIndex = 2000;

        this.showModal = function (selector) {
            this.modalIndex++;

            $(selector).modal({
                backdrop: 'static',
                keyboard: true
            });
            $(selector).modal('show');
            $(selector).css('z-index', this.modalIndex);
        }

    }

    var sys = new system();
    /*
    */
    (function () {
        $.ajax({
            url: '/Landlordmanagement/getAllPropertyImages',
            type: 'GET',
            success: function (data) {
                if (!$.isEmptyObject(data) && !data.hasErrors) {
                    $.each(data, function (index, value) {
                        $('#rooms-collection')
                             .append('<a class="property-image" href="' + value.propertyID + '"><img title="click to manage property" id="' + value.propertyID + '" src="/Uploads/' + value.imageURL + '"/></a>');
                    });
                } else {
                    alert('An unexpected error occurred!, Contact system administrator');
                }
            },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }

        });
    })();

    //generates modal to compose a new message
    $(document.body).on('click', '#new-msg-btn', function (event) {
        event.preventDefault();

        //displays the modal whenever this is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-pencil"></span> Compose New Message');

        $('#action-body').html('<form id="new-msg-form" action="landlordmanagement/composenewmessage" method="post"><strong>To</strong> <input type="text" id="recipient" class="form-control" placeholder="Recipients Name"><br/>'
                              + '<strong>Message</strong> <textarea class="form-control" placeholder="Compose new message" rows="4" cols="30"></textarea></form>');
        $('#new-msg-form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                                + '<input class="btn btn-primary" type="submit" value="Send Message" id="submit-message" /></div>');

        // $('#action-body').html('<h3>This feature is coming soon......</h3>');
    });
    //generates modal to compose a new message and broadcast to every tennant
    $('#broadcast-message').click(function (event) {
        event.preventDefault();
        //displays the modal whenever this is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-pencil"></span> Broadcast New Message');
        /*
        $('#action-body').html('<form action="landlordmanagement/broadcastnewmessage" method="post"><strong>To</strong> <input disabled type="text" id="recipient" class="form-control" value="All"><br/>'
                              + '<strong>Message</strong> <textarea class="form-control" placeholder="Compose new message" rows="4" cols="30"></textarea></form>');
        $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                                + '<input class="btn btn-primary" type="submit" value="Broadcast Message" id="broadcast-message" /></div>');
                                */
        $('#action-body').html('<h3>This feature is coming soon......</h3>');
    });
    //generates modal to view all messages for a pariticular individual
    $('#view-all-messages').click(function (event) {
        event.preventDefault();
        //displays the modal whenever an image is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-eye-open"></span> View All Messages');
        /*
        //getting all messages
        $.ajax({
            url: '/Landlordmanagement/getMessagesHeaders',
            type: 'GET',
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $('#action-body').html('<ul id="view-messages"></ul>');
                    $.each(data, function (index, value) {
                        $('#action-body #view-messages').append('<li><a id="' + value.ID + '" alt="' + value.Date + '">' + '<strong>' + value.MessengerName + '/<strong>' + '</a></li>');
                    });
                    $('#action-body').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div>');
                } else {
                    $('#action-body').html('<li>No Messages Found</li>');
                    $('#action-body').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div>');
                }
            }
        });*/
        $('#action-body').html('<h3>This feature is coming soon......</h3>');


    });
    //adds new utility bill that should be paid by tennant
    $('#new-bill').click(function (event) {
        event.preventDefault();
        //displays the modal whenever this is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-pencil"></span> Broadcast New Bill');
        /*
        $('#action-body').html('<form enctype="multipart/form-data" action="/landlordmanagement/billsubmission" method="post"><select class="form-control" name="bill_type"><option disabled selected>Select Bill Type</option><option value="electricity">Electricity</option><option value="water">Water</option>'
                              + '<option value="cable">Cable</option><option value="internet">Internet</option><option value="gas">Gas</option></select><br/>'
                              + '<table id="bills-table" class="table-condensed"><tr><td>Date Issued</td><td><input class="form-control" type="text" name="date_issued" placeholder="MM/DD/YYYY"/></td></tr>'
                              + '<tr><td>Date Due</td><td><input class="form-control" type="text" name="date_due" placeholder="MM/DD/YYYY"/></td></tr>'
                              + '<tr><td>Bill Amount</td><td><input placeholder="$" class="form-control" type="text" name="bill_amount"/></td></tr>'
                              + '<tr><td>Message To Tennants</td><td><textarea name="tennats_message" class="form-control" rows="4" cols="30" placeholder="Broadcast Any Message To All Your Tennants"></textarea></td></tr>'
                              + '<tr><td>Image Of Bill</td><td><input type="file" name="bill_image" /></td> </tr></table></form>');
        //getting all rooms to select which ones to submit the bill to
        $.ajax({
            url: '/Landlordmanagement/getAllRooms',
            type: 'GET',
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $('#action-body table').after('<div id="room_selection"><br/><em>Select accommodations that the bill should be sent to</em> <hr/></div>')
                    $.each(data, function (index, value) {
                        $('#action-body #room_selection')
                            .after('<div class="modal-rooms"><input type="checkbox" id="' + value.id + '" value="' + value.id + '" name="room_selection[]"/><a href="' + value.id + '"><img id="' + value.id + '" src="/Uploads/' + value.room_pic_url + '"/></a></div>');
                    });
                }
            }
        });

        $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                                + '<input class="btn btn-primary" type="submit" value="Broadcast Bill" id="broadcast-bill" /></div>');*/
        $('#action-body').html('<h3>This feature is coming soon......</h3>');

    });
    //displays all bills for the current user
    $('#view-all-bills').click(function (event) {
        event.preventDefault();
        //displays the modal whenever an image is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-eye-open"></span> View All Bills');

        //getting all bills
        //coming soon........
        /*
        $.ajax({
            url: '/Landlordmanagement/getAllBills',
            type: 'GET',
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    var count = 1;

                    $('#action-body').html('<table class="table-condensed" id="view-bills"><tr><th></th><th>Bill Type</th><th>Date Issued</th><th>Date Due</th><th>Bill Amount</th></tr>');

                    $.each(data, function (index, value) {
                        $('#action-body #view-bills tr:last').after('<tr><td>' + count + '</td><td>' + value.BType + '</td><td>' + value.DateIssued + '</td><td>' + value.DateDue + '</td><td>' + value.BAmount + '</td></tr></table>');
                        count++;
                    });
                    $('#action-body').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div>');
                } else {
                    $('#action-body').html('<li>No Bills Found</li>');
                    $('#action-body').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div>');
                }
            }
        
        });*/

        $('#action-body').html('<h3>This feature is coming soon......</h3>');

    });
    $(document.body).on('click', '#removeProperty', function () {
        var id = $('#id').val();

        $.ajax({
            url: '/landlordmanagement/removeProperty',
            type: 'post',
            data: { 'id': id },
            success: function () {
                window.location = '/landlordmanagement/dashboard';
            }
        });
    });
    //updates specific property
    $(document.body).on('click', '.property-image', function (event) {
        event.preventDefault();

        //displays the modal whenever an image is selected
        sys.showModal('#managementModal');

        $('#action-header').html('<span class="glyphicon glyphicon-refresh"></span> Update Property Information');

        var propertyID = $(this).attr('href');
        //gets all information for the property
        $.ajax({
            url: '/Landlordmanagement/getproperty',
            type: 'GET',
            data: { 'property_id': propertyID },
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $.each(data, function (index, value) {

                        if (value.BedroomAmount !== undefined) {

                            $('#action-body').html('<input type="button" class="btn btn-danger btn-block" id="removeProperty" value="Remove Property"/><br/><br/><img style="width:480px;height:300px;"  id="' + value.ID + '" src="/Uploads/' + value.ImageURL + '"/><hr/><form action="/landlordmanagement/updatehouse" method="post"><table class="table-condensed" id="property-information">');

                            $('#action-body #property-information ').append('<tr><td>Price</td><<td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td>'
                                                                            + '<td>Bedroom Amount</td><<td><input  class="form-control" type="text" name="bedroomAmount" value="' + value.BedroomAmount + '"></td></tr>'
                                                                            + '<tr><td>Bathroom Amount</td><<td><input  class="form-control" type="text" name="bathroomAmount" value="' + value.BathroomAmount + '"></td>'
                                                                            + '<td>Purpose</td><<td><select class="form-control" id="purpose" name="purpose"><option value="Rent">Rent</option><option value="Sale">Sale</option></select></td></tr>'
                                                                            + '<tr><td>Is Room Furnished</td><<td><select class="form-control" id="isFurnished" name="isFurnished"><option value="True">Yes</option><option value="False">No</option></select></td></tr>'
                                                                            + '<tr><td>Description</td><td><textarea class="form-control" rows="6" cols="35" name="description">' + value.Description + '</textarea></td></tr>'
                                                                            + '<tr><td><input type="hidden" name="id" id="id" value="' + value.ID + '"/></td></tr></table></form>');

                            value.Purpose == 'Rent' ? $('#purpose option[value="Rent"]').attr('selected', 'selected') : $('#purpose option[value="Sale"]').attr('selected', 'selected');
                            value.isFurnished == 'true' ? $('#isFurnished option[value="true"]').attr('selected', 'selected') : $('#isFurnished option[value="false"]').attr('selected', 'selected');
                        }
                        else
                            if (value.Area !== undefined) {

                                $('#action-body').html('<input type="button" class="btn btn-danger btn-block" id="removeProperty" value="Remove Property"/><br/><br/><img style="width:480px;height:300px;"  id="' + value.ID + '" src="/Uploads/' + value.ImageURL + '"/><hr/><form action="/landlordmanagement/updateland" method="post"><table class="table-condensed" id="property-information">');

                                $('#action-body #property-information ').append('<tr><td>Price</td><<td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td>'
                                                                                + '<td>Purpose</td><<td><select class="form-control" id="purpose" name="purpose"><option value="Rent">Rent</option><option value="Sale">Sale</option><option value="Lease">Lease</option></select></td></tr>'
                                                                                + '<tr><td>Area</td><<td><input  class="form-control" type="text" name="area" value="' + value.Area + '"></td></tr>'
                                                                                + '<tr><td>Description</td><td><textarea class="form-control" rows="6" cols="35" name="description">' + value.Description + '</textarea></td></tr>'
                                                                                + '<tr><td><input type="hidden" name="id" id="id" value="' + value.ID + '"/></td></tr></table></form>');

                                switch (value.Purpose) {
                                    case 'Rent': $('#purpose option[value="Rent"]').attr('selected', 'selected');
                                        break;
                                    case 'Sale': $('#purpose option[value="Sale"]').attr('selected', 'selected');
                                        break;
                                    case 'Lease': $('#purpose option[value="Lease"]').attr('selected', 'selected');
                                        break;
                                }
                            }
                            else {

                                $('#action-body').html('<input type="button" class="btn btn-danger btn-block" id="removeProperty" value="Remove Property"/><br/><br/><img style="width:480px;height:300px;"  id="' + value.ID + '" src="/Uploads/' + value.ImageURL + '"/><hr/><form action="/landlordmanagement/updateaccommodation" method="post"><table class="table-condensed" id="property-information">');

                                $('#action-body #property-information ').append('<tr><td>Price</td><td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td>'
                                + '<td>Occupancy</td><td><input  class="form-control" type="text" name="occupancy" value="' + value.Occupancy + '"></td></tr>'
                                + '<tr><td>Security Deposit</td><td><input class="form-control" type="text" name="security_deposit" value="' + value.SecurityDeposit + '"></td>'
                                + '<td>Water</td><td><select id="water" name="water" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Internet</td><td><select id="internet" name="internet" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td>'
                                + '<td>Electricity</td><td><select id="electricity" name="electricity" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Cable</td><td><select id="cable" name="cable" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td>'
                                + '<td>Gas</td><td><select id="gas" name="gas" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Availability</td><td><select id="availability" name="availability" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td>'
                                + '<td>Gender Preference</td><td><select class="form-control" id="gender_preference" name="gender_preference" class="form-control"><option value="B">Both</option><option value="M">Male</option><option value="F">Female</option></select></td></tr>'
                                + '<tr><td>Security Deposit</td><td><input class="form-control" type="text" name="security_deposit" value="' + value.SecurityDeposit + '"></td></tr>'
                                + '<tr><td>Description</td><td><textarea class="form-control" rows="6" cols="35" name="description">' + value.Description + '</textarea></td></tr>'
                                + '<tr><td><input type="hidden" name="id" id="id" value="' + value.ID + '"/></td></tr></table></form>');
                                //setting the values for the dropdown list
                                value.Water == 'selected' ? $('#water option[value="true"]').attr('selected', 'selected') : $('#water option[value="false"]').attr('selected', 'selected');
                                value.Internet == 'selected' ? $('#internet option[value="true"]').attr('selected', 'selected') : $('#internet option[value="false"]').attr('selected', 'selected');
                                value.Electricity == 'selected' ? $('#electricity option[value="true"]').attr('selected', 'selected') : $('#electricity option[value="false"]').attr('selected', 'selected');
                                value.Cable == 'selected' ? $('#cable option[value="true"]').attr('selected', 'selected') : $('#cable option[value="false"]').attr('selected', 'selected');
                                value.Gas == 'selected' ? $('#gas option[value="true"]').attr('selected', 'selected') : $('#gas option[value="false"]').attr('selected', 'selected');
                                value.Availability == 'selected' ? $('#availability option[value="true"]').attr('selected', 'selected') : $('#availability option[value="false"]').attr('selected', 'selected');

                                switch (value.GenderPreference) {
                                    case 'B': $('#gender_preference option[value="B"]').attr('selected', 'selected');
                                        break;
                                    case 'F': $('#gender_preference option[value="F"]').attr('selected', 'selected');
                                        break;
                                    case 'M': $('#gender_preference option[value="M"]').attr('selected', 'selected');
                                        break;
                                }
                            }
                    });
                    $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                                + '<input class="btn btn-primary" type="submit" value="Update Property" id="update-property" /></div>');
                }
            }
        });
    });
    //adds new property to a customer's account
    $(document.body).on('click', '#add-new-property', function (event) {
        event.preventDefault();

        $.ajax({
            url: '/servicer/GetAdvertisePropertyView',
            type: 'GET',
            success: function (data) {
                //displays the modal whenever an image is selected
                $('#action-header').html('<span class="glyphicon glyphicon-plus"></span> Add Property');
                $('#action-body').html(data);

                sys.showModal('#managementModal');
            },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }

        });
    });
    //sends acceptance mail to requestee 
    $(document.body).on('click', '.btnAcceptRequest', function (event) {
        var reqID = $(this).attr('id');

        $.ajax({
            url: '/landlordmanagement/acceptrequest',
            type: 'POST',
            data: { 'reqID': reqID },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function () {
                window.location = "/landlordmanagement/dashboard";
            },
            complete: function () {
                $('#modal-loading').fadeIn();
            }
        });
    });

    //cancels request 
    $(document.body).on('click', '.btnDenyRequest', function (event) {
        var reqID = $(this).attr('id');

        $.ajax({
            url: '/landlordmanagement/cancelrequest',
            type: 'POST',
            data: { 'reqID': reqID },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function () {
                window.location = "/landlordmanagement/dashboard";
            },
            complete: function () {
                $('#modal-loading').fadeIn();
            }
        });
    });
    //loads the details for the specific property type 
    $(document.body).on('change', '#property-type', function (event) {

        var propertyType = $(this).val();

        switch (propertyType) {
            case 'room': $.ajax({
                url: '/accounts/getroompartialview',
                type: 'GET',
                beforeSend: function () {
                    $('#loader').show();
                },
                success: function (data) {
                    $('#property-registration-container').html(data);

                    $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                    + '<input class="btn btn-primary" type="submit" value="Add Property" id="add-property" /></div>');

                    $('form').removeData('validator');
                    $.validator.unobtrusive.parse($('form'));
                },
                complete: function () {
                    $('#loader').hide();
                }
            });
                break;
            case 'house': $.ajax({
                url: '/accounts/gethousepartialview',
                type: 'GET',
                beforeSend: function () {
                    $('#loader').show();
                },
                success: function (data) {
                    $('#property-registration-container').html(data);
                    $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                    + '<input class="btn btn-primary" type="submit" value="Add Property" id="add-property" /></div>');

                    $('form').removeData('validator');
                    $.validator.unobtrusive.parse($('form'));
                },
                complete: function () {
                    $('#loader').hide();
                }
            });
                break;
            case 'land': $.ajax({
                url: '/accounts/getlandpartialview',
                type: 'GET',
                beforeSend: function () {
                    $('#loader').show();
                },
                success: function (data) {
                    $('#property-registration-container').html(data);
                    $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                    + '<input class="btn btn-primary" type="submit" value="Add Property" id="add-property" /></div>');

                    $('form').removeData('validator');
                    $.validator.unobtrusive.parse($('form'));
                },
                complete: function () {
                    $('#loader').hide();
                }
            });
                break;
        }
    });

    //displays all messages on the page 
    $('#view-all-msgs-btn').click(function (event) {
        event.preventDefault();

        var btnViewAll = $(this);
        var btnCmd = $(this).text();

        if (btnCmd == 'View All') {
            $('.msg:gt(1)').slideDown('fast', function () {
                $(this).removeClass('hide').addClass('show');
                btnViewAll.text('Hide');
            });
        } else {
            $('.msg:gt(1)').slideUp('fast', function () {
                $(this).removeClass('show').addClass('hide');
                btnViewAll.text('View All');
            });
        }
    });

    //display the selected message
    $(document.body).on('click', '.msg a', function (event) {
        event.preventDefault();

        currentMsgIdSelected = $(this).attr('id');
        currentUserNameSelected = $(this).find('#from-user-name').text();

        $(this).parents('ul').find('a').removeClass('active');
        $(this).addClass('active');//makes message active

        var isNotSeen = $(this).hasClass('not-seen');
        if (isNotSeen) {
            $(this).removeClass('not-seen');
            $(this).addClass('seen');//makes message seen
        }

        var msgBox = $('#msg-box');
        msgBox.prop('disabled', false);
        msgBox.val('');//clear msgbox

        getMsgThread();
    });

    //sends message
    $(document.body).on('click', '#send-msg-btn', function (event) {
        event.preventDefault();

        var msgBox = $('#msg-box');
        var isMsgBoxEmpty = msgBox.val() == '' ? true : false;
        var msg = msgBox.val();
        msgBox.val('');

        if (!isMsgBoxEmpty) {
            $.ajax({
                url: '/landlordmanagement/sendMsg',
                type: 'GET',
                data: { id: currentMsgIdSelected, msg: msg },
                success: function (data) {
                    getMsgThread();
                },
                error: function () {
                    alert('An error occurred while loading the selected message');
                }
            });
        } else {
            alert('Enter a message');
        }
    });

    $(document.body).on('mouseenter', '.single-msg', function () {
        var singleMsgMenu = '<div id="delete-msg" class="single-msg-menu">'
                            + 'Delete <span class="glyphicon glyphicon-trash"></span></div>';

        $(this).prepend(singleMsgMenu);
    });

    $(document.body).on('mouseleave', '.single-msg', function () {
        $('.single-msg-menu').remove();
    });

    $(document.body).on('click', '#delete-msg', function () {
        var id = $(this).parent().attr('id');
        $.ajax({
            url: '/landlordmanagement/deleteMsg',
            type: 'GET',
            data: { id: id },
            success: function (data) {
            },
            error: function () {
                alert('An error occurred while deleting the selected message');
            }
        });
    });

    $(document.body).on('click', '#schedule-meeting', function (event) {
        event.preventDefault();

        var validator = $('#meetingRequestForm').validate({});

        var isValid = validator.form();

        if (isValid) {
            var id = $('#id').val();
            var title = $('#meetingTitle').val();
            var date = $('#meetingDate').val();
            var time = $('#meetingTime').val();
            var location = $('#location').val();
            var purpose = $('#purpose').val();
            var isEdit = $('#isEdit').val();
            var MeetingMemberUserIDs = [];

            $("#meetingMembers option").each(function (i) {
                MeetingMemberUserIDs.push($(this).attr('id'));
            });
            alert(isEdit);

            $.ajax({
                url: '/landlordmanagement/scheduleMeeting',
                type: 'Post',
                data: {
                    ID: id, MeetingTitle: title, MeetinDate: date, MeetingTime: time, Location: location,
                    Purpose: purpose, isEdit: isEdit, MeetingMemberUserIDs: MeetingMemberUserIDs
                },
                success: function (data) {
                    alert('Meeting uploaded successfully');
                },
                error: function () {
                    alert('An error occurred while addoing meeting');
                }
            });
        }
    });

    $('.management-action a').click(function (event) {
        event.preventDefault();

        var actionId = $(this).children().attr('id');

        //distinct selections
        $('.management-action .active').removeClass('active');
        $(this).addClass('active');

        var position = $(this).position();
        pointToSelectedAction(position);
        loadView(actionId);
    });

    $(window).resize(function () {
        var position = $('.management-action .active').position();
        pointToSelectedAction(position);
    });

});