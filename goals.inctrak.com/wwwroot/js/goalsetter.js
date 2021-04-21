var goalSetterApp = angular.module('goalSetter', ['ngRoute', 'ngCookies', 'ngSanitize', 'ui.select']);

goalSetterApp.directive('script', function () {
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

goalSetterApp.controller('GoalSetterCtrl', function ($scope, $rootScope, $route, $routeParams, $location, $window, $cookies) {
    $rootScope.metadata = {
        title: 'IncTrak',
        description: 'IncTrak provides easy access for employers to allow members to manager their goals.',
        keywords: 'sprint development goal startup employee'
    };

    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };

    $scope.isAdmin = function () {
        return $cookies.Role == "admin";
    };

    $scope.isMember = function () {
        return $cookies.Role == "member";
    };

    $scope.memberView = function () {
        $cookies.Role = "member";
        $cookies.MEMBER_VIEW = "true";
        $location.path("/");
        $route.reload();
    };

    $scope.switchMemberView = function () {
        return $cookies.Role == "member" && $cookies.MEMBER_VIEW == "true";
    };

    $scope.companyView = function () {
        if ($cookies.Role == "member" && $cookies.MEMBER_VIEW == "true") {
            $cookies.Role = "admin";
            $cookies.MEMBER_VIEW = "false";
            $location.path("/");
            $route.reload();
        }
    };

    $scope.loggedIn = function () {
        return $cookies.UUID != null && $cookies.UUID != "" && $cookies.UUID != "not set" && $cookies.UUID != "null" && $cookies.UUID != "undefined";
    };

    $scope.$watch(function () { return $location.path(); }, function (newValue, oldValue) {
        var uuid = $cookies.UUID;
        if ((!uuid || uuid == "not set") && ($location.path().indexOf("/company") != -1 || $location.path().indexOf("/member") != -1)) {
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

goalSetterApp.config(function ($routeProvider) {
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
        when('/company_departments/:key/', {
            templateUrl: '/company/departments.html',
            controller: 'DepartmentsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/departments.js');
                }
            }
        }).
        when('/company_teams/:key/', {
            templateUrl: '/company/teams.html',
            controller: 'TeamsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/teams.js');
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
        when('/company_members/:key/', {
            templateUrl: '/company/members.html',
            controller: 'MembersCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/members.js');
                }
            }
        }).
        when('/company_goals/:key/', {
            templateUrl: '/company/goals.html',
            controller: 'GoalsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/company/js/goals.js');
                }
            }
        }).
        when('/member_company/:key/', {
            templateUrl: '/member/company_goals.html',
            controller: 'CompanyGoalsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/member/js/company_goals.js');
                }
            }
        }).
        when('/member_department/:key/', {
            templateUrl: '/member/department_goals.html',
            controller: 'DepartmentGoalsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/member/js/department_goals.js');
                }
            }
        }).
        when('/member_team/:key/', {
            templateUrl: '/member/team_goals.html',
            controller: 'TeamGoalsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/member/js/team_goals.js');
                }
            }
        }).
        when('/member_goals/:key/', {
            templateUrl: '/member/goals.html',
            controller: 'MemberGoalsCtrl',
            resolve: {
                deps: function ($q, $rootScope) {
                    return ctrlResolver($q, $rootScope, '/member/js/goals.js');
                }
            }
        }).
        otherwise({ redirectTo: '/' });
});
