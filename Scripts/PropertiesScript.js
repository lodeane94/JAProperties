var selectedLocation = { country: 'Jamaica', countryCode: 'jm', division: '' };
var postBackInfo, postBackInfoRaw = null;
var distanceMtxInfo = null;
var activeSearchFilterElement = null;
var activeSearchFilterId = null;
var activeSearchFilterHeight = null;
var rangeDistanceRadius = null;

//used to display a loading element
var loadingGifHTML = '<div id="loading-gif" class="col-xs-1">'
    + '<img src="/Content/ajax-loader-dark.gif" />'
    + '</div>';

//populates the division field
function populateDivisionByCountryCode() {
    var elementId = '#division';
    var division = $(elementId);

    division.find("option:disabled").nextAll('option').remove();//removes all items from the division list

    $.ajax({
        url: '/servicer/GetDivisionNames',
        type: 'get',
        /* dataType: "json",
         data: { Url: 'http://westclicks.com/webservices/?f=json&c=' + selectedLocation.countryCode },*/
        beforeSend: function () {
            $('#division').append($('<option></option>').attr('value', 'selectHolder').attr('selected', true).text('Loading Divisions ...'));
        },
        success: function (data) {
            if (data != null && data != '') {
                $.each(data, function (index, value) {
                    $('#division').append($('<option></option>').attr('value', value).text(value));
                });

                if (postBackInfo != null) {
                    reapplySelectSearchFilterOnPostback(elementId, postBackInfo.Division);
                }
            } else {
                alert('Error occurred while retrieving divisions, contact system administrator');
            }
        },
        complete: function () {
            $('#division option[value="selectHolder"]').remove();
        },
        error: function () {
            alert('Error occurred while retrieving divisions, contact system administrator');
        }
    });


}

//get all property types by the category name and loads select element with the values
function populatePropertyTypeByCategoryName(categoryName, isStudentAccommodationCat) {
    $.ajax({
        url: '/servicer/GetPropertyTypesByCategoryName',
        type: 'get',
        data: { propertyCategoryName: categoryName },
        success: function (data) {
            $.each(data, function (index, value) {
                $('#PropertyType').append($('<option></option>').attr('value', value).text(value));
            });

            if (postBackInfo != null) {
                $('#PropertyType option[value="' + postBackInfo.PropertyType + '"').prop('selected', true);
            }
        },
        error: function () {
            alert('Error occurred while retrieving property types, contact system administrator');
        }
    });

    var input = $('<input>').attr('type', 'hidden').attr('name', 'propertycategory').val(categoryName);
    $('#search-properties-form').append(input);

    if (isStudentAccommodationCat != null && isStudentAccommodationCat == 'True') {
        var input = $('<input>').attr('type', 'hidden').attr('name', 'IsStudentAccommodationCat').val('true');
        $('#search-properties-form').append(input);
    }
}

//get all property types and loads select element with the values
function populatePropertyType() {
    var elementId = '#PropertyType';
    $.ajax({
        url: '/servicer/GetAllPropertyTypeNames',
        type: 'get',
        success: function (data) {
            $.each(data, function (index, value) {
                $(elementId).append($('<option></option>').attr('value', value).text(value));
            });
            //reinitialize filter values on server postback
            if (postBackInfo != null) {
                reapplySelectSearchFilterOnPostback(elementId, postBackInfo.PropertyType);
            }
        },
        error: function () {
            alert('Error occurred while retrieving property types, contact system administrator');
        }
    });
}

//get all property purpose and loads select element with the values
function populatePropertyPurpose() {
    var elementId = '#PropertyPurpose';

    $.ajax({
        url: '/servicer/GetAllPropertyPurposeNames',
        type: 'get',
        success: function (data) {
            $.each(data, function (index, value) {
                $(elementId).append($('<option></option>').attr('value', value).text(value));
            });

            if (postBackInfo != null) {
                reapplySelectSearchFilterOnPostback(elementId, postBackInfo.PropertyPurpose);
            }
        },
        error: function () {
            alert('Error occurred while retrieving property types, contact system administrator');
        }
    });
}

//populates the price range values on postback of the server
function populatePriceRange() {
    var elementId = '#MinPrice';

    //max price cannot be 0
    if (postBackInfo.MaxPrice > 0) {
        $(elementId).attr('value', postBackInfo.MinPrice);
        $('#MaxPrice').attr('value', postBackInfo.MaxPrice);

        reapplyAddFilterBtnOnPostBack(elementId, postBackInfo.MinPrice, postBackInfo.MaxPrice);
    }
}

//reselects the property adtype that was selected
function reselectPropertyAdType() {
    var elementId = '#rdoAdType';

    $(elementId).filter(`[value=${postBackInfo.RdoAdType}]`).prop('checked', true);

    reapplyAddFilterBtnOnPostBack(elementId, postBackInfo.RdoAdType, null);
}

//reapplies the search filters on postback of the server
//since these selected would be lost
function reapplySelectSearchFilterOnPostback(elementId, selectValue) {

    if (selectValue != null) {
        $(elementId + ' option[value="' + selectValue + '"').prop('selected', true);

        var searchFilterElement = $(elementId).closest('.search-filter-popdown').siblings();
        appySearchFilterName(searchFilterElement, $(elementId));
    }
}

function reapplyAddFilterBtnOnPostBack(elementId, value1, value2) {

    if (value1 != null) {

        var filterElement = $(elementId).closest('.search-filter-popdown').siblings();

        var filterBtn = $(elementId).parent().siblings('.add-filter-btn');

        addFilterBtnClick(filterElement, value1, value2, filterBtn);
    }
}

//populates google maps MVC array that will hold the LatLng objects of 
//property coordinates
function populateMapsMVCArray(data) {

    var mvcArray = new google.maps.MVCArray();

    $.each(data, function (key, val) {
        var mapLatLng = new google.maps.LatLng(val.Latitude, val.Longitude);

        mvcArray.push(mapLatLng);

    });

    return mvcArray;
}

//populates the distance matrix object which contains 
//the origin location and all the destination locations along
//with thier duration and distances away from the origin location
//This is done to get a better object structure that is easy to manipulate
function populateDistanceMatrixInformation(data) {

    var destinationAddrLength = data.destination_addresses.length;

    distanceMtxInfo = {
        originAddress: data.origin_addresses,
        destinationInformation: [],
    };

    // alert(data.destination_addresses.count);

    $.each(data.destination_addresses, function (indx, val) {

        var distance, duration = '';
        console.log(val);

        $.each(data.rows[0].elements, function (index, value) {
            if (value.status != null
                && value.status != undefined
                && value.status != 'ZERO_RESULTS'
                && index == indx) {
                distance = value.distance.text;
                duration = value.duration.text;
            }
        });

        if (distance != '' && duration != '') {
            var destinationInfo = {};
            var streetAddr = '';
            //remove unit section from the distance
            var spcIndex = distance.indexOf(' ');
            var newDistanceOutput = distance.substring(0, spcIndex);
            distance = newDistanceOutput;
            //retrieving long name for the addresses found because the distance matrix
            // only returns the short addresses

            var geocoder = new google.maps.Geocoder();

            geocoder.geocode({ address: val }, function (locations, status) {
                if (status == 'OK') {
                    for (var i = 0; i < locations[0].address_components.length; i++) {
                        var addressType = locations[0].address_components[i].types[0];

                        if (addressType == 'route') {
                            streetAddr = locations[0].address_components[i]['long_name'];

                            destinationInfo = {
                                streetAddress: streetAddr,
                                distance: distance,
                                duration: duration
                            };

                            destinationGeocodeCallback(destinationInfo, destinationAddrLength);
                        }
                    }
                }
            });
        } else {
            //whenever an empty data set is returned then reduce the length
            destinationAddrLength--;
            destinationGeocodeCallback(null, destinationAddrLength);
        }
    });
}

function destinationGeocodeCallback(destinationInfo, length) {
    if (destinationInfo != null)
        distanceMtxInfo.destinationInformation.push(destinationInfo);

    if (distanceMtxInfo.destinationInformation.length == length)
        searchNearByProperties();
}

//makes call to the server that will make call to the google distance matrix
//api to calculate the distances between the locations
//it also post the newly created distance matrix object to the server for 
//further manipulation
function setDistanceMatrixInformation(orLat, orlng, encodedCoordinatesUrl) {

    var distanceMatrixUrl = 'https://maps.googleapis.com/maps/api/distancematrix/json?origins=' + orLat + ',' + orlng +
        '&destinations=enc:' + encodedCoordinatesUrl + ':&key=AIzaSyBkscrDmY_ngoabLcmg6yJuAZio7-Tjf3w';

    $.ajax({
        url: '/servicer/RequestJsonDataFromUrl',
        type: 'get',
        data: { Url: distanceMatrixUrl },
        success: function (data) {
             console.log(data);

            if (data.status == "OK") {
                populateDistanceMatrixInformation(data);
            } else {
                alert('No properties found for the given criteria');
            }
        },
        error: function () {
            alert('Error occurred while retrieving details, contact system administrator');
        }
    });
}
//submits form containing the search distance matrix informatin
//to the server for processing
function searchNearByProperties() {
    var data = JSON.stringify(distanceMtxInfo);

    var form = $('<form enctype="application/json" action="/properties/getNearbyProperties" method="POST">' +
        '<input type="hidden" name="distanceMtxInfo" value="' + escapeHtml(data) + '">' +
        '<input type="hidden" name="distanceRadius" value="' + rangeDistanceRadius + '">' +
        '</form>');

    $(document.body).append(form);

    form.submit();
}

//displays the search filter popdown
//remove bottom border of the search-filter button
//and increase the height of the element by 16px
function activateSearchFilterPopdown(element, searchFilterId, searchFilterHeight) {
    $(element).css({
        'border-bottom': '1px solid #FFFFFF', 'border-bottom-right-radius': '0px',
        'border-bottom-left-radius': '0px', 'height': (searchFilterHeight + 16) + 'px'
    });

    //$(searchFilterPopdownHtml).insertAfter(element);
    //displaySearchFilterContent(searchFilterId);

    $('.' + searchFilterId).show();

    //will be used to clear a section of the top border of the search-filter class
    var clearBorderWidth = $(element).outerWidth(true);
    var clearBorderWidthFix = clearBorderWidth - 2;//removing 2 pixels from the width ; unable to get the correct fix
    $('.clear-border').css({ 'width': clearBorderWidthFix });

    activeSearchFilterElement = element;
    activeSearchFilterHeight = searchFilterHeight + 16;
}

//inserts the criteria that user can use to filter their searches
/*function displaySearchFilterContent(searchFilterId) {
    var content = '';

    switch (searchFilterId) {
        case 'search-filter-parish': content = '<select class="form-control" id="division" name="division"><option disabled selected value="">Parish</option></select>';
            break;
    }

    $(content).appendTo('.search-filter-popdown-content');
}*/

//removes the search filter popdown display from the dom
//and resets the search filter element to normal
function deactivateSearchFilterPopdown(element, searchFilterHeight) {
    //remove popdown
    $('.search-filter-popdown').hide();

    //return search filter element to normal
    $(element).css({
        'border-bottom': '1px solid #F2F2F2', 'border-bottom-right-radius': '4px',
        'border-bottom-left-radius': '4px', 'height': (searchFilterHeight - 16) + 'px'
    });

    activeSearchFilterElement = null;
    activeSearchFilterHeight = null;
}

//Displays the selected filter item and highlights the selected element
function appySearchFilterName(filterElement, selectedFilter) {

    var selectedValue = $(selectedFilter).find('option:selected').attr('value');

    $(filterElement).text(selectedValue);
    $(filterElement).addClass('active');

    //deactivates the current filter popdown content
    deactivateSearchFilterPopdown(activeSearchFilterElement, activeSearchFilterHeight);

    var selectElementId = $(selectedFilter).attr('id');
    var searchFilterElementId = $(filterElement).attr('id');
    var filterResetValue = $(filterElement).attr('title');

    //only create element if it does not already exist
    if ($(filterElement).siblings('.search-filter-popdown').siblings('.remove-filter-container').length < 1) {
        var removeFilterElementHtml = createRemoveFilterElement(selectElementId, searchFilterElementId, filterResetValue, 'this');

        //find the closest 'search-filter-popdown' element then insert the remove filter html after it
        $(selectedFilter).closest('.search-filter-popdown').after(removeFilterElementHtml);
    }
}

//Removes the search filter from the search criteria and remove highlight from the element
function removeSearchFilter(filterlinkElement, returnedFilterLinkElement, removeFilterElement) {

    $(filterlinkElement).text(returnedFilterLinkElement);
    $(filterlinkElement).removeClass('active');

    $(removeFilterElement).parent().remove();

    resetInputValues(filterlinkElement);
}

//creates the html that creates the remove filter element for filters
//that are drop down lists
function createRemoveFilterElement(selectElementId, searchFilterElementId, filterResetValue, removeElement) {
    var html = '<div class="remove-filter-container">'
        + '<div class="horizontal-separator" style="border-bottom:4px;"></div>'
        + `<a onclick="${selectElementId != null ? `resetSelectOption('${(selectElementId)}');` : ''} removeSearchFilter(document.getElementById('${searchFilterElementId}'), '${filterResetValue}', ${removeElement}); $('.btn-search').click();"`
        + 'class="remove-filter" aria-label="Remove Filter">'
        + 'Remove <span aria-hidden="true">&times; </span> </a>'
        + '</div>';

    return html;
}

//deselects all option on a select element
function resetSelectOption(filterCriteriaElementId) {
    $('select#' + filterCriteriaElementId).prop('selectedIndex', 0);
}

//sets input values to their  default value
function resetInputValues(searchFilterElement) {
    $(searchFilterElement).siblings('.search-filter-popdown').find('input').each(function (index) {
        if ($(this).attr('type') != 'radio')
            $(this).val('');

        $(this).prop('checked', false);
    });
}

function addFilterBtnClick(filterElement, value1, value2, thisBtn) {
    var filterText = '';

    //alert(value1.value);

    //price range
    if (value2 != null)
        filterText = '$' + value1 + ' - ' + '$' + value2;
    else
        filterText = value1;

    $(filterElement).text(filterText);
    $(filterElement).addClass('active');

    deactivateSearchFilterPopdown(activeSearchFilterElement, activeSearchFilterHeight);

    var searchFilterElementId = $(filterElement).attr('id');
    var filterResetValue = $(filterElement).attr('title');

    //only create element if it does not already exist
    if ($(filterElement).siblings('.search-filter-popdown').siblings('.remove-filter-container').length < 1) {

        var removeFilterElementHtml = createRemoveFilterElement(null, searchFilterElementId, filterResetValue, 'this');

        // alert($(thisBtn).attr('id'));
        //find the closest 'search-filter-popdown' element then insert the remove filter html after it
        $(thisBtn).closest('.search-filter-popdown').after(removeFilterElementHtml);
    }
}

function escapeHtml(text) {
    var map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return text.replace(/[&<>"']/g, function (m) { return map[m]; });
}

//dynamically sets the search type
function setSearchType() {
    if (postBackInfoRaw != null && postBackInfo != null
        && postBackInfo.SearchType != null && postBackInfo.SearchType != 'SearchTerm') {

        var searchTypeElement = $('input[name="searchType"][value="' + postBackInfo.SearchType + '"]');
        searchTypeElement.prop('checked', true);
        searchTypeElement.parent().siblings().removeClass('active');
        searchTypeElement.parent().addClass('active');
    } else {
        //default behavior for the search type element
        var searchTypeElement = $('input[name="searchType"][value="SearchTerm"]');
        searchTypeElement.prop('checked', true);
        searchTypeElement.parent().addClass('active');
    }
}

//stores the distance radius value
function setRangeDistanceRadius() {
    var element = $('#range-distance-radius');

    if (element != null && element.length) {
        rangeDistanceRadius = element.val(); 
        $("#range-distance-radius-val").text(rangeDistanceRadius + ' KM');
    }
}

//sets the body's height which is determine by it's contents 
function setBodyHeight() {
    var filterTagsHeight = $('.filter-tags').outerHeight();
    var propertyListingHeight = $('.property-listing').outerHeight();

    if (propertyListingHeight > filterTagsHeight)
        $('#body').height(propertyListingHeight);
    else
        $('#body').height(filterTagsHeight);
}

//displays previous and next signs depending on the number of properties returned
$(function () {
    var noOfPages = $('#noOfPages').val();
    var currentPageNumber = $('#pgNo').val();
    //front of list show only forward button
    if (currentPageNumber == 1 && noOfPages - 1 > 0) {
        $('.next-page').show();
    }
    //end of list show only back button
    if (currentPageNumber == noOfPages && noOfPages - 1 > 0) {
        $('.previous-page').show();
    }
    //somewhere within the list show both back and forward button
    if (currentPageNumber != 1 && currentPageNumber != noOfPages && noOfPages - 1 > 0) {
        $('.next-page').show();
        $('.previous-page').show();
    }

    //properties pagination next 
    $('.next-page').click(function (event) {
        event.preventDefault();

        var redirectionURL = '';
        var queryStr = $('#queryString').val();
        queryStr = queryStr.replace(/&pgNo=[0-9]/g, '');
        var currentPageNumber = parseInt($('#pgNo').val());

        redirectionURL = window.location.pathname + '?' + queryStr + '&pgNo=' + (currentPageNumber + 1);
        window.location = redirectionURL;
    });

    //properties pagination previous 
    $('.previous-page').click(function (event) {
        event.preventDefault();

        var redirectionURL = '';
        var queryStr = $('#queryString').val();
        queryStr = queryStr.replace(/&pgNo=[0-9]/g, '');
        var currentPageNumber = parseInt($('#pgNo').val());

        redirectionURL = window.location.pathname + '?' + queryStr + '&pgNo=' + (currentPageNumber - 1);
        window.location = redirectionURL;
    });

    //properties pagination jump to
    $('.pageNumber').click(function (event) {
        event.preventDefault();

        var redirectionURL = '';
        var queryStr = $('#queryString').val();
        queryStr = queryStr.replace(/&pgNo=[0-9]/g, '');
        redirectionURL = window.location.pathname + '?' + queryStr + '&pgNo=' + $(this).text();
        window.location = redirectionURL;
    });

    //sets the current page that the user is on
    var currentPageNumber = $('#pgNo').val();

    $('.pageNumber').each(function (index, val) {
        if ($(this).text() == currentPageNumber) {
            $(this).parent().attr('class', 'active');
        }
    });

});

$(document).ready(function () {
    postBackInfoRaw = $('#_postBackInformation').val();

    //setting back the search filter values after postback response from the server
    if (postBackInfoRaw != 'null' && postBackInfoRaw != undefined) {
        postBackInfo = JSON.parse(postBackInfoRaw);//information posted to the server for search

        populatePriceRange();
        reselectPropertyAdType();

        $('#SearchTerm').attr('value', postBackInfo.SearchTerm);

        $('#coordinateLat').val(postBackInfo.coordinateLat);
        $('#coordinateLng').val(postBackInfo.coordinateLng);
    }

    setSearchType();
    populateDivisionByCountryCode();
    populatePropertyPurpose();
    //populatePropertyType();// why is this giving problem ? TODO resolve
    setRangeDistanceRadius();

    //sets the menu to fixed after scrolling pass a certain amount of pixels
    $(window).scroll(function () {
        if ($(this).scrollTop() > 280) {
            //       $('.header-content').addClass('fixed');
            $('.search-bar-container').addClass('fixed-search');
        } else {
            //      $('.header-content').removeClass('fixed');
            $('.search-bar-container').removeClass('fixed-search');
        }
    });

    //this will not be effected since Jamaica is the primary focus
    /*
    //get all countries and populate the country select element
    $.ajax({
        url: '/servicer/RequestJsonDataFromUrl',
        type: 'get',
        dataType: "json",
        data: { Url: 'http://westclicks.com/webservices/?f=json' },
        beforeSend: function () {
            // $('#country').append($('<option></option>').attr('value', 'selectHolder').attr('selected', true).text('Detecting Country ...'));
        },
        success: function (countriesData) {
            //detect selectedLocation of user
            $.getJSON('//freegeoip.net/json/?callback=?', function (data) {
                selectedLocation.country = data.country_name;

                $.each(countriesData, function (index, value) {
                    if (selectedLocation.country.toLowerCase() == value.toLowerCase()
                        && data.country_name != null
                        && data.country_name != '') {//only current country if auto country detect is possible
                        $('#country').append($('<option></option>').attr('id', index).attr('value', value).attr('selected', true).text(value));
                        selectedLocation.countryCode = index;
                    } else {
                        $('#country').append($('<option></option>').attr('id', index).attr('value', value).text(value));
                    }
                });

                if (data.country_name != null && data.country_name != '')
                    populateDivisionByCountryCode();

                if (postBackInfo != null) {
                    $('#country option[value="' + postBackInfo.Country + '"').prop('selected', true);
                }
            });
        },
        complete: function () {
            //  $('#country option[value="selectHolder"]').remove();
        },
        error: function () {
            alert('Error occurred while retrieving countries, contact system administrator');
        }
    });*/

    //updates the division select element whenever the country option is changed
    $('#country').change(function () {
        selectedLocation.country = $(this).find("option:selected").text();
        selectedLocation.countryCode = $(this).find("option:selected").attr('id');

        populateDivisionByCountryCode();
    });

    //set division in the selectedLocation object
    $('#division').change(function () {
        selectedLocation.division = $(this).find("option:selected").text();
    });

    //scrolls to the bottom of the getProperty page when button is clicked
    $('#requestPropertyProxyBtn').click(function (event) {
        event.preventDefault();

        $('html, body').animate({
            scrollTop: $('#property-requisition-question').offset().top
        }, 'fast');
    });

    //submits form that will request a property or send a message to the property owner
    $('#requestPropertyBtn').click(function (event) {
        event.preventDefault();

        var validator = $('#requestPropertyForm').validate({ errorPlacement: function (error, element) { } });
        var isValid = validator.form();

        if (isValid) {
            loadingGifLocation = $('.requestPropertySec');
            var formData = new FormData($('#requestPropertyForm')[0]);
            $.ajax({
                url: '/properties/requestProperty',
                type: 'Post',
                data: formData,
                contentType: false,
                processData: false,
                beforeSend: function () { $(loadingGifHTML).insertAfter(loadingGifLocation); },
                success: function (data) {
                    if (data.hasErrors) {
                        $.each(data.ErrorMessages, function (index, value) {
                            addErrorMessage(value);
                        });
                        displayErrorMessages();

                        $('#mvcCaptcha').load('/servicer/GetMvcCaptchaView');//reloads captcha image if error occurred
                    } else {
                        sys.showModal('#propertyRequisitionModal');

                        $('html, body').animate({
                            scrollTop: $('#top-header').offset().top
                        }, 'fast');
                    }
                },
                error: function (data) {
                    $('#mvcCaptcha').load('/servicer/GetMvcCaptchaView');//reloads captcha image if error occurred
                    alert('Error encountered while uploading property. Contact Website Administrator');
                },
                complete: function () { $('#loading-gif').remove(); },
            });
        }
    });
    //submits the search form depending on the search type that
    //is selected i.e. if it's all, then it will submit the form as normal
    //otherwise it will submit the form asynchronously to the server to retrieve
    //all the coordinates of the properties then it will send the new results to the server
    $('.btn-search').click(function (event) {
        event.preventDefault();

        var form = $('#search-properties-form');
        var searchType = $('input[name=searchType]:checked').val();
        var propTags = $('.chkTags');

        propTags.each(function () {
            if ($(this).attr('value') == 'True')
                form.append(`<input type="hidden" name="${$(this).attr('name')}" value="${$(this).attr('value')}">`);
        });


        if (searchType == 'SearchTerm') {
            form.submit();
        } else {
            //coordinates of the near by place/establishment
            var orLat = $('#coordinateLat').val();
            var orLng = $('#coordinateLng').val();

            if (orLat != '' && orLng != '') {
                //retrieve coordinates for all / conditioned properties within the database
                $.ajax({
                    url: '/properties/GetPropertiesCoordinates',
                    type: 'get',
                    data: form.serialize(),
                    beforeSend: function () {
                        $('#modal-loading').fadeIn();
                    },
                    success: function (data) {

                        var mvcArray = populateMapsMVCArray(data);
                        //console.log(mvcArray);
                        //encoding coordinates to compress url in the event that the URl exceeds its limit
                        var encodedCoordinatesUrl = google.maps.geometry.encoding.encodePath(mvcArray);

                        setDistanceMatrixInformation(orLat, orLng, encodedCoordinatesUrl);
                    },
                    complete: function () {
                        $('#modal-loading').fadeOut();
                    },
                    error: function () {
                        alert('Error occurred while retrieving property coordinates, contact system administrator');
                    }
                });
            } else {
                alert('Unable to identify coordinates for that location');
            }
        }

    });

    //activates and or deactivates that search filter popdown
    $('.search-filter').click(function (event) {
        event.preventDefault();
        event.stopPropagation();//prevents search filter element from being removed when element is clicked

        var newSearchFilterId = $(this).attr('id');
        var searchFilterHeight = $(this).outerHeight(true);

        if (activeSearchFilterElement == null) {

            activateSearchFilterPopdown($(this), newSearchFilterId, searchFilterHeight);

        } else if (activeSearchFilterElement != null
            && activeSearchFilterElement.attr('id') != newSearchFilterId) {

            deactivateSearchFilterPopdown(activeSearchFilterElement, activeSearchFilterHeight);
            activateSearchFilterPopdown($(this), newSearchFilterId, searchFilterHeight);
        }
        else {
            deactivateSearchFilterPopdown($(this), searchFilterHeight);
        }
    });

    $(document.body).on('click', '.search-filter-popdown', function (event) {
        event.stopPropagation();
    });

    //removes search filter element from DOM whenever anywhere else is clicked
    $(document).click(function () {
        if (activeSearchFilterElement != null) {

            //check if a search filter is not activated before reseting the input values on document click
            if ($(activeSearchFilterElement).siblings('.search-filter-popdown').siblings('.remove-filter-container').length < 1) {
                //clear input and radio filters upon document click
                resetInputValues(activeSearchFilterElement);
            }

            deactivateSearchFilterPopdown(activeSearchFilterElement, activeSearchFilterHeight);
        }
    });

    //ensures that only numbers are entered into the MinPrice and MaxPrice fields
    $('#MinPrice, #MaxPrice').keypress(function (e) {
        //if the letter is not digit then display error and don't type anything
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }
    });

    //prevent pasting within the MinPrice and MaxPrice fields
    //TODO allow pasting only if it is numbers
    $('#MinPrice, #MaxPrice').bind('paste', function (e) {
        e.preventDefault();
    });

    //adds property to a collection of saved / liked propertys that
    //can be viewed from the dashboard
    $('.save-property').click(function () {
        var propertyId = $(this).attr('id');
        var isAlreadySaved = $(this).parent().hasClass('saved');
        var obj = $(this);

        if (!isAlreadySaved) {
            $.ajax({
                url: '/properties/SaveLikedProperty',
                type: 'get',
                data: { propertyId: propertyId },
                success: function (data) {
                    if (data == 'True') {
                        obj.parent().addClass('saved');
                        obj.fadeOut().fadeIn();
                    } else {
                        alert(data);
                    }
                },
                error: function () {
                    alert('Error occurred while saving your liked property, contact system administrator');
                }
            });
        }
    });

    $('.contactPurpose').click(function () {
        var input = $(this).children();
        var val = input.val();

        input.attr('checked', true);

        if (val == 'requisition') {
            $('input[id="contact-purpose-msg"]').removeAttr('checked');
            $('#msg').attr('placeholder', 'Optional message to the property owner');
        } else {
            $('input[id="contact-purpose-req"').removeAttr('checked');
            $('#msg').attr('placeholder', 'Ask your question here');
        }
    });
    //adds or remove the selected tags 
    $('.chkTags').change(function () {

        var newId = $(this).attr('name').replace(' ', '_').replace('Tags', '').replace('[', '').replace(']', '');

        if ($(this).prop('checked')) {
            var form = $('#search-properties-form');

            form.append(`<input type="hidden" id="${newId}" name="${$(this).attr('name')}" value="true">`);

            $(this).attr('value', 'True');
        } else {
            $('#' + newId).remove();
            $(this).attr('value', 'False');
        }

        $('.btn-search').click();
    });

    $(document).on("input","#range-distance-radius", function (e) {
        rangeDistanceRadius = $(e.target).val();
        $("#range-distance-radius-val").text(rangeDistanceRadius + ' KM');
    });

    $('#range-distance-radius').on('change keyup', function () {
        searchNearByProperties();
    });

            

});