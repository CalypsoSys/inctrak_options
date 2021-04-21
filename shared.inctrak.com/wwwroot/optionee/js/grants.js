function OptioneeGrantsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Grant',
        description: 'Create your Company Stock option Vesting grant dates.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.showGrants = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.grantKey = null;
    $scope.isShowPeriod = true;
    $scope.isShowDetails = true;
    $scope.isShowTotal = false;
    $scope.isShowVested = false;
    $scope.vestScheduleTemplate = '/common/vestschedule.html';
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/participant/grants/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadGrant = function (key) {
        var uuid = $cookies.UUID;
        $scope.grantKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set grant dates - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/participant/grant/" + $scope.grantKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/optionee_grants/" + Math.random() + "/", name: "> Grant" }];
                        $scope.listItems = [];
                        $scope.showGrants = false;
                        $scope.showGrant = true;
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };
}

