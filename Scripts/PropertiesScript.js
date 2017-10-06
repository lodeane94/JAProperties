var userlocation = { country: '', division: '' };

//populates the specified selected element with its options
function populateSelectElement(element, options) {
    element.append(options);
}

//get all countries and populate the country select element
$.ajax({
    type: "get",
    contentType: 'application/json',
    url: '//westclicks.com/webservices/?f=json',
    success: function (data) {
        alert(data);
        populateSelectElement($('#country'), data);
    }
});

//detect location of user
$.getJSON('//freegeoip.net/json/?callback=?', function (data) {
    
});