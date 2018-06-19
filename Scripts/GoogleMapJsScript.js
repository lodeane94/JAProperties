var placeSearch, autocomplete, searchType = null;
var geocoder = null;
var mapCountryBounds, countryBounds = null;
var elementCalledBy = null;
var address, shortAddress, establishment = null;

var componentForm = {
   // street_number: 'long_name',
    route: 'short_name',
    //locality: 'long_name',
    //administrative_area_level_1: 'short_name',
    //country: 'long_name',
    //postal_code: 'long_name',
    premise: 'long_name'
};

//function initiate the autocomplete object
//if it is called by the searchterm element then it should filter results by establishments
//otherwise it should filter by address
function initAutocomplete(calledBy) {

    //clear element first before any operation
   // $('#' + elementCalledBy).val('');
    var country = 'Jamaica';

    if (autocomplete != null) {
        google.maps.event.clearInstanceListeners(autocomplete);
        $(".pac-container").remove();
    }

    elementCalledBy = calledBy;

    searchType = $('#searchType:checked').val();

    geocoder = new google.maps.Geocoder();

    if ((searchType != null && searchType == 'nearByPlaces' && elementCalledBy == 'SearchTerm')
        || elementCalledBy == 'nearBy') {

        var options = {
            types: ['establishment'],
            componentRestrictions: { country: 'jm' }
        };

        autocomplete = new google.maps.places.Autocomplete((document.getElementById(elementCalledBy)), options);
        autocomplete.addListener('place_changed', fillInAddress);
    } else if (elementCalledBy == 'StreetAddress' || elementCalledBy == 'community' || calledBy == 'nearBy') {

        var options = {
            types: ['address'],
            componentRestrictions: { country: 'jm' }
        };

        autocomplete = new google.maps.places.Autocomplete((document.getElementById(elementCalledBy)), options);
        autocomplete.addListener('place_changed', fillInAddress);
    }
}
//call back function that is called after selecting a location
//from the google api autocomplete functionality
//TODO clear appropriate coordinate html element onfocus
function fillInAddress() {
    var element = null;
    var lat, lng = null;
    var isEstablishmentSearch = false;

    //setting coordinates
    if (elementCalledBy == 'SearchTerm') {
        element = $('#SearchTerm');

        lat = $('#coordinateLat');
        lng = $('#coordinateLng');

        isEstablishmentSearch = true;
    } else if (elementCalledBy == 'StreetAddress') {
        element = $('#StreetAddress');
        //street address coordinates
        lat = $('#saCoordinateLat');
        lng = $('#saCoordinateLng');
    } else if (elementCalledBy == 'community') {
        element = $('#community');
        //community coordinates
        lat = $('#cCoordinateLat');
        lng = $('#cCoordinateLng');
    } else if (elementCalledBy == 'nearBy') {
        element = $('#nearBy');
        //nearby coordinates
        lat = $('#nearByCoordinateLat');
        lng = $('#nearByCoordinateLng');

        isEstablishmentSearch = true;
    }
    
    setLocationComponents();

    if (!isEstablishmentSearch) {
        setLocation(address, lat, lng);
        element.val(shortAddress);
    } else {
        var output = element.val();
        setLocation(output, lat, lng);
        //remove string starting from the first comma location in the result
       // var firstCommaIndex = output.indexOf(',');
       // var newOutput = output.substring(0, firstCommaIndex);
       // element.val(newOutput);
    }
}

//geolocate function is used to bind results returned by the autocomplete list
//within the specified country selected
/*function geolocate(calledBy) {
    searchType = $('#searchType:checked').val();
    
    if ((searchType != null && searchType == 'nearByPlaces' && calledBy == 'SearchTerm')
        || (calledBy == 'StreetAddress' || calledBy == 'community' || calledBy == 'nearBy')) {
        var map = null;
        var country = 'Jamaica'//$('#country').find("option:selected").text();

        //validation: if country is null then alert user and return
        if (country == null) {
            alert('Select country first');
            return;
        }

        
        initAutocomplete(calledBy);
        geocoder.geocode({ address: country }, function (locations, status) {
            if (status == 'OK') {
                mapCountryBounds = { lat: locations[0].geometry.location.lat(), lng: locations[0].geometry.location.lng() };
                //mapCountryBounds = new google.maps.LatLngBounds({ lat: locations[0].geometry.location.lat(), lng: locations[0].geometry.location.lng()});

                map = new google.maps.Map(document.getElementById('map'), {
                    center: { lat: mapCountryBounds.lat, lng: mapCountryBounds.lng },
                    zoom: 7
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
*/
////////////////////////////////////////////////////////
//function sets the value on the appropriate coordinate hidden element
function setLocation(address, lat, lng) {

    /*   var coordinateElements = {
           streetAddress: $('#streetaddress').val(),
           community: $('#community').val()
       };*/
    geocoder.geocode({ address: address }, function (results, status) {
        if (status == 'OK') {
            lat.attr('value', results[0].geometry.location.lat());
            lng.attr('value', results[0].geometry.location.lng());
        } else {
            alert('Geocode was not successful for the following reason: ' + status);
        }
    });
}

function setLocationComponents() {   
    // Get the place details from the autocomplete object.
    var place = autocomplete.getPlace();
    
    if (place != undefined) {
        // Get each component of the address from the place details
        // and fill the corresponding field on the form.
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];

            if (addressType == 'route') {
                var val = place.address_components[i][componentForm[addressType]];
                shortAddress = val;
            }
        }
        address = place.formatted_address;
    } else {
        alert('Unable to identify coordinates for that location');
    }
}
