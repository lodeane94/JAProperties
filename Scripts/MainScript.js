﻿
var asideLinkID = null;
var isFiltered = false;
var parishFilterValue = '';
var accommodationPg = 0;
var housePg = 0;
var landPg = 0;

//sets the current page that the user is on
$(function () {
    var currentPageNumber = $('#currentPageNumber').val();

    $('.pageNumber').each(function (index, val) {
        if ($(this).text() == currentPageNumber) {
            $(this).parent().attr('class', 'active');
        }
    });
});
//displays previous and next signs depending on the number of properties returned
$(function () {
    var noOfPages = $('#noOfPages').val();
    var currentPageNumber = $('#currentPageNumber').val();

    if (currentPageNumber == 1 && noOfPages > 1) {
        $('.next-page').show();
    }

    if (currentPageNumber == noOfPages && noOfPages > 1) {
        $('.previous-page').show();
    }

    if (currentPageNumber != 1 && currentPageNumber != noOfPages && noOfPages > 1) {
        $('.next-page').show();
        $('.previous-page').show();
    }

});
//loads new dynamic information for all properties
function loadPropertiesHTML(data, propertyPagination) {
    var tagName = "";
    if (propertyPagination == null) {
        //clears anything that were in tags if the request is not for pagination 
        $('.rooms-load').empty();
        $('.house-load').empty();
        $('.land-load').empty();
    } else {//TODO remove hard coding to check if the returned data is null
        switch (propertyPagination) {
            case 'accommodations':
                if (data[0].length > 0)
                    $('.rooms-load').empty();
                break;
            case 'house':
                if (data[1].length > 0)
                    $('.house-load').empty();
                break;
            case 'land':
                if (data[2].length > 0)
                    $('.land-load').empty();
                break;
        }
    }
    //used to display location that was selected
    if ($.isEmptyObject(asideLinkID)) {
        $('.property-location').html('ALL-PARISHES');
    } else {
        $('.property-location').html(asideLinkID.toUpperCase());
    }

    for (var count = 0; count < data.length; count++) {
        for (var count2 = 0; count2 < data[count].length; count2++) {
            //using variables tied to each property to determine if their values are set
            if ((data[count][count2].Occupancy != "" && data[count][count2].Occupancy != undefined) || propertyPagination == 'accommodations') {
                tagName = 'rooms-load';
            } else if ((data[count][count2].BedroomAmount != "" && data[count][count2].BedroomAmount != undefined) || propertyPagination == 'houses') {
                tagName = 'house-load';
            } else if ((data[count][count2].Area != "" && data[count][count2].Area != undefined) || propertyPagination == 'lands') {
                tagName = 'land-load';
            }
            //TODO show message if no property is found 
            /*
            if (data.length < 1) {
                $('.' + tagName).append('<div class="col-md-12"><div class="label-warning">NO PROPERTY WAS FOUND IN THIS PARISH</div></div>');
            }*/
            // if ((data[count][count2].Occupancy != "" && data[count][count2].Occupancy != undefined) || propertyPagination == 'accommodations') {
            //alert('room not null');
            $('.' + tagName).append('<a class="image-link" data-toggle="popover" data-content="No Content" title="Click to view more information" href="' + data[count][count2].ID + '">'
                        + '<div class="col-md-3">'
                            + '<div class="image-container">'
                             + '<img id="' + data[count][count2].ID + '" class="room-images" src="/Uploads/' + data[count][count2].ImageURL + '">'
                             + '</a>'
                            + '<div class="image-information-container">'
                                + '<ul>'
                                    + '<li><label> Address: ' + data[count][count2].StreetAddress + '</label></li>'
                                    + '<li><label> Parish: ' + data[count][count2].Parish + '</label></li>'
                                    + '<li><label> Cost: $' + data[count][count2].Price + ' JMD </label></li>'
                                + '<ul>'
                            + '</div>'
                            + '<a href="" class="btn btn-warning btn-block">Show Details <span class="caret"></span></a>'
                            + '</div>'
                     + '</div>'
                     );
            /*} else if ((data[count][count2].BedroomAmount != "" && data[count][count2].BedroomAmount != undefined) || propertyPagination == 'houses') {
                // alert('House not null');
                $('.' + "house-load").append('<a class="image-link" data-toggle="popover" data-content="No Content" title="Click to view more information" href="' + data[count][count2].ID + '">'
                            + '<div class="col-md-3">'
                                + '<div class="image-container">'
                                 + '<img id="' + data[count][count2].ID + '" class="room-images" src="/Uploads/' + data[count][count2].ImageURL + '">'
                                + '</div>'
                                + '<div class="image-information-container">'
                                    + '<table width="100%" class="table-condensed table-striped">'
                                        + '<tbody>'
                                            + '<tr>'
                                                + '<td>Address</td>'
                                                + '<td>' + data[count][count2].StreetAddress + '</td>'
                                            + '</tr>'
                                            + '<tr>'
                                                + '<td>Parish</td>'
                                                + '<td>' + data[count][count2].Parish + '</td>'
                                            + '</tr>'
                                            + '<tr>'
                                                + '<td>Price</td>'
                                                + '<td>' + data[count][count2].Price + '</td>'
                                            + '</tr>'
                                        + '</tbody>'
                                    + '</table>'
                                + '</div>'
                         + '</div>'
                         + '</a>');
            } else if ((data[count][count2].Area != "" && data[count][count2].Area != undefined) || propertyPagination == 'lands') {
                // alert('land not null');
                $('.' + "land-load").append('<a class="image-link" data-toggle="popover" data-content="No Content" title="Click to view more information" href="' + data[count][count2].ID + '">'
                            + '<div class="col-md-3">'
                                + '<div class="image-container">'
                                 + '<img id="' + data[count][count2].ID + '" class="room-images" src="/Uploads/' + data[count][count2].ImageURL + '">'
                                + '</div>'
                                + '<div class="image-information-container">'
                                    + '<table width="100%" class="table-condensed table-striped">'
                                        + '<tbody>'
                                            + '<tr>'
                                                + '<td>Address</td>'
                                                + '<td>' + data[count][count2].StreetAddress + '</td>'
                                            + '</tr>'
                                            + '<tr>'
                                                + '<td>Parish</td>'
                                                + '<td>' + data[count][count2].Parish + '</td>'
                                            + '</tr>'
                                            + '<tr>'
                                                + '<td>Price</td>'
                                                + '<td>' + data[count][count2].Price + '</td>'
                                            + '</tr>'
                                        + '</tbody>'
                                    + '</table>'
                                + '</div>'
                         + '</div>'
                         + '</a>');
            }*/
        }
    }
}
/*
//loads new dynamic information for filtered properties
function loadFilteredPropertiesHTML(data, tagName) {
    //clears anything that were in tags
    $(tagName).empty();

    for (var count = 0; count < data.length; count++) {
        //checking if object is empty
        if ($.isEmptyObject(data)) {
            $(tagName).append('<div class="col-md-12"><div class="label-warning">NO ITEM WAS FOUND</div></div>');
        }

        $(tagName).append('<a class="image-link" href="' + data[count].id + '">'
                    + '<div class="col-md-3">'
                        + '<div class="image-container">'
                         + '<img id="' + data[count].id + '" class="room-images" src="/Uploads/' + data[count].image_url + '">'
                        + '</div>'
                        + '<div class="image-information-container">'
                            + '<table width="100%" class="table-condensed table-striped">'
                                + '<tbody>'
                                    + '<tr>'
                                        + '<td>Address</td>'
                                        + '<td>' + data[count].street_address + '</td>'
                                    + '</tr>'
                                    + '<tr>'
                                        + '<td>Parish</td>'
                                        + '<td>' + data[count].parish + '</td>'
                                    + '</tr>'
                                    + '<tr>'
                                        + '<td>Price</td>'
                                        + '<td>' + data[count].price + '</td>'
                                    + '</tr>'
                                + '</tbody>'
                            + '</table>'
                        + '</div>'
                 + '</div>'
                 + '</a>');
    }
}*/
//loads the properties for the rooms,houses and lands
function getProperties(parish) {
    var data = null;
    //clears all other filters if selected
    if (parish == null) {
        $('.aside-property a').removeAttr('class', 'active');
        $('#all-parishes').attr('class', 'active')
        data = { 'fetchAmount': 4, 'parish': "", 'isPagination': false, 'propertyType': "", 'pgNo': 0 };
    } else {
        data = { 'fetchAmount': 4, 'parish': parish.trim(), 'isPagination': false, 'propertyType': "", 'pgNo': 0 };
    }

    $.ajax({
        url: '/home/getProperties',
        type: 'Get',
        data: data,
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        },
        error: function () {
            alert('An error occurred');
        }

    }).success(function (data) {
        loadPropertiesHTML(data);
    });
}
/*
//loads filters properties
function getFilteredProperties(parish) {

    $.ajax({
        url: '/home/getFilteredProperties',
        type: 'Get',
        data: { 'fetchAmount': 4, 'parish': parish.trim() },
        beforeSend: function () {
            $('#modal-loading').fadeIn();
        },
        complete: function () {
            $('#modal-loading').fadeOut();
        },
        error: function () {
            alert('An error occurred');
        }
    }).success(function (data) {
        loadPropertiesHTML(data);
    });
}*/

//storing files on change of the input
var files;
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

    //sets the menu to fixed after scrolling pass a certain amount of pixels
    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('.header-content').addClass('fixed');
        } else {
            $('.header-content').removeClass('fixed');
        }
    });

    //modal display for selected house
    $(document.body).on('click', '.house-load .image-link', function (event) {
        event.preventDefault();
        //getting id selected
        var imageSelectedID = $(this).attr('href');
        //displays the modal whenever an image is selected
        sys.showModal('#propertyModal');
        //send id of image to the getSelected action in the rooms controller
        $.ajax({
            url: '/Properties/RetrieveSelectedHouse',
            type: 'get',
            data: { 'property_id': imageSelectedID },
            beforeSend: function () {
                $('.ajax-load').show();
            },
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $.each(data, function (index, value) {
                        //setting the modal image with this same id
                        $('.modal-image').attr('id', imageSelectedID);
                        //setting modal image source with the returned image location
                        $('.modal-image').attr('src', '/Uploads/' + value.ImageURL);
                        //setting accommodation id for form to be sent
                        $('#propertyID').attr('value', imageSelectedID);

                        //dynamically adding the data for the property
                        $('#image-details').html('<table class="properties-table-style"><tr><th colspan="8">House For Sale/Rent</th></tr><tr><th>Street Address</th><th>City</th><th>Parish</th><th>Price</th><th>Bedrooms</th><th>Bathrooms</th><th>Purpose</th><th>Furnished</th></tr>'
                                                + '<tr><td>' + value.StreetAddress + '</td><td>' + value.City + '</td><td>' + value.Parish + '</td><td>' + value.Price + '</td><td>' + value.BedroomAmount + '</td><td>' + value.BathroomAmount + '</td><td>' + value.Purpose + '</td><td>' + value.isFurnished + '</td></tr>'
                                                + '<tr><th colspan="14">Property Description</th></tr><tr><td colspan="14">' + value.Description + '</td></tr><tr><th colspan="14">Property Owner Information</th></tr><tr><th>Name</th><th>Gender</th><th>Cellphone Number</th><th>Email Address</th></tr>'
                                                + '<tr><td>' + value.ownerModel.FirstName + ' ' + value.ownerModel.LastName + '</td><td>' + value.ownerModel.Gender + '</td><td>' + value.ownerModel.Cell + '</td><td>' + value.ownerModel.Email + '</td></tr></table>');
                    });
                }
            },
            complete: function () {
                $('.ajax-load').hide();
            },
            error: function () {
                alert('An error occurred');
            }

        });
    });
    //modal display for selected land
    $(document.body).on('click', '.land-load .image-link', function (event) {
        event.preventDefault();
        //getting id selected
        var imageSelectedID = $(this).attr('href');
        //displays the modal whenever an image is selected
        sys.showModal('#propertyModal');
        //send id of image to the getSelected action in the rooms controller
        $.ajax({
            url: '/Properties/RetrieveSelectedLand',
            type: 'get',
            data: { 'property_id': imageSelectedID },
            beforeSend: function () {
                $('.ajax-load').show();
            },
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $.each(data, function (index, value) {
                        //setting the modal image with this same id
                        $('.modal-image').attr('id', imageSelectedID);
                        //setting modal image source with the returned image location
                        $('.modal-image').attr('src', '/Uploads/' + value.ImageURL);
                        //setting accommodation id for form to be sent
                        $('#propertyID').attr('value', imageSelectedID);

                        //dynamically adding the data for the property
                        $('#image-details').html('<table class="properties-table-style"><tr><th colspan="8">Land For Sale/Rent/Lease</th></tr><tr><th>Street Address</th><th>City</th><th>Parish</th><th>Price</th><th>Purpose</th><th>Area</th></tr>'
                                                + '<tr><td>' + value.StreetAddress + '</td><td>' + value.City + '</td><td>' + value.Parish + '</td><td>' + value.Price + '</td><td>' + value.Purpose + '</td><td>' + value.Area + '</td></tr>'
                                                + '<tr><th colspan="14">Property Description</th></tr><tr><td colspan="14">' + value.Description + '</td></tr><tr><th colspan="14">Property Owner Information</th></tr><tr><th>Name</th><th>Gender</th><th>Cellphone Number</th><th>Email Address</th></tr>'
                                                + '<tr><td>' + value.ownerModel.FirstName + ' ' + value.ownerModel.LastName + '</td><td>' + value.ownerModel.Gender + '</td><td>' + value.ownerModel.Cell + '</td><td>' + value.ownerModel.Email + '</td></tr></table>');
                    });
                }
            },
            complete: function () {
                $('.ajax-load').hide();
            },
            error: function () {
                alert('An error occurred');
            }

        });
    });
    //modal display for selected room
    $(document.body).on('click', '.rooms-load .image-link', function (event) {
        event.preventDefault();
        //getting id selected
        var imageSelectedID = $(this).attr('href');
        //displays the modal whenever an image is selected
        sys.showModal('#propertyModal');
        //send id of image to the getSelected action in the rooms controller
        $.ajax({
            url: '/Properties/RetrieveSelectedAccommodation',
            type: 'get',
            data: { 'property_id': imageSelectedID },
            beforeSend: function () {
                $('.ajax-load').show();
            },
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $.each(data, function (index, value) {
                        //setting the modal image with this same id
                        $('.modal-image').attr('id', imageSelectedID);
                        //setting modal image source with the returned image location
                        $('.modal-image').attr('src', '/Uploads/' + value.ImageURL);
                        //setting accommodation id for form to be sent
                        $('#propertyID').attr('value', imageSelectedID);

                        //dynamically adding the data for the property
                        $('#image-details').html('<table class="properties-table-style">'
                            + '<tr><th>Location</th><th>Pricing</th><th>Other Details</th></tr>'
                            + '<tr><td>Address: ' + value.StreetAddress + '</td><td>Monthly Rent: ' + value.Price + '</td><td>Occupancy: ' + value.Occupancy + '</td></tr>'
                            + '<tr><td>City: ' + value.City + '</td><td>Security Deposit: ' + value.SecurityDeposit + '</td><td>Gender Preference: ' + value.GenderPreference + '</td></tr>'
                            + '<tr><td>Parish: ' + value.Parish + '</td><td>&nbsp;</td>'
                            + '<td><table><tr><th>Availability</th><th>Bathrooms</th><th>Water</th><th>Electricity</th><th>Internet</th><th>Cable</th><th>Gas</th></tr>'
                            + '<tr><td>' + value.Availability + '</td><td>' + value.BathroomAmount + '</td><td>' + value.Water + '</td><td>' + value.Electricity + '</td><td>' + value.Internet + '</td><td>' + value.Cable + '</td><td>' + value.Gas + '</td></tr></table></td></tr>'
                            + '</table>');
                    });
                }
            },
            complete: function () {
                $('.ajax-load').hide();
            },
            error: function () {
                alert('An error occurred');
            }

        });
    });
    //function to filter rooms
    function filterProperties(filterName, filterCookieName, filterValue, filterOffRegex, filterOnRegex) {
        var queryString = $.cookie('queryString');
        //if filtername is gender or occupancy or bathrooms, use this method
        if (filterName == 'bedrooms' || filterName == 'purpose' || filterName == 'gender' || filterName == 'occupancy' || filterName == 'bathrooms') {
            if (filterValue == 'no_filter') {
                queryString = queryString.replace(filterOffRegex, '');
                $.cookie(filterCookieName, null);
                $.cookie('queryString', queryString);
            }
            else {
                $.cookie(filterCookieName, filterValue);

                if (queryString != 'null' && queryString.indexOf(filterName) == -1) {
                    $.cookie('queryString', queryString + '&' + filterName + '=' + filterValue);
                }
                else {
                    if (queryString == 'null' || queryString === undefined) {
                        $.cookie('queryString', window.location.pathname + '?' + filterName + '=' + filterValue);
                    } else {
                        $.cookie('queryString', queryString.replace(filterOnRegex, filterName + '=' + filterValue));
                    }
                }
            }
        } else if (filterName == 'isFurnished' || filterName == 'isStudent' || filterName == 'hasWater' || filterName == 'hasCable' || filterName == 'hasElectricity' || filterName == 'hasGas' || filterName == 'hasInternet') {
            //if filtername is any of the checked filter values
            if (!filterValue) {
                queryString = queryString.replace(filterOffRegex, '');
                $.cookie(filterCookieName, null);
                $.cookie('queryString', queryString);
            }
            else {
                $.cookie(filterCookieName, filterValue);

                if (queryString != 'null' && queryString.indexOf(filterName) == -1) {
                    $.cookie('queryString', queryString + '&' + filterName + '=' + filterValue);
                }
                else {
                    if (queryString == 'null' || queryString === undefined) {
                        $.cookie('queryString', window.location.pathname + '?' + filterName + '=' + filterValue);
                    } else {
                        $.cookie('queryString', queryString.replace(filterOnRegex, filterName + '=' + filterValue));
                    }
                }
            }
        }

        var qSRefinement = $.cookie('queryString');
        $.cookie('queryString', qSRefinement.replace(/&$/g, ''));

        queryString = $.cookie('queryString');
        var pathToMatch = window.location.pathname + '?';

        if (queryString.toUpperCase() == pathToMatch.toUpperCase()) {
            $.cookie('queryString', null);
            window.location = window.location.pathname;
        }
        else
            window.location = $.cookie('queryString');
    }
    //filter rooms by gender
    $('#genderPreference').change(function () {
        filterProperties('gender', 'filterGender', $(this).val(), /gender=[a-zA-Z]+&*/g, /gender=[a-zA-Z]+/g);
    });
    //filter houses and lands by purpose
    $('#purpose').change(function () {
        filterProperties('purpose', 'filterPurpose', $(this).val(), /purpose=[a-zA-Z]+&*/g, /purpose=[a-zA-Z]+/g);
    });
    //filter rooms by occupancy
    $('#occupancy').change(function () {
        filterProperties('occupancy', 'filterOccupancy', $(this).val(), /occupancy=[0-9]+&*/g, /occupancy=[0-9]+/g);
    });
    //filter rooms by number of bathrooms
    $('#bathroomAmount').change(function () {
        filterProperties('bathrooms', 'filterBathrooms', $(this).val(), /bathrooms=[0-9]+&*/g, /bathrooms=[0-9]+/g);
    });
    //filter houses by number of bedrooms
    $('#bedroomAmount').change(function () {
        filterProperties('bedrooms', 'filterBedrooms', $(this).val(), /bedrooms=[0-9]+&*/g, /bedrooms=[0-9]+/g);
    });
    //filter rooms by is student
    $('#isStudent').change(function () {
        filterProperties('isStudent', 'filterIsStudent', $(this).is(':checked'), /isStudent=[a-z]+&*/g, /isStudent=[a-z]+/g);
    });
    //filter rooms by has water
    $('#hasWater').change(function () {
        filterProperties('hasWater', 'filterHasWater', $(this).is(':checked'), /hasWater=[a-z]+&*/g, /hasWater=[a-z]+/g);
    });
    //filter rooms by has cable
    $('#hasCable').change(function () {
        filterProperties('hasCable', 'filterHasCable', $(this).is(':checked'), /hasCable=[a-z]+&*/g, /hasCable=[a-z]+/g);
    });
    //filter rooms by has elextricity
    $('#hasElectricity').change(function () {
        filterProperties('hasElectricity', 'filterHasElectricity', $(this).is(':checked'), /hasElectricity=[a-z]+&*/g, /hasElectricity=[a-z]+/g);
    });
    //filter rooms by has gas
    $('#hasGas').change(function () {
        filterProperties('hasGas', 'filterHasGas', $(this).is(':checked'), /hasGas=[a-z]+&*/g, /hasGas=[a-z]+/g);
    });
    //filter rooms by has internet
    $('#hasInternet').change(function () {
        filterProperties('hasInternet', 'filterHasInternet', $(this).is(':checked'), /hasInternet=[a-z]+&*/g, /hasInternet=[a-z]+/g);
    });
    //filter houses by isfurnished
    $('#isFurnished').change(function () {
        filterProperties('isFurnished', 'filterIsFurnished', $(this).is(':checked'), /isFurnished=[a-z]+&*/g, /isFurnished=[a-z]+/g);
    });
    //filter lands by area
    $('#area').change(function () {

        var rangeSelected = $(this).val();
        var rangeSelectedSplit = rangeSelected.split('-');

        var area1 = rangeSelectedSplit[0];
        var area2 = rangeSelectedSplit[1];

        var queryString = $.cookie('queryString');

        if ($(this).val() == 'no_filter') {
            $.cookie('filterArea', null);
            $.cookie('queryString', queryString.replace(/ar1=[0-9a-z]+&ar2=[0-9]+&*/g, ''));
        }
        else {
            $.cookie('areaRangeSelection', rangeSelected);

            $.cookie('filterArea', 'ar1=' + area1 + '&' + 'ar2=' + area2);

            if (queryString != 'null' && queryString.indexOf('ar1') == -1) {
                $.cookie('queryString', queryString + '&' + $.cookie('filterArea'));
            }
            else {
                if (queryString == 'null' || queryString === undefined) {
                    $.cookie('queryString', window.location.pathname + '?' + $.cookie('filterArea'));
                } else {
                    $.cookie('queryString', queryString.replace(/ar1=[0-9]+&ar2=[0-9]+/g, $.cookie('filterArea')));
                }
            }
        }

        var qSRefinement = $.cookie('queryString');
        $.cookie('queryString', qSRefinement.replace(/&$/g, ''));

        queryString = $.cookie('queryString');
        var pathToMatch = window.location.pathname + '?';

        if (queryString.toUpperCase() == pathToMatch.toUpperCase()) {
            $.cookie('queryString', null);
            window.location = window.location.pathname;
        }
        else
            window.location = $.cookie('queryString');
    });
    //filter rooms by price range
    $('.aside-property#range-cost a').click(function (event) {
        event.preventDefault();

        var rangeSelected = $(this).attr('id');
        var rangeSelectedSplit = rangeSelected.split('-');

        var price1 = rangeSelectedSplit[0];
        var price2 = rangeSelectedSplit[1];

        var queryString = $.cookie('queryString');

        if ($(this).attr('class') == 'active') {
            $.cookie('costRangeSelection', null);
            $.cookie('filterCostRange', null);

            $.cookie('queryString', queryString.replace(/cr1=[0-9a-z]+&cr2=[0-9]+&*/g, ''));
        }
        else {
            $.cookie('costRangeSelection', $(this).attr('id'));

            $.cookie('filterCostRange', 'cr1=' + price1 + '&' + 'cr2=' + price2);

            if (queryString != 'null' && queryString.indexOf('cr1') == -1) {
                $.cookie('queryString', queryString + '&' + $.cookie('filterCostRange'));
            }
            else {
                if (queryString == 'null' || queryString === undefined) {
                    $.cookie('queryString', window.location.pathname + '?' + $.cookie('filterCostRange'));
                } else {
                    $.cookie('queryString', queryString.replace(/cr1=[0-9]+&cr2=[0-9]+/g, $.cookie('filterCostRange')));
                }
            }
        }

        var qSRefinement = $.cookie('queryString');
        $.cookie('queryString', qSRefinement.replace(/&$/g, ''));

        queryString = $.cookie('queryString');
        var pathToMatch = window.location.pathname + '?';

        if (queryString.toUpperCase() == pathToMatch.toUpperCase()) {
            $.cookie('queryString', null);
            window.location = window.location.pathname;
        }
        else
            window.location = $.cookie('queryString');
    });
    //filter rooms by parishes
    $('.aside-property#filter-room-parish a').click(function (event) {
        event.preventDefault();

        var queryString = $.cookie('queryString');

        if ($(this).attr('class') == 'active') {
            $.cookie('parishSelection', null);
            $.cookie('filterParish', null);
            $.cookie('queryString', queryString.replace(/parish=[a-z-]+&*/g, ''));
        }
        else {
            $.cookie('parishSelection', $(this).attr('id'));

            $.cookie('filterParish', 'parish=' + $(this).attr('id'));

            if (queryString != 'null' && queryString.indexOf('parish') == -1) {
                $.cookie('queryString', queryString + '&' + $.cookie('filterParish'));
            }
            else {
                if (queryString == 'null' || queryString === undefined) {
                    $.cookie('queryString', window.location.pathname + '?' + $.cookie('filterParish'));
                } else {
                    $.cookie('queryString', queryString.replace(/parish=[a-z]+/g, $.cookie('filterParish')));
                }
            }
        }

        var qSRefinement = $.cookie('queryString');
        $.cookie('queryString', qSRefinement.replace(/&$/g, ''));

        queryString = $.cookie('queryString');
        var pathToMatch = window.location.pathname + '?';

        if (queryString.toUpperCase() == pathToMatch.toUpperCase()) {
            $.cookie('queryString', null);
            window.location = window.location.pathname;
        }
        else
            window.location = $.cookie('queryString');

    });
    //properties pagination next 
    $('.next-page').click(function (event) {
        event.preventDefault();

        var currentPageNumber = parseInt($('#currentPageNumber').val());
        var redirectionURL = '';

        if ($.cookie('queryString') != 'null' && $.cookie('queryString') !== undefined) {
            var queryString = $.cookie('queryString');

            redirectionURL = queryString + '&' + 'pg=' + (currentPageNumber + 1);
        }
        else
            redirectionURL = window.location.pathname + '?' + 'pg=' + (currentPageNumber + 1);

        window.location = redirectionURL;

    });
    //properties pagination next 
    $('.previous-page').click(function (event) {
        event.preventDefault();

        var currentPageNumber = parseInt($('#currentPageNumber').val());
        var redirectionURL = '';

        if ($.cookie('queryString') != 'null' && $.cookie('queryString') !== undefined) {
            var queryString = $.cookie('queryString');

            redirectionURL = queryString + '&' + 'pg=' + (currentPageNumber - 1);
        }
        else
            redirectionURL = window.location.pathname + '?' + 'pg=' + (currentPageNumber - 1);

        window.location = redirectionURL;

    });
    //properties pagination jump to
    $('.pageNumber').click(function (event) {
        event.preventDefault();

        var redirectionURL = '';

        if ($.cookie('queryString') != 'null' && $.cookie('queryString') !== undefined) {
            var queryString = $.cookie('queryString');

            redirectionURL = queryString + '&' + 'pg=' + $(this).text();
        }
        else
            redirectionURL = window.location.pathname + '?' + 'pg=' + $(this).text();

        window.location = redirectionURL;
    });
    //filters properties by parishes
    $('.aside-property#filter-parish a').click(function (event) {

        event.preventDefault();

        var tableName = '';
        var tagName = '';

        asideLinkID = $(this).attr('id');
        parishFilterValue = $(this).text();//value that will be used to filter by parish
        //if link is already active, remove the active class from it then clear that filter
        if ($(this).attr('class') == 'active' && $(this).attr('id') != 'all-parishes') {
            //removes active link
            $('#' + asideLinkID).removeAttr('class', 'active');
            $('#all-parishes').attr('class', 'active');

            isFiltered = false;
            //gets all properties//not filtered
            getProperties();
        } else {
            if (asideLinkID == 'all-parishes') {
                isFiltered = false;
                //resets all offset values
                accommodationsOffset = 0;
                houseOffset = 0;
                landOffset = 0;

                //gets all properties//not filtered
                getProperties();

            } else {
                //removes all-parishes filter highlight
                $('#filter-parish a').removeAttr('class', 'active');
                $('#' + asideLinkID).attr('class', 'active');

                isFiltered = true;//used to ensure that the pagination uses the filter

                getProperties(parishFilterValue);
            }
        }
    });
    //loads the appropriate page in the registration field
    $(document.body).on('change', 'input[name="chkProperty"]', function () {
        var clickVal = $(this).prop('value');
        if (clickVal == 'House') {
            $('#property-type').load('getHousePartialView', function () {
                $('form').removeData('validator');
                $.validator.unobtrusive.parse($('form'));
            });
        } else if (clickVal == 'Room') {
            $('#property-type').load('getRoomPartialView', function () {
                $('form').removeData('validator');
                $.validator.unobtrusive.parse($('form'));
            });
        } else {
            $('#property-type').load('getLandPartialView', function () {
                $('form').removeData('validator');
                $.validator.unobtrusive.parse($('form'));
            });
        }
    });
    //displays registration confirmation message for 10s
    $('#registration-success').fadeIn('slow', function () {
        $(this).fadeOut(20000);
    })
    //submits all registration forms at the same time
    $('#btnRegister').click(function (event) {
        event.preventDefault();
        //validates form that will submit by ajax
        $('#fm_owner').validate().form();

        /*
        * The request first attempts to submit the landlord's information
        * If it was submitted successfully then it submits the property information:land/house/room
        * If it is not successful then the error message is alerted to the user
        */
        $.ajax({
            url: 'RegistrationLandlord',
            type: 'Post',
            data: $('#fm_owner').serialize(),
            success: function (data) {
                if (data == '') {
                    if ($('#chkProperty:checked').prop('value') == 'House') {
                        $('#fm_house').submit();
                    } else if ($('#chkProperty:checked').prop('value') == 'Room') {
                        $('#fm_room').submit();
                    } else if ($('#chkProperty:checked').prop('value') == 'Land') {
                        $('#fm_land').submit();
                    }
                } else {
                    alert(data);
                }
            }

        });
    });
    function propertiesPagination(propertyName, parish, pgCount) {
        $.ajax({
            url: '/home/getProperties',
            type: 'Get',
            data: { 'fetchAmount': 4, 'parish': parish, 'isPagination': true, 'propertyType': propertyName, 'pgNo': pgCount },
            success: function (data) {
                if (!$.isEmptyObject(data)) {//TODO use intuitive method to determine when specific object is empty
                    loadPropertiesHTML(data, propertyName);
                } else {
                    //returning 1 to indicate that no results came back from the request
                    alert("An error occurred while processing request.");
                    return 1;
                }
            },
            error: function () {
                //returning 1 to indicate that an error occurred
                alert("An error occurred while processing request.");
                return 1;
            }
        });
    }
    //pagination function for home
    $('.home-property-category a').click(function (event) {
        event.preventDefault();

        var pageClicked = $(this).attr('id');
        var pageClickedArray = pageClicked.split("-");

        var tableName = pageClickedArray[0];
        var pageDirection = pageClickedArray[1];
        var parish = parishFilterValue == '' ? '' : parishFilterValue;
        //TODO find a way to know if the object is null
        switch (tableName) {
            case 'accommodations':
                //get next set of result
                if (pageDirection == 'next') {
                    accommodationPg += 1;
                } else {
                    accommodationPg -= 1;
                }
                //preventing paging from having a negative value
                accommodationPg < 0 ? 0 : accommodationPg;
                var success = propertiesPagination(tableName, parish, accommodationPg);
                //go back if object is empty or an error had occurred
                if (success == 1) {
                    if (pageDirection == 'next') {
                        accommodationPg -= 1;
                    } else {
                        accommodationPg += 1;
                    }
                }
                break;
            case 'house':
                //get next set of result
                if (pageDirection == 'next') {
                    housePg += 1;
                } else {
                    housePg -= 1;
                }
                //preventing paging from having a negative value
                housePg < 0 ? housePg = 0 : housePg = housePg;
                var success = propertiesPagination(tableName, parish, housePg);
                //go back if object is empty or an error had occurred
                if (success == 1) {
                    if (pageDirection == 'next') {
                        housePg -= 1;
                    } else {
                        housePg += 1;
                    }
                }
                break;
            case 'land':
                //get next set of result
                if (pageDirection == 'next') {
                    landPg += 1;
                } else {
                    landPg -= 1;
                }
                //preventing paging from having a negative value
                landPg < 0 ? landPg = 0 : landPg = landPg;
                var success = propertiesPagination(tableName, parish, landPg);
                //go back if object is empty or an error had occurred
                if (success == 1) {
                    if (pageDirection == 'next') {
                        landPg += 1;
                    } else {
                        landPg -= 1;
                    }
                }
                break;
        }
    });

    //hides university information if the false option is selected
    var universityStudentCheck = function () {
        var student = $('#university_student');
        if (student.val() == "False") {
            $("#university_info").hide();
            $(".end_date").hide();
        }
    }

    universityStudentCheck();

    $('input[name=university_student]').change(function () {
        if ($(this).val() == "False") {
            $("#university_info").hide();
            $(".end_date").hide();
        } else {
            $("#university_info").show();
            $(".end_date").show();
        }
    });

    $(document.body).on('change', 'input[type=file]', function (event) {
        //adding any file to the global file variable to be later used
        files = event.target.files;
    });


});

