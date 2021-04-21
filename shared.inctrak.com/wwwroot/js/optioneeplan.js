var optioneePlanApp = angular.module('optioneeplan', ['ngRoute', 'ngCookies', 'ngSanitize', 'ui.select']);

optioneePlanApp.directive('script', function () {
    return {
        restrict: 'E',
        scope: false,
        link: function (scope, elem, attr) {
            if (attr.type === 'text/javascript-lazy') {
                var code = elem.text();
                var f = new Function(code);
                //f();
            }
        }
    };
});

optioneePlanApp.controller('OptioneePlanCtrl', function ($scope, $rootScope, $route, $routeParams, $location, $window, $cookies) {
    $rootScope.metadata = {
        title: 'IncTrak',
        description: 'IncTrak provides easy access for employers to allow participants to manager their stock and options.',
        keywords: 'stock options grant plan startup employee benefits'
    };

    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };

    $scope.isAdmin = function () {
        return $cookies.Role == "admin";
    };

    $scope.isOptionee = function () {
        return $cookies.Role == "optionee";
    };

    $scope.loggedIn = function () {
        return $cookies.UUID != null && $cookies.UUID != "" && $cookies.UUID != "not set" && $cookies.UUID != "null" && $cookies.UUID != "undefined";
    };

    $scope.$watch(function () { return $location.path(); }, function (newValue, oldValue) {
        var uuid = $cookies.UUID;
        if ( (!uuid || uuid == "not set") && ($location.path().indexOf("/company") != -1 || $location.path().indexOf("/optionee") != -1)) {
            $location.path('/login');
        }
    });
});

function ctrlResolver($q, $rootScope, path) {
    var deferred = $q.defer();
    var script = document.createElement('script');
    script.src = path;
    script.onload = function () {
        $rootScope.$apply(deferred.resolve());
    };
    document.body.appendChild(script);
    return deferred.promise;
}

optioneePlanApp.config(function ($routeProvider) {
      $routeProvider.
        when('/', {
            templateUrl: '/home.html',
            controller: 'HomesCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/js/homes.js');
                }
            }
        }).
        when('/about', {
            templateUrl: '/etc/about.html',
            controller: 'AboutCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/about.js');
                }
            }
        }).
        when('/privacy_policy', {
            templateUrl: '/etc/privacy_policy.html',
            controller: 'PrivacyCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/privacy_policy.js');
                }
            }
        }).
        when('/contact', {
            templateUrl: '/etc/contact.html',
            controller: 'ContactCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/contact.js');
                }
            }
        }).
        when('/login', {
            templateUrl: '/etc/login.html',
            controller: 'LoginCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/login.js');
                }
            }
        }).
        when('/activateaccount/:key/', {
            templateUrl: '/etc/activateaccount.html',
            controller: 'AcctivateAccountCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/activateaccount.js');
                }
            }
        }).
        when('/resetpassword', {
            templateUrl: '/etc/resetpassword.html',
            controller: 'ResetPasswordCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/resetpassword.js');
                }
            }
        }).
        when('/resetpasswordlink/:key/', {
            templateUrl: '/etc/resetpasswordlink.html',
            controller: 'ResetPasswordLinkCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/resetpasswordlink.js');
                }
            }
        }).
        when('/accept_terms/:key/', {
            templateUrl: '/etc/accept_terms.html',
            controller: 'AcceptTermsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/etc/js/accept_terms.js');
                }
            }
        }).
        when('/company_stockclasses/:key/', {
            templateUrl: '/company/stockclasses.html',
            controller: 'StockClassesCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/stockclasses.js');
                }
            }
        }).
        when('/company_stockholders/:key/', {
            templateUrl: '/company/stockholders.html',
            controller: 'StockHoldersCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/stockholders.js');
                }
            }
        }).
        when('/company_plans/:key/', {
            templateUrl: '/company/plans.html',
            controller: 'PlansCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/plans.js');
                }
            }
        }).
        when('/company_schedules/:key/', {
            templateUrl: '/company/schedules.html',
            controller: 'SchedulesCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/schedules.js');
                }
            }
        }).
        when('/company_participants/:key/', {
            templateUrl: '/company/participants.html',
            controller: 'ParticipantsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/participants.js');
                }
            }
        }).
        when('/company_terminations/:key/', {
            templateUrl: '/company/terminations.html',
            controller: 'TerminationsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/terminations.js');
                }
            }
        }).
        when('/optionee_stocks/:key/', {
            templateUrl: '/optionee/stocks.html',
            controller: 'OptioneeStockSummaryCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/optionee/js/stocks.js');
                }
            }
        }).
        when('/company_grants/:key/', {
            templateUrl: '/company/grants.html',
            controller: 'GrantsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/grants.js');
                }
            }
        }).
        when('/optionee_summary/:key/', {
            templateUrl: '/optionee/summary.html',
            controller: 'OptioneeSummaryCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/optionee/js/summary.js');
                }
            }
        }).
        when('/optionee_grants/:key/', {
            templateUrl: '/optionee/grants.html',
            controller: 'OptioneeGrantsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/optionee/js/grants.js');
                }
            }
        }).
        otherwise({ redirectTo: '/' });
});
