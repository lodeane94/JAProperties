var placeSearch, autocomplete, searchType = null;
var componentForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'short_name',
    country: 'long_name',
    postal_code: 'short_name'
};

//sets up listener for the search near by places
(function () {
    searchType = $('#searchType:checked').val();

    initAutocomplete();
   
    $('input[type=radio][name=searchType]').change(function () {
        searchType = $(this).val();
        initAutocomplete();
    });
})();

function initAutocomplete() {
    if (searchType != null && searchType == 'nearByPlaces') {
        // Create the autocomplete object, restricting the search to geographical
        // location types.
        autocomplete = new google.maps.places.Autocomplete(
            /** @type {!HTMLInputElement} */(document.getElementById('SearchTerm')),
            { types: ['establishment'] });

        // When the user selects an address from the dropdown, populate the address
        // fields in the form.
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
function geolocate() {
    if (searchType != null && searchType == 'nearByPlaces') {
       // if (navigator.geolocation) {
           // navigator.geolocation.getCurrentPosition(function (position) {
              /*  var geolocation = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                };*/
                initializeGeoCoder();
                var circle = new google.maps.Circle({
                    center: countrybounds,
                    radius: countrybounds
                });
                autocomplete.setBounds(countrybounds);
           // });
      //  }
    }
}
////////////////////////////////////////////////////////
var geocoder;
var countrybounds;

function setCountryBounds() {
    var country = $('#country').find("option:selected").text();

    geocoder.geocode({ address: country }, function (locations, status) {
        if (status == 'OK') {
          /*  var north = locations[0].geometry.location.lat() + 0.0003;
            var south = locations[0].geometry.location.lat() - 0.0003;
            var east =  locations[0].geometry.location.lng() + 0.0003;
            var west =  locations[0].geometry.location.lng() - 0.0003;*/
          ///  alert(north);

            //countrybounds = new google.maps.LatLngBounds(new google.maps.LatLng(south, west),
            //                                new google.maps.LatLng(north, east));

            countrybounds = new google.maps.LatLngBounds({ lat: locations[0].geometry.location.lat(), lng: locations[0].geometry.location.lng()});
        } else {
            alert('Geocode was not successful for the following reason: ' + status);
        }
    });
}

function initializeGeoCoder() {
    geocoder = new google.maps.Geocoder();

    setCountryBounds();
}

function codeAddress() {

    var lat = $('#coordinateLat');
    var lng = $('#coordinateLng');

    var coordinateElements = {
        streetAddress: $('#streetaddress').val(),
        community: $('#community').val()
    };
    
    $.each(coordinateElements, function (index, value) {
        geocoder.geocode({ address: value , bounds: countrybounds}, function (results, status) {
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
