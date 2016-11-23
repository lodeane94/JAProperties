(function () {
    'use strict';

    angular
        .module('dashboardApp')
            .controller('PropertiesListsController', PropertiesListsController);
            //.controller('PropertiesAddController', PropertiesAddController);

    PropertiesListsController.$inject('$scope', 'PropertiesServices');

    function PropertiesListsController($scope, PropertiesServices) {

    }

        
})();