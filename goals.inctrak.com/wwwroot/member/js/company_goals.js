function CompanyGoalsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Current Company Goals',
        description: 'Current Company Goals.',
        keywords: 'sprint development goal startup employee'
    });

    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.baseGoalGet(" - Loading...", "/api/member/company_goals/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });
}

