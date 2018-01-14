var selectedLocation = { country: 'Jamaica', countryCode: 'jm', division: '' };
var postBackInfo = null;
var distanceMtxInfo = null;

//used to display a loading element
var loadingGifHTML = '<div id="loading-gif" class="col-xs-1">'
                     + '<img src="/Content/ajax-loader-dark.gif" />'
                     + '</div>';

function populateDivisionByCountryCode() {
    var division = $('#division');

    division.find("option:disabled").nextAll('option').remove();//removes all items from the division list

    $.ajax({
        url: '/servicer/RequestJsonDataFromUrl',
        type: 'get',
        dataType: "json",
        data: { Url: 'http://westclicks.com/webservices/?f=json&c=' + selectedLocation.countryCode },
        beforeSend: function () {
            $('#division').append($('<option></option>').attr('value', 'selectHolder').attr('selected', true).text('Loading Divisions ...'));
        },
        success: function (data) {
            $.each(data, function (index, value) {
                $('#division').append($('<option></option>').attr('value', value).text(value));
            });

            if (postBackInfo != null) {
                $('#division option[value="' + postBackInfo.Division + '"').prop('selected', true);
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
function populatePropertyTypeByCategoryName(categoryName) {
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
}

//get all property types and loads select element with the values
function populatePropertyType() {
    $.ajax({
        url: '/servicer/GetAllPropertyTypeNames',
        type: 'get',
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
}

//populates google maps MVC array that will hold the LatLng objects of 
//property coordinates
function populateMapsMVCArray(data) {
    console.log(data);

    var mvcArray = new google.maps.MVCArray();
    console.log(mvcArray);

    $.each(data, function (key, val) {
        var mapLatLng = new google.maps.LatLng(val.Latitude, val.Longitude);
        console.log(mapLatLng);

        mvcArray.push(mapLatLng);

    });

    console.log(mvcArray);

    return mvcArray;
}

//populates the distance matrix object which contains 
//the origin location and all the destination locations along
//with thier duration and distances away from the origin location
//This is done to get a better object structure that is easy to manipulate
function populateDistanceMatrixInformation(data) {

    distanceMtxInfo = {
        originAddress: data.origin_addresses,
        destinationInformation: [],
    };

    $.each(data.destination_addresses, function (index, val) {

        var distance, duration = '';

        $.each(data.rows, function (index, value) {
            distance = value.elements[index].distance.text;
            duration = value.elements[index].duration.text;
        });

        //remove unit section from the distance
        var spcIndex = distance.indexOf(' ');
        var newDistanceOutput = distance.substring(0, spcIndex);
        distance = newDistanceOutput;

        //remove string starting from the first comma location in the result
        var firstStrAddrCommaIndex = val.indexOf(',');
        var newStrAddrOutput = val.substring(0, firstStrAddrCommaIndex);

        var destinationInfo = {
            streetAddress: newStrAddrOutput,
            distance: distance,
            duration: duration
        }

        distanceMtxInfo.destinationInformation.push(destinationInfo);
    });   
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
        data:{ Url : distanceMatrixUrl},
        success: function (data) {
            if (data.status == "OK"
                && data.rows[0].elements[0].status != 'ZERO_RESULTS') {
                populateDistanceMatrixInformation(data);

                searchNearByProperties();
            } else {
                alert('Unable to detemine distance between locations');
            }
        },
        error: function () {
            alert('Error occurred while retrieving details, contact system administrator');
        }
    });
}

function searchNearByProperties(){
  
    if (distanceMtxInfo != null) {
        var data = JSON.stringify(distanceMtxInfo);

        console.log(escapeHtml(data));

        var form = $('<form enctype="application/json" action="/properties/getNearbyProperties" method="POST">' +
          '<input type="hidden" name="distanceMtxInfo" value="' + escapeHtml(data) + '">' +
          '</form>');

        $(document.body).append(form);

        form.submit();
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

    return text.replace(/[&<>"']/g, function(m) { return map[m]; });
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

    populateDivisionByCountryCode();

    var postBackInfoRaw = $('#_postBackInformation').val();

    if (postBackInfoRaw != 'null') {
        postBackInfo = JSON.parse(postBackInfoRaw);//information posted to the server for search

        $('#MinPrice').attr('value', postBackInfo.MinPrice);
        $('#MaxPrice').attr('value', postBackInfo.MaxPrice);
        $('#SearchTerm').attr('value', postBackInfo.SearchTerm);
    }
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

    //get all property purpose and loads select element with the values
    $.ajax({
        url: '/servicer/GetAllPropertyPurposeNames',
        type: 'get',
        success: function (data) {
            $.each(data, function (index, value) {
                $('#PropertyPurpose').append($('<option></option>').attr('value', value).text(value));
            });

            if (postBackInfo != null) {
                $('#PropertyPurpose option[value="' + postBackInfo.PropertyPurpose + '"').prop('selected', true);
            }
        },
        error: function () {
            alert('Error occurred while retrieving property types, contact system administrator');
        }
    });

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
            scrollTop: $('#property-reviews').offset().top
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

        var searchType = $('input[name=searchType]:checked').val();

        if (searchType == 'all') {
            $('#search-properties-form').submit();
        } else {
            //coordinates of the near by place/establishment
            var orLat = $('#coordinateLat').val();
            var orlng = $('#coordinateLng').val();

            //retrieve coordinates for all / conditioned properties within the database
            $.ajax({
                url: '/properties/GetPropertiesCoordinates',
                type: 'get',
                data: $('#search-properties-form').serialize(),
                success: function (data) {
                    
                    var mvcArray = populateMapsMVCArray(data);
                    //encoding coordinates to compress url in the event that the URl exceeds its limit
                    var encodedCoordinatesUrl = google.maps.geometry.encoding.encodePath(mvcArray);
                    
                    setDistanceMatrixInformation(orLat, orlng, encodedCoordinatesUrl);
                },
                error: function () {
                    alert('Error occurred while retrieving property coordinates, contact system administrator');
                }
            });
        }

    });
});