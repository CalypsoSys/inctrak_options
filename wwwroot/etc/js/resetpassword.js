function ResetPasswordCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Reset Your Password ',
        description: 'Forgot your credentials? If you can\'t log into and need to reset your password the just enter your details and we will send you password on your e-mail.',
    });

    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    $scope.userNameEmail = "";

    // process the form
    $scope.processForm = function () {
        if (!$scope.userNameEmail) {
            $scope.save_message = "Please enter username or email address - thx";
            $scope.showDialog = true;
            return;
        }

        $scope.preBusy();
        $http.post("/api/login/resetpassword/", { UserNameEmail: $scope.userNameEmail } ).
            success(function (data) {
                $scope.save_message = data.message;
                if (data.success) {
                    $scope.userNameEmail = "";
                }
                $scope.showDialog = true;
                $scope.postBusy();
            }).error(function (data) {
                $scope.save_message = "An error occured resetting your password, try again?";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
