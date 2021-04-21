function AcceptTermsCtrl($scope, $cookies, $http, $routeParams, $location) {
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    $scope.acceptKey = $routeParams.key;
    $scope.navigateUrl = null;

    $scope.baseGoalGet(" - Loading...", "/api/login/get_creds/").baseGoalComplete(function (data) {
        $scope.formData = data;
    });

    // process the form
    $scope.processForm = function () {
        if (!$scope.ACCEPT_TERMS) {
            $scope.save_message = "Please read and accept the terms and conditions.";
            $scope.showDialog = true;
            return;
        }

        $scope.preBusy();
        $http.post("/api/login/accept_terms/", { AcceptTermsKey: $scope.acceptKey, AcceptTerms: $scope.ACCEPT_TERMS }).
            success(function (data) {
                $scope.save_message = data.message;
                if (data.success) {
                    $scope.acceptKey = null;
                };
                $cookies.UUID = data.uuid;
                $cookies.Role = data.Role;
                $scope.navigateUrl = "/";
                $scope.showDialog = true;
                $scope.postBusy();
            }).error(function (data) {
                $scope.save_message = "An error occured during accept terms, try again?";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
