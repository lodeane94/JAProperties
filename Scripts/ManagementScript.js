﻿//loads properties in the dashboard page
$(document).ready(function () {
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
        var hasAccommodation = $('#hasAccommodation').val();
        var hasHouse = $('#hasHouse').val();
        var hasLand = $('#hasLand').val();

        $.ajax({
            url: '/Landlordmanagement/getAllPropertyImages',
            type: 'GET',
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $.each(data, function (index, value) {
                        $('#rooms-collection')
                            .append('<a class="property-image" href="' + value.ID + '"><img title="click to manage property" id="' + value.ID + '" src="/Uploads/' + value.ImageURL + '"/></a>');
                    });
                }
            },
            beforeSend: function () {
                $('#modal-loading').fadeIn();
            },
            complete: function () {
                $('#modal-loading').fadeOut();
            }

        });

        if (hasAccommodation != 0) {
            $.ajax({
                url: '/Landlordmanagement/getRequisitions',
                type: 'GET',
                success: function (data) {
                    if (!$.isEmptyObject(data)) {
                        var count = 1;
                        var countHistory = 1;

                        $('#dashboard-requisitions')
                                .append('<table id="tblRequisitions" class="table table-condensed table-striped">'
                                + '<tr><th></th><th>First Name</th><th>Last Name</th><th>Gender</th><th>Email</th>'
                                + '<th>Cell</th><th>Date</th><th>&nbsp;</th><th>&nbsp;</th></tr>');

                        $('#dashboard-requisitions-history')
                                           .append('<table id="tblRequisitionsHistory" class="table table-condensed table-striped">'
                                           + '<tr><th></th><th>First Name</th><th>Last Name</th><th>Gender</th><th>Email</th>'
                                           + '<th>Cell</th><th>Date</th><th></th><th></th></tr>');
                        $.each(data, function (index, value) {
                            //if the requisition is accepted, do not display that record
                            if (!value.accepted) {
                                $('#dashboard-requisitions tr:last').after('<tr>'
                                + '<td class="property_image">' + '<img style="width:100px;height:100px" src="' + '/Uploads/' + value.Image_URL + '"/>' + '</td>'
                                + '<td class="first_name">' + value.FirstName + '</td>'
                                + '<td class="last_name">' + value.LastName + '</td>'
                                + '<td class="gender">' + value.Gender + '</td>'
                                + '<td class="email">' + value.Email + '</td>'
                                + '<td class="cell">' + value.Cell + '</td>'
                                + '<td class="date">' + value.Date + '</td>'
                                + '<td class="acc_id"><input type="hidden" value="' + value.ID + '"/></td>'
                                + '<td><input class="btnAcceptRequest btn btn-primary" type="button" value="Accept"></td>'
                                + '<td><input class="btnDenyRequest btn btn-danger" type="button" value="Deny"></td>'
                                + '</tr>'
                                + '</table>');
                            } else {
                                $('#dashboard-requisitions-history tr:last').after('<tr>'
                                            + '<td class="property_image">' + '<img style="width:100px;height:100px" src="' + '/Uploads/' + value.Image_URL + '"/>' + '</td>'
                                            + '<td class="first_name">' + value.FirstName + '</td>'
                                            + '<td class="last_name">' + value.LastName + '</td>'
                                            + '<td class="gender">' + value.Gender + '</td>'
                                            + '<td class="email">' + value.Email + '</td>'
                                            + '<td class="cell">' + value.Cell + '</td>'
                                            + '<td class="date">' + value.Date + '</td>'
                                            + '<td class="acc_id"><input type="hidden" value="' + value.ID + '"/></td>'
                                            + '</tr>'
                                            + '</table>');
                            }
                        });
                    } else {
                        //shows complaints and requisitions if accommodations was found
                        $('#d-complaints').hide();
                        $('#d-requisitions').hide();
                    }
                },
                beforeSend: function () {
                    $('#modal-loading').fadeIn();
                },
                complete: function () {
                    $('#modal-loading').fadeOut();
                }
            });
            $.ajax({
                url: '/Landlordmanagement/getLatestMessages',
                type: 'GET',
                success: function (data) {
                    if (!$.isEmptyObject(data)) {
                        $.each(data, function (index, value) {
                            $('#dashboard-messages')
                                .append('<li><a id="' + value.MessageID + '"><strong>' + value.MessengerName + '</strong> <em>' + value.Message + '</em></a></li>');
                        });
                    } else {
                        $('#dashboard-messages')
                                .append('<li>No Messages Found</li>');
                    }
                }
            });
        }

    })();

    //generates modal to compose a new message
    $('#new-message').click(function (event) {
        event.preventDefault();
        //displays the modal whenever this is selected
        sys.showModal('#propertyModal');

        $('#action-header').html('<span class="glyphicon glyphicon-pencil"></span> Compose New Message');
        /*
        $('#action-body').html('<form action="landlordmanagement/composenewmessage" method="post"><strong>To</strong> <input type="text" id="recipient" class="form-control" placeholder="Recipients Name"><br/>'
                              + '<strong>Message</strong> <textarea class="form-control" placeholder="Compose new message" rows="4" cols="30"></textarea></form>');
        $('form').append('<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button>'
                                + '<input class="btn btn-primary" type="submit" value="Send Message" id="submit-message" /></div>');
                                */
        $('#action-body').html('<h3>This feature is coming soon......</h3>');
    });
    //generates modal to compose a new message and broadcast to every tennant
    $('#broadcast-message').click(function (event) {
        event.preventDefault();
        //displays the modal whenever this is selected
        sys.showModal('#propertyModal');

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
        sys.showModal('#propertyModal');

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
        sys.showModal('#propertyModal');

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
        sys.showModal('#propertyModal');

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
        sys.showModal('#propertyModal');

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

                            $('#action-body #property-information ').append('<tr><td>Price</td><<td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td></tr>'
                                                                            + '<tr><td>Bedroom Amount</td><<td><input  class="form-control" type="text" name="bedroomAmount" value="' + value.BedroomAmount + '"></td></tr>'
                                                                            + '<tr><td>Bathroom Amount</td><<td><input  class="form-control" type="text" name="bathroomAmount" value="' + value.BathroomAmount + '"></td></tr>'
                                                                            + '<tr><td>Purpose</td><<td><select class="form-control" id="purpose" name="purpose"><option value="Rent">Rent</option><option value="Sale">Sale</option></select></td></tr>'
                                                                            + '<tr><td>Is Room Furnished</td><<td><select class="form-control" id="isFurnished" name="isFurnished"><option value="True">Yes</option><option value="False">No</option></select></td></tr>'
                                                                            + '<tr><td>Description</td><td><textarea class="form-control" rows="6" cols="35" name="description">' + value.Description + '</textarea></td></tr>'
                                                                            + '<tr><td><input type="hidden" name="id" id="id" value="' + value.ID + '"/></td></tr></table></form>');

                            value.Purpose == 'Rent' ? $('#purpose option[value="Rent"]').attr('selected', 'selected') : $('#purpose option[value="Sale"]').attr('selected', 'selected');
                            value.isFurnished == 'true' ? $('#isFurnished option[value="true"]').attr('selected', 'selected') : $('#isFurnished option[value="false"]').attr('selected', 'selected');
                        }
                        else
                            if (value.Area !== undefined) {

                                $('#action-body').html('<input type="button" class="btn btn-danger btn-block" id="removeProperty" value="Remove Property"/><br/><br/><img style="width:480px;height:300px;"  id="' + value.ID + '" src="/Uploads/' + value.ImageURL + '"/><hr/><form action="/landlordmanagement/updateland" method="post"><table class="table-condensed" id="property-information">');

                                $('#action-body #property-information ').append('<tr><td>Price</td><<td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td></tr>'
                                                                                + '<tr><td>Purpose</td><<td><select class="form-control" id="purpose" name="purpose"><option value="Rent">Rent</option><option value="Sale">Sale</option><option value="Lease">Lease</option></select></td></tr>'
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

                                $('#action-body #property-information ').append('<tr><td>Price</td><td><input  class="form-control" type="text" name="price" value="' + value.Price + '"></td></tr>'
                                + '<tr><td>Occupancy</td><td><input  class="form-control" type="text" name="occupancy" value="' + value.Occupancy + '"></td></tr>'
                                + '<tr><td>Security Deposit</td><td><input class="form-control" type="text" name="security_deposit" value="' + value.SecurityDeposit + '"></td></tr>'
                                + '<tr><td>Water</td><td><select id="water" name="water" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Internet</td><td><select id="internet" name="internet" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Electricity</td><td><select id="electricity" name="electricity" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Cable</td><td><select id="cable" name="cable" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Gas</td><td><select id="gas" name="gas" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Availability</td><td><select id="availability" name="availability" class="form-control"><option value="true">Yes</option><option value="false">No</option></select></td></tr>'
                                + '<tr><td>Gender Preference</td><td><select class="form-control" id="gender_preference" name="gender_preference" class="form-control"><option value="B">Both</option><option value="M">Male</option><option value="F">Female</option></select></td></tr>'
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

        //displays the modal whenever an image is selected
        sys.showModal('#propertyModal');

        $('#action-header').html('<span class="glyphicon glyphicon-plus"></span> Add Property');

        $('#action-body').html('<table id="properties"><tr><td>Select Property Type</td><td><select class="form-control" id="property-type"><option disabled selected>Property Type</option><option value="room">Room</option><option value="house">House</option><option value="land">Land</option></select></td><td><img id="loader" style="display:none;width:50px;height:50px" src="/content/ajax-loader-dark.gif"/></td></tr></table><div id="property-registration-container">');
    });
    //sends acceptance mail to requestee 
    $(document.body).on('click', '.btnAcceptRequest', function (event) {
        var reqID = $(this).parent().prevAll(".acc_id").children().val();
        var firstName = $(this).parent().prevAll('.first_name').text();
        var lastName = $(this).parent().prevAll('.last_name').text();
        var gender = $(this).parent().prevAll('.gender').text();
        var emailAddress = $(this).parent().prevAll('.email').text();
        var cell = $(this).parent().prevAll('.cell').text();
        var requestInfo = { 'firstName': firstName, 'lastName': lastName, 'gender': gender, 'email': emailAddress, 'cell': cell };

        $.ajax({
            url: '/landlordmanagement/acceptrequest',
            type: 'POST',
            data: { 'reqID': reqID, 'requestInfo': requestInfo },
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
        var reqID = $(this).parent().prevAll(".acc_id").children().val();
        var cell = $(this).parent().prevAll('.cell').text();
        var emailAddress = $(this).parent().prevAll('.email').text();

        $.ajax({
            url: '/landlordmanagement/cancelrequest',
            type: 'POST',
            data: { 'reqID': reqID, 'cell': cell, 'email': emailAddress },
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


});