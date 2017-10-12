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
        data: {propertyCategoryName : categoryName},
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

$(document).ready(function () {
    postBackInfo = JSON.parse($('#_postBackInformation').val());//information posted to the server for search
    if (postBackInfo != null) {
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

});