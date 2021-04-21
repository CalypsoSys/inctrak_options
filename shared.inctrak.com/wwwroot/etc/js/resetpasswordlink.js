function ResetPasswordLinkCtrl($scope, $cookies, $http, $routeParams, $location) {
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    $scope.resetKey = $routeParams.key;
    $scope.navigateUrl = null;

    $scope.optioneePlanGet(" - Loading...", "/api/login/get_creds/").optioneePlanComplete(function (data) {
        $scope.formData = data;
    });

    // process the form
    $scope.processForm = function () {
        if (!$scope.ACCEPT_TERMS) {
            $scope.save_message = "Please read and accept the terms and conditions.";
            $scope.showDialog = true;
            return;
        }

        if (!$scope.PASSWORD1 || !$scope.PASSWORD2) {
            $scope.save_message = "Please enter at new password - thx";
            $scope.showDialog = true;
            return;
        }

        if ($scope.PASSWORD1 != $scope.PASSWORD2) {
            $scope.save_message = "Passwords do not match, please enter identical passwords - thx";
            $scope.showDialog = true;
            return;
        }

        $scope.preBusy();
        $http.post("/api/login/resetpasswordlink/", { ResetPasswordKey: $scope.resetKey, Password1: $scope.PASSWORD1, Password2: $scope.PASSWORD2, AcceptTerms: $scope.ACCEPT_TERMS }).
            success(function (data) {
                $scope.save_message = data.message;
                if (data.success) {
                    $scope.resetKey = null;
                    $scope.PASSWORD1 = "";
                    $scope.PASSWORD2 = ""
                };
                $scope.navigateUrl = "/login";
                $scope.showDialog = true;
                $scope.postBusy();
            }).error(function (data) {
                $scope.save_message = "An error occured during login/registering, try again?";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
