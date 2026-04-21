function OptioneeSummaryCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Summary',
        description: 'Summarize Stock option grants.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/participant/summary/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });
}

