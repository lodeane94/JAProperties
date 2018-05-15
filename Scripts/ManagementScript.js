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
        loadMessagesView();
    }

    dashboardHub.client.updateMeeting = function () {
        alert('A meeting has been updated on your portal');
        loadMeetingCalender();
    }

    dashboardHub.client.newRequisitionAlert = function () {
        alert('A new property requisition was just made');
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
        case 'owned-properties':
        case 'saved-properties':
            loadPropertiesView(actionId);
            break;
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
        case 'profile': loadProfileView();
            break;
        case 'subscription': loadSubscriptionView();
            break;
        case 'payments': loadPaymentsView();
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
            $('.management-main-content').html(data);

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
            $('.management-main-content').html(data);
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
            $('.management-main-content')
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

function loadPropertiesView(viewId) {
    var url = '';

    if (viewId == 'owned-properties')
        url = '/landlordmanagement/GetManagementPropertiesView';
    else
        url = '/landlordmanagement/GetSavedPropertiesView';

    $.ajax({
        url: url,
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-main-content').html(data);
        },
        error: function () {
            alert('An error occurred while loading view');
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
                        $('#meetingMembers option[id="' + value + '"').prop("selected", true);
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
                    + '<span class="fa fa-user-o fa-2x img-circle img-circle-sm" style="margin-right:10px; font-size:25px;"></span><span id="from-user-name" style="font-size:14px; margin-right:1px;">' + value.From + '</span> <em><span class="glyphicon glyphicon-calendar"> </span> ' + value.DateTCreated + '</em>'
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
                        msgBoxContainer.append('<div id="' + value.ID + '" class="single-msg user">' + value.Msg + ' <span class="fa fa-user fa-2x" style="font-size:14px;">You</span></div>');
                    } else {
                        msgBoxContainer.append('<div id="' + value.ID + '" class="single-msg not-user"><span class="fa fa-user   fa-2x img-circle img-circle-sm" style="font-size:25px;"> </span> ' + value.Msg + '</div>');
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
            $('.management-main-content').html(data).promise().done(function () {
                $('#accordion').accordion({ header: "h5", collapsible: true, active: false });

                //initialization of bootstrap popover
                $('[data-toggle="popover"]').popover({
                    trigger: 'hover',
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
        data: { id: tennantId },
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

function loadProfileView() {
    $.ajax({
        url: '/landlordmanagement/GetProfileView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-main-content').html(data);
        },
        error: function () {
            alert('An error occurred while loading view');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function loadSubscriptionView() {
    $.ajax({
        url: '/landlordmanagement/GetSubscriptionView',
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-main-content').html(data);
        },
        error: function () {
            alert('An error occurred while loading view');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}

function loadPaymentsView(pgNo) {
    $.ajax({
        url: '/landlordmanagement/GetPaymentsView',
        data: { pgNo: pgNo },
        type: 'GET',
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        success: function (data) {
            $('.management-main-content').html(data).promise().done(function () {
                initializePaymentPagination();
            });
        },
        error: function () {
            alert('An error occurred while loading view');
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        }
    });
}
//creates the functionality that is necessary for the payment pagination
function initializePaymentPagination() {
    var itemsCount = $('#itemsCount').val();
    var pgNo = Number($('#pgNo').val());
    var take = $('#pgTake').val();

    if (itemsCount < 2) {
        $('.page-item').addClass('disabled');
        $('.page-link').attr('disabled', true);
        $('.page-link').attr('tabindex', -1);
    }

    if (itemsCount > 1 && (pgNo + 1) == 1) {//at the beginning
        $('.previous').addClass('disabled');
        $('.previous').attr('disabled', true);
        $('#previous').attr('tabindex', -1);
    }

    if (itemsCount > 1 && (pgNo + 1) > 1 && ((pgNo + 1) * take) < itemsCount) {//somewhere in the middle
        /*  $('.page-item').removeClass('disabled');
          $('.page-link').attr('disabled', false);
          $('.page-link').removeAttr('tabindex');*/
    }

    if (itemsCount > 1 && ((pgNo + 1) * take) >= itemsCount) {//at the end
        $('.next').addClass('disabled');
        $('.next').attr('disabled', true);
        $('#next').attr('tabindex', -1);

        /*    $('.previous').removeClass('disabled');
            $('.previous').attr('disabled', false);
            $('.previous').removeAttr('tabindex');*/
    }
}
//computes and display the subscription changing price
function computeSubChgPrice() {
    var clickedSubCost = $('#clickedSubscriptionCost').html();
    var isSubPeriodChgActive = $('#subscription-period-chg').prop('disabled');
    var price = 0;

    if (!isSubPeriodChgActive) {
        var period = $('#subscription-period-chg').val();
        price = clickedSubCost * period;
    } else
        price = clickedSubCost;

    $('#chg-sub-price').html(price);
    $('.chg-sub-price-container').removeClass('hide');
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
        trigger: 'hover',
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

        if (msg != null && msg != '') {

            $("#messageRecipients option").each(function (i) {
                msgRecipientIds.push($(this).attr('id'));
            });

            $.ajax({
                url: '/landlordmanagement/sendMessage',
                type: 'POST',
                data: { msgRecipients: msgRecipientIds, msg: msg },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function () {
                    $('#new-message').val('');
                    $('#close-modal').click();
                    loadMessagesView();
                },
                error: function () {
                    alert('An error occurred while sending message');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                }
            });
        } else {
            alert('Cannot send blank message');
        }
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
                    //alert('Message Thread Deleted');
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
        //pointToSelectedAction(position);
        loadView(actionId);
    });

    $(document.body).on('click', '.property-image', function (event) {
        event.preventDefault();

        var href = $(this).attr('href');

        $.ajax({
            url: '/servicer/GetModalUpdatePropertyView',
            type: 'Get',
            data: { ID: href },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function (data) {

                $('#updatePropertyModalContainer').remove();
                $('#modal-displays').append(data);

                sys.showModal('#updatePropertyModal');

                //initialization of bootstrap popover
                $('[data-toggle="popover"]').popover({
                    trigger: 'hover',
                    container: 'body'
                });
            },
            error: function () {
                alert('An error occurred while retrieving property\'s information');
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }
        });
    });

    //fades out temporary alerts after they are displayed
    $('.temp-alert').fadeIn("slow", function () {
        $(this).fadeOut(10000);
    });

    //stops propagation on the dropdown menu
    $(document.body).on('click', '.dd-primary-img .dropdown-menu', function (e) {
        e.stopPropagation();
    });

    $(document.body).on('click', '.select-as-primary', function (event) {
        event.preventDefault();

        var isDisabled = $(this).hasClass('disabled');

        if (!isDisabled) {
            var propertyId = $('#ID').val();
            var imgId = $(this).parent().prev('.dropdown-item').find('img').attr('id');
            var imgUrl = $(this).parent().prev('.dropdown-item').find('img').attr('src');

            $.ajax({
                url: '/landlordmanagement/UpdatePropertyPrimaryImg',
                type: 'Put',
                data: { imgId: imgId, propertyId: propertyId },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != '' && data != null) {
                        $('.modal-body').prepend('' +
                            '<div class="alert-success temp-alert">Property Updated Successfully</div> ')
                            .hide()
                            .fadeIn('slow');

                        //updating modal primary image
                        $('#primary-property-img').hide().fadeIn(1000).attr('src', imgUrl);

                        //updating dashboard primary image 
                        $('.property-image#' + propertyId + ' img').attr('src', imgUrl);

                        $('.upd-property-images-ctnr').html(data);

                    } else {
                        $('.modal-body').prepend('' +
                            '<div class="alert-danger temp-alert">Property Was Not Update - Contact Administrator</div> ')
                            .hide()
                            .fadeIn('slow');
                    }
                },
                error: function () {
                    alert('An error occurred while retrieving property\'s information');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                    $('.temp-alert').fadeOut(10000);
                }
            });
        } else {
            alert("Image already is already primary");
        }
    });

    $(document.body).on('click', '.delete-img', function (event) {
        event.preventDefault();

        var isDisabled = $(this).hasClass('disabled');

        if (!isDisabled) {
            var propertyId = $('#ID').val();
            var imageId = $(this).parent().prev('.dropdown-item').find('img').attr('id');

            $.ajax({
                url: '/landlordmanagement/DeletePropertyImage',
                type: 'Put',
                data: { propertyId: propertyId, imageId: imageId },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != '' && data != null) {
                        $('.modal-body').prepend('' +
                            '<div class="alert-success temp-alert">Property Deleted Successfully</div> ')
                            .hide()
                            .fadeIn('slow');

                        $('.upd-property-images-ctnr').html(data);
                        $('.upd-property-images-ctnr').fadeOut().fadeIn('10000');
                    } else {
                        $('.modal-body').prepend('' +
                            '<div class="alert-danger temp-alert">Property Was Not Update - Contact Administrator</div> ')
                            .hide()
                            .fadeIn('slow');
                    }
                },
                error: function () {
                    alert('An error occurred while retrieving property\'s information');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                    $('.temp-alert').fadeOut(10000);
                }
            });
        } else {
            alert('Cannot delete primary image');
        }
    });

    $(document.body).on('click', '#add-image', function (event) {
        event.preventDefault();

        var formData = new FormData();
        var file = $('#flPropertyImg')[0].files[0];

        if (file != undefined && file != null) {
            var propertyId = $('#ID').val();

            formData.append('propertyImgUpload', file);
            formData.append('ID', propertyId);

            $.ajax({
                url: '/landlordmanagement/AddPropertyImage',
                type: 'Post',
                data: formData,
                processData: false,
                contentType: false,
                cache: false,
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != '' && data != null) {

                        $('.modal-body').prepend('' +
                            '<div class="alert-success temp-alert">Property Image Added Successfully</div> ')
                            .hide()
                            .fadeIn('slow');

                        $('.upd-property-images-ctnr').fadeOut().fadeIn('10000');
                        $('.upd-property-images-ctnr').html(data);
                        $('#flPropertyImg').val('');//clearing file

                    } else {
                        $('.modal-body').prepend('' +
                            '<div class="alert-danger temp-alert">Property Image Was Not Added - Contact Administrator</div> ')
                            .hide()
                            .fadeIn('slow');
                    }
                },
                error: function () {
                    alert('An error occurred while retrieving property\'s information');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                    $('.temp-alert').fadeOut(10000);
                }
            });
        } else {
            alert('Select an image first');
        }

    });

    $(document.body).on('click', '#edit-profile-btn', function (event) {
        event.preventDefault();

        var action = $(this).text();

        if (action == 'Edit') {
            $('input').removeAttr('disabled');
            $('#updateProfileBtn').removeClass('disabled');
            $(this).text('Cancel');
        } else {
            $('input').attr('disabled', true);
            $('#updateProfileBtn').addClass('disabled');
            $(this).text('Edit');
        }
    });

    $(document.body).on('click', '#updateProfileBtn', function (event) {
        event.preventDefault();

        var form = $('#updateProfileForm');
        var validator = form.validate();

        var isValid = validator.form();

        if (isValid) {
            var formData = new FormData($('#updateProfileForm')[0]);
            $.ajax({
                url: '/landlordmanagement/UpdateProfile',
                data: formData,
                type: 'POST',
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data == true) {
                        $('.management-main-content').prepend('' +
                            '<div class="alert-success temp-alert">Profile Updated Successfully</div>');
                    } else {
                        $('.management-main-content').prepend('' +
                            '<div class="alert-warning temp-alert">A error was encountered while updating your profile</div>');
                    }

                    $('.temp-alert').fadeOut(10000);
                },
                error: function () {
                    alert('An error occurred while loading view');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();

                    $('input').attr('disabled', true);
                    $('#updateProfileBtn').addClass('disabled');
                    $('#edit-profile-btn').text('Edit');
                }
            });
        }
    });

    $(document.body).on('change', '#subscription-period', function () {
        var period = $(this).val();
        var price = $('#monthlyCost').text();
        var total = price * period;

        $('#sub-ext-price-cal').html('Price | &#36; ' + total + '.00' + ' ');
        $('#sub-ext-price-cal').append('<button id="make-payment" class="btn btn-sm btn-outline-success">Make Payment</button>');
    });

    $(document.body).on('click', '#make-payment', function (event) {
        event.preventDefault();

        var subscriptionID = $('#subscriptionID').val();
        var subPeriod = $('#subscription-period').val();
        var period = (subPeriod == '' || subPeriod == undefined) ? 1 : $('#subscription-period').val();
        var price = $('#monthlyCost').text();
        var total = price * period;

        //if an subscription expiry date exists then the subscription should be extended with this payment
        var isExtension = $('#expiry-date').length ? true : false;

        $.ajax({
            url: '/landlordmanagement/GetModalMakePayment',
            type: 'Get',
            data: { Period: period, Amount: total, SubscriptionID: subscriptionID, isExtension: isExtension },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function (data) {
                $('#paymentModalContainer').remove();
                $('#modal-displays').append(data);

                sys.showModal('#paymentModal');

                //initialization of bootstrap popover
                $('[data-toggle="popover"]').popover({
                    container: 'body'
                });
            },
            error: function () {
                alert('An error occurred while retrieving property\'s information');
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }
        });
    });

    $(document.body).on('change', '#PaymentMethodID', function (event) {
        var method = $(this).val();

        $('#soon-feature').remove();

        if (method == 'D' || method == 'F') {//D - Digicel | F - Flow
            $('#soon-feature').remove();
            $('#voucher').slideToggle();
            $('#submit-payment').removeAttr('disabled');
            $('#submit-payment').removeAttr('tabindex');
            $('#submit-payment').removeClass('disabled');
        } else {
            if (!$('#submit-payment').hasClass('disabled')) {

                $('#voucher').slideToggle();
                $('#submit-payment').attr('disabled', true);
                $('#submit-payment').attr('tabindex', -1);
                $('#submit-payment').addClass('disabled');
            }

            $('.payment-method').append('<center id="soon-feature">Feature Coming Soon</center>');
        }
    });

    $(document.body).on('click', '#extend-subscription', function (event) {
        event.preventDefault();

        $('.extend-subsciption-container').slideToggle();

        if (!$(this).hasClass('active')) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });

    //ensures that only numbers are entered into the voucherNumber input
    $(document.body).on('keypress', '#voucherNumber', function (e) {
        //if the letter is not digit then display error and don't type anything
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }
    });

    //prevent pasting within the voucherNumber field
    //TODO allow pasting only if it is numbers
    $(document.body).on('paste', '#voucherNumber', function (e) {
        e.preventDefault();
    });

    $(document.body).on('click', '#submit-payment', function (event) {
        event.preventDefault();
        /*
        var subscriptionID = $('#subscriptionID').val();
        var period = $('#period').val();
        var total = $('#amount').val();*/
        var voucherNumber = $('#voucherNumber').val();

        var form = $('#form-make-payment');

        var validator = form.validate();

        var isValid = validator.form();

        if (isValid) {
            var formData = new FormData($('#form-make-payment')[0]);
            $.ajax({
                url: '/landlordmanagement/makepayment',
                type: 'post',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != '' && data != null && data == true) {
                        $('.modal-body').prepend('' +
                            '<div class="alert-success temp-alert">Your payment has been submitted for verification. ' +
                            'An email was sent to your email address with additional information</div > ')
                            .hide()
                            .fadeIn('slow');
                    } else {
                        $('.modal-body').prepend('' +
                            '<div class="alert-danger temp-alert">Payment submission failed - Contact Administrator</div> ')
                            .hide()
                            .fadeIn('slow');
                    }

                },
                error: function () {
                    alert('An error occurred while submiting your payment');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                    $('.temp-alert').fadeOut(100000);
                    $('#voucherNumber').val('');
                }
            });
        }
    });

    $(document.body).on('click', '.page-link', function (event) {
        event.preventDefault();

        var direction = $(this).attr('id');
        var pgNo = Number($('#pgNo').val());

        if (direction == 'next') {
            pgNo += 1;
        } else {
            pgNo -= 1;
        }

        loadPaymentsView(pgNo);
    });

    $('.verify').click(function (event) {
        event.preventDefault();

        var element = $(this);
        var paymentID = $(this).attr('id');

        if (confirm("Are you sure ?")) {
            $.ajax({
                url: '/admin/VerifyPayment',
                type: 'Post',
                data: { PaymentID: paymentID },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != null && data != '' && !data.hasErrors) {
                        var child = element.children();
                        child.removeClass('badge-danger');
                        child.addClass('badge-primary');
                        child.text('Verified');

                        var html = '<div class="badgeMsg badge badge-success" style="width:100%;">Payment Verified </div>';
                        $(html).insertAfter('.ctnr-headers').fadeOut().fadeIn(1000, function () {
                            $('.badgeMsg').fadeOut(10000);
                        });
                    } else {
                        $.each(data.ErrorMessages, function (idx, val) {
                            var html = '<div class="badgeMsg badge badge-danger" style="width:100%;">Error Occurred <br/>' + val + '</div>';
                            $(html).insertAfter('.ctnr-headers').fadeOut().fadeIn(1000, function () {
                                $('.badgeMsg').fadeOut(10000);
                            });
                        });
                    }
                },
                error: function () {
                    alert('An error occurred while verifying payment');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                }
            });
        }
    });

    $(document.body).on('click', '.remove-saved-prop', function (event) {
        event.preventDefault();

        var propID = $(this).attr('id');

        if (confirm("Are you sure ?")) {
            $.ajax({
                url: '/landlordmanagement/RemoveSavedProperty',
                type: 'Delete',
                data: { propertyID: propID },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                success: function (data) {
                    if (data != '' && data != null && data == true) {
                        loadPropertiesView('saved-properties');
                    } else {
                        alert('An error occurred while removing saved property');
                    }
                },
                error: function () {
                    alert('An error occurred while removing saved property');
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                }
            });
        }
    });

    $(document).on('click', '.property-availability-badge', function (event) {
        event.preventDefault();

        var propertyID = $(this).attr('id');

        if (confirm("Are you sure ?")) {
            $.ajax({
                url: '/landlordmanagement/TogglePropertyAvailability',
                type: 'put',
                data: { propertyID: propertyID },
                success: function (data) {
                    if (data.hasErrors == false) {
                        loadPropertiesView('owned-properties');
                    } else {
                        $.each(data.ErrorMessages, function (inx, val) {
                            var html = '<div class="badge badge-danger" style="width:100%;">' + val + '</div> <br/>';
                            $(html).insertAfter('.ctnr-headers').hide().fadeIn(1000, function () {
                                $(this).fadeOut(10000);
                            });
                        });
                    }
                },
                error: function () {
                    alert('Error occurred while saving your liked property, contact system administrator');
                }
            });
        }
    });

    $(document).on('click', '.remove-prop', function (event) {
        event.preventDefault();

        var propertyID = $(this).attr('id');

        if (confirm("Are you sure ?")) {
            $.ajax({
                url: '/landlordmanagement/RemoveProperty',
                type: 'delete',
                data: { propertyID: propertyID },
                success: function (data) {
                    if (data == true) {
                        loadPropertiesView('owned-properties');
                    } else {
                        alert('Error occurred while removing your property, contact system administrator');
                    }
                },
                error: function () {
                    alert('Error occurred while saving your liked property, contact system administrator');
                }
            });
        }
    });

    $(document.body).on('click', '#change-subscription-modal-btn', function (event) {
        event.preventDefault();
        var subscriptionID = $('#subscriptionID').val();

        $.ajax({
            url: '/landlordmanagement/GetModalSubscriptionChange',
            type: 'Get',
            data: { subscriptionID: subscriptionID },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            success: function (data) {
                $('#subscriptionChangeModal').remove();
                $('#modal-displays').append(data);

                sys.showModal('#subscriptionChangeModal');

                var currentSubName = $('#activeSubscriptionName').val();
                var clickableBox = $('.clickable-box' + '#' + currentSubName).parent();
                clickableBox.remove();
                //initialization of bootstrap popover
                $('[data-toggle="popover"]').popover({
                    trigger: 'hover focus',
                    placement: 'top',
                    container: 'body'
                });
            },
            error: function () {
                alert('An error occurred while retrieving data');
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }
        });
    });

    $(document).on('click', '#change-subscription-btn', function (event) {
        event.preventDefault();

        var newSubscriptionType = lastSelectedClickableSubscriptionType.attr('id');
        var subscriptionID = $('#subscriptionID').val();
        var period = null;

        if ($('#subscription-period').length)
            period = $('#subscription-period').val();

        if (newSubscriptionType != null) {
            if (confirm("Are you sure ?")) {
                $.ajax({
                    url: '/landlordmanagement/changeSubscription',
                    type: 'post',
                    data: { subscriptionId: subscriptionID, subscriptionType: newSubscriptionType, period: period },
                    success: function (data) {
                        if (data.HasMessage) {
                            $.each(data.ReturnedMessages, function (inx, val) {
                                var html = '<div class="badge badge-success" style="width:100%;">' + val + '</div> <br/>';
                                $(html).prependTo('.subscription-container-main').hide().fadeIn(1000, function () {
                                    $(this).fadeOut(10000, function () {
                                        loadSubscriptionView();
                                    });
                                });
                            });
                        } else if (data.HasErrors) {
                            $.each(data.ErrorMessages, function (inx, val) {
                                var html = '<div class="badge badge-danger" style="width:100%;">' + val + '</div> <br/>';
                                $(html).prependTo('.subscription-container-main').hide().fadeIn(1000, function () {
                                    $(this).fadeOut(90000, function () {
                                        loadSubscriptionView();
                                    });
                                });
                            });
                        }
                    },
                    error: function () {
                        alert('Error occurred while changing your subscription, contact system administrator');
                    },
                    complete: function () {
                        $('.modal-body').html('');
                        $('.modal-footer').html('');
                        $('.close').click();
                    }
                });
            }
        } else {
            alert('Select a subscription type first');
        }
    });

    $(document.body).on('click', '#subscription-type .clickable-box', function (event) {
        event.preventDefault();
        var currentSubCost = $('#activeSubscriptionCost').val();
        var clickedSubCost = $(this).find(".monthly-cost").html();

        $('#subscription-period-chg option[value="0"]').prop('selected', true);
        $('#subscription-period-chg').prop('disabled', true);
        $('#chg-subscription-period').prop('checked', false);

        if (clickedSubCost > currentSubCost)
            $('#chg-sub-fc').removeClass('hide');
        else
            $('#chg-sub-fc').addClass('hide');

        $('#clickedSubscriptionCost').html(clickedSubCost);

        computeSubChgPrice();
    });

    $(document.body).on('change', '#chg-subscription-period', function (event) {

        var isChecked = $(this).prop('checked');

        if (isChecked)
            $('#subscription-period-chg').prop('disabled', false);
        else {
            $('#subscription-period-chg').prop('disabled', true);
            computeSubChgPrice();
        }
    });

    $(document.body).on('change', '#subscription-period-chg', function (event) {
        computeSubChgPrice();
    });

    /* $(window).resize(function () {
         var position = $('.management-action .active').position();
       //  pointToSelectedAction(position);
     });*/

});