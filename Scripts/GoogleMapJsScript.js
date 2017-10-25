﻿var placeSearch, autocomplete, searchType = null;
var geocoder = null;
var mapCountryBounds, countryBounds = null;
var elementCalledBy = null;

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
/*
(function () {
    searchType = $('#searchType:checked').val();

    $('input[type=radio][name=searchType]').change(function () {
        searchType = $(this).val();
        initAutocomplete('SearchTerm');//called by the search term text box
    });
})();*/

//function initiate the autocomplete object
//if it is called by the searchterm element then it should filter results by establishments
//otherwise it should filter by address
function initAutocomplete(calledBy) {
    elementCalledBy = calledBy;

    searchType = $('#searchType:checked').val();

    if ((searchType != null && searchType == 'nearByPlaces' && elementCalledBy == 'SearchTerm')
        || elementCalledBy == 'nearBy') {

        autocomplete = new google.maps.places.Autocomplete((document.getElementById(elementCalledBy)), { types: ['establishment'] });
        autocomplete.addListener('place_changed', fillInAddress);
    } else if (elementCalledBy == 'StreetAddress' || elementCalledBy == 'community' || elementCalledBy == 'nearBy') {

        autocomplete = new google.maps.places.Autocomplete((document.getElementById(elementCalledBy)), { types: ['address'] });
        autocomplete.addListener('place_changed', fillInAddress);
    }
}
//call back function that is called after selecting a location
//from the google api autocomplete functionality
//TODO clear appropriate coordinate html element onfocus
function fillInAddress() {
    // Get the place details from the autocomplete object.
    //var place = autocomplete.getPlace();
   // console.log(place.address_components);
    var element = null;
    var lat, lng = null;

    //setting coordinates
    if (elementCalledBy == 'SearchTerm') {
        element = $('#SearchTerm');

        lat = $('#coordinateLat');
        lng = $('#coordinateLng');
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
        //community coordinates
        lat = $('#nearByCoordinateLat');
        lng = $('#nearByCoordinateLng');
    }
    var output = element.val();

    setLocation(output, lat, lng);
    //remove string starting from the first comma location in the result
    
    var firstCommaIndex = output.indexOf(',');
    var newOutput = output.substring(0, firstCommaIndex);
    element.val(newOutput);
}

//geolocate function is used to bind results returned by the autocomplete list
//within the specified country selected
function geolocate() {
    if ((searchType != null && searchType == 'nearByPlaces' && elementCalledBy == 'SearchTerm')
        || (elementCalledBy == 'StreetAddress' || elementCalledBy == 'community' || elementCalledBy == 'nearBy')) {
        var map = null;
        var country = $('#country').find("option:selected").text();

        //validation: if country is null then alert user and return
        if (country == null) {
            alert('Select country first');
            return;
        }

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
//function sets the value on the appropriate coordinate hidden element
function setLocation(address, lat, lng) {

    /*   var coordinateElements = {
           streetAddress: $('#streetaddress').val(),
           community: $('#community').val()
       };*/
    geocoder.geocode({ address: address }, function (results, status) {
        if (status == 'OK') {
            lat.val(results[0].geometry.location.lat());
            lng.val(results[0].geometry.location.lng());
        } else {
            alert('Geocode was not successful for the following reason: ' + status);
        }
    });
}
