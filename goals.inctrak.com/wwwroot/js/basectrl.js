function BaseGoalCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.busyBtn = "btn btn-lg";
    $scope.working = "glyphicon glyphicon-refresh";
    $scope.busyStatus = "";
    $scope.dialogTemplate = '/common/dialog.html';

    $scope.showDialog = false;
    $scope.closeDialog = function () {
        $scope.showDialog = false;
        if ($scope.navigateUrl)
            $location.path($scope.navigateUrl);
    };

    $scope.preBusy = function (status) {
        $scope.busyBtn = "btn btn-lg btn-success";
        $scope.working = "glyphicon glyphicon-refresh spinning";
        $scope.busyStatus = status;
    };
    $scope.postBusy = function () {
        $scope.busyBtn = "btn btn-lg";
        $scope.working = "glyphicon glyphicon-refresh";
        $scope.busyStatus = "";
    };
    $scope.commonReturn = function (fnComplete, data) {
        if (data && data.success === false && data.login === true) {
            $scope.navigateUrl = "/login";
            if (data.message)
                $scope.save_message = data.message;
            else
                $scope.save_message = "A security issue as occured, please login.";
            $scope.showDialog = true;
            $cookies.UUID = null;
            $cookies.Role = null;
        } else if (fnComplete) {
            fnComplete(data);
        } else {
            $scope.navigateUrl = null
            if (data.message)
                $scope.save_message = data.message;
            else
                $scope.save_message = "A error has occured.";
            $scope.showDialog = true;
            $scope.data = data;
        }
    };
    $scope.baseGoalComplete = function (fnComplete) {
        $http.get($scope.url).success(function (data) {
            $scope.commonReturn(fnComplete, data);
        }).finally(function () {
            $scope.postBusy();
        })
        return $scope;
    };
    $scope.baseGoalGet = function (status, url) {
        $scope.preBusy(status);
        $scope.url = url;
        return $scope;
    };
    $scope.baseGoalRemoved = function (fnComplete) {
        $http.delete($scope.url).success(function (data) {
            $scope.commonReturn(fnComplete, data);
        }).finally(function () {
            $scope.postBusy();
        })
        return $scope;
    };
    $scope.baseGoalDelete = function (status, url) {
        $scope.preBusy(status);
        $scope.url = url;
        return $scope;
    };
}