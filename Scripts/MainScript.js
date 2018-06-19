//Defining Global Variables
var isCollapsable = false;
var isClickableContentValid = true;
var lastSelectedClickablePropertyCategory = null;
var lastSelectedClickablePropertyPurpose = null;
var lastSelectedClickableAdvertismentType = null;
var lastSelectedClickableSubscriptionType = null;
var lastSelectedClickableAdvertismentPriority = null;
var lastSelectedSubscriptionPeriod = null;
var propertyTagsSelected = [];
var loadingGifLocation = null;//element at which the loading gif will be displayed
var errMessages = [];//array of error messages related to the submission of a property 

//used to display a loading element
var loadingGifHTML = '<div id="loading-gif" class="col-xs-3">'
    + '<img src="/Content/ajax-loader-dark.gif" />'
    + '</div>';

//enable bootstrap tooptip display
(function () {
    //initialization of bootstrap popover
    $('[data-toggle="popover"]').popover({
        trigger: 'hover focus',
        placement: 'top',
        container: 'body'
    });
})();

/***GENERAL FUNCTIONS*/
//initialize functions to be called by SignalR
function initializeSockets() {
    var applicationHub = $.connection.applicationHub;

    $.connection.hub.logging = true;

    $.connection.hub.start().done(function () {
        console.log('Connection establised');
    });

    applicationHub.client.updateUserSessionCount = function (usersSessionCount) {
        updateVisitorCount(usersSessionCount);
    }
}
function getVisitorsCount() {
    $.ajax({
        url: '/servicer/GetVisitorsCount',
        type: 'get',
        success: function (data) {
            updateVisitorCount(data);
        },
        error: function () {
            alert('An error occurred while retrieving visitors count');
        }
    });
}
//updates the visitor-count field 
function updateVisitorCount(usersSessionCount) {
    $('#visitor-count').html(usersSessionCount);
}
//go back to previous page
function goBack() {
    window.history.back();
}
//highlights navigation link curRently active
function setActiveNavigation(id) {
    if (id != null && id != '')
        $('#' + id).attr('class', 'active');
    else
        $('#home').attr('class', 'active');
}

//displays jumbotrun container
function hideJumbotrun() {
    $('#jumbotron-container').hide();
}

//dynamically change the content that will be presented in the jumbotrun
//it accepts in html format
function setJumbotRunContent(content) {
    $('#jumbotron-container').html(content);
}

//sets the text that should be written on the jumbotrun button
function setJumbotrunActionBtnText(value) {
    $('#jumbotron-button').text(value);
}

//sets the isCollapsable action flag for the jumbotrun button; this decides whether or not
//the jumbotrun container should be hidden whenever it's button has been click
function setIsCollapsable(value) {
    isCollapsable = value;
}

//loads the subscription period select element with it;s values
//load subscription period values
function loadAdvertisePropertySubscriptionPeriod() {
    var monthVar = '';
    for (var i = 1; i <= 12; i++) {
        if (i > 1) {
            monthVar = 'Months';
        } else {
            monthVar = 'Month';
        }

        $('#subscription-period').append($('<option></option>').attr('value', i).text(i + ' ' + monthVar));
    }
}

//decides the action to be taken when the jumbotrun button is clicked
$('#jumbotron-button').click(function () {
    if (isCollapsable) {
        $('#jumbotron-container').slideUp(500);
    }
});

function getSubscriptionCosts() {
    var subscriptions = $('#subscription-type .clickable-box-link .clickable-box');
    var subscriptionItemCosts = { Basic: 0, Realtor: 0, Landlord: 0 };

    subscriptions.each(function (index, value) {
        var subscriptionName = $(this).attr('id');
        var subscriptionCost = $(this).children('.monthly-cost').html();

        switch (subscriptionName) {
            case 'Basic': subscriptionItemCosts.Basic = subscriptionCost;
                break;
            case 'Realtor': subscriptionItemCosts.Realtor = subscriptionCost;
                break;
            case 'Landlord': subscriptionItemCosts.Landlord = subscriptionCost;
                break;
        }
    });

    return subscriptionItemCosts;
}

//updates the shopping cart with the selected items
function updateShoppingCart() {
    // alert(lastSelectedClickableSubscriptionType)
    // alert($('#subscription-period').val())

    if (lastSelectedClickableSubscriptionType != null && $('#subscription-period').val()) {
        //var advertismentPriorityItemCost = { Regular: 0, AdPro: 1000, AdPremium: 4000 };
        var subscriptionItemCost = getSubscriptionCosts();
        var period = $('#subscription-period').val();
        var totalCost = 0.00;
        var monthsTerm = period > 1 ? 'Months' : 'Month';

        $('.shopping-cart-container').empty();

        /* $.each(advertismentPriorityItemCost, function (index, value) {
             switch (index) {
                 case lastSelectedClickableAdvertismentPriority.attr('id'):
                     $('.shopping-cart-container')
                         .append('<br />'
                         + '<div class="col-md-4"><span class="ctnr-info">' + 'Priority : ' + index + '</span></div>'
                         + '<div class="col-md-4"><span class="ctnr-info">' + period + '</span></div>'
                         + '<div class="col-md-4"><span class="ctnr-info">' + '$' + value + '.00 JMD' + '</span></div>'
                         );
                     totalCost += value;
                     break;
             }
         });*/

        $.each(subscriptionItemCost, function (index, value) {
            switch (index) {
                case lastSelectedClickableSubscriptionType.attr('id'):
                    $('.shopping-cart-container')
                        .append('<br />'
                        + '<div class="col-md-4"><span class="ctnr-info">' + '<b>Subscription</b> : ' + index + '</span></div><br/>'
                        + '<div class="col-md-4"><span class="ctnr-info">' + '<b>Period</b> : ' + period + ' ' + monthsTerm + '</span></div><br/>'
                        + '<div class="col-md-4"><span class="ctnr-info">' + '<b>Price</b> : ' + '$' + value + '.00 JMD' + '</span></div><br/> '
                        );
                    totalCost += value;
                    break;
            }
        });

        $('.shopping-cart-container').append('<hr />'
            + '<div class="col-md-12"><div class="float-md-right" style="border-bottom:1px solid #000000"><b>Total</b></div></div>'
            + '<br/><div class="col-md-12"><div class="float-md-right"> ' + '$' + (totalCost * period) + '.00 JMD' + '</div></div>');
    }
}

//adds the clickable items to a hidden form input to be submitted
function addHiddenInputToForm(name, value) {
    var input = $('<input>').attr('type', 'hidden').attr('name', name).val(value);
    $('#ad-submission').append(input);
}

//highlights the field that was not successfully validated
function highlightErrorField(field) {
    field.addClass('error');
}

//removes the field that was successfully validated
function removeErrorField(field) {
    field.removeClass('error');
}

//displays error messages from property upload
function displayErrorMessages() {
    if (errMessages != null && errMessages.length > 0) {
        $('.error-container').empty();

        $.each(errMessages, function (index, value) {
            $('.error-container').append(value + '<br>').show();
        });
    }
}

//adds error messages to the errMessages Array
function addErrorMessage(msg) {
    //insert error message if it is not already in the array
    if (errMessages.indexOf(msg) == -1) {
        errMessages.push(msg);
    }
}

//displays registration confirmation message for 10s
function displayFadedAnnouncment() {
    $('#fadedAnnouncment').fadeIn('slow', function () {
        $(this).fadeOut(20000);
    })
}

//navigates browser to the homepage of the application
function returnToHome() {
    window.location.href = window.location.protocol + '//' + window.location.host + '/';
}

//navigates browser to the sign in page of the application
function returnToSignInPage() {
    window.location.href = window.location.protocol + '//' + window.location.host + '/' + 'accounts/signin';
}

//navigates browser to the sign in page of the application
function returnToDashboardPage() {
    window.location.href = window.location.protocol + '//' + window.location.host + '/' + 'landlordManagement/dashboard';
}

//navigates browser to the URL passed into the URL
function navigateToUrl(url) {
    window.location.href = window.location.protocol + '//' + window.location.host + '/' + url;
}

//sets configuration for the modal display
var system = function () {

    this.modalIndex = 2000;

    this.showModal = function (selector) {
        this.modalIndex++;

        $(selector).modal({
            backdrop: 'static',
            keyboard: true
        });
        $(selector).modal('show');
        $(selector).css('z-index', this.modalIndex);
    }

}

var sys = new system();

/***GENERAL FUNCTIONS*/
/***DATA RETRIEVAL FUNCTIONS*/
function loadPropertyTypes(selectedItem) {
    /*loading property type drop down list with the appropriate names*/
    loadingGifLocation = $('#property-type').parent();

    $.ajax({
        url: '/servicer/GetPropertyTypesByCategoryName',
        type: 'get',
        data: { propertyCategoryName: selectedItem },
        beforeSend: function () { $(loadingGifHTML).insertAfter(loadingGifLocation); },
        success: function (data) {
            $('#property-type').prop('disabled', false);
            $('#property-type').empty();
            $.each(data, function (index, value) {
                $('#property-type').append($('<option></option>').attr('value', value).text(value));
            });
        },
        error: function () {
            alert('An error occurred while retrieving property types');
        },
        complete: function () { $('#loading-gif').remove(); }
    });
}

function loadPropertyTags(selectedItem) {
    loadingGifLocation = $('#property-tags');

    $.ajax({
        url: '/servicer/GetTagNamesByPropertyCategoryCode',
        type: 'get',
        data: { propertyCategoryName: selectedItem },
        beforeSend: function () { $(loadingGifHTML).appendTo(loadingGifLocation); },
        success: function (data) {
            $('#property-tags').empty();
            $.each(data, function (index, value) {
                $('#property-tags').append(
                    '<div class="col-md-2">'
                    + '  <a href="#" class="clickable-box-link">'
                    + '  <div id="' + value + '" class="clickable-box">' + value + '</div>'
                    + '  </a></div> '
                );
                if (index % 5 == 0 && index != 0) {
                    $('#property-tags').append('<br /><br /><br />');
                }
            });

            if ($.isEmptyObject(data)) {
                $('#property-tags').append(
                    '<div class="col-xs-12"><em>No Tags Found For Selected Property Category</em></div>'
                );
            }
        },
        error: function () {
            alert('An error occurred while retrieving property tags');
        },
        complete: function () { $('#loading-gif').remove(); }
    });
}

//checks the email address for an active subscription
function SubscriptionCheck(email) {
    $.ajax({
        url: '/accounts/SubscriptionCheck',
        type: 'get',
        data: { email: email },
        beforeSend: function () { },
        success: function (data) {
            if (data.HasErrors) {
                $.each(data.ErrorMessages, function (inx, val) {
                    alert(val);
                });

                throw "Error encountered";
            } else if (data.HasBool) {

                if (data.ReturnedBool) {
                    isAddPropertyFormInputsValid = false;
                    var errMsg = 'This email address is already associated with a subscription <br/> Please ' +
                        'log into your account, extend your subscription if necessary then click on the add property button ' +
                        'on your dashboard';

                    addErrorMessage(errMsg);
                    displayErrorMessages();
                    $('.next-advertise-section').addClass('disabled');
                    $('.next-advertise-section').prop('disabled', true);
                }
            }
        },
        error: function () {
            alert('An error occurred while checking subscription');
        },
        complete: function () { }
    });
}

/***DATA RETRIEVAL FUNCTIONS*/

/***ADVERTISEPROPERTY FUNCTIONS*/
(function () {
    //enables disabled elements
    function enableElements(elements) {
        $.each(elements, function (index, value) {
            $(value).prop('disabled', false);
        });
    }

    //disables enabled elements
    function disableElements(elements) {
        $.each(elements, function (index, value) {
            $(value).prop('disabled', true);
        });
    }

    //displays animated tool tip when a clickable box is hovered
    /*  $('.clickable-box-link a').hover(function () {
          var html = '<div class="tooltip-container">'
                      + '<div class="arrow-top"></div></div>';
  
          $(html).insertAfter(this).hide().slideDown('fast');
      }, function () {
          $('.tooltip-container').slideUp('fast', function () {
              $(this).remove();
          });
      });*/

    //makes the clickable box active when clicked and handles the shift and loads the tags for the appropriate property categories
    //property type category
    $('#property-category .clickable-box').click(function (event) {
        event.preventDefault();

        var selectedItem = $(this).attr('id');

        if (lastSelectedClickablePropertyCategory == null) {
            $(this).addClass('clickable-box-active');
            lastSelectedClickablePropertyCategory = $(this);
        } else {
            lastSelectedClickablePropertyCategory.removeClass('clickable-box-active');
            $(this).addClass('clickable-box-active');
            lastSelectedClickablePropertyCategory = $(this);
        }

        //enabling the area element if realestate or lot is selected
        var elements = ['#area'];

        if (selectedItem == 'RealEstate' || selectedItem == 'Lot')
            enableElements(elements);
        else
            disableElements(elements);

        //enabling the gendepreference,TotAvailableBathroom,TotRooms,occupancy element if realestate and (Rent or Lease) is selected
        var elements = ['#GenderPreferenceCode', '#TotAvailableBathroom', '#TotRooms', '#occupancy'];
        if (lastSelectedClickableAdvertismentType != null && selectedItem == 'RealEstate'
            && (lastSelectedClickableAdvertismentType.attr('id') == 'Rent'
                || lastSelectedClickableAdvertismentType.attr('id') == 'Lease'))
            enableElements(elements);
        else if (lastSelectedClickableAdvertismentType != null && selectedItem == 'RealEstate'
            && lastSelectedClickableAdvertismentType.attr('id') == 'Sale') {
            var elements = ['#TotAvailableBathroom', '#TotRooms'];
            enableElements(elements);
            var elements = ['#GenderPreferenceCode', '#occupancy'];
            disableElements(elements);
        } else
            disableElements(elements);
        //loads the property type drop down list
        loadPropertyTypes(selectedItem);

        //loads the appropriate property tags for the selected category
        loadPropertyTags(selectedItem);

        var field = $('#property-category');
        removeErrorField(field);

    });

    //makes the clickable box active when clicked and handles the shift 
    //property purpose category
    $('#property-purpose .clickable-box').click(function (event) {
        event.preventDefault();

        var selectedItem = $(this).attr('id');

        if (lastSelectedClickablePropertyPurpose == null) {
            $(this).addClass('clickable-box-active');
            lastSelectedClickablePropertyPurpose = $(this);
        } else {
            lastSelectedClickablePropertyPurpose.removeClass('clickable-box-active');
            $(this).addClass('clickable-box-active');
            lastSelectedClickablePropertyPurpose = $(this);
        }

        var field = $('#property-purpose');
        removeErrorField(field);
    });

    //makes the clickable box active when clicked and handles the shift 
    //advertisment type category
    $('#advertisment-type .clickable-box').click(function (event) {
        event.preventDefault();

        if (lastSelectedClickablePropertyCategory != null) {
            var selectedItem = $(this).attr('id');

            if (lastSelectedClickableAdvertismentType == null) {
                $(this).addClass('clickable-box-active');
                lastSelectedClickableAdvertismentType = $(this);
            } else {
                lastSelectedClickableAdvertismentType.removeClass('clickable-box-active');
                $(this).addClass('clickable-box-active');
                lastSelectedClickableAdvertismentType = $(this);
            }

            //enabling the gendepreference,TotAvailableBathroom,TotRooms,occupancy element if realestate and (Rent or Lease) is selected
            var elements = ['#GenderPreferenceCode', '#TotAvailableBathroom', '#TotRooms', '#occupancy'];

            if ((selectedItem == 'Rent' || selectedItem == 'Lease')
                && lastSelectedClickablePropertyCategory.attr('id') == 'RealEstate')
                enableElements(elements);
            else if (selectedItem == 'Sale' && lastSelectedClickablePropertyCategory.attr('id') == 'RealEstate') {
                var elements = ['#TotAvailableBathroom', '#TotRooms'];
                enableElements(elements);
                var elements = ['#GenderPreferenceCode', '#occupancy'];
                disableElements(elements);
            } else
                disableElements(elements);

            var field = $('#advertisment-type');
            removeErrorField(field);
        } else {
            alert('Select property type first');
        }

    });

    //makes the clickable box active when clicked and handles the shift 
    //subscription type category
    $(document.body).on('click', '#subscription-type .clickable-box', function (event) {
        event.preventDefault();
        var selectedItem = $(this).attr('id');

        if (lastSelectedClickableSubscriptionType == null) {
            $(this).addClass('clickable-box-active');
            lastSelectedClickableSubscriptionType = $(this);
        } else {
            lastSelectedClickableSubscriptionType.removeClass('clickable-box-active');
            $(this).addClass('clickable-box-active');
            lastSelectedClickableSubscriptionType = $(this);
        }


        $('#subscription-period').prop('disabled', false);

        //basic subscription should be free to advertise for one month
        if (selectedItem == 'Basic')
            $("select#subscription-period").prop('selectedIndex', 1);

        //accounts should only be created for the subscription type of a realtor/landlord
        //2017-03-11 : removed as decision was made to allow every one to have an account
        /* var elements = ['#password', '#ConfirmPassword'];
 
         if (selectedItem == 'Landlord' || selectedItem == 'Realtor')
             enableElements(elements);
         else
             disableElements(elements);*/

        updateShoppingCart();

        var field = $('#subscription-type');
        removeErrorField(field);
    });

    //makes the clickable box active when clicked and handles the shift 
    //subscription type category
    $('#advertisment-priority .clickable-box').click(function (event) {
        event.preventDefault();
        if (lastSelectedClickableAdvertismentPriority == null) {
            $(this).addClass('clickable-box-active');
            lastSelectedClickableAdvertismentPriority = $(this);
        } else {
            lastSelectedClickableAdvertismentPriority.removeClass('clickable-box-active');
            $(this).addClass('clickable-box-active');
            lastSelectedClickableAdvertismentPriority = $(this);
        }

        //updateShoppingCart();

        var field = $('#advertisment-priority');
        removeErrorField(field);
    });

    //makes the clickable box active or inactive when clicked and stores the variable clicked
    //property-tags
    $(document).on('click', '#property-tags .clickable-box', function (event) {
        event.preventDefault();

        var isActive = $(this).hasClass('clickable-box-active');
        var tagSelected = $(this).attr('id');

        if (!isActive) {
            $(this).addClass('clickable-box-active');
            propertyTagsSelected.push(tagSelected);
        } else {
            //find index of item to remove
            var tagIndex = propertyTagsSelected.indexOf(tagSelected);
            propertyTagsSelected.splice(tagIndex, 1);
            $(this).removeClass('clickable-box-active');
        }
    });

    //validates populates subscription period drop down list element with the appropriate options
    $('#subscription-period').click(function () {
        if (lastSelectedClickablePropertyCategory != null
            || lastSelectedClickableAdvertismentType != null
            || lastSelectedClickableSubscriptionType != null) {

            updateShoppingCart();

        } else {
            alert('Select a subscription type first');
        }
    });

    $('#subscription-period').change(function () {
        var newPeriod = $(this).val();

        if (lastSelectedSubscriptionPeriod == null)
            lastSelectedSubscriptionPeriod = newPeriod;

        if (lastSelectedClickableSubscriptionType.attr('id') == 'Basic') {
            if (newPeriod != lastSelectedSubscriptionPeriod) {
                alert('The basic subscription period cannot exceed 1 month');
                $(this).prop('selectedIndex', 1);

                lastSelectedSubscriptionPeriod = newPeriod;
            }
        } else
            lastSelectedSubscriptionPeriod = newPeriod;
    });

    //outputs the name of the selected company logo image that were selected
    $('#organizationLogo').change(function () {

        var file = $(this)[0].files[0];

        if (file != undefined) {
            var filenameOutput = file.name;

            $('#organizationFileNameOutput').val(filenameOutput);//outputing to the textbox
        }
    });

    //outputs the selected images that were selected
    $('#flPropertyPics').change(function () {
        var files = $(this)[0].files;
        var filenameOutput = '';

        //clearing any image from the image output
        $('.flPropertyPics').remove();

        $.each(files, function (index, value) {
            filenameOutput += value.name;

            //prevents comma from being at the end  of the list
            if (index < files.length - 1)
                filenameOutput += ', ';

            //outputing images
            $('.flPropertyPicsImageOutput').append(''
                + '<div class="col-xs-3 flPropertyPics">'
                + '<a href="#"><img id="' + value.name + '"/></a>'
                + '</div>').hide().fadeIn('slow');

            var img = document.getElementById(value.name);//$('#' + value.name)[0];
            img.file = value;

            var reader = new FileReader();

            reader.onload = (function (aimg) {
                return function (e) {
                    aimg.src = e.target.result;
                };
            })(img);

            reader.readAsDataURL(value);
        });

        $('#flPropertyPicsFileNameOutput').val(filenameOutput);//outputing to the textbox

        var field = $('#flPropertyPicsFileNameOutput');

        isClickableContentValid = true;
        removeErrorField(field);
    });

    $('#submitEnrolment').click(function (event) {
        event.preventDefault();

        var validator = $('#EnrollTennantForm').validate({
            rules: {
                password: {
                    minlength: 5
                },
                ConfirmPassword: {
                    minlength: 5,
                    equalTo: password
                }
            },

            messages: {
                password: {
                    minlength: 'Password should be at least 5 characters long'
                },
                ConfirmPassword: {
                    minlength: 'Password should be at least 5 characters long',
                    equalTo: 'Password and Confirm Password values must be the same value'
                }
            },

            errorPlacement: function (error, element) { },

            showErrors: function (errorMap, errorList) {
                $.each(errorList, function () {
                    addErrorMessage(this.message);
                });

                displayErrorMessages();

                this.defaultShowErrors();
            }
        });

        var isValid = validator.form();

        if (isValid) {
            loadingGifLocation = $('.enrollBtn');
            var formData = new FormData($('#EnrollTennantForm')[0]);

            $.ajax({
                url: '/accounts/enroll',
                type: 'Post',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () { $(loadingGifHTML).insertAfter(loadingGifLocation); },
                success: function (data) {
                    if (data.hasErrors) {
                        $.each(data.ErrorMessages, function (index, value) {
                            addErrorMessage(value);
                        });

                        displayErrorMessages();

                        $('html, body').animate({
                            scrollTop: $('.error-container').offset().top
                        }, 'fast');
                    } else {
                        sys.showModal('#tennantCreatedModal');
                    }
                },
                error: function (data) { alert('Error encountered while creating user. Contact Website Administrator'); },
                complete: function () { $('#loading-gif').remove(); },
            });
        }

    });

    $('#signUpBtn').click(function (event) {
        event.preventDefault();

        var validator = $('#signUpUser').validate({
            rules: {
                password: {
                    minlength: 5
                },
                ConfirmPassword: {
                    minlength: 5,
                    equalTo: password
                }
            },

            messages: {
                password: {
                    minlength: 'Password should be at least 5 characters long'
                },
                ConfirmPassword: {
                    minlength: 'Password should be at least 5 characters long',
                    equalTo: 'Password and Confirm Password values must be the same value'
                }
            },

            errorPlacement: function (error, element) { },

            showErrors: function (errorMap, errorList) {
                $.each(errorList, function () {
                    addErrorMessage(this.message);
                });

                displayErrorMessages();

                this.defaultShowErrors();
            }
        });

        var isValid = validator.form();

        if (isValid) {
            loadingGifLocation = $('.signUpBtn');
            var formData = new FormData($('#signUpUser')[0]);

            $.ajax({
                url: '/accounts/signup',
                type: 'Post',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () { $(loadingGifHTML).insertAfter(loadingGifLocation); },
                success: function (data) {
                    if (data.hasErrors) {
                        $.each(data.ErrorMessages, function (index, value) {
                            addErrorMessage(value);
                        });

                        displayErrorMessages();

                        $('html, body').animate({
                            scrollTop: $('.error-container').offset().top
                        }, 'fast');
                    } else {
                        sys.showModal('#userCreatedModal');
                    }
                },
                error: function (data) { alert('Error encountered while creating user. Contact Website Administrator'); },
                complete: function () { $('#loading-gif').remove(); },
            });
        }

    });

    $('#AdvertisePropertyBtn').click(function (event) {
        event.preventDefault();
        //TODO create better error handlers for clickable contents
        //var genderPreferenceCode = $('#GenderPreferenceCode');
        //var isReviewable = $('#isReviewable');

        //adds hidden fields containing the selected tags
        if (propertyTagsSelected != null) {
            $.each(propertyTagsSelected, function (index, value) {
                addHiddenInputToForm('selectedTags', value);
            });
        }

        var uploadedFiles = $('#flPropertyPics')[0].files;

        validateSection3(uploadedFiles);
        /*
        if (lastSelectedClickableAdvertismentPriority != null) {
            addHiddenInputToForm('advertismentpriority', lastSelectedClickableAdvertismentPriority.attr('id'));
        } else {
            //       alert('Select advertisment priority');
            isClickableContentValid = false;

            var field = $('#advertisment-priority');
            highlightErrorField(field);
        }*/

        if (isClickableContentValid && isAddPropertyFormInputsValid) {
            var formData = new FormData($('#ad-submission')[0]);

            $.ajax({
                url: '/accounts/advertiseproperty',
                type: 'Post',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: function () { $('#modal-loading').fadeIn(); },
                success: function (data) {
                    if (data.hasErrors) {
                        alert('Error encountered while uploading property. Contact Website Administrator');
                        $.each(data.ErrorMessages, function (index, value) {
                            addErrorMessage(value);
                        });

                        displayErrorMessages();

                        $('html, body').animate({
                            scrollTop: $('.error-container').offset().top
                        }, 'fast');
                    } else {
                        sys.showModal('#propertyModal');
                    }
                },
                error: function (data) { alert('Error encountered while uploading property. Contact Website Administrator'); },
                complete: function () { $('#modal-loading').fadeOut(); },
            });
        } else {
            var errMessage = 'Highlight fields are required';

            if (errMessages != null && errMessages.length > 0)
                addErrorMessage('<br>');

            displayErrorMessages();

            $('html, body').animate({
                scrollTop: $('.error-container').offset().top
            }, 'fast');
        }
    });

    var isAddPropertyFormInputsValid = false;

    function validateSection1() {
        isClickableContentValid = true;

        isAddPropertyFormInputsValid = true;//set to true on section 1 since there are no form inputs there

        if (lastSelectedClickablePropertyCategory != null) {
            addHiddenInputToForm('propertycategory', lastSelectedClickablePropertyCategory.attr('id'));
        } else {
            isClickableContentValid = false;

            var field = $('#property-category');
            highlightErrorField(field);
        }

        if (lastSelectedClickablePropertyPurpose != null) {
            addHiddenInputToForm('propertypurpose', lastSelectedClickablePropertyPurpose.attr('id'));
        } else {
            isClickableContentValid = false;

            var field = $('#property-purpose');
            highlightErrorField(field);
        }

        if (lastSelectedClickableAdvertismentType != null) {
            addHiddenInputToForm('advertismenttype', lastSelectedClickableAdvertismentType.attr('id'));
        } else {
            isClickableContentValid = false;

            var field = $('#advertisment-type');
            highlightErrorField(field);
        }
    }

    function validateSection2() {
        var validator = null;
        isAddPropertyFormInputsValid = false;

        var passwordCheck = $('#password').attr('id');

        if (passwordCheck != null && passwordCheck != undefined) {
            validator = $('#ad-submission').validate({
                rules: {
                    password: {
                        minlength: 5
                    },
                    ConfirmPassword: {
                        minlength: 5,
                        equalTo: password
                    }
                },

                messages: {
                    password: {
                        minlength: 'Password should be at least 5 characters long',
                        equalTo: 'Password and Confirm Password values must be the same value'
                    },
                    ConfirmPassword: {
                        minlength: 'Password should be at least 5 characters long',
                        equalTo: 'Password and Confirm Password values must be the same value'
                    }
                },

                // onfocusout: false,
                // onkeyup: false,

                errorPlacement: function (error, element) { },

                showErrors: function (errorMap, errorList) {
                    $.each(errorList, function () {
                        //if (this.message != 'This field is required.')
                        addErrorMessage(this.message);
                    });

                    displayErrorMessages();

                    this.defaultShowErrors();
                }
            });
        } else {
            validator = $('#ad-submission').validate();
        }

        isAddPropertyFormInputsValid = validator.form();
    }

    function validateSection3(uploadedFiles) {
        var validator = null;
        isAddPropertyFormInputsValid = false;

        if (lastSelectedClickableSubscriptionType != null) {
            addHiddenInputToForm('subscriptiontype', lastSelectedClickableSubscriptionType.attr('id'));

        } else {
            //subscription type is not necessary if new property is being added from the management controller
            //hence do not do the following if that is the case
            var isCalledByManagement = $('#calledByManagement').val();
            if (!isCalledByManagement) {
                isClickableContentValid = false;

                var field = $('#subscription-type');
                highlightErrorField(field);
            }
        }

        if (uploadedFiles.length == 0) {
            isClickableContentValid = false;

            var field = $('#flPropertyPics');
            highlightErrorField(field);

            addErrorMessage('Upload at least one property image');
            displayErrorMessages();
        }


        validator = $('#ad-submission').validate();
        isAddPropertyFormInputsValid = validator.form();

    }

    $('.previous-advertise-section').click(function () {
        var sectionId = $(this).parents('.prop-upload-info').attr('id');

        var nextId = Number(sectionId) - 1;

        $('.prop-upload-info#' + sectionId).slideUp(400, function () {
            $(this).addClass('hide')

            $('.prop-upload-info#' + nextId).slideDown(400, function () {
                $(this).removeClass('hide');

                $('html, body').animate({
                    scrollTop: $('.error-container').offset().top
                }, 'fast');
            });
        });
    });

    $('.next-advertise-section').click(function () {
        //clear error messages upon each advancement
        errMessages = [];
        $('.error-container').html('');
        $('.error-container').css({ 'display': 'none' });

        var sectionId = Number($(this).parents('.prop-upload-info').attr('id'));

        switch (sectionId) {
            case 1: validateSection1();
                break;
            case 2: validateSection2();
                break;
        }

        if (isClickableContentValid && isAddPropertyFormInputsValid) {
            var nextId = sectionId + 1;

            $('.prop-upload-info#' + sectionId).slideUp(400, function () {
                $(this).addClass('hide')

                $('.prop-upload-info#' + nextId).slideDown(400, function () {
                    $(this).removeClass('hide');

                    $('html, body').animate({
                        scrollTop: $('.error-container').offset().top
                    }, 'fast');
                });
            });
        } else {
            $('html, body').animate({
                scrollTop: $('.error-container').offset().top
            }, 'fast');
        }
    });

    $('#resetPassword').click(function (event) {
        event.preventDefault();

        var form = $('#resetPasswordForm');

        var validator = validatePasswordForm();
        var isFormValid = validator.form();

        if (isFormValid) {
            form.submit();
        }
    });

    function validatePasswordForm() {
        return $('#resetPasswordForm').validate({
            rules: {
                password: {
                    minlength: 5
                },
                confirmPassword: {
                    minlength: 5,
                    equalTo: password
                }
            },

            messages: {
                password: {
                    minlength: 'Password should be at least 5 characters long',
                    equalTo: 'Password and Confirm Password values must be the same value'
                },
                confirmPassword: {
                    minlength: 'Password should be at least 5 characters long',
                    equalTo: 'Password and Confirm Password values must be the same value'
                }
            },

            errorPlacement: function (error, element) { },

            showErrors: function (errorMap, errorList) {
                $.each(errorList, function () {
                    //if (this.message != 'This field is required.')
                    addErrorMessage(this.message);
                });

                displayErrorMessages();

                this.defaultShowErrors();
            }
        });
    }

    //ensures that only numbers are entered into the MinPrice and MaxPrice fields
    $(document.body).on('keyup', '#cellnum, #price, #securitydeposit, #StreetNumber, #occupancy, #TotAvailableBathroom, #TotRooms, #area', function (e) {
        //if the letter is not digit then display error and don't type anything
        /*if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }*/
        var numericExpression = /^[0-9]+$/;
        if (this.value.match(numericExpression)) {
            return true;
        } else {
            $(this).val('');
        }
    });

    //checks if user already has a subscription
    $(document.body).on('blur', '#email', function (event) {
        try {
            var email = $(this).val();

            SubscriptionCheck(email);
        } catch (err) {
            alert(err);
        }
    });

})();
/***ADVERTISEPROPERTY FUNCTIONS*/
/***Enrolment Function*/
