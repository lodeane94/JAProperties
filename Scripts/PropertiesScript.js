var selectedLocation = { country: '', countryCode: '', division: '' };
var postBackInfo = null;

//used to display a loading element
var loadingGifHTML = '<div id="loading-gif" class="col-xs-1">'
                     + '<img src="/Content/ajax-loader-dark.gif" />'
                     + '</div>';

function populateDivisionByCountryCode() {
    $('#division ').find("option:disabled").nextAll('option').remove();
    $.ajax({
        url: '/servicer/RequestJsonDataFromUrl',
        type: 'get',
        dataType: "json",
        data: { Url: 'http://westclicks.com/webservices/?f=json&c=' + selectedLocation.countryCode },
        success: function (data) {
            $.each(data, function (index, value) {
                $('#division').append($('<option></option>').attr('value', value).text(value));
            });

            if (postBackInfo != null) {
                $('#division option[value="' + postBackInfo.Division + '"').prop('selected', true);
            }
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
    var postBackInfoRaw = $('#_postBackInformation').val();

    if (postBackInfoRaw != null || postBackInfoRaw != undefined) {
        postBackInfo = JSON.parse(postBackInfoRaw);//information posted to the server for search

        $('#MinPrice').attr('value', postBackInfo.MinPrice);
        $('#MaxPrice').attr('value', postBackInfo.MaxPrice);
        $('#SearchTerm').attr('value', postBackInfo.SearchTerm);
    }
    //sets the menu to fixed after scrolling pass a certain amount of pixels
    $(window).scroll(function () {
        if ($(this).scrollTop() > 280) {
            $('.header-content').addClass('fixed');
            $('.search-bar-container').addClass('fixed-search');
        } else {
            $('.header-content').removeClass('fixed');
            $('.search-bar-container').removeClass('fixed-search');
        }
    });

    //get all countries and populate the country select element
    $.ajax({
        url: '/servicer/RequestJsonDataFromUrl',
        type: 'get',
        dataType: "json",
        data: { Url: 'http://westclicks.com/webservices/?f=json' },
        beforeSend: function () {
            $('#country').append($('<option></option>').attr('value', 'selectHolder').attr('selected', true).text('Detecting Country ...'));
        },
        success: function (countriesData) {
            //detect selectedLocation of user
            $.getJSON('//freegeoip.net/json/?callback=?', function (data) {
                selectedLocation.country = data.country_name;

                $.each(countriesData, function (index, value) {
                    if (selectedLocation.country.toLowerCase() == value.toLowerCase()) {
                        $('#country').append($('<option></option>').attr('value', value).attr('selected', true).text(value));
                        selectedLocation.countryCode = index;
                    } else {
                        $('#country').append($('<option></option>').attr('value', value).text(value));
                    }
                });

                populateDivisionByCountryCode();

                if (postBackInfo != null) {
                    $('#country option[value="' + postBackInfo.Country + '"').prop('selected', true);
                }
            });
        },
        complete: function () {
            $('#country option[value="selectHolder"]').remove();
        },
        error: function () {
            alert('Error occurred while retrieving countries, contact system administrator');
        }
    });

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
        selectedLocation.countryCode = $(this).val();

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
});