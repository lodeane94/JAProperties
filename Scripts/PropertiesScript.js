var selectedLocation = { country: '', countryCode: '', division: '' };

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
        data: {Url: 'http://westclicks.com/webservices/?f=json&c=' + selectedLocation.countryCode},
        success: function (data) {
            $.each(data, function (index, value) {
                $('#division').append($('<option></option>').attr('value', index).text(value));
            });
        },
        error: function () {
            alert('Error occurred while retrieving divisions, contact system administrator');
        }
    });
}

$(document).ready(function () {
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
                        $('#country').append($('<option></option>').attr('value', index).attr('selected', true).text(value));
                        selectedLocation.countryCode = index;
                    } else {
                        $('#country').append($('<option></option>').attr('value', index).text(value));
                    }
                });

                populateDivisionByCountryCode();
            });
        },
        complete: function () {
            $('#country option[value="selectHolder"]').remove();
        },
        error: function () {
            alert('Error occurred while retrieving countries, contact system administrator');
        }
    });

    $('#country').change(function () {
        selectedLocation.country = $(this).find("option:selected").text();
        selectedLocation.countryCode = $(this).val();
        
        populateDivisionByCountryCode();
    });

    $('#division').change(function () {
        selectedLocation.division = $(this).find("option:selected").text();
    });

});