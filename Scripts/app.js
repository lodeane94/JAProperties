(function () {
    'use-strict';//strict javascript mode

    config.$inject = ['$routeProvider', '$locationProvider'];
    //creation of angular module
    angular.module('dashboardApp', ['ngRoute'])
        .config(config);
    //configuring routes for the dashboard page
    function config ($routeProvider,$locationProvider){
        $routeProvider
            .when('/', {
                templateUrl: '/angularapp/views/home.html'
            });
        //$locationProvider.html5Mode(true);
    };

})();
