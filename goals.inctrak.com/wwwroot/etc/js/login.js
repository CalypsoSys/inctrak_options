function LoginCtrl($scope, $cookies, $http, $routeParams, $cookies, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Login here with IncTrak',
        description: 'Looks interesting? If yes, then just login with appropriate credentials',
    });

    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    if ($routeParams.redirect) {
        $scope.save_message = $routeParams.message;
        if ($routeParams.success) {
            $cookies.UUID = $routeParams.uuid;
            $cookies.Role = $routeParams.role;
            if (!$scope.formData) {
                $scope.formData = new Object();
            }
            $scope.formData.USER_NAME = "";
            $scope.formData.PASSWORD = "";
            $scope.formData.EMAIL_ADDRESS = "";
            $scope.formData.GROUP = "";
            $scope.formData.PASSWORD2 = "";
            $scope.formData.IS_REGISTERING = false;
            $scope.navigateUrl = "/";
        }
        $scope.showDialog = true;
    }

    $scope.baseGoalGet(" - Loading...", "/api/login/get_creds/").baseGoalComplete(function (data) {
        $scope.formData = data;
    });

    // process the form
    $scope.processForm = function () {
        if (!$scope.formData.GOOGLE_LOGON && (!$scope.formData.USER_NAME || !$scope.formData.PASSWORD)) {
            $scope.save_message = "Please enter at name and password - thx";
            $scope.showDialog = true;
            return;
        }

        if ($scope.formData.IS_REGISTERING) {
            if (!$scope.formData.ACCEPT_TERMS) {
                $scope.save_message = "Please read and accept the terms and conditions.";
                $scope.showDialog = true;
                return;
            }

            if (!$scope.formData.GOOGLE_LOGON) {
                if (($scope.formData.EMAIL_ADDRESS || $scope.formData.PASSWORD2) &&
                    (!$scope.formData.EMAIL_ADDRESS || !$scope.formData.PASSWORD2)) {
                    $scope.save_message = "If registering please enter email and password verification - thx";
                    $scope.showDialog = true;
                    return;
                }

                if ($scope.formData.password2 && $scope.formData.PASSWORD2 != $scope.formData.PASSWORD) {
                    $scope.save_message = "Passwords do not match, please enter identical passwords - thx";
                    $scope.showDialog = true;
                    return;
                }
            }
        }

        $scope.preBusy();
        var endPoint = ""
        if ($scope.formData.GOOGLE_LOGON) {
            if ($scope.formData.IS_REGISTERING) {
                endPoint = "register_google/";
            } else {
                endPoint = "login_google";
            }
        } else {
            if ($scope.formData.IS_REGISTERING) {
                endPoint = "register_internal/";
            } else {
                endPoint = "login_internal/";
            }
        }

        $http.post("/api/login/" + endPoint, $scope.formData).
            success(function (data) {
                if (data.google_redirect) {
                    window.location = data.google_redirect;
                } else {
                    $scope.save_message = data.message;
                    if (data.success) {
                        $cookies.UUID = data.uuid;
                        $cookies.Role = data.Role;
                        $scope.formData.USER_NAME = "";
                        $scope.formData.PASSWORD = "";
                        $scope.formData.EMAIL_ADDRESS = "";
                        $scope.formData.GROUP = "";
                        $scope.formData.PASSWORD2 = "";
                        $scope.formData.IS_REGISTERING = false;
                        $scope.navigateUrl = "/";
                    }
                    $scope.showDialog = true;
                    $scope.postBusy();
                }
            }).error(function (data) {
                var credType;
                if ($scope.formData.IS_REGISTERING) {
                    credType = "registering";
                } else {
                    credType = "login";
                }
                $scope.save_message = "An error occured during " + credType + ", try again?";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
