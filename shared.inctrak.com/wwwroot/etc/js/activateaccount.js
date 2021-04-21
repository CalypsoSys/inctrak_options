function AcctivateAccountCtrl($scope, $cookies, $http, $routeParams, $location) {
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    $scope.activateKey = $routeParams.key;
    $scope.navigateUrl = null;

    // process the form
    $scope.processForm = function () {
        $scope.preBusy();
        $http.post("/api/login/activateaccount/", { ActivateKey: $scope.activateKey }).
            success(function (data) {
                $scope.save_message = data.message;
                $scope.navigateUrl = "/login";
                $scope.showDialog = true;
                $scope.postBusy();
            }).error(function (data) {
                $scope.save_message = "An error occured during activating your account, try again?";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
