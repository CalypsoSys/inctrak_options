function TeamGoalsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Current Team Goals',
        description: 'Current Team Goals.',
        keywords: 'sprint development goal startup employee'
    });

    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.baseGoalGet(" - Loading...", "/api/member/team_goals/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });
}

