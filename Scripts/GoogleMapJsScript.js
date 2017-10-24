var placeSearch, autocomplete, searchType = null;
var geocoder;
var mapCountryBounds, countryBounds = null;

var componentForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'short_name',
    country: 'long_name',
    postal_code: 'short_name'
};

//sets up listener for the search near by places
//TODO allow init auto complete generic
(function () {
    searchType = $('#searchType:checked').val();

    initAutocomplete();

    $('input[type=radio][name=searchType]').change(function () {
        searchType = $(this).val();
        initAutocomplete();
    });
})();

//function initiate the autocomplete object
//if the search type is specified
function initAutocomplete() {
    if (searchType != null && searchType == 'nearByPlaces') {

        autocomplete = new google.maps.places.Autocomplete((document.getElementById('SearchTerm')), { types: ['establishment'] });
        autocomplete.addListener('place_changed', fillInAddress);
    }
}

function fillInAddress() {
    // Get the place details from the autocomplete object.
    var place = autocomplete.getPlace();

    /* for (var component in componentForm) {
         document.getElementById(component).value = '';
         document.getElementById(component).disabled = false;
     }
 
     // Get each component of the address from the place details
     // and fill the corresponding field on the form.
     for (var i = 0; i < place.address_components.length; i++) {
         var addressType = place.address_components[i].types[0];
         if (componentForm[addressType]) {
             var val = place.address_components[i][componentForm[addressType]];
             document.getElementById(addressType).value = val;
         }
     }*/
}

// Bias the autocomplete object to the user's geographical location,
// as supplied by the browser's 'navigator.geolocation' object.
//TODO bounds should be within the selected country rather than the gps coordinate of the user
function geolocate(calledBy) {
    if (searchType != null && searchType == 'nearByPlaces') {
        var map = null;
        var country = $('#country').find("option:selected").text();

        geocoder = new google.maps.Geocoder();

        geocoder.geocode({ address: country }, function (locations, status) {
            if (status == 'OK') {
                mapCountryBounds = { lat: locations[0].geometry.location.lat(), lng: locations[0].geometry.location.lng() };
                //mapCountryBounds = new google.maps.LatLngBounds({ lat: locations[0].geometry.location.lat(), lng: locations[0].geometry.location.lng()});
                map = new google.maps.Map(document.getElementById('map'), {
                    center: { lat: mapCountryBounds.lat, lng: mapCountryBounds.lng },
                    zoom: 0
                });

                autocomplete.bindTo('bounds', map);
                //autocomplete.setOptions({ strictBounds: true });
                //autocomplete.setBounds(mapCountryBounds);
            } else {
                alert('Geocode was not successful for the following reason: ' + status);
            }
        });
    }
}
////////////////////////////////////////////////////////

function codeAddress() {
    geocoder = new google.maps.Geocoder();
    var lat = $('#coordinateLat');
    var lng = $('#coordinateLng');

    var coordinateElements = {
        streetAddress: $('#streetaddress').val(),
        community: $('#community').val()
    };

    $.each(coordinateElements, function (index, value) {
        geocoder.geocode({ address: value, bounds: countryBounds }, function (results, status) {
            if (status == 'OK') {
                lat.val(results[0].geometry.location.lat());
                lng.val(results[0].geometry.location.lng());

                return false;
            } else {
                alert('Geocode was not successful for the following reason: ' + status);
            }
        });
    });

}
