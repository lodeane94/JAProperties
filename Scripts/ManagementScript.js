var currentMsgIdSelected = null;
var currentUserNameSelected = null;
var $calendar = null;
var orRentCntrDataContent = null;

function initializeSockets() {
    var dashboardHub = $.connection.notificationHub;

    $.connection.hub.logging = true;
    
    $.connection.hub.start().done(function () {
        console.log('Connection establised');
    });

    dashboardHub.client.updateUserMessages = function () {
        alert('message updated');
        loadMessagesView();
    }

    dashboardHub.client.updateMeeting = function () {
        alert('A meeting has been updated on your portal');
        loadMeetingCalender();
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

            var startDate = moment(start).format('LL');
            
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

//populates the meeting hour, minute and period with their
//values
function populateMeetingTime() {
    //meeting hour
    for (var i = 1; i <= 12; i++) {
        $('#meetingHour').append($('<option></option>')
                    .attr('id', i)
                    .attr('value', i)
                    .text(i));
    }

    //meeting minute
    for (var i = 1; i <= 59; i++) {
        $('#meetingMinute').append($('<option></option>')
                    .attr('id', i)
                    .attr('value', i)
                    .text(i));
    }
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

                $.each(data, function (index, value) {
                    var time = value.MeetingHour + ':' + value.MeetingMinute + ' ' + value.MeetingPeriod;

                    var date = moment(value.MeetingDate).format('LL');
                    var dateTime = moment(date + ' ' + time);

                    var event = {
                        id: value.ID,
                        title: value.MeetingTitle,
                        start: moment(dateTime)
                    };

                    $calendar.fullCalendar("renderEvent", event, true);
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

function getMsgRecipients() {
    $.ajax({
        url: '/landlordmanagement/getMsgRecipients',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('#messageRecipients').empty();//clears div first before loading recipients

            $.each(data, function (index, value) {
                $('#messageRecipients').append($('<option></option>')
                    .attr('id', value.UserID)
                    .attr('value', value.FullName)
                    .text(value.FullName));
            });
        },
        error: function () {
            alert('An error occurred while loading message recipients');
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
            var date = moment(data.MeetingDate).format('LL');//removes time from the date
            
            $('#id').val(data.ID);
            $('#meetingTitle').val(data.MeetingTitle);
            $('#meetingDate').val(date);
            $('#location').val(data.Location);
            $('#purpose').val(data.Purpose);
            $('#isEdit').val(true);

            $('#meetingHour option').each(function () {
                var id = $(this).attr('id');

                if (data.MeetingHour == id) {
                    $(this).prop('selected', true);
                }
            });

            $('#meetingMinute option').each(function () {
                var id = $(this).attr('id');

                if (data.MeetingMinute == id) {
                    $(this).prop('selected', true);
                }
            });

            $('#meetingPeriod option').each(function () {
                var id = $(this).attr('id');

                if (data.MeetingPeriod == id) {
                    $(this).prop('selected', true);
                }
            });
            
            //select the invitees which were originally selected within the meeting
            $("#meetingMembers option").each(function (i) {
                var inviteeUserID = $(this).attr('id');
                
                $.each(data.MeetingMemberUserIDs, function (index, value) {
                    if (inviteeUserID == value) {
                        $('#meetingMembers option[id="'+value+'"').prop("selected", true);
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
                populateMeetingTime();

                getInvitees();//loads the combo box with the invitees for each user

                if (event != null)
                    getMeeting(event.id);//used when editing meetings
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
            $('#dashboard-messages').empty();

            $.each(data, function (index, value) {
                var seenVal = value.Seen ? "seen" : "not-seen";
                var activeVal = currentUserNameSelected != null
                                && value.From == currentUserNameSelected ? "active" : "";                

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
                //if this message is sent from the current logged in user
                //then dispaly it to the right and it should not have any glyphicon to it
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

//loads the tennants view on the management portal
function loadTennantsView() {
    $.ajax({
        url: '/landlordmanagement/GetTennantsView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-action-content-holder').html(data).promise().done(function () {
                $('#accordion').accordion({ header: "h5", collapsible: true, active: false });

                //initialization of bootstrap popover
                $('[data-toggle="popover"]').popover({
                    trigger: 'hover click',
                    container: '#accordion'
                });
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

function updateRent(tennantId, updateVal) {
    
    $.ajax({
        url: '/landlordmanagement/updateRent',
        type: 'GET',
        data: { id: tennantId, newRentAmt: updateVal },
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function () {
            alert('Rent updated');
        },
        error: function () {
            alert('An error occurred while updating rent');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function unenrollTennant(tennantId) {
    $.ajax({
        url: '/landlordmanagement/unenrollTennant',
        type: 'GET',
        data: { id: tennantId},
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function () {
            alert('Tennant unenrolled successfully');
            loadTennantsView();
        },
        error: function () {
            alert('An error occurred while unenrolling tennant');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}
/*TODO later if necessary
function sendNotice(tennantId) {
    $.ajax({
        url: '/landlordmanagement/sendNotice',
        type: 'GET',
        data: { tennantId: tennantId },
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {

        },
        error: function () {
            alert('An error occurred while unenrolling tennant');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}*/

//loads properties in the dashboard page
$(document).ready(function () {
    //initialize signalr sockets which will broadcast notifications 
    //in real time
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

        sys.showModal('#createNewMessageModal');

        getMsgRecipients();
    });

    //sends a message to a user
    $(document.body).on('click', '#send-new-message', function (event) {
        event.preventDefault();
        
        var msgRecipientIds = [];
        var msg = $('#new-message').val();

        $("#messageRecipients option").each(function (i) {
            msgRecipientIds.push($(this).attr('id'));
            alert($(this).attr('id'));
        });

        $.ajax({
            url: '/landlordmanagement/sendMessage',
            type: 'POST',
            data: { msgRecipients: msgRecipientIds, msg: msg },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function () {
                $('#close-modal').click();

                alert('Message sent');
                loadMessagesView();
            },
            error: function () {
                alert('An error occurred while sending message');
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }
        });
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
        var isUserPropOwner = $('#isUserPropOwner').val();
        
        $.ajax({
            url: '/landlordmanagement/cancelrequest',
            type: 'POST',
            data: { 'reqID': reqID, 'isUserPropOwner': isUserPropOwner },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function (data) {
                alert(data);
               // window.location = "/landlordmanagement/dashboard";
            },
            complete: function () {
                $('#modal-loading').fadeOut();
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

    //displays the delete message pop down over a message list
    //upon mouse enter
    $(document.body).on('mouseenter', '.msg', function () {
        var singleMsgMenu = '<div id="delete-all-msgs" class="delete-msg-bar">'
                            + 'Delete All <span class="glyphicon glyphicon-trash"></span></div>';

        $(this).append(singleMsgMenu);
    });

    //removes the delete message pop down over a message list
    //upon mouse leave
    $(document.body).on('mouseleave', '.msg', function () {
        $('.delete-msg-bar').remove();
    });

    //displays the delete message pop up over a single message
    //upon mouse enter
    $(document.body).on('mouseenter', '.single-msg', function () {
        var singleMsgMenu = '<div id="delete-msg" class="single-msg-menu">'
                            + 'Delete <span class="glyphicon glyphicon-trash"></span></div>';

        $(this).prepend(singleMsgMenu);
    });

    //removes the delete message pop up over a single message
    //upon mouse leave
    $(document.body).on('mouseleave', '.single-msg', function () {
        $('.single-msg-menu').remove();
    });

    //deletes one message from the message thread
    $(document.body).on('click', '#delete-msg', function () {
        var id = $(this).parent().attr('id');

        if (confirm("Are you sure ?") == true) {//prompt user before deleting the message
            $.ajax({
                url: '/landlordmanagement/deleteMsg',
                type: 'GET',
                data: { id: id },
                success: function () {
                },
                error: function () {
                    alert('An error occurred while deleting the selected message');
                }
            });
        }
    });

    //delete all messages from the message thread
    $(document.body).on('click', '#delete-all-msgs', function () {
        var id = $(this).prev().attr('id');
     
        if (confirm("Are you sure ?") == true) {//prompt user before deleting the message thread
            $.ajax({
                url: '/landlordmanagement/deleteMsgThread',
                type: 'GET',
                data: { id: id },
                success: function () {
                    alert('Message Thread Deleted');
                },
                error: function () {
                    alert('An error occurred while deleting the selected message');
                }
            });
        }
    });

    $(document.body).on('click', '#schedule-meeting', function (event) {
        event.preventDefault();

        var validator = $('#meetingRequestForm').validate({
            showErrors: function (errorMap, errorList) {
                this.defaultShowErrors();
            }
        });

        var isValid = validator.form();

        if (isValid) {
            var id = $('#id').val();
            var title = $('#meetingTitle').val();
            var date = $('#meetingDate').val();
            var hour = $('#meetingHour').val();
            var minute = $('#meetingMinute').val();
            var period = $('#meetingPeriod').val();
            var location = $('#location').val();
            var purpose = $('#purpose').val();
            var isEdit = $('#isEdit').val();
            var MeetingMemberUserIDs = [];

            $("#meetingMembers option").each(function (i) {
                MeetingMemberUserIDs.push($(this).attr('id'));
            });


            $.ajax({
                url: '/landlordmanagement/scheduleMeeting',
                type: 'Post',
                data: {
                    ID: id, MeetingTitle: title, MeetingDate: date, MeetingHour: hour,
                    MeetingMinute: minute, MeetingPeriod: period, Location: location,
                    Purpose: purpose, isEdit: isEdit, MeetingMemberUserIDs: MeetingMemberUserIDs
                },
                success: function (data) {
                    alert('Meeting uploaded successfully');
                },
                error: function () {
                    alert('An error occurred while adding meeting');
                }
            });
        }
    });

    $(document.body).on('click', '.update-rent', function () {
        var btnAction = $(this).text();
        var tennantId = $(this).attr('id');
        var price = $(this).parent().find('.price');
        var rentCtnr = $(this).parent();
        
        
        if (btnAction == 'Update') {
            orRentCntrDataContent = rentCtnr.attr('data-content');

            price.prop('readonly', false);
            $(this).text('Save');

            //change text content of the popover to prompt user to save their changes
            rentCtnr.attr('data-content', 'Click the save button to change the rent amount');
        } else {
            var priceChanged = price.val();

            price.prop('readonly', true);
            $(this).text('Update');

            //change text content of the popover to prompt user to save their changes
            rentCtnr.attr('data-content', orRentCntrDataContent);

            updateRent(tennantId, priceChanged);
        }

    });

    $(document.body).on('click', '.unenroll-tennant', function () {
        var tennantId = $(this).attr('id');
        alert(tennantId);
        unenrollTennant(tennantId);
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